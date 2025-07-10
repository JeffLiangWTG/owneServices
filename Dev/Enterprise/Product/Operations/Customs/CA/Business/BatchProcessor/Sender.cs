using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.BatchProcessor
{
	public class Sender : BaseInterchangeSender
	{
		#region Implementation

		#region Override Protected
		protected override bool IsEnvironmentDataValid()
		{
			bool result = true;
			if (!CACustomsDataRegistry.Instance.SendG7ExportMessages.Value && BatchProcessorUtilities.CompanyInCanada && CACustomsDataRegistry.Instance.ExportDeclarationActive.Value)
			{
				string directoryName = CACustomsDataRegistry.Instance.DataLoadingModuleOutputDirectory.Value;
				if (string.IsNullOrEmpty(directoryName))
				{
					result = false;
					Logger.Log("Data Loading Module output directory hasn't been setup. Please set it up in " + BatchProcessorUtilities.RegistryLocation(CACustomsDataRegistry.Instance.DataLoadingModuleOutputDirectory));
				}
				else if (!Directory.Exists(directoryName))
				{
					result = false;
					Logger.Log("Data Loading Module Output directory does not exist : " + directoryName);
				}
			}
			return result;
		}

		protected override void SendOutboundInterchanges(CancellationToken token)
		{
			if (BatchProcessorUtilities.CompanyInCanada)
			{
				SendOutboundInterchanges(EDIInterchange.ApplicationCodes.CACustoms, token);
				SendOutboundInterchanges(EDIInterchange.ApplicationCodes.CAEXP, token);
				SendOutboundInterchanges(EDIInterchange.ApplicationCodes.CAIMP, token);
			}
			SendOutboundInterchanges(EDIInterchange.ApplicationCodes.CAACI, token);
		}

		protected override ZQuery ValidBranchesForMessageFilter(string[] applicationCode) => BatchProcessorUtilities.AllActiveComapnyBranchesMessageFilter;

		internal const string FileOutputDateFormat = "yyyyMMddHHmmss";
		protected override bool SendInt(EDIInterchange interchange)
		{
			switch (interchange.EI_ApplicationCode)
			{
				case EDIInterchange.ApplicationCodes.CAACI:
					return (SendCIGInt(interchange));
				case EDIInterchange.ApplicationCodes.CAEXP:
					return (SendCIGInt(interchange));
				case EDIInterchange.ApplicationCodes.CAIMP:
					return (SendCIGInt(interchange));
				case EDIInterchange.ApplicationCodes.CACustoms:
					return (SendExportInt(interchange));
				default:
					throw new ApplicationException("Invalid CA Application code: " + interchange.EI_ApplicationCode);
			}
		}

		protected bool SendCIGInt(EDIInterchange interchange)
		{
			string fileName = Path.Combine(CACustomsDataRegistry.Instance.MessageOutputDirectory.Value, interchange.EI_InterchangeNum + "_" + ZDateTime.Now.ToString(FileOutputDateFormat) + ".TXT");
			return DepositInterchangeInToFolder(fileName, interchange);
		}

		protected bool SendExportInt(EDIInterchange interchange)
		{
			string fileName = Path.Combine(CACustomsDataRegistry.Instance.DataLoadingModuleOutputDirectory.Value, interchange.EI_InterchangeNum + "_" + ZDateTime.Now.ToString(FileOutputDateFormat) + ".TXT");
			return DepositInterchangeInToFolder(fileName, interchange);
		}

		public bool DepositInterchangeInToFolder(string fileName, EDIInterchange interchange)
		{
			string errorText = string.Empty;
			bool result = BatchProcessorUtilities.DepositInterchangeInToFolderSafely(fileName, interchange, out errorText);
			if (result)
			{
				interchange.EI_Status = EDIInterchange.Status.Sent;
			}
			else
			{
				interchange.EI_Status = EDIInterchange.Status.Error;
				Logger.LogError("Cannot write to file: " + fileName + "\r\nError: " + errorText);
			}
			return result;
		}

		protected override ZQuery AdditionalFilter
		{
			get
			{
				ZQuery result = base.AdditionalFilter;
				result.AddToFilter(EDIMessageSchema.EM_IsActive, true);
				return result;
			}
		}

		protected override void PackageMessagesIntoInterchanges(NonDependentEDIMessageCollection messages)
		{
			if (messages.Count > 0)
			{
				switch (messages[0].EM_ApplicationCode.ToString())
				{
					case EDIMessage.ApplicationCodes.CAACI:
						ACIInterchangeProvider aCIProvider = new ACIInterchangeProvider(messages);
						aCIProvider.PackCollatedMessagesIntoInterchanges();
						break;
					case EDIMessage.ApplicationCodes.CAEXP:
						EXPInterchangeProvider eXPProvider = new EXPInterchangeProvider(messages);
						eXPProvider.PackCollatedMessagesIntoInterchanges();
						break;
					case EDIMessage.ApplicationCodes.CAIMP:
						foreach (var messageCollection in SplitMessagesBaseOnMessageType(messages.ToArray<Enterprise.Messaging.Business.EDIMessage>()))
						{
							var msgType = messageCollection[0].EM_MessageType;
							if (msgType == MessageTypeList.Codes.IntegratedImportDeclaration)
							{
								CAUDMInterchangeProvider uDMProvider = new CAUDMInterchangeProvider(messageCollection);
								uDMProvider.PackCollatedMessagesIntoInterchanges();
							}
							else if (msgType == MessageTypeList.Codes.CommercialAccountingDeclaration)
							{
								CADInterchangeProvider cadProvider = new CADInterchangeProvider(messageCollection);
								cadProvider.PackCollatedMessagesIntoInterchanges();
							}
							else if (msgType == MessageTypeList.Codes.B3CUSDEC)
							{
								CAB3CInterchangeProvider b3cProvider = new CAB3CInterchangeProvider(messageCollection);
								b3cProvider.PackCollatedMessagesIntoInterchanges();
							}
							else
							{
								IMPInterchangeProvider iMPProvider = new IMPInterchangeProvider(messageCollection);
								iMPProvider.PackCollatedMessagesIntoInterchanges();
							}
						}
						break;
					case EDIMessage.ApplicationCodes.CACustoms:
						InterchangeProvider provider = new InterchangeProvider(messages);
						provider.PackCollatedMessagesIntoInterchanges();
						break;
					default:
						throw new ApplicationException("Invalid CA Application code: " + messages[0].EM_ApplicationCode.ToString());
				}
			}
		}

		IEnumerable<NonDependentEDIMessageCollection> SplitMessagesBaseOnMessageType(Enterprise.Messaging.Business.EDIMessage[] messages)
		{
			if (messages != null && messages.Length > 0)
			{
				var factory = messages[0].Factory;
				var grouped = messages.ToList().GroupBy(message => message.EM_MessageType);
				foreach (var group in grouped)
				{
					var result = new NonDependentEDIMessageCollection(factory);
					result.AddRange(group);
					yield return result;
				}
			}
		}

		protected sealed override bool IsDateFilterUTC
		{
			get { return true; }
		}

		#endregion

		public void ExecuteBatchForDebug()
		{
			ExecuteBatch(CancellationToken.None);
		}

		#endregion
	}
}
