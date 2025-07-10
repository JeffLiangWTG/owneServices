using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.HK.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.HK.ServiceTasks
{
	public class Sender : BaseInterchangeSender
	{
		protected override void SendOutboundInterchanges(CancellationToken token)
		{
			NumberOfOperationsAttempted = 0;
			SendOutboundInterchanges(EDIInterchange.ApplicationCodes.Traxon, token);
		}

		protected override ZQuery ValidBranchesForMessageFilter(string[] applicationCode) => new ZQuery();
		protected override ZQuery ValidBranchesForInterchangesFilter => new ZQuery();

		protected override bool IsEnvironmentDataValid()
		{
			shouldUseEHub = Registry.Business.eHubMessagingRegistry.Instance.SendHKISACEViaEHub.Value;
			return true;
		}
		bool shouldUseEHub;

		OutputEnvironmentSetting GetSetting(ZGuid branchPK)
		{
			if (!branchDictionary.TryGetValue(branchPK, out var result))
			{
				if (factory.Load<GlbBranch>(branchPK) is GlbBranch branch && branch.Company is GlbCompany company)
				{
					var companyPK = company.PK.ToGuid();
					if (!companySettings.TryGetValue(companyPK, out result))
					{
						result = new OutputEnvironmentSetting()
						{
							ShouldUseFTP = FTPProcessor.CheckFTPOutputEnvironments(companyPK),
							ShouldUseLocalDirectory = FTPProcessor.CheckOutputEnvironments(companyPK)
						};
						FTPProcessor.CheckAnyOutputEnvironments(result.ShouldUseFTP, result.ShouldUseLocalDirectory);
						companySettings.Add(companyPK, result);
						foreach (var otherBranch in company.Branches.Where(x => x != branch))
						{
							var otherBranchPK = otherBranch.PK;
							if (branchDictionary.ContainsKey(otherBranchPK))
							{
								branchDictionary[otherBranch.PK] = result;
							}
							else
							{
								branchDictionary.Add(otherBranchPK, result);
							}
						}
					}
				}
				branchDictionary.Add(branchPK, result ?? new OutputEnvironmentSetting());
			}
			return result;
		}

		readonly BusinessObjectFactory factory = new BusinessObjectFactory();

		class OutputEnvironmentSetting
		{
			public bool ShouldUseFTP;
			public bool ShouldUseLocalDirectory;
		}
		readonly Dictionary<Guid, OutputEnvironmentSetting> companySettings = new Dictionary<Guid, OutputEnvironmentSetting>();
		readonly Dictionary<ZGuid, OutputEnvironmentSetting> branchDictionary = new Dictionary<ZGuid, OutputEnvironmentSetting>();

		protected override bool SendInt(EDIInterchange interchange)
		{
			var result = shouldUseEHub;
			if (!result && interchange.Branch is GlbBranch branch)
			{
				var branchPK = branch.PK;
				var setting = GetSetting(branchPK);
				if (setting.ShouldUseFTP || setting.ShouldUseLocalDirectory)
				{
					var companyPK = branch.GB_GC.ToGuid();
					if (GlbBranch.CurrentBranch.PK != branchPK)
					{
						DisposeCurrentSetting();
						currentSetting = branch.SetAsTemporaryContext();
					}
					if (setting.ShouldUseFTP)
					{
						result = UploadViaFTP(companyPK, interchange);
					}
					else if (setting.ShouldUseLocalDirectory)
					{
						result = SendIntToFile(interchange, HKDataRegistry.Instance.HKTraxonOutputDirectory.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty),
							interchange.EI_InterchangeNum + ".edi");
					}
				}
			}
			return result;
		}
		IDisposable currentSetting;

		bool UploadViaFTP(Guid companyPK, EDIInterchange interchange)
		{
			var result = false;
			using (var uploadDirectory = new TempDirectory())
			{
				var filename = interchange.EI_InterchangeNum + ".edi";
				if (SendIntToFile(interchange, uploadDirectory.DirectoryName, filename))
				{
					var fullFilename = Path.Combine(uploadDirectory.DirectoryName, filename);
					result = FTPProcessor.UploadViaFTP(companyPK, fullFilename);
				}
			}
			return result;
		}

		ISACFTPProcessor FTPProcessor
		{
			get { return ftpProcessor ?? (ftpProcessor = new ISACFTPProcessor(Logger)); }
		}
		ISACFTPProcessor ftpProcessor;

		protected override void PackageMessagesIntoInterchanges(NonDependentEDIMessageCollection messages)
		{
			var provider = new TraxonInterchangeProvider(messages);
			_ = provider.Interchanges;
		}

		public override void Dispose()
		{
			DisposeCurrentSetting();
			base.Dispose();
		}

		void DisposeCurrentSetting()
		{
			if (currentSetting != null)
			{
				currentSetting.Dispose();
				currentSetting = null;
			}
		}
	}
}
