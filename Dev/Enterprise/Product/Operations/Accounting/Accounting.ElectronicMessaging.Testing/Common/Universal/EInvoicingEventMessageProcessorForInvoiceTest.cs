using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.ServiceTasks;
using Enterprise.ZArchitecture.Business;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Universal.Testing
{
	internal class EInvoicingEventMessageProcessorForInvoiceTest : TestCaseWithFactory
	{
		public void TestAuthorisationRecordIsCreatedAndUpdated()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Uganda))
			{
				EnableCustomFunctionality(isPilotFunctionality: true, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));

				var objectCreator = new TestObjectCreator(UniversalFactory.BOFactory);
				var invoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", objectCreator.USD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);

				var message = CreateMessage();

				var originalContext = new StringBuilder();
				AddContext(originalContext, EventContextTypeCode.AHF_Counter, "1");
				AddContext(originalContext, EventContextTypeCode.AHF_Number, "100");
				AddContext(originalContext, EventContextTypeCode.AHF_DateTime, "2022-08-01T17:50:00");
				AddContext(originalContext, EventContextTypeCode.AHF_IDType, "TYP");

				message.EM_MessageText = string.Format(ATHMessageForInvoice,
					$"{invoice.AH_Ledger} {invoice.AH_TransactionType} {invoice.AH_TransactionNum}",
					originalContext.ToString());

				UniversalFactory.SaveForTesting();

				AssertEquals("Precondition: Invoice has no ATH log.", 0, invoice.Logs.Find(l => l.SL_SE_NKEvent == Events.AuthorisedCode).Count());
				AssertNull("Precondition: Invoice has no AuthorisationRecord", AccTransactionHeaderAuthorisationRecordLoader.LoadByParentID(UniversalFactory.BOFactory, invoice.PK, Constants.CountryCodes.Uganda));

				var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
				var logger = (TestServiceLogger)serviceTask.ServiceLogger;

				serviceTask.RunTask();

				var newFactory = new BusinessObjectFactory();
				var msgLoadedInNewFactory = newFactory.Load<EDIMessage>(message.PK);
				var invoiceInNewFactory = newFactory.Load<InvoicingBase>(invoice.PK);

				AssertEquals("Message should be processed", EDIMessageStatusList.Codes.ProcessedOK, msgLoadedInNewFactory.EM_Status);
				AssertEquals("invoice should be authorised", 1, invoiceInNewFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.AuthorisedCode).Count());
				var authRecord = AccTransactionHeaderAuthorisationRecordLoader.LoadByParentID(newFactory, invoice.PK, Constants.CountryCodes.Uganda);
				AssertNotNull("Invoice has AuthorisationRecord", authRecord);

				AssertKeyProperties("Created:");
				var authRecordPK = authRecord.PK;

				message = CreateMessage();

				var modifiedKeysContext = new StringBuilder();
				AddContext(modifiedKeysContext, EventContextTypeCode.AHF_Counter, "2");
				AddContext(modifiedKeysContext, EventContextTypeCode.AHF_Number, "101");
				AddContext(modifiedKeysContext, EventContextTypeCode.AHF_DateTime, "2022-08-01T18:35:00");
				AddContext(modifiedKeysContext, EventContextTypeCode.AHF_IDType, "YUP");

				message.EM_MessageText = string.Format(ATHMessageForInvoice,
					$"{invoice.AH_Ledger} {invoice.AH_TransactionType} {invoice.AH_TransactionNum}",
					modifiedKeysContext.ToString());

				UniversalFactory.SaveForTesting();

				logger.ClearLog();
				serviceTask.RunTask();

				newFactory = new BusinessObjectFactory();
				msgLoadedInNewFactory = newFactory.Load<EDIMessage>(message.PK);
				invoiceInNewFactory = newFactory.Load<InvoicingBase>(invoice.PK);

				AssertEquals("Message should be processed with Warning", EDIMessageStatusList.Codes.Warning, msgLoadedInNewFactory.EM_Status);
				Assert(logger.ToString().Contains("Warning|Authorization record field(s) EINV_Counter,EINV_DateTime,EINV_IDType,EINV_Number have already been set for invoice batch , transaction AR INV 00001000, in Eagle Datamation International. You may only set AccTransactionHeaderAuthorisationRecord fields once."));
				AssertEquals("Authorised events", 2, invoiceInNewFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.AuthorisedCode).Count());
				authRecord = AccTransactionHeaderAuthorisationRecordLoader.LoadByParentID(newFactory, invoice.PK, Constants.CountryCodes.Uganda);
				AssertNotNull("Invoice has AuthorisationRecord", authRecord);
				AssertEquals("Same AuthorisationRecord", authRecordPK, authRecord.PK);

				AssertKeyProperties("No update:");

				message = CreateMessage();
				AddContext(originalContext, EventContextTypeCode.AHF_AuthorisationData, "QXV0aG9yaXplZA==");
				AddContext(originalContext, EventContextTypeCode.AHF_IDNumber, "TYP0001");
				AddContext(originalContext, EventContextTypeCode.AHF_ITransactionHash, "SEFTSEFTSEFTSA==");
				AddContext(originalContext, EventContextTypeCode.AHF_PublicKey, "UFVCTElDS0VZ");
				AddContext(originalContext, EventContextTypeCode.AHF_VerificationUrl, "https://check.me/here");

				message.EM_MessageText = string.Format(ATHMessageForInvoice,
					$"{invoice.AH_Ledger} {invoice.AH_TransactionType} {invoice.AH_TransactionNum}",
					originalContext.ToString());

				UniversalFactory.SaveForTesting();

				logger.ClearLog();
				serviceTask.RunTask();

				newFactory = new BusinessObjectFactory();
				msgLoadedInNewFactory = newFactory.Load<EDIMessage>(message.PK);
				invoiceInNewFactory = newFactory.Load<InvoicingBase>(invoice.PK);

				AssertEquals("Message should be processed", EDIMessageStatusList.Codes.ProcessedOK, msgLoadedInNewFactory.EM_Status);
				AssertEquals("Authorised events", 3, invoiceInNewFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.AuthorisedCode).Count());
				authRecord = AccTransactionHeaderAuthorisationRecordLoader.LoadByParentID(newFactory, invoice.PK, Constants.CountryCodes.Uganda);
				AssertNotNull("Invoice has AuthorisationRecord", authRecord);
				AssertEquals("Same AuthorisationRecord", authRecordPK, authRecord.PK);

				AssertKeyProperties("Not chamged after update:");

				AssertEquals("AHF_AuthorisationData", ZBlob.FromAscii("Authorized"), authRecord.AHF_AuthorisationData);
				AssertEquals("AHF_IDNumber", "TYP0001", authRecord.AHF_IDNumber);
				AssertEquals("AHF_ITransactionHash", ZBlob.FromAscii("HASHASHASH"), authRecord.AHF_ITransactionHash);
				AssertEquals("AHF_PublicKey", ZBlob.FromAscii("PUBLICKEY"), authRecord.AHF_PublicKey);
				AssertEquals("AHF_VerificationUrl", "https://check.me/here", authRecord.AHF_VerificationUrl);

				void AssertKeyProperties(string prefix)
				{
					AssertEquals(prefix + "AHF_Number", "100", authRecord.AHF_Number);
					AssertEquals(prefix + "AHF_Counter", "1", authRecord.AHF_Counter);
					AssertEquals(prefix + "AHF_DateTime", new ZDateTime(2022, 8, 1, 17, 50, 0), authRecord.AHF_DateTime.ToZDateTime());
					AssertEquals(prefix + "AHF_IDType", "TYP", authRecord.AHF_IDType);
				}
			}
		}

		public void TestAuthorisationRecordIsNotCreatedAndUpdatedIfValuesExceedMaxLength()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Uganda))
			{
				EnableCustomFunctionality(isPilotFunctionality: true, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));

				var objectCreator = new TestObjectCreator(UniversalFactory.BOFactory);
				var invoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", objectCreator.USD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);

				var message = CreateMessage();

				var originalContext = new StringBuilder();
				AddContext(originalContext, EventContextTypeCode.AHF_Counter, "1".PadLeft(51, '0'));
				AddContext(originalContext, EventContextTypeCode.AHF_IDNumber, "ID001".PadLeft(51, '0'));
				AddContext(originalContext, EventContextTypeCode.AHF_Number, "100".PadLeft(151, '0'));
				AddContext(originalContext, EventContextTypeCode.AHF_IssuerCertificateIdentifier, "Certificate");
				AddContext(originalContext, EventContextTypeCode.AHF_DebtorNumber, "Debtor");
				AddContext(originalContext, EventContextTypeCode.AHF_PlaceOfIssue, "POI");
				AddContext(originalContext, EventContextTypeCode.AHF_DateTime, "2022-08-01T17:50:00");
				AddContext(originalContext, EventContextTypeCode.AHF_IDType, "IDT");

				message.EM_MessageText = string.Format(ATHMessageForInvoice,
					$"{invoice.AH_Ledger} {invoice.AH_TransactionType} {invoice.AH_TransactionNum}",
					originalContext.ToString());

				UniversalFactory.SaveForTesting();

				AssertEquals("Precondition: Invoice has no ATH log.", 0, invoice.Logs.Find(l => l.SL_SE_NKEvent == Events.AuthorisedCode).Count());
				AssertNull("Precondition: Invoice has no AuthorisationRecord", AccTransactionHeaderAuthorisationRecordLoader.LoadByParentID(UniversalFactory.BOFactory, invoice.PK, Constants.CountryCodes.Uganda));

				var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
				var logger = (TestServiceLogger)serviceTask.ServiceLogger;

				serviceTask.RunTask();

				var newFactory = new BusinessObjectFactory();
				var msgLoadedInNewFactory = newFactory.Load<EDIMessage>(message.PK);
				var invoiceInNewFactory = newFactory.Load<InvoicingBase>(invoice.PK);

				AssertEquals("Message should be processed with Warning", EDIMessageStatusList.Codes.Warning, msgLoadedInNewFactory.EM_Status);
				AssertEquals("invoice should NOT be authorised", 1, invoiceInNewFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.AuthorisedCode).Count());
				var authRecord = AccTransactionHeaderAuthorisationRecordLoader.LoadByParentID(newFactory, invoice.PK, Constants.CountryCodes.Uganda);
				AssertNull("Invoice has AuthorisationRecord", authRecord);
				var loggerStr = logger.ToString();
				AssertContains("logger", "Information|Starting processing Message #", loggerStr);
				AssertContains("logger", "Information|Linked Event to Accounts Receivable Invoice.", loggerStr);
				AssertContains("logger", "Warning|Invoice batch , transaction AR INV 00001000, in Eagle Datamation International: The maximum length of following fields has been exceeded", loggerStr);
				AssertContains("logger", "Counter: Maximum Length 50, but 51 were entered", loggerStr);
				AssertContains("logger", "IDNumber: Maximum Length 50, but 51 were entered", loggerStr);
				AssertContains("logger", "Number: Maximum Length 150, but 151 were entered", loggerStr);
				AssertContains("logger", "Information|Finished processing Message #", loggerStr);
			}
		}

		public void TestFunctionalityEnabledForEverybody()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Uganda))
			{
				EnableCustomFunctionality(isPilotFunctionality: false, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

				AssertFubctionalityWorks();
			}
		}

		public void TestFunctionalityEnabledForCompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Uganda))
			{
				EnableCustomFunctionality(isPilotFunctionality: true, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), isCompanyLevel: true);

				AssertFubctionalityWorks();
			}
		}

		public void TestDateIsOutOfEnabledRange()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Uganda))
			{
				EnableCustomFunctionality(isPilotFunctionality: false, ZDateTime.Today.AddDays(-365), ZDateTime.Today.AddDays(-1));

				AssertFunctionalityDoesNotWork();
			}
		}

		public void TestFunctionalityIsNotEnabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Uganda))
			{
				AssertFunctionalityDoesNotWork();
			}
		}

		public void TestFunctionalityIsEnabledForOtherCountry()
		{
			EnableCustomFunctionality(isPilotFunctionality: false, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Uganda))
			{
				AssertFunctionalityDoesNotWork();
			}
		}

		void AssertFubctionalityWorks()
		{
			var objectCreator = new TestObjectCreator(UniversalFactory.BOFactory);
			var invoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", objectCreator.USD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var message = CreateMessageWithContexts(invoice);

			UniversalFactory.SaveForTesting();

			AssertEquals("Precondition: Invoice has no ATH log.", 0, invoice.Logs.Find(l => l.SL_SE_NKEvent == Events.AuthorisedCode).Count());
			AssertNull("Precondition: Invoice has no AuthorisationRecord", AccTransactionHeaderAuthorisationRecordLoader.LoadByParentID(UniversalFactory.BOFactory, invoice.PK, Constants.CountryCodes.Uganda));

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			var logger = (TestServiceLogger)serviceTask.ServiceLogger;

			serviceTask.RunTask();

			AssertNotContains("Warning|Currently Accounting EInvoicing Generic Authorisation Message Import feature is not implemented", logger.ToString());

			var newFactory = new BusinessObjectFactory();
			var msgLoadedInNewFactory = newFactory.Load<EDIMessage>(message.PK);
			var invoiceInNewFactory = newFactory.Load<InvoicingBase>(invoice.PK);

			AssertEquals("Message should be processed", EDIMessageStatusList.Codes.ProcessedOK, msgLoadedInNewFactory.EM_Status);
			AssertEquals("invoice should be authorised", 1, invoiceInNewFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.AuthorisedCode).Count());
			var authRecord = AccTransactionHeaderAuthorisationRecordLoader.LoadByParentID(newFactory, invoice.PK, Constants.CountryCodes.Uganda);
			AssertNotNull("Invoice has AuthorisationRecord", authRecord);
		}

		void AssertFunctionalityDoesNotWork()
		{
			var objectCreator = new TestObjectCreator(UniversalFactory.BOFactory);
			var invoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", objectCreator.USD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var message = CreateMessageWithContexts(invoice);

			UniversalFactory.SaveForTesting();

			AssertEquals("Precondition: Invoice has no ATH log.", 0, invoice.Logs.Find(l => l.SL_SE_NKEvent == Events.AuthorisedCode).Count());
			AssertNull("Precondition: Invoice has no AuthorisationRecord", AccTransactionHeaderAuthorisationRecordLoader.LoadByParentID(UniversalFactory.BOFactory, invoice.PK, Constants.CountryCodes.Uganda));

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			var logger = (TestServiceLogger)serviceTask.ServiceLogger;

			serviceTask.RunTask();

			AssertContains("Warning|Currently Accounting EInvoicing Generic Authorisation Message Import feature is not implemented", logger.ToString());

			var newFactory = new BusinessObjectFactory();
			var msgLoadedInNewFactory = newFactory.Load<EDIMessage>(message.PK);
			var invoiceInNewFactory = newFactory.Load<InvoicingBase>(invoice.PK);

			AssertEquals("Message should be processed", EDIMessageStatusList.Codes.Warning, msgLoadedInNewFactory.EM_Status);
			AssertEquals("Event is still recorded on invoice", 1, invoiceInNewFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.AuthorisedCode).Count());
			var authRecord = AccTransactionHeaderAuthorisationRecordLoader.LoadByParentID(newFactory, invoice.PK, Constants.CountryCodes.Uganda);
			AssertNull("Invoice has no AuthorisationRecord", authRecord);
		}

		public static void EnableCustomFunctionality(bool isPilotFunctionality, ZDateTime startDate, ZDateTime endDate, bool isCompanyLevel = true)
		{
			var factory = new BusinessObjectFactory();
			var refCusCode = factory.NewWithValidTestData<ZZRefCusCodeListCombined>();
			refCusCode.ZZD_Code = EInvoicingEventMessageProcessorForInvoice.AuthorisedFunctionalityCode;
			refCusCode.ZZD_CountryOrGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			refCusCode.ZZD_StartDate = startDate;
			refCusCode.ZZD_EndDate = endDate;

			if (isPilotFunctionality)
			{
				/*PFUNC code is used to enable the feature for pilot customers.
				Attributes will be associated with each code that selectively turn on
					a.CW1 companies using 9 character unique
					b.or CW1 systems using the 6 character unique system code.*/

				refCusCode.ZZD_CodeType = Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC;
				var refCusCodeListAttribute = factory.NewWithValidTestData<ZZRefCusCodeListAttributeCombined>();
				refCusCodeListAttribute.ZZE_ZXE_NKName = isCompanyLevel
					? RefCusCodeListAttributeTypes.Codes.Company
					: RefCusCodeListAttributeTypes.Codes.System;
				refCusCodeListAttribute.ZZE_Value = isCompanyLevel
					? GlbCompany.CurrentCompany.LicenceKeyIdentifier
					: new ZString(GlbCompany.CurrentCompany.LicenceKeyIdentifier.Left(3) + GlbCompany.CurrentCompany.LicenceKeyIdentifier.Right(3));
				refCusCodeListAttribute.ZZE_ZZD_CodeList = refCusCode.PK;
			}
			else
			{
				//FUNCS code type used to enable feature for specified Company
				refCusCode.ZZD_CodeType = Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS;
			}

			factory.Save();
			ZZCustomsFunctionalityEffectiveDate.ClearDictionary();
		}

		EDIMessage CreateMessage()
		{
			var message = UniversalFactory.New<EDIMessage>();
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			return message;
		}

		EDIMessage CreateMessageWithContexts(InvoicingBase invoice)
		{
			var message = CreateMessage();

			var originalContext = new StringBuilder();
			AddContext(originalContext, EventContextTypeCode.AHF_Counter, "1");
			AddContext(originalContext, EventContextTypeCode.AHF_Number, "100");
			AddContext(originalContext, EventContextTypeCode.AHF_DateTime, "2022-08-01T17:50:00");
			AddContext(originalContext, EventContextTypeCode.AHF_IDType, "TYP");

			message.EM_MessageText = string.Format(ATHMessageForInvoice,
				$"{invoice.AH_Ledger} {invoice.AH_TransactionType} {invoice.AH_TransactionNum}",
				originalContext.ToString());

			return message;
		}

		const string ATHMessageForInvoice = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<Company>
				<Code>EDI</Code>
				<Country>
					<Code>UG</Code>
					<Name>Uganda</Name>
				</Country>
				<Name>Eagle Datamation International</Name>
			</Company>
			<DataProvider>EDIDATEDI</DataProvider>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>DAT</ServerID>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccountingInvoice</Type>
					<Key>{0}</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2019-08-01T17:51:00</EventTime>
		<EventType>ATH</EventType>
		<EventParameters>
			<MessageType>Update If Single Match Is Found</MessageType>
		</EventParameters>
		<ContextCollection>{1}
		</ContextCollection>
	</Event>
</UniversalEvent>";

		void AddContext(StringBuilder contexts, string type, string value)
		{
			contexts.AppendLine($@"
			<Context>
				<Type>{type}</Type>
				<Value>{value}</Value>
			</Context>");
		}

		UniversalObjectFactory UniversalFactory => universalFactory ?? (universalFactory = new UniversalObjectFactory());
		UniversalObjectFactory universalFactory;
	}
}
