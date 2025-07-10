using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.CryptoUtilities;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MessageProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Messaging.Business.Res;

[assembly: MessageFilter("MAP", MailDBItemsSchema.Constants.TableName, typeof(Enterprise.BatchProcessor.MailManager.DiagnosticsReportGenerator))]

namespace Enterprise.BatchProcessor.MailManager
{
	public class DiagnosticsReportGenerator
	{
		[MessageFilterCondition(MailDBItemsSchema.Constants.MI_Subject, @"^Messaging Diagnostics Request \(AU\)$")]
		[MessageFilterCondition(MailDBItemsSchema.Constants.MI_From, @"\@acsedi\.edi\.net\.au|\@edi\.com\.au")]
		public bool ProcessMailItem(MailItem item)
		{
			Env.OutgoingMailManager.CreateAndSaveSimple(Res.GetString("1a57a32d-d9d3-449a-ae8f-2e85391ca2cb", "Messaging Diagnostics Report"), GetReport(), item.MI_From);
			return true;
		}

		#region Report

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer only string")]
#if DEBUG
		public virtual
#endif
 string GetReport()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ZStringBuilder builder = new ZStringBuilder();
			try
			{
				builder.Append("===CargoWise One Info===\r\n");
				builder.Append(GetEnterpriseReport() + "\r\n");

				builder.Append("\r\n===Certificate Info===\r\n");
				builder.Append(GetCertificatesReport(factory) + "\r\n");

				foreach (GlbCompany company in factory.Load(typeof(GlbCompany), new ZQuery()))
				{
					builder.Append("\r\n===Company Info===\r\n");
					builder.Append(GetCompanyReport(company));
					builder.Append("\r\n===Branch Info===\r\n");
					foreach (GlbBranch branch in company.Branches.OrderBy(x => x.GB_Code))
					{
						builder.Append(GetBranchReport(branch, company) + "\r\n");
					}
					builder.Append("\r\n\r\n");
				}

				builder.Append(GetMessageAndInterchangeInfo(factory, EDIInterchange.ApplicationCodes.CMR));

				builder.Append("\r\n===Last 5 Queued Outgoing Emails===\r\n");
				builder.Append(GetMailItemsReport(factory, "TRX", "QUE"));

				builder.Append("\r\n===Last 5 Sent Outgoing Emails===\r\n");
				builder.Append(GetMailItemsReport(factory, "TRX", "SNT"));

				builder.Append("\r\n===Last 5 Queued Incoming Emails===\r\n");
				builder.Append(GetMailItemsReport(factory, "RCV", "QUE"));

				builder.Append("\r\n===Last 5 Processed Incoming Emails===\r\n");
				builder.Append(GetMailItemsReport(factory, "RCV", "PRS"));
			}
			catch (Exception exception) when (!exception.IsCriticalException())
			{
				builder.Append("An exception of type " + exception.GetType().Name + " occured whilst generating the report.  Exception message = '" + exception.Message + "'");
			}
			return builder.ToString();
		}

		#endregion

		#region Mail Item Report

		string GetMailItemsReport(BusinessObjectFactory factory, ZString direction, ZString status)
		{
			ZStringBuilder builder = new ZStringBuilder();

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(factory);
			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add("@Direction", direction, MailDBItemsSchema.MI_Direction);
			@params.Add("@Status", status, MailDBItemsSchema.MI_Status);

			collection.Load(LoadLast5MailItemPKs, @params);
			foreach (DynamicBusinessObject bizo in collection)
			{
				builder.Append(GetMailItemReport((MailItem)factory.Load(typeof(MailItem), (ZGuid)bizo[MailItem.Schema.PK])) + "\r\n");
			}
			return builder.ToString();
		}

		string GetMailItemReport(MailItem mailItem)
		{
			StringCollectionX builder = new StringCollectionX();
			builder.Add(FieldValueString(MailDBItemsSchema.MI_Direction, mailItem.MI_Direction));
			builder.Add(FieldValueString(MailDBItemsSchema.MI_Status, mailItem.MI_Status));
			builder.Add(FieldValueString(MailDBItemsSchema.MI_ReceivedDateTime, mailItem.MI_ReceivedDateTime));
			builder.Add(FieldValueString(MailDBItemsSchema.MI_Subject, mailItem.MI_Subject));
			builder.Add(FieldValueString(MailDBItemsSchema.MI_From, mailItem.MI_From));
			return CommaJoinedStringCollection(builder);
		}

		readonly string LoadLast5MailItemPKs = "SELECT TOP 5 " + MailItem.Schema.PK +
			" FROM " + MailItem.Schema.TableName + " WHERE " +
			MailItem.Schema.MI_Direction + " = @Direction AND " +
			MailItem.Schema.MI_Status + " = @Status " +
			" ORDER BY " + MailItem.Schema.MI_ReceivedDateTime + " DESC";

		#endregion

		#region Message And Interchange Report

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer only string")]
		string GetMessageAndInterchangeInfo(BusinessObjectFactory factory, ZString applicationCode)
		{
			ZStringBuilder builder = new ZStringBuilder();
			builder.Append("\r\n===Last 5 Outgoing " + applicationCode + " Messages===\r\n");
			builder.Append(GetMessagesReport(factory, EDIMessage.Direction.Transmit, applicationCode));
			builder.Append("\r\n===Last 5 Incoming " + applicationCode + " Messages===\r\n");
			builder.Append(GetMessagesReport(factory, EDIMessage.Direction.Receive, applicationCode));

			builder.Append("\r\n===Last 5 Outgoing " + applicationCode + " Interchanges===\r\n");
			builder.Append(GetInterchangesReport(factory, EDIInterchange.Direction.Transmit, applicationCode));
			builder.Append("\r\n===Last 5 Incoming " + applicationCode + " Interchanges===\r\n");
			builder.Append(GetInterchangesReport(factory, EDIInterchange.Direction.Receive, applicationCode));

			builder.Append("\r\n");
			return builder.ToString();
		}

		string GetMessagesReport(BusinessObjectFactory factory, ZString direction, ZString applicationCode)
		{
			ZStringBuilder builder = new ZStringBuilder();

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(factory);
			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add("@Direction", direction, EDIMessageSchema.EM_ReceiveTransmit);
			@params.Add("@ApplicationCode", applicationCode, EDIMessageSchema.EM_ApplicationCode);

			collection.Load(LoadLast5MessagePKs, @params);
			foreach (DynamicBusinessObject bizo in collection)
			{
				builder.AppendLine(GetMessageReport(factory.Load<EDIMessage>((ZGuid)bizo[EDIMessage.Schema.PK])));
			}
			return builder.ToString();
		}

		readonly string LoadLast5MessagePKs =
			"SELECT TOP 5 " + EDIMessage.Schema.PK +
			" FROM " + EDIMessageSchema.Constants.SqlSchemaName + "." + EDIMessageSchema.Constants.TableName +
			" WHERE " + EDIMessageSchema.EM_ReceiveTransmit.Name + " = @Direction" +
			" AND " + EDIMessageSchema.EM_ApplicationCode.Name + " = @ApplicationCode" +
			" ORDER BY " + EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " DESC";

		string GetInterchangesReport(BusinessObjectFactory factory, ZString direction, ZString applicationCode)
		{
			ZStringBuilder builder = new ZStringBuilder();

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(factory);
			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add("@Direction", direction, EDIInterchangeSchema.EI_ReceiveTransmit);
			@params.Add("@ApplicationCode", applicationCode, EDIInterchangeSchema.EI_ApplicationCode);

			collection.Load(LoadLast5InterchangePKs, @params);
			foreach (DynamicBusinessObject bizo in collection)
			{
				builder.AppendLine(GetInterchangeReport((EDIInterchange)factory.Load(typeof(EDIInterchange), (ZGuid)bizo[EDIInterchange.Schema.PK])));
			}
			return builder.ToString();
		}

		readonly string LoadLast5InterchangePKs =
			"SELECT TOP 5 " + EDIInterchange.Schema.PK +
			" FROM " + EDIInterchangeSchema.Constants.SqlSchemaName + "." + EDIInterchangeSchema.Constants.TableName +
			" WHERE " + EDIInterchangeSchema.EI_ReceiveTransmit.Name + " = @Direction" +
			" AND " + EDIInterchangeSchema.EI_ApplicationCode.Name + " = @ApplicationCode " +
			" ORDER BY " + EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";

		string GetInterchangeReport(EDIInterchange interchange)
		{
			StringCollectionX builder = new StringCollectionX();
			builder.Add(FieldValueString(EDIInterchangeSchema.EI_ReceiveTransmit.Name, interchange.EI_ReceiveTransmit));
			builder.Add(FieldValueString(EDIInterchangeSchema.EI_ApplicationCode, interchange.EI_ApplicationCode));
			builder.Add(FieldValueString(EDIInterchangeSchema.EI_Status, interchange.EI_Status));
			builder.Add(FieldValueString(EDIInterchangeSchema.EI_HeaderText, interchange.EI_HeaderText));
			return CommaJoinedStringCollection(builder);
		}

		string GetMessageReport(EDIMessage message)
		{
			StringCollectionX builder = new StringCollectionX();
			builder.Add(FieldValueString(EDIMessageSchema.EM_ReceiveTransmit, message.EM_ReceiveTransmit));
			builder.Add(FieldValueString(EDIMessageSchema.EM_ApplicationCode, message.EM_ApplicationCode));
			builder.Add(FieldValueString(EDIMessageSchema.EM_MessageType, message.EM_MessageType));
			builder.Add(FieldValueString(EDIMessageSchema.EM_MessageSubType, message.EM_MessageSubType));
			builder.Add(FieldValueString(EDIMessageSchema.EM_Status, message.EM_Status));
			builder.Add(FieldValueString(EDIMessageSchema.EM_MessageText, message.EM_MessageText.Left(50)));
			builder.Add(FieldValueString("AddedTime", message.EM_SystemCreateTimeUtc));
			return CommaJoinedStringCollection(builder);
		}

		string FieldValueString(SchemaColumn column, object value)
		{
			return FieldValueString(column.Name, value);
		}

		string FieldValueString(string columnName, object value)
		{
			return columnName + " = '" + value.ToString() + "'";
		}

		string CommaJoinedStringCollection(StringCollectionX collection)
		{
			return string.Join(", ", collection.ToArray());
		}

		#endregion

		#region Enterprise Level Report

#if DEBUG
		protected virtual
#endif
 string GetEnterpriseReport()
		{
			StringCollectionX builder = new StringCollectionX();
			builder.Add(FieldValueString("SMTPServer", Env.Registry.SMTPServer));
			builder.Add(FieldValueString("SMTPPort", Env.Registry.SMTPPort));
			builder.Add(FieldValueString("SMTPUsername", Env.Registry.SMTPUsername));
			builder.Add(FieldValueString("MailServer", Env.Registry.MailServer));
			builder.Add(FieldValueString("MailServerPort", Env.Registry.MailServerPort));
			builder.Add(FieldValueString("MailboxEmailAddress", Env.Registry.MailboxEmailAddress));
			builder.Add(FieldValueString("MailboxUserName", Env.Registry.MailboxUserName));
			return CommaJoinedStringCollection(builder);
		}

		#endregion

		#region Certificate Info Report

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant, May be a part of SQL expression.")]
		string GetCertificatesReport(BusinessObjectFactory factory)
		{
			ZStringBuilder builder = new ZStringBuilder();

			using (var certificateManager = ObjectFactory.New<Enterprise.Integration.Customs.AU.ICertificateManager>(factory))
			{
				var trustPointCertificateData = certificateManager.TrustPointCertificateData;
				if (trustPointCertificateData != null && trustPointCertificateData.Length != 0)
				{
					builder.Append("Trust Point: ");
					try
					{
						using (Certificate cert = new Certificate(trustPointCertificateData))
						{
							builder.Append(GetCertificateReport(cert) + "\r\n");
						}
					}
					catch (Exception exception) when (!exception.IsCriticalException())
					{
						builder.Append("ERROR = " + exception.Message + "\r\n");
					}
				}

				var customsCertificateData = certificateManager.CustomsCertificateData;
				if (customsCertificateData != null && customsCertificateData.Length != 0)
				{
					builder.Append("Customs Encryption: ");
					try
					{
						using (Certificate cert = new Certificate(customsCertificateData))
						{
							builder.Append(GetCertificateReport(cert) + "\r\n");
						}
					}
					catch (Exception exception) when (!exception.IsCriticalException())
					{
						builder.Append("ERROR = " + exception.Message + "\r\n");
					}
				}

				var companyCertificateData = certificateManager.CompanyCertificateData;
				var companyCertificatePassword = certificateManager.CompanyCertificatePassword;
				if (companyCertificateData != null && companyCertificateData.Length != 0 && companyCertificatePassword != null && companyCertificatePassword.Length != 0)
				{
					builder.Append("Company Cert: ");
					try
					{
						using (Store store = new Store(companyCertificateData, companyCertificatePassword, Enterprise.Registry.Business.SystemDataRegistry.Instance.StoreCertificatesUnderCurrentUser.Value))
						{
							builder.Append(GetCertificateReport(store) + "\r\n");
						}
					}
					catch (Exception exception) when (!exception.IsCriticalException())
					{
						builder.Append("ERROR = " + exception.Message + "\r\n");
					}
				}
			}

			return builder.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		string GetCertificateReport(Enterprise.CryptoUtilities.ICryptoContainer cert)
		{
			StringCollectionX builder = new StringCollectionX();
			builder.Add(FieldValueString("Name", cert.Name));
			builder.Add(FieldValueString("EmailAddress", cert.EmailAddress));
			builder.Add(FieldValueString("SerialNumber", cert.SerialNumber));
			builder.Add(FieldValueString("ValidFromDate", cert.ValidFromDate));
			builder.Add(FieldValueString("ValidToDate", cert.ValidToDate));
			return CommaJoinedStringCollection(builder);
		}

		#endregion

		#region Company Report

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		string GetCompanyReport(GlbCompany company)
		{
			StringCollectionX builder = new StringCollectionX();
			builder.Add(FieldValueString("Company Name", company.GC_Name));
			builder.Add(FieldValueString(GlbCompanySchema.Constants.GC_Code, company.GC_Code));
			builder.Add(FieldValueString(GlbCompanySchema.Constants.GC_BusinessRegNo, company.GC_BusinessRegNo));
			builder.Add(FieldValueString(GlbCompanySchema.Constants.GC_CustomsRegistrationNo, company.GC_CustomsRegistrationNo));
			builder.Add(FieldValueString("CustomsMailbox", Env.Registry.GetAUCustomsSenderID(company.PK.ToGuid(), Guid.Empty)));
			builder.Add(FieldValueString("SeaDepotMailbox", Env.Registry.GetAUCustomsSeaCargoDepotMailbox(company.PK.ToGuid(), Guid.Empty)));
			builder.Add(FieldValueString("CMRTestMode", Env.Registry.GetCMRTestMode(company.PK.ToGuid())));
			return CommaJoinedStringCollection(builder);
		}

		#endregion

		#region Branch Report

		string GetBranchReport(GlbBranch branch, GlbCompany company)
		{
			StringCollectionX builder = new StringCollectionX();
			builder.Add(FieldValueString("BranchCode", branch.GB_Code));
			builder.Add(FieldValueString("CustomsMailbox", Env.Registry.GetAUCustomsSenderID(company.PK.ToGuid(), branch.PK.ToGuid())));
			builder.Add(FieldValueString("SeaDepotMailbox", Env.Registry.GetAUCustomsSeaCargoDepotMailbox(company.PK.ToGuid(), branch.PK.ToGuid())));
			builder.Add(FieldValueString("CMRTestMode", Env.Registry.GetCMRTestMode(company.PK.ToGuid())));
			return CommaJoinedStringCollection(builder);
		}

		#endregion
	}
}
