using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.VersionReport;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Licensing
{
	public class LicenceUsageReportBuilder
	{
		public LicenceUsageReportBuilder(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public bool HasUsage
		{
			get { return logToBuild != null; }
		}

		internal void Add(IList<UsageLog> logItems)
		{
			if (logItems.Count == 0)
			{
				return;
			}

			logToBuild = new LicenceConsumptionLogSchema();
			var companyLicenceCodeMap = new Dictionary<Guid, string>();
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var enterpriseCode = registrationKey.EnterpriseCode;
			var serverCode = registrationKey.ServerCode;

			foreach (var logItem in logItems)
			{
				ZString branchCode = ZString.Empty;
				ZGuid parentPk = logItem.S7_ParentID;
				ZGuid companyPk = ZGuid.Empty;
				if (logItem.S7_ParentTableCode == GlbBranchSchema.Constants.Prefix)
				{
					GlbBranch branch = Factory.Load<GlbBranch>(parentPk);
					if (branch != null)
					{
						companyPk = branch.GB_GC;
						branchCode = branch.GB_Code;
					}
				}
				else
				{
					companyPk = parentPk;
				}

				if (!companyPk.IsEmpty)
				{
					Guid companyGuid = companyPk.ToGuid();
					string companyLicenceCode;
					if (!companyLicenceCodeMap.TryGetValue(companyGuid, out companyLicenceCode))
					{
						var company = Factory.Load<GlbCompany>(companyPk);
						if (company != null)
						{
							companyLicenceCode = enterpriseCode + company.GC_Code + serverCode;
							companyLicenceCodeMap.Add(companyGuid, companyLicenceCode);
						}
					}

					if (companyLicenceCode != null)
					{
						LicenceConsumptionLogSchemaLicenceConsumptionLogs item = logToBuild.Items.AddNew();
						item.UsageTime = logItem.S7_OpenDateTimeUtc;
						item.CompanyLicenceCode = companyLicenceCode;
						item.LicenceModuleCode = logItem.S7_FormCaption;
						item.LicenceType = logItem.LicenceType;
						item.StaffCode = logItem.S7_GS_NKUser;
						item.BranchCode = branchCode;

						var controllerSplit = logItem.WebKey.Split('|');
						if (controllerSplit.Length >= 2 && logItem.S7_GS_NKUser == "ZZ")
						{
							item.StaffEmail = controllerSplit[1];
							StringBuilder name = new StringBuilder();
							for (int i = 2; i < controllerSplit.Length; i++)
							{
								name.Append(i == 2 ? controllerSplit[i] : " " + controllerSplit[i]);
							}
							item.StaffName = name.ToString();
						}
						else
						{
							GlbStaff user = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, logItem.S7_GS_NKUser);
							if (user != null)
							{
								item.StaffName = user.GS_FullName;
								item.StaffEmail = user.GS_EmailAddress;
							}
						}
					}
				}
			}
		}

		public void BuildLatest(ZDateTime dateFromUtcInclusive, ZDateTime dateToUtcExclusive)
		{
			Build(dateFromUtcInclusive, dateToUtcExclusive);
		}

		void Build(ZDateTime dateFromUtcInclusive, ZDateTime dateToUtcExclusive, string requestedBy)
		{
			Build(dateFromUtcInclusive, dateToUtcExclusive);

			if (logToBuild != null)
			{
				if (!string.IsNullOrEmpty(requestedBy))
				{
					logToBuild.RequestedBy = requestedBy;
				}
				logToBuild.IsRequest = true;
				logToBuild.IsRequestSpecified = true;
				var converter = new BillingTimeConverter();
				logToBuild.DateFrom = converter.ConvertUtcToTimeInBillingTimeZone(dateFromUtcInclusive).Date;
				logToBuild.DateTo = converter.ConvertUtcToTimeInBillingTimeZone(dateToUtcExclusive).Date.AddDays(-1);
			}
		}

		internal sealed class UsageLog
		{
			public string WebKey;
			public DateTime S7_OpenDateTimeUtc;
			public Guid S7_ParentID;
			public string S7_ParentTableCode;
			public string S7_FormCaption;
			public int S7_MouseClicks;
			public string S7_GS_NKUser;

			public string LicenceType => ((ModuleLicenceType)S7_MouseClicks).ToString();
		}

		void Build(ZDateTime dateFromUtcInclusive, ZDateTime dateToUtcExclusive)
		{
			var licenceLogs = new List<UsageLog>(1000);

			const string sql =
@"select
	WebKey = substring(S7_ControllerID, 37, len(S7_ControllerID) - 36)
	, S7_OpenDateTimeUtc
	, S7_ParentID
	, S7_ParentTableCode
	, S7_FormCaption
	, S7_MouseClicks
	, S7_GS_NKUser
from dbo.StmActivityLog
where S7_ControllerID like @Key and S7_OpenDateTimeUtc >= @DateFrom and S7_OpenDateTimeUtc < @DateTo
order by S7_OpenDateTimeUtc
option (recompile)";

			const int TimeoutSeconds = 15 * 60;
			using (var cmd = Db.Connection.Command(sql, TimeoutSeconds)) // optimization of potentially long running query to load only required data
			{
				cmd.AddParameter("@Key", System.Data.SqlDbType.VarChar, LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString() + "%");
				cmd.AddParameter("@DateFrom", System.Data.SqlDbType.DateTime, dateFromUtcInclusive.ToDateTime());
				cmd.AddParameter("@DateTo", System.Data.SqlDbType.DateTime, dateToUtcExclusive.ToDateTime());

				using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
				{
					while (reader.Read())
					{
						int i = 0;
						var log = new UsageLog()
						{
							WebKey = reader.GetString(i++),
							S7_OpenDateTimeUtc = reader.GetDateTime(i++),
							S7_ParentID = reader.GetGuid(i++),
							S7_ParentTableCode = reader.GetString(i++),
							S7_FormCaption = reader.GetString(i++),
							S7_MouseClicks = reader.GetInt32(i++),
							S7_GS_NKUser = reader.GetString(i++)
						};
						licenceLogs.Add(log);
					}
				}
			}
			Add(licenceLogs);
		}

		public string GetCompressedEncryptedText()
		{
			return logToBuild != null ? GetCompressedEncryptedText(logToBuild) : null;
		}

#if DEBUG
		public
#endif
		static string GetCompressedEncryptedText(LicenceConsumptionLogSchema data)
		{
			byte[] compressedData = GetCompressedXml(data);
			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			return Convert.ToBase64String(encoder.Encrypt(compressedData), Base64FormattingOptions.InsertLineBreaks);
		}

		static byte[] GetCompressedXml(LicenceConsumptionLogSchema data)
		{
			using (MemoryStream xmlStream = new MemoryStream())
			{
				ZXmlSerializer serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
				serializer.Serialize(xmlStream, data);
				xmlStream.Position = 0;

				return Compress(xmlStream);
			}
		}

		static byte[] Compress(Stream stream)
		{
			using (MemoryStream zipStream = new MemoryStream())
			{
				ZipCreator creator = new ZipCreator();
				creator.ZipStream("EnterpriseReport.xml", stream, zipStream);
				return zipStream.ToArray();
			}
		}

		public void BuildAndSend(ZDateTime dateFromUtcInclusive, ZDateTime dateToUtcExclusive, string requestedBy)
		{
			Build(dateFromUtcInclusive, dateToUtcExclusive, requestedBy);
			Send();
		}

		internal void Send()
		{
			if (logToBuild != null)
			{
				var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
				logToBuild.EnterpriseCode = registrationKey.EnterpriseCode;
				logToBuild.ServerCode = registrationKey.ServerCode;
				logToBuild.DbServerName = Db.Connection.ServerNameReportedByDatabase;
				logToBuild.DbName = Db.DatabaseName;
				Send(logToBuild);
			}
		}

		static void Send(LicenceConsumptionLogSchema data)
		{
			VersionReportBuilderFactory versionReportFactory = new VersionReportBuilderFactory();
			versionReportFactory.SendCurrent(GetCompressedEncryptedText(data), true);
		}

		internal LicenceConsumptionLogSchema logToBuild;

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;
	}
}
