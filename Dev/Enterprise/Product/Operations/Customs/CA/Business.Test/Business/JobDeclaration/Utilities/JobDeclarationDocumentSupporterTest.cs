using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(JobDeclarationDocumentSupporter))]
	sealed class JobDeclarationDocumentSupporterTest : BaseJobDeclarationDocumentSupportTest
	{
		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		public void TestB3XAdjustmentsDataContext()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var dataContextValue = new DataContextValue(JobDeclarationDocumentSupporter.B3XAdjustmentsDataContext);
			var stmMenuItem = Factory.New<StmMenuItem>();
			AssertNoExceptionThrown(() =>
			{
				var providers = declaration.DocumentSupporter.GetBODocDataProviders(dataContextValue, stmMenuItem);
			});
			AssertContains("Declaration must be a B3X Adjustments.", declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(dataContextValue, stmMenuItem));
		}

		public void TestB2AdjustmentsDataContext()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var dataContextValue = new DataContextValue(JobDeclarationDocumentSupporter.B2AdjustmentsDataContext);
			var stmMenuItem = Factory.New<StmMenuItem>();
			AssertNoExceptionThrown(() =>
			{
				var providers = declaration.DocumentSupporter.GetBODocDataProviders(dataContextValue, stmMenuItem);
			});
			AssertContains("Declaration must be a B2 Adjustments.", declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(dataContextValue, stmMenuItem));
		}

		public void TestB2AdjustmentsForIM2DataContext()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var dataContextValue = new DataContextValue(JobDeclarationDocumentSupporter.B2AdjustmentsForIM2DataContext);
			var stmMenuItem = Factory.New<StmMenuItem>();
			AssertNoExceptionThrown(() =>
			{
				var providers = declaration.DocumentSupporter.GetBODocDataProviders(dataContextValue, stmMenuItem);
			});
			AssertContains("Declaration must be a IM2 Adjustments.", declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(dataContextValue, stmMenuItem));
		}

		public void TestB3XAdjustmentsDataContextSupported()
		{
			var b3x = Factory.New<JobDeclaration>();
			b3x.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			var dataContextValue = new DataContextValue(JobDeclarationDocumentSupporter.B3XAdjustmentsDataContext);
			Assert(b3x.DocumentSupporter.IsDataContextSupported(dataContextValue));

			var providers = b3x.DocumentSupporter.GetBODocDataProviders(dataContextValue, Factory.New<StmMenuItem>());
			AssertEquals(1, providers.Length);
			AssertEquals(typeof(B3XAdjustmentsDocumentWrapper), providers[0].ParentBusinessObject.GetType());
		}

		public void TestB2AdjustmentForIM2Supporterd()
		{
			var originalDeclaration = Factory.New<JobDeclaration>();
			originalDeclaration.JE_DeclarationReference = "B000000001";
			originalDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceHeader = originalDeclaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1";

			invoiceHeader.InvoiceLines.AddNew().JI_FormattedTariff = "1111111";
			invoiceHeader.InvoiceLines.AddNew().JI_FormattedTariff = "2222222";
			originalDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			originalDeclaration.DoMerge();
			Factory.Save();
			var im2Declaration = originalDeclaration.GetNewCopyToB2Declaration();
			originalDeclaration.InvoiceLines[0].Delete();
			im2Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			im2Declaration.DoMerge();
			Factory.Save();

			var dataContextValue = new DataContextValue(JobDeclarationDocumentSupporter.B2AdjustmentsForIM2DataContext);
			Assert(im2Declaration.DocumentSupporter.IsDataContextSupported(dataContextValue));

			var providers = im2Declaration.DocumentSupporter.GetBODocDataProviders(dataContextValue, Factory.New<StmMenuItem>());
			AssertEquals(1, providers.Length);
			AssertEquals(typeof(IM2AdjustmentsDocumentWrapper), providers[0].ParentBusinessObject.GetType());

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "B2 Adjustment";

			var expectedMessage = "At least one invoice line was deleted from previous job(Previous Declaration : B000000001).";
			AssertEquals("ErrorMessage for B2 Adjustment", expectedMessage, im2Declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem).ErrorMessage);
		}

		public void TestCheckThatNoExceptionThrownWhenLastB3MessageDoesntExist()
		{
			var menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = ForwardingShipmentDocumentSupporter.CACustomsDocList.B3AsLodged;

			var dec = Factory.New<JobDeclaration>();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

			var dataCtxVal = new DataContextValue(JobDeclarationDocumentSupporter.B3ImportDataContext);
			IBODocDataProvider[] providers = null;
			AssertNoExceptionThrown(() => providers = dec.DocumentSupporter.GetBODocDataProviders(dataCtxVal, menuItem));

			AssertEquals("BODocDataProviders Count", 0, providers.Length);
		}

		public void TestCheckThatNoExceptionThrownWhenLastCADMessageDoesntExist()
		{
			var menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = ForwardingShipmentDocumentSupporter.CACustomsDocList.CADAsLodged;

			var dec = Factory.New<JobDeclaration>();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			var dataCtxVal = new DataContextValue(JobDeclarationDocumentSupporter.CADImportDataContext);
			IBODocDataProvider[] providers = null;
			AssertNoExceptionThrown(() => providers = dec.DocumentSupporter.GetBODocDataProviders(dataCtxVal, menuItem));

			AssertEquals("BODocDataProviders Count", 0, providers.Length);
		}

		public void TestCADEXLeadSheetSupported()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var dataCtxVal = new DataContextValue(JobDeclarationDocumentSupporter.CADEXLeadSheet);
			Assert(dec.DocumentSupporter.IsDataContextSupported(dataCtxVal));

			var providers = dec.DocumentSupporter.GetBODocDataProviders(dataCtxVal, Factory.New<StmMenuItem>());
			AssertEquals(1, providers.Length);
			AssertEquals(typeof(LeadSheetDocumentWrapper), providers[0].ParentBusinessObject.GetType());
		}

		public void TestB2AdjustmentsSupporterd()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var dataContextValue = new DataContextValue(JobDeclarationDocumentSupporter.B2AdjustmentsDataContext);
			Assert(b2.DocumentSupporter.IsDataContextSupported(dataContextValue));

			var providers = b2.DocumentSupporter.GetBODocDataProviders(dataContextValue, Factory.New<StmMenuItem>());
			AssertEquals(1, providers.Length);
			AssertEquals(typeof(B2AdjustmentsDocumentWrapper), providers[0].ParentBusinessObject.GetType());
		}

		public void TestB3ImportSupported()
		{
			var declaration = Factory.New<JobDeclaration>();
			var dataContextValue = new DataContextValue(JobDeclarationDocumentSupporter.B3ImportDataContext);
			AssertEquals(true, declaration.DocumentSupporter.IsDataContextSupported(dataContextValue));
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = ForwardingShipmentDocumentSupporter.CACustomsDocList.B3CurrentData;
			var menuItem2 = Factory.New<StmMenuItem>();
			menuItem2.SU_MenuName = ForwardingShipmentDocumentSupporter.CACustomsDocList.B3AsLodged;

			declaration.MarkApportionmentDirty();
			var expectedMessage = "Apportionment must be run. Please follow Brokerage->Perform Apportionment.";
			AssertEquals("ErrorMessage for B3 Current Data", expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem).ErrorMessage);
			AssertEquals("ErrorMessage for B3 As Accounted", expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem2).ErrorMessage);
			declaration.ResumeApportionment();

			expectedMessage = "B3C entry doesn't exist, or requires merge. Please follow Brokerage->Generate Entries (Merge) to generate entries.";
			AssertEquals("ErrorMessage for B3 Current Data", expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem).ErrorMessage);
			AssertEquals("ErrorMessage for B3 As Accounted", expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem2).ErrorMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			declaration.ResumeApportionment();

			entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
			AssertEquals("GetDataStateBeforeRun IsValid", true, declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem).IsValid);

			declaration.CA_RequiresMerge = true;
			declaration.ResumeApportionment();
			AssertEquals("ErrorMessage for B3 Current Data", expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem).ErrorMessage);
			AssertEquals("ErrorMessage for B3 As Accounted", expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem2).ErrorMessage);

			declaration.CA_RequiresMerge = false;
			declaration.ResumeApportionment();
			AssertEquals("GetDataStateBeforeRun IsValid", true, declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem).IsValid);

			var providers = declaration.DocumentSupporter.GetBODocDataProviders(dataContextValue, menuItem);
			declaration.ResumeApportionment();
			AssertEquals("BODocDataProviders Count", 1, providers.Length);
			AssertEquals("BODocDataProvider Type", typeof(B3ImportDocumentWrapper), providers[0].ParentBusinessObject.GetType());

			expectedMessage = "No accepted B3C message has been found to print B3 (As Accounted).";
			declaration.ResumeApportionment();
			AssertEquals("ErrorMessage for B3 As Accounted when message not sent", expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem2).ErrorMessage);

			AddB3Message(entryHeader, string.Empty, EDIMessage.Direction.Transmit, ZDateTime.Now);
			AddB3Message(entryHeader, B3EntryStatusList.Codes.Error, EDIMessage.Direction.Receive, ZDateTime.Now.AddDays(1));
			AssertEquals("ErrorMessage for B3 As Accounted when accepted message not received", expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem2).ErrorMessage);

			const string interchangeText = @"UNB+UNOA:3+YUSAIRXPN+INETCECPT+110607:0915+7696'UNG+CUSDEC+U10207V1+KI+110607:0915+700+UN+S:99B+10207YUSENT'UNH+679+CUSDEC:S:99B:UN'BGM+:::AB+419+9'LOC+41+497'RFF+TN:400004228'UNS+D'UNS+S'UNT+57+679'UNE+1+700'UNZ+1+7696'";
			var message = B3AsLodgedDocumentWrapperTest.CreateMessageFromInterchangeString(Factory, interchangeText);
			message.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(2);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Sent;
			entryHeader.Messages.Add(message);
			AddB3Message(entryHeader, B3EntryStatusList.Codes.Accepted, EDIMessage.Direction.Receive, ZDateTime.Now.AddDays(3));

			AssertEquals("B3 As Accounted when accepted message received", true, declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem2).IsValid);

			providers = declaration.DocumentSupporter.GetBODocDataProviders(dataContextValue, menuItem2);
			AssertEquals("BODocDataProviders Count", 1, providers.Length);
			AssertEquals("BODocDataProvider Type", typeof(B3ImportDocumentWrapper), providers[0].ParentBusinessObject.GetType());
			var wrapper = (B3ImportDocumentWrapper)providers[0].ParentBusinessObject;
			AssertEquals("Last accepted B3C message should be wrapped", "10207 - 400004228", wrapper.TransactionNumberFormated);

			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			providers = declaration.DocumentSupporter.GetBODocDataProviders(dataContextValue, menuItem);
			declaration.ResumeApportionment();
			AssertEquals("BODocDataProviders Count", 0, providers.Length);
		}

		public void TestCADImportSupported()
		{
			var declaration = Factory.New<JobDeclaration>();
			var dataContextValue = new DataContextValue(JobDeclarationDocumentSupporter.CADImportDataContext);
			AssertEquals(true, declaration.DocumentSupporter.IsDataContextSupported(dataContextValue));
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = ForwardingShipmentDocumentSupporter.CACustomsDocList.CADCurrentData;
			var menuItem2 = Factory.New<StmMenuItem>();
			menuItem2.SU_MenuName = ForwardingShipmentDocumentSupporter.CACustomsDocList.CADAsLodged;

			declaration.MarkApportionmentDirty();
			var expectedMessage = "Apportionment must be run. Please follow Brokerage->Perform Apportionment.";
			AssertEquals("ErrorMessage for CAD Current Data", expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem).ErrorMessage);
			AssertEquals("ErrorMessage for CAD As Accounted", expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem2).ErrorMessage);
			declaration.ResumeApportionment();

			expectedMessage = "CAD entry doesn't exist, or requires merge. Please follow Brokerage->Generate Entries (Merge) to generate entries.";
			AssertEquals("ErrorMessage for CAD Current Data", expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem).ErrorMessage);
			AssertEquals("ErrorMessage for CAD As Accounted", expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem2).ErrorMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			declaration.ResumeApportionment();

			entryHeader.CH_EntryStatus = CADEntryStatusList.Codes.Approved;
			AssertEquals("GetDataStateBeforeRun IsValid", true, declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem).IsValid);

			declaration.CA_RequiresMerge = true;
			declaration.ResumeApportionment();
			AssertEquals("ErrorMessage for CAD Current Data", expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem).ErrorMessage);
			AssertEquals("ErrorMessage for CAD As Accounted", expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem2).ErrorMessage);

			declaration.CA_RequiresMerge = false;
			declaration.ResumeApportionment();
			AssertEquals("GetDataStateBeforeRun IsValid", true, declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem).IsValid);

			var providers = declaration.DocumentSupporter.GetBODocDataProviders(dataContextValue, menuItem);
			declaration.ResumeApportionment();
			AssertEquals("BODocDataProviders Count", 1, providers.Length);
			AssertEquals("BODocDataProvider Type", typeof(CADImportDocumentWrapper), providers[0].ParentBusinessObject.GetType());

			expectedMessage = "No response CAD message has been found to print CAD (As Accounted).";
			declaration.ResumeApportionment();
			AssertEquals("ErrorMessage for CAD As Accounted when no response message", expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem2).ErrorMessage);

			var messageText = ZString.Empty;
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				messageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.DocWrappers.TestFiles.CADMessage1.xml");
			}
			var message = Factory.New<CADMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.CAIMP;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Received;
			message.EM_MessageSubType = MessageStatusList.Codes.ClearOriginal;
			message.EM_MessageText = messageText;
			entryHeader.Messages.Add(message);
			AssertEquals("CAD As Accounted when cad response message exsited", true, declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem2).IsValid);

			providers = declaration.DocumentSupporter.GetBODocDataProviders(dataContextValue, menuItem2);
			AssertEquals("BODocDataProviders Count", 1, providers.Length);

			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			providers = declaration.DocumentSupporter.GetBODocDataProviders(dataContextValue, menuItem);
			declaration.ResumeApportionment();
			AssertEquals("BODocDataProviders Count", 0, providers.Length);
		}

		public void TestCreditCheckOnB3CurrentDataOnNewDeclaration_NoExceptionThrown()
		{
			var declaration = Factory.New<JobDeclaration>();
			var dataContextValue = new DataContextValue(JobDeclarationDocumentSupporter.B3ImportDataContext);

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = ForwardingShipmentDocumentSupporter.CACustomsDocList.B3CurrentData;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			declaration.ResumeApportionment();

			AssertNoExceptionThrown("Credit check shouldn't run on a new Declaration", () => declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem));
		}

		public void TestMessageErrorWhenCreditCheck()
		{
			var expectedMessage = @"Delivery of this document is restricted because:
       The Importer, Supplier, Local Client for Billing or any Debtors in associated Shipment
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.";

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.CompanyData.OB_IsDebtor = true;
			supplier.MiscServ.OM_ARGlobalCreditApproved = true;
			supplier.MiscServ.OM_ARGlobalOnCreditHold = true;

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = supplier.PK;
			var command = Factory.New<DocumentCommand>();
			command.SU_BusinessContext = "DummyContext";
			command.SU_MenuName = ForwardingShipmentDocumentSupporter.CACustomsDocList.B3CurrentData;
			command.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.FillWithValidTestData();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.TotalDutyAmount, 200m);

			var jobHeader = new JobHeader.Loader(declaration).TryLoadOrCreate();
			Factory.Save();

			using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions(() =>
				{
					declaration.ResumeApportionment();
					declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
					AssertEquals(expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(command).ErrorMessage);

					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					entryHeader.CH_EntryStatus = "";
					AssertEquals(expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(command).ErrorMessage);
					var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobHeader.PK));
					AssertEquals("No charge should be created", 0, charges.Length);

					entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
					AssertEquals(expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(command).ErrorMessage);
					charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobHeader.PK));
					AssertEquals("No charge should be created", 0, charges.Length);

					entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Error;
					AssertEquals(expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(command).ErrorMessage);
					charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobHeader.PK));
					AssertEquals("No charge should be created", 0, charges.Length);

					entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Confirmed;
					AssertEquals(expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(command).ErrorMessage);
					charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobHeader.PK));
					AssertEquals("No charge should be created", 0, charges.Length);

					entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.SyntaxError;
					AssertEquals(expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(command).ErrorMessage);
					charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobHeader.PK));
					AssertEquals("No charge should be created", 0, charges.Length);
				});
			}

			command.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.NON);
			Factory.Save();

			using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions(() =>
				{
					declaration.ResumeApportionment();
					declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
					AssertNotEquals(expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(command).ErrorMessage);

					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					entryHeader.CH_EntryStatus = "";
					AssertNotEquals(expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(command).ErrorMessage);
					var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobHeader.PK));
					AssertEquals("No charge should be created", 0, charges.Length);

					entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
					AssertNotEquals(expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(command).ErrorMessage);
					charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobHeader.PK));
					AssertEquals("No charge should be created", 0, charges.Length);

					entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Error;
					AssertNotEquals(expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(command).ErrorMessage);
					charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobHeader.PK));
					AssertEquals("No charge should be created", 0, charges.Length);

					entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Confirmed;
					AssertNotEquals(expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(command).ErrorMessage);
					charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobHeader.PK));
					AssertEquals("No charge should be created", 0, charges.Length);

					entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.SyntaxError;
					AssertNotEquals(expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(command).ErrorMessage);
					charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobHeader.PK));
					AssertEquals("No charge should be created", 0, charges.Length);
				});
			}

			expectedMessage = $"User defined delivery restriction condition is not met. {System.Environment.NewLine}";

			command.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.UDF);
			command.SU_DeliveryRestrictionMacro = "#$#$";
			Factory.Save();

			using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions(() =>
				{
					declaration.ResumeApportionment();
					declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
					AssertEquals(expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(command).ErrorMessage);

					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					entryHeader.CH_EntryStatus = "";
					AssertEquals(expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(command).ErrorMessage);
					var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobHeader.PK));
					AssertEquals("No charge should be created", 0, charges.Length);

					entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
					AssertEquals(expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(command).ErrorMessage);
					charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobHeader.PK));
					AssertEquals("No charge should be created", 0, charges.Length);

					entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Error;
					AssertEquals(expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(command).ErrorMessage);
					charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobHeader.PK));
					AssertEquals("No charge should be created", 0, charges.Length);

					entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Confirmed;
					AssertEquals(expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(command).ErrorMessage);
					charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobHeader.PK));
					AssertEquals("No charge should be created", 0, charges.Length);

					entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.SyntaxError;
					AssertEquals(expectedMessage, declaration.DocumentSupporter.GetDataStateBeforeRun(command).ErrorMessage);
					charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobHeader.PK));
					AssertEquals("No charge should be created", 0, charges.Length);
				});
			}
		}

		public void TestLVSIdentifierDetailsSupported()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().JZ_InvoiceNumber = "1";
			declaration.Invoices.AddNew().JZ_InvoiceNumber = "2";
			declaration.Invoices.AddNew().JZ_InvoiceNumber = "3";
			EventHandler<CancelEventArgs> func1 = (s, e) => e.Cancel = true;
			var invoicesToPrint = declaration.LinesToPrint;
			EventHandler<CancelEventArgs> func2 = (s, e) => invoicesToPrint[1].ShouldBePrinted = false;
			declaration.OnGetLinesToPrint += func1;

			foreach (JobComInvoiceHeaderToPrint invoice in invoicesToPrint)
			{
				AssertEquals("Pre-condition: JZ_ShouldBePrinted", true, invoice.ShouldBePrinted);
			}

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "LVS Identifier Details";

			var dataState = declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("Printing should be canceled", false, dataState.IsValid);
			AssertEquals("No message should be shown when canceled", string.Empty, dataState.ErrorMessage);
			AssertEquals("Invoice 1 should be printed", true, invoicesToPrint[0].ShouldBePrinted);
			AssertEquals("Invoice 2 should be printed", true, invoicesToPrint[1].ShouldBePrinted);
			AssertEquals("Invoice 3 should be printed", true, invoicesToPrint[2].ShouldBePrinted);

			declaration.OnGetLinesToPrint -= func1;
			declaration.OnGetLinesToPrint += func2;

			dataState = declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("DataStateBeforeRun.IsValid", true, dataState.IsValid);
			AssertEquals("Invoice 1 should be printed", true, invoicesToPrint[0].ShouldBePrinted);
			AssertEquals("Invoice 2 should NOT be printed", false, invoicesToPrint[1].ShouldBePrinted);
			AssertEquals("Invoice 3 should be printed", true, invoicesToPrint[2].ShouldBePrinted);

			var wrappers = declaration.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.CommercialInvoice, menuItem);
			AssertEquals("2 invoices should be wrapped", 2, wrappers.Length);
			AssertEquals("Invoice 1 should be wrapped", "1", wrappers[0]["InvoiceNumber"]);
			AssertEquals("Invoice 3 should be wrapped", "3", wrappers[1]["InvoiceNumber"]);

			declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().JZ_InvoiceNumber = "1";
			var count = 0;
			declaration.OnGetLinesToPrint += (s, c) => count++;
			dataState = declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("DataStateBeforeRun.IsValid", true, dataState.IsValid);
			AssertEquals("OnGetInvoicesToPrint should not be run", 0, count);
			wrappers = declaration.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.CommercialInvoice, menuItem);
			AssertEquals("1 invoice should be wrapped", 1, wrappers.Length);
			AssertEquals("Invoice 1 should be wrapped", "1", wrappers[0]["InvoiceNumber"]);
		}

		public void TestReleaseStatusDocumentSupported()
		{
			var testHelper = new DeclarationTestHelper(Factory, true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN1";
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN2";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entry.Messages.Add(testHelper.GetEDIReleaseResponseMessage("CCN1", "1"));
			entry.Messages.Add(testHelper.GetEDIReleaseResponseMessage("CCN2", "2"));
			entry.Messages.Add(testHelper.GetEDIReleaseResponseMessage("CCN3", "3"));
			declaration.ResumeApportionment();

			var dataContextValue = new DataContextValue(nameof(Constants.DataContext.GenericFreightJobByReleaseStatus));
			AssertEquals("GenericFreightJobByReleaseStatus data context supported", true, declaration.DocumentSupporter.IsDataContextSupported(dataContextValue));

			var releaseStatusesToPrint = declaration.ReleaseStatusesToPrint;
			EventHandler<CancelEventArgs> func1 = (s, e) => e.Cancel = true;
			EventHandler<CancelEventArgs> func2 = (s, e) => releaseStatusesToPrint[1].RL_ShouldBePrinted = false;
			declaration.OnGetReleaseStatusesToPrint += func1;

			AssertEquals("Release Status 1 should be printed by default", true, releaseStatusesToPrint[0].RL_ShouldBePrinted);
			AssertEquals("Release Status 2 should be printed by default", true, releaseStatusesToPrint[1].RL_ShouldBePrinted);

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Customs Release Status";

			var dataState = declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem);

			AssertEquals("Printing of this document was canceled.", false, dataState.IsValid);
			AssertEquals("Printing of this document was canceled.", dataState.ErrorMessage);
			AssertEquals("Release Status 1 should be printed", true, releaseStatusesToPrint[0].RL_ShouldBePrinted);
			AssertEquals("Release Status 2 should be printed", true, releaseStatusesToPrint[1].RL_ShouldBePrinted);

			declaration.OnGetReleaseStatusesToPrint -= func1;
			declaration.OnGetReleaseStatusesToPrint += func2;

			dataState = declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("DataStateBeforeRun.IsValid", true, dataState.IsValid);
			AssertEquals("Release Status 1 should be printed", true, releaseStatusesToPrint[0].RL_ShouldBePrinted);
			AssertEquals("Release Status 2 should NOT be printed", false, releaseStatusesToPrint[1].RL_ShouldBePrinted);

			var wrappers = declaration.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJobByReleaseStatus, menuItem);
			AssertEquals("1 release statuses should be wrapped", 1, wrappers.Length);
			AssertEquals("Release Status 1 should be wrapped", "CCN1", GetCCNFromDocReleaseStatusWrapper(wrappers[0]));

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			declaration.ResumeApportionment();

			dataState = declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("No messages to print", false, dataState.IsValid);
			AssertEquals("No release status update has been found to print Customs Release Status.", dataState.ErrorMessage);

			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN4";
			entry.Messages.Add(testHelper.GetEDIReleaseResponseMessage("CCN4", "4"));
			declaration.ReleaseStatuses.Load();
			var count = 0;
			declaration.OnGetReleaseStatusesToPrint += (s, c) => count++;

			dataState = declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("DataStateBeforeRun.IsValid", true, dataState.IsValid);
			AssertEquals("OnGetReleaseStatusesToPrint should not be run", 0, count);
			wrappers = declaration.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJobByReleaseStatus, menuItem);
			AssertEquals("1 release status should be wrapped", 1, wrappers.Length);
			AssertEquals("Release Status 1 should be wrapped", "CCN4", GetCCNFromDocReleaseStatusWrapper(wrappers[0]));
		}

		public void TestRoutingDocumentSupported()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var dataContextValue = new DataContextValue(nameof(Constants.DataContext.GenericFreightJobRouting));
			AssertEquals("GenericFreightJobRouting data context supported", true, declaration.DocumentSupporter.IsDataContextSupported(dataContextValue));

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "A8A In Bond (Routing)";

			var dataState = declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("DataStateBeforeRun.IsValid", true, dataState.IsValid);
			AssertNull("No transport", declaration.TransportToPrint);

			var transport1 = declaration.TransportsIncludingRelated.AddNew();
			dataState = declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("DataStateBeforeRun.IsValid", true, dataState.IsValid);
			AssertEquals("Select the only transport", transport1, declaration.TransportToPrint);

			var wrappers = declaration.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJobRouting, menuItem);
			AssertEquals("1 declaration should be wrapped", 1, wrappers.Length);
			AssertEquals("The declaration should be wrapped", declaration.PK, ((BusinessObject)wrappers[0].WrappedObject).PK);

			var transport2 = declaration.TransportsIncludingRelated.AddNew();
			declaration.OnGetTransportToPrint += declaration_OnGetTransportToPrint;
			dataState = declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("DataStateBeforeRun.IsValid", false, dataState.IsValid);

			declaration.OnGetTransportToPrint -= declaration_OnGetTransportToPrint;
			declaration.OnGetTransportToPrint += delegate
			{ declaration.TransportToPrint = transport2; };
			dataState = declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("DataStateBeforeRun.IsValid", true, dataState.IsValid);
			AssertEquals("Select the specified transport", transport2, declaration.TransportToPrint);

			wrappers = declaration.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJobRouting, menuItem);
			AssertEquals("1 declaration should be wrapped", 1, wrappers.Length);
			AssertEquals("The declaration should be wrapped", declaration.PK, ((BusinessObject)wrappers[0].WrappedObject).PK);
		}

		void declaration_OnGetTransportToPrint(object sender, CancelEventArgs e)
		{
			e.Cancel = true;
		}

		public void TestDeclarationDocumentSupported()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var dataContextValue = new DataContextValue(nameof(Constants.DataContext.Declaration));
			AssertEquals("Declaration data context supported", true, declaration.DocumentSupporter.IsDataContextSupported(dataContextValue));

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Invoice Lines Report for Canada";

			var dataState = declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("DataStateBeforeRun.IsValid", true, dataState.IsValid);

			var wrappers = declaration.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.Declaration, menuItem);
			AssertEquals("1 declaration should be wrapped", 1, wrappers.Length);
			AssertEquals("The declaration should be wrapped", declaration.PK, ((BusinessObject)wrappers[0].WrappedObject).PK);
		}

		public void TestGenericFreightDocumentSupported()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var dataContextValue = new DataContextValue(nameof(Constants.DataContext.GenericFreightJob));
			AssertEquals("GenericFreightJob data context supported", true, declaration.DocumentSupporter.IsDataContextSupported(dataContextValue));

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "A8A In Bond";

			var dataState = declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("DataStateBeforeRun.IsValid", true, dataState.IsValid);

			var wrappers = declaration.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, menuItem);
			AssertEquals("1 declaration should be wrapped", 1, wrappers.Length);
			AssertEquals("The declaration should be wrapped", declaration.PK, ((BusinessObject)wrappers[0].WrappedObject).PK);
		}

		public void TestGetFilterValueForCA()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Filter for others OK", "EXP", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKR));
			Declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			AssertEquals("Filter for CAIM2SUPPORT for Export", "Y", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.CAIM2SUPPORT));
			Declaration.JE_IsCancelled = true;
			AssertEquals("Filter for CAIM2SUPPORT for Export", "N", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.CAIM2SUPPORT));

			var b3EntryHeader = Declaration.CustomsEntryHeaders.AddNew();
			b3EntryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertCAAsAccountedDataSupportAndCACurrentDataSupport(Declaration.DocumentSupporter, "B3 Import", "B3C", "B3C");
			Declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertCAAsAccountedDataSupportAndCACurrentDataSupport(Declaration.DocumentSupporter, "B3 LVS", "B3C", "B3C");
			Declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertCAAsAccountedDataSupportAndCACurrentDataSupport(Declaration.DocumentSupporter, "B3 LVX", null, "B3C");

			b3EntryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertCAAsAccountedDataSupportAndCACurrentDataSupport(Declaration.DocumentSupporter, "CAD Import", "CAD", "CAD");
			Declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertCAAsAccountedDataSupportAndCACurrentDataSupport(Declaration.DocumentSupporter, "CAD LVS", "CAD", "CAD");
			Declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertCAAsAccountedDataSupportAndCACurrentDataSupport(Declaration.DocumentSupporter, "CAD LVX", null, "CAD");

			void AssertCAAsAccountedDataSupportAndCACurrentDataSupport(DocumentSupporter supporter, string message, string caAsAccountedDataSupport, string caCurrentDataSupport)
			{
				AssertEquals(message + " CAAsAccountedDataSupport", caAsAccountedDataSupport, supporter.GetFilterValue(DocumentFilters.CAAsAccountedDataSupport));
				AssertEquals(message + " CACurrentDataSupport", caCurrentDataSupport, supporter.GetFilterValue(DocumentFilters.CACurrentDataSupport));
			}
		}

		public new void TestTransportMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var supporter = new JobDeclarationDocumentSupporter(declaration);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Air, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Other, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.NoCarrier;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Other, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Other, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Other, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Rail, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Road, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Sea, supporter.TransportMode);
		}

		public void TestGetB2AdjustmentDocProviderWhenDeclarationIsNotB2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			var dataContextValue = new DataContextValue(JobDeclarationDocumentSupporter.B2AdjustmentsDataContext);
			var providers = declaration.DocumentSupporter.GetBODocDataProviders(dataContextValue, Factory.New<StmMenuItem>());
			AssertEquals(0, providers.Length);

			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			providers = declaration.DocumentSupporter.GetBODocDataProviders(dataContextValue, Factory.New<StmMenuItem>());
			AssertEquals(1, providers.Length);
			AssertEquals(typeof(B2AdjustmentsDocumentWrapper), providers[0].ParentBusinessObject.GetType());
		}

		#region Implementation

		static void AddB3Message(CusEntryHeader entryHeader, string subType, string direction, ZDateTime date, string text = "")
		{
			var message = entryHeader.Factory.New<B3Message>();
			message.EM_MessageSubType = subType;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = message.IsTransmitMessage ? EDIMessage.Status.Sent : EDIMessage.Status.Received;
			message.EM_SystemCreateTimeUtc = date;
			message.EM_MessageText = text;
			entryHeader.Messages.Add(message);
		}

		static string GetCCNFromDocReleaseStatusWrapper(DocumentWrapper wrapper)
		{
			var releaseStatuses = ((BusinessObjectCollection)((BusinessObject)((BusinessObject)((BusinessObject)wrapper["Customs"])["CA"])["DocDeclaration"])["ReleaseStatuses"]);
			AssertEquals("Unrelated statuses should be removed", 1, releaseStatuses.Count);
			return releaseStatuses.First()["CargoControlNumber"].ToString();
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var branch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "BCA");
			if (branch == null)
			{
				var company = Factory.New<GlbCompany>();
				company.GC_Code = "CCA";
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
				branch = Factory.New<GlbBranch>();
				branch.GB_Code = "BCA";
				branch.GB_GC = company.PK;
				branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
				Factory.Save();
			}
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;
			return declaration;
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return GetDocumentSupportBusinessObjectWithLandedCosting();
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			return !documentCommand.SU_MenuName.StartsWith(Enterprise.Customs.Business.BaseJobDeclarationDocumentSupporter.LandedCostingMenuText) || base.ExcludeDocumentCommandTest(documentCommand);
		}

		protected override void DoSetupForDocument(IDocumentCommand command, IDocumentSupportable businessObject)
		{
			if (command.SU_MenuName == ForwardingShipmentDocumentSupporter.CACustomsDocList.ReleaseStatusDocument)
			{
				var helper = new DeclarationTestHelper(Factory, true);
				var declaration = (JobDeclaration)businessObject;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN3";
				declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN4";
				declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN5";

				var entry = declaration.CustomsEntryHeaders[0];
				entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
				entry.Messages.Add(helper.GetEDIReleaseResponseMessage("CCN1", "1"));
				entry.Messages.Add(helper.GetEDIReleaseResponseMessage("CCN2", "2"));
				entry.Messages.Add(helper.GetEDIReleaseResponseMessage("CCN3", "3"));

				Assert(declaration.ReleaseStatusesToPrint.Count > 0);
			}
			else if (command.SU_MenuName == ForwardingShipmentDocumentSupporter.CACustomsDocList.B3AsLodged)
			{
				var declaration = (JobDeclaration)businessObject;
				var entryHeader = declaration.CustomsEntryHeaders[0];
				entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

				const string interchangeText = @"UNB+UNOA:3+YUSAIRXPN+INETCECPT+110607:0915+7696
UNG+CUSDEC+U10207V1+KI+110607:0915+700+UN+S:99B+10207YUSENT
UNH+679+CUSDEC:S:99B:UN
BGM+:::AB+419+9
CST++I
LOC+41+497
LOC+11+423
LOC+18+12345
RFF+TN:400004228
RFF+AEA:0495
RFF+ARA:123241838RM0001
TDT+11++2++3713
DOC+785+3713882555
DTM+204:20110603:102
MOA+43:27587
UNS+D
DMS+1
MOA+64:1023
NAD+SE++MITSUBISHI MATERIALS USA CORP.++++UCA+92708
DOC+935
DTM+129:20110429:102
LOC+27+JP+UCA+3801
PAT+1+CONSIGN:::02+66::D:14
MOA+6::USD
DMS+2
MOA+64:1450
NAD+SE++MITSUBISHI MATERIALS USA CORP.++++UCA+92708
DOC+935
DTM+129:20110501:102
LOC+27+JP+UCA+3801
PAT+1+CONSIGN:::02
MOA+6::USD
CST+1+POS+1+8536509111+23+9902
MOA+40:1200000
MOA+43:1141320
MOA+125:1141320
RFF+ABG:123-456789
RFF+ABA:123456789:1
RFF+MF::1
GIN+PN+NUMBER DESCRIPTION UP TO 39 CHARACT:ERS1+NUMBER DESCRIPTION UP TO 39 CHARS
RFF+LI:1:2
MOA+38:1200000
TAX+1+ADD++31
MOA+46:40200
TAX+1+EXC++5.0
MOA+161:1234
TAX+7+VAT++5.0
MOA+1:57066
GIR+1+1
MEA+AAR++NMB:1200000
MEA+AAA++KGM:9000
TAX+5+++8.0
MOA+155:1234
CST+2+POS+2+8207130010+23
MOA+40:1700500
MOA+43:1617346
MOA+125:1617346
RFF+LI:1:1
MOA+38:1700500
TAX+7+VAT++48
MOA+1:000
GIR+1+2
MEA+AAR++NMB:100000
TAX+5+++0.0678
MOA+155:34567
UNS+S
TAX+5+:::K90
MOA+155:1247200
TAX+1+:::K90
MOA+105:49200
TAX+3+:::K90
MOA+4:19500
TAX+7+:::K90
MOA+1:266366
TAX+4+:::K90
MOA+176:1582266
TAX+5+:::K92
MOA+155:247200
TAX+1+:::K92
MOA+105:9200
TAX+3+:::K92
MOA+4:9500
TAX+7+:::K92
MOA+1:66366
TAX+4+:::K92
MOA+176:582266
UNT+57+679
UNE+1+700
UNZ+1+7696
";
				var message = B3AsLodgedDocumentWrapperTest.CreateMessageFromInterchangeString(Factory, interchangeText);
				message.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(2);
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_Status = EDIMessage.Status.Sent;
				entryHeader.Messages.Add(message);
				AddB3Message(entryHeader, B3EntryStatusList.Codes.Accepted, EDIMessage.Direction.Receive, ZDateTime.Now.AddDays(3));
			}
			else
			{
				base.DoSetupForDocument(command, businessObject);
			}
		}

		protected override ZString GetMainNameSpace() => "Enterprise.Customs.CA.Business.";

		#endregion

	}
}
