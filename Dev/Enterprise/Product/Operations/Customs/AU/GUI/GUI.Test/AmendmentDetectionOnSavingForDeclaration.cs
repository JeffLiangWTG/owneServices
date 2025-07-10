using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using IMessageManageableBizObj = Enterprise.Customs.Business.IMessageManageableBizObj;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AmendmentDetectionOnSavingForDeclaration : Customs.GUI.Testing.AmendmentDetectionOnSavingWithBackDoorSupporterTest
	{
		public void TestAutomaticAmendmentDetectionHandlesOnlyEntriesWithChanges()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_HouseBill = "123";
			testDec.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;
			JobComInvoiceLine line3 = SetUpEntriesForAmendmentTesting(testDec);
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);
			CusEntryHeader originalEntry = testDec.CustomsEntryHeaders[0];
			CusEntryHeader amendEntryWithoutChange = testDec.CustomsEntryHeaders[1];
			CusEntryHeader amendEntryWithChange = testDec.CustomsEntryHeaders[2];
			Factory.Save();
			AssertEquals("PreCondition:Original Entry", CustomsEntryStatus.NotSent.Code, originalEntry.CH_Status);
			AssertEquals("PreCondition:AmendEntryWithoutChange", CustomsEntryStatus.ClearAmendment.Code, amendEntryWithoutChange.CH_Status);
			AssertEquals("PreCondition:AmendEntryWithChange", CustomsEntryStatus.ClearFormalLodge.Code, amendEntryWithChange.CH_Status);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var reason = new CMRAmendmentWithdrawalReason();
			var factory2 = new BusinessObjectFactory();
			var mockDeclaration = factory2.LoadMoq<JobDeclaration>(testDec.PK);
			mockDeclaration.Setup(m => m.IsSavedByFactory)
				.Returns(true);
			mockDeclaration.Object.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var originalEntryLoaded = factory2.Load<CusEntryHeader>(originalEntry.PK);
			var amendEntryWithoutChangeLoaded = factory2.Load<CusEntryHeader>(amendEntryWithoutChange.PK);
			var amendEntryWithChangeLoaded = factory2.Load<CusEntryHeader>(amendEntryWithChange.PK);
			mockDeclaration
				.Protected()
				.Setup<Customs.Business.AmendmentWithdrawalReason>("GetAmendmentWithdrawalReasonCore")
				.Returns(reason);
			var mockController = new Mock<MessagingActionsController>(mockDeclaration.Object) { CallBase = true };
			mockController.Setup(m => m.GetAmendmentWithdrawalReason(reason))
				.Returns(ContinueWithSave.Yes);
			var collection = new CusEntryHeaderMessageStatusFilteredCollection(mockDeclaration.Object.ActiveEntryHeaders);
			mockController
				.Protected()
				.Setup<CusEntryHeaderMessageStatusFilteredCollection>("GetFilteredEntryHeaders", mockDeclaration.Object, new CusEntryHeader[] { amendEntryWithChangeLoaded })
				.Returns(collection);
			mockController
				.Protected()
				.Setup<bool>("ShowCPQAForm", collection)
				.Returns(true);
			new List<Customs.Business.BaseJobComInvoiceLine>(amendEntryWithChangeLoaded.InvoiceLines)[0].JI_Description = "Changed";
			ContinueWithSave canSave = mockController.Object.DetermineRequiredMessagesAndSendThem(new IMDMultiMessageManager(mockDeclaration.Object, CMRMessageTypes.OriginalForAmendmentDetection));
			factory2.Save(); //To trigger status update
			AssertEquals(ContinueWithSave.Yes, canSave);
			AssertEquals("There should be no message generated for OriginalEntry as this was an amendment menu clicked", 0, originalEntryLoaded.Messages.Count);
			AssertEquals("0 message for AmendEntryWithoutChange as there is no change", 0, amendEntryWithoutChangeLoaded.Messages.Count);
			AssertEquals("1 message for AmendEntryWithChange as users choose to send an amendment for all entries", 1, amendEntryWithChangeLoaded.Messages.Count);
			AssertEquals("Status for AmendEntryWithoutChange", CustomsEntryStatus.AwaitingAmendment.Code, amendEntryWithChangeLoaded.CH_Status);
		}

		public void TestMessagingSensitiveChangesStoppedWhileWaitingForResponse()
		{
			SetUpCertificatesAndBrokersLicence();
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "AAA";
			var mockDeclaration = Factory.NewMoq<JobDeclaration>();

			var testDec = mockDeclaration.Object;
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			testDec.JE_ApplicationCode = "CMR";
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = invoice.LocalCurrencyCode;
			invoice.JZ_InvoiceAmount = 10000m;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			testDec.DoMerge();
			Factory.Save();
			testDec.CustomsEntryHeaders[0].CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;

			mockDeclaration.Setup(m => m.JE_MessageStatus)
				.Returns(new ZString(CustomsEntryStatus.AwaitingFormalLodge.Code));

			invoiceLine.JI_Tariff = "0000000000"; //changed
			using (IShowPreSaveDialog form = new ZAUCustomsDeclarationForm(testDec))
			{
				AssertEquals("Should not be able to save as it is waiting for responses", ContinueWithSave.No, form.ShowPreSaveDialogs());
				invoiceLine.JI_Tariff = "0000000001"; //changed
				testDec.CustomsEntryHeaders[0].CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
				var savingOptions = new DeferredAmendmentSavingOptions(testDec);
				savingOptions.SaveWithoutEntryChanges = true;
				var manager = new Mock<IMDMultiMessageManager>(testDec, CMRMessageTypes.OriginalForAmendmentDetection) { CallBase = true };
				manager
					.Protected()
					.Setup<DeferredAmendmentSavingOptions>("GetDeferredAmendmentSavingOptionsCore")
					.Returns(savingOptions);
				mockDeclaration
					.Protected()
					.Setup<Customs.Business.IMessageManager>("GetMessageManagerForAmendmentDetectionCore")
					.Returns(manager.Object);

				AssertEquals("Should be able to save as there is no message pending", ContinueWithSave.Yes, form.ShowPreSaveDialogs());
			}
		}

		public void TestOnBackDoorSavedWithEntryChanges()
		{
			SetUpCertificatesAndBrokersLicence();
			var mockDeclaration = Factory.NewMoq<JobDeclaration>();

			var declaration = mockDeclaration.Object;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = ZDateTime.Today;
			var sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			header.JobComInvoiceLines.AddNew();
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "1";
			declaration.JE_TotalNoOfPacks = 1;
			PackingGroup pack = declaration.PackingGroups[0];
			pack.CR_HouseContainerNumber = 1;
			declaration.DoMerge();
			Factory.Save();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;

			mockDeclaration.Setup(m => m.JE_MessageStatus)
				.Returns(new ZString(CustomsEntryStatus.ClearFormalLodge.Code));

			declaration.JE_HouseBill = "2123 changed";
			var savingOption = new DeferredAmendmentSavingOptions(declaration);
			savingOption.IsCancelled = true;
			var manager = new Mock<IMDMultiMessageManager>(declaration, CMRMessageTypes.OriginalForAmendmentDetection) { CallBase = true };
			manager
				.Protected()
				.Setup<DeferredAmendmentSavingOptions>("GetDeferredAmendmentSavingOptionsCore")
				.Returns(savingOption);
			mockDeclaration
				.Protected()
				.Setup<Customs.Business.IMessageManager>("GetMessageManagerForAmendmentDetectionCore")
				.Returns(manager.Object);

			using (IShowPreSaveDialog form = new ZAUCustomsDeclarationForm(declaration))
			{
				AssertEquals("Users have cancelled the process", ContinueWithSave.No, form.ShowPreSaveDialogs());
				savingOption.SaveWithEntryChanges = true;
				AssertEquals("Users have chosen to save without sending", ContinueWithSave.Yes, form.ShowPreSaveDialogs());
			}
		}

		public void TestContinueWithSaveWhenUserCancelsOnCPDecForm()
		{
			var mockDeclaration = GetAmendableDeclarationMoq(true);

			var declaration = mockDeclaration.Object;
			declaration.JE_HouseBill = "1233 changed";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var mockController = new Mock<MessagingActionsController>(declaration) { CallBase = true };

			var collection = new CusEntryHeaderMessageStatusFilteredCollection(declaration.ActiveEntryHeaders);

			mockController
				.Protected()
				.Setup<CusEntryHeaderMessageStatusFilteredCollection>("GetFilteredEntryHeaders", declaration, new CusEntryHeader[] { declaration.CustomsEntryHeaders[0] })
				.Returns(collection);
			mockController
				.Protected()
				.Setup<bool>("ShowCPQAForm", collection)
				.Returns(false); //user cancels

			var controller = mockController.Object;
			AssertEquals(ContinueWithSave.No, controller.DetermineRequiredMessagesAndSendThem(new IMDMultiMessageManager(declaration, CMRMessageTypes.OriginalForAmendmentDetection)));
		}

		public void TestContinueWithSaveWhenUserCancelsOnAmendmentReasonForm()
		{
			Mock<JobDeclaration> mockDeclaration = GetAmendableDeclarationMoq(true);

			var declaration = mockDeclaration.Object;
			var reason = new CMRAmendmentWithdrawalReason();

			mockDeclaration
				.Protected()
				.Setup<Customs.Business.AmendmentWithdrawalReason>("GetAmendmentWithdrawalReasonCore")
				.Returns(reason);

			var manager = new IMDMultiMessageManager(declaration, CMRMessageTypes.OriginalForAmendmentDetection);

			mockDeclaration
				.Protected()
				.Setup<Customs.Business.IMessageManager>("GetMessageManagerForAmendmentDetectionCore")
				.Returns(manager);

			declaration.JE_HouseBill = "1233 changed";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var mockController = new Mock<MessagingActionsController>(declaration) { CallBase = true };

			var collection = new CusEntryHeaderMessageStatusFilteredCollection(declaration.ActiveEntryHeaders);

			mockController
				.Protected()
				.Setup<CusEntryHeaderMessageStatusFilteredCollection>("GetFilteredEntryHeaders", declaration, new CusEntryHeader[] { declaration.CustomsEntryHeaders[0] })
				.Returns(collection);
			mockController
				.Protected()
				.Setup<bool>("ShowCPQAForm", collection)
				.Returns(true);
			mockController.Setup(m => m.GetAmendmentWithdrawalReason(reason))
				.Returns(ContinueWithSave.No);

			var checker = mockController.Object;
			AssertEquals(ContinueWithSave.No, checker.DetermineRequiredMessagesAndSendThem(manager));
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			mockController.Setup(m => m.GetAmendmentWithdrawalReason(reason))
				.Returns(ContinueWithSave.Yes);

			AssertEquals(ContinueWithSave.Yes, checker.DetermineRequiredMessagesAndSendThem(manager));
		}

		protected override Type GetTypeOfDeclarationToMock() => typeof(JobDeclaration);

		public Mock<JobDeclaration> GetUnMergedDeclarationMoq()
		{
			var lastYear = ZDateTime.Now.Year - 1;
			OrgHeader supplier = OrgHeader.LoadFromCode(Factory, "ABIGAS");
			OrgHeader importer = OrgHeader.LoadFromCode(Factory, "ABABEU");

			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_IsShippingLine = true;
			shippingLine.OH_IsShippingProvider = true;

			importer.PrimaryRegistrationNumber.Number = "90093519530";
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientID, "955411R");

			var result = Factory.NewMoq<JobDeclaration>();
			JobDeclaration testDec = result.Object;
			testDec.JE_OH_Importer = importer.PK;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_TransportMode = "SEA";
			testDec.JE_VesselName = "ADMIRALENGRACHT";
			testDec.JE_VoyageFlightNo = "92";
			testDec.JE_OH_ShippingLine = shippingLine.PK;
			testDec.JE_ContainerCount = (short)1;
			testDec.JE_ContainerMode = "FCL";
			testDec.JE_DateAtFinalDestination = new ZDateTime(lastYear, 1, 17);
			testDec.JE_DateAtOrigin = new ZDateTime(lastYear, 1, 3);
			testDec.JE_DateOfArrival = new ZDateTime(lastYear, 1, 17);
			testDec.JE_DateOfFirstArrival = new ZDateTime(lastYear, 1, 17);
			testDec.JE_DeclarationReference = "B00001001";
			testDec.JE_ExportDate = new ZDateTime(lastYear, 1, 3);
			testDec.JE_ExportGoodsType = "OT";
			testDec.JE_GB = GlbBranch.CurrentBranch.PK;
			testDec.JE_GoodsDescription = "STATIONERY";
			testDec.JE_HouseBill = "HBL9387439";
			testDec.JE_MasterBill = "OBL93874394";
			testDec.JE_MergeBy = "NON";
			testDec.JE_MessageSubType = "FRM";
			testDec.JE_OwnerRef = "PO038349";
			testDec.JE_PaymentMethod = "BRK";
			testDec.JE_RL_NKFinalDestination = "AUSYD";
			testDec.JE_RL_NKOrigin = "HKHKG";
			testDec.JE_RL_NKPortOfArrival = "AUSYD";
			testDec.JE_RL_NKPortOfFirstArrival = "AUSYD";
			testDec.JE_RL_NKPortOfLoading = "HKHKG";
			testDec.JE_ShipmentIncoTerm = "FOB";
			testDec.JE_TotalNoOfPacks = 5;
			testDec.JE_TotalNoOfPacksPackType = "PKG";
			testDec.JE_TotalVolumeUnit = "M3";
			testDec.JE_TotalWeight = 10.000m;
			testDec.JE_TotalWeightUnit = "KG";

			testDec.JobComInvoiceGroupHeaders[0].JZ_InvoiceNumber = "All Invoices";
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000m, testDec.LocalCurrencyCode);
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 20m, testDec.LocalCurrencyCode);

			CusContainer testContainer1 = testDec.CusContainers.AddNew();
			testContainer1.CO_ContainerNumber = "MOLU9387391";
			testContainer1.CO_FCL_LCL_AIR = "FCL";
			testContainer1.CO_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;

			JobComInvoiceHeader testHeader1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader1.JZ_IncoTerm = "FOB";
			testHeader1.JZ_InvoiceAmount = 12500.0000m;
			testHeader1.JZ_InvoiceCurrExRate = 1.000000000m;
			testHeader1.JZ_InvoiceCurrLandedCostExRate = 1.000000000m;
			testHeader1.JZ_InvoiceDate = new ZDateTime(lastYear, 1, 13);
			testHeader1.JZ_InvoiceNumber = "1";
			testHeader1.JZ_PaymentExRate = 1.000000000m;
			testHeader1.JZ_RX_NKInvoice_Currency = "AUD";
			testHeader1.JZ_Weight = 10.000m;
			testHeader1.JZ_WeightUQ = "KG";
			testHeader1.JZ_OH_Buyer = supplier.PK;

			JobComInvoiceLine testLine1 = testHeader1.JobComInvoiceLines.AddNew();
			testLine1.JI_LineNo = (short)1;
			testLine1.JI_LinePrice = 2500.0000m;
			testLine1.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine2 = testHeader1.JobComInvoiceLines.AddNew();
			testLine2.JI_LineNo = (short)2;
			testLine2.JI_LinePrice = 10000.0000m;
			testLine2.JI_WeightUQ = "KG";

			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			testDec.MessageInitiator = TestSender;
			testDec.JE_ApplicationCode = "CMR";
			testDec.JE_MergeBy = "NON";
			testDec.JE_MessageSubType = "FRM";
			testLine1.JI_AddInfo = "RNO=001";
			testLine1.JI_CustomsQuantity = 10000.0000m;
			testLine1.JI_CustomsUnitQty = "NO";
			testLine1.JI_Description = "DISPOSABLE, LIQUID INK";
			testLine1.JI_Tariff = "9608.10.00 47";
			testLine2.JI_AddInfo = "RNO=001";
			testLine2.JI_CustomsQuantity = 200000.0000m;
			testLine2.JI_CustomsUnitQty = "NO";
			testLine2.JI_Description = "IN THE FORM OF BOOKLETS";
			testLine2.JI_Tariff = "4813.10.00 15";
			SetLodgementQuestions();
			return result;
		}

		protected override IMessageManageableBizObj GetBizObjWithoutAnyMessagesToSend() => GetAmendableDeclaration(true);

		protected override IMessageManageableBizObj GetBizObjWithMessagesToSendButWithErrorsFromMessageManager()
		{
			JobDeclaration result = GetAmendableDeclaration(false);
			result.JE_HouseBill = "123"; //changed
			return result;
		}

		protected override IMessageManageableBizObj GetBizObjWithMessagesToSendButMessageErrors()
		{
			var mockDeclaration = GetAmendableDeclarationMoq(true);

			JobDeclaration result = mockDeclaration.Object;
			result.JE_OwnerRef = "";
			AssertEquals("Empty owner ref causes message errors", true, result.JE_OwnerRefInfo.HasMessageErrors());
			result.JE_HouseBill = "123"; //changed
			return result;
		}

		protected override IMessageManageableBizObj GetBizObjWithMessagesToSendButWithWarningsFromMessageManager()
		{
			var mockDeclaration = GetAmendableDeclarationMoq(true);

			var result = mockDeclaration.Object;
			result.CustomsEntryHeaders[0].AddInfo.ZA_IsPAYRECAck_Hidden = true; //paid
			result.CustomsEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.TotalDutyTaxForLine, 10000m);
			JobComInvoiceHeader invoice = result.Invoices[0];
			invoice.JZ_InvoiceAmount = 10000m; //less amount
			invoice.JobComInvoiceLines[0].JI_LinePrice = 2000m;
			invoice.JobComInvoiceLines[1].JI_LinePrice = 8000m;
			return result;
		}

		protected override Customs.Business.IBackDoorSavingSupportableBizObj GetBizObjWithMessagesToSendButWithCancelledSavingOptions()
		{
			var mockDeclaration = GetAmendableDeclarationMoq(true);

			var result = mockDeclaration.Object;
			result.JE_HouseBill = "123"; //changed
			return result;
		}

		protected override Customs.Business.IBackDoorSavingSupportableBizObj GetBizObjWithMessagesToSendButWithSaveWithoutSendingOptions()
		{
			var mockDeclaration = GetAmendableDeclarationMoq(true);

			var result = mockDeclaration.Object;
			result.JE_HouseBill = "123"; //changed
			return result;
		}

		protected override IShowPreSaveDialog GetFormOrPlugInToWhichAmendmentDetectionIsHookedUp(IMessageManageableBizObj bizObj)
		{
			var declaration = (JobDeclaration)bizObj;
			Customs.Business.IBackDoorSavingSupportableBizObj complexBizObj = (Customs.Business.IBackDoorSavingSupportableBizObj)bizObj;

			var form = new ZAUCustomsDeclarationFormForTesting_AmendmentDetectionOnSavingForDeclaration(declaration);
			var mockController = new Mock<MessagingActionsController>(declaration) { CallBase = true };
			form.GetNewMessagingActionsControllerReturns = mockController.Object;
			var collection = new CusEntryHeaderMessageStatusFilteredCollection(declaration.ActiveEntryHeaders);
			mockController
				.Protected()
				.Setup<CusEntryHeaderMessageStatusFilteredCollection>("GetFilteredEntryHeaders", declaration, new CusEntryHeader[] { declaration.CustomsEntryHeaders[0] })
				.Returns(collection);
			mockController
				.Protected()
				.Setup<bool>("ShowCPQAForm", collection)
				.Returns(true);
			mockController.Setup(m => m.GetAmendmentWithdrawalReason(complexBizObj.GetAmendmentWithdrawalReason()))
				.Returns(ContinueWithSave.Yes);
			return form;
		}

		JobComInvoiceLine SetUpEntriesForAmendmentTesting(JobDeclaration testDec)
		{
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_EntryStatus = CMRImportEntryAdvice.Held.Code;
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			CusEntryHeader originalEntry = testDec.CustomsEntryHeaders.AddNew();
			originalEntry.CH_Status = CustomsEntryStatus.NotSent.Code;
			AssertEquals("IsStatusPostLodge", false, originalEntry.IsStatusPostLodge);
			CusEntryHeader amendEntryWithoutChange = testDec.CustomsEntryHeaders.AddNew();
			amendEntryWithoutChange.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			AssertEquals("IsStatusPostLodge", true, amendEntryWithoutChange.IsStatusPostLodge);
			CusEntryHeader amendEntryWithChange = testDec.CustomsEntryHeaders.AddNew();
			amendEntryWithChange.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("IsStatusPostLodge", true, amendEntryWithChange.IsStatusPostLodge);
			CusEntryLine entryLine1 = originalEntry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = (ZShort)1;
			CusEntryLine entryLine2 = amendEntryWithoutChange.MergedLines.AddNew();
			entryLine2.CL_LineNumber = (ZShort)1;
			CusEntryLine entryLine3 = amendEntryWithChange.MergedLines.AddNew();
			entryLine3.CL_LineNumber = (ZShort)1;
			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.AddInfo.ZA_EFD = "010101";
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine1.PK;
			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.AddInfo.ZA_EFD = "020202";
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine2.PK;
			JobComInvoiceHeader invoice3 = testDec.Invoices.AddNew();
			invoice3.AddInfo.ZA_EFD = "030303";
			JobComInvoiceLine line3 = invoice3.JobComInvoiceLines.AddNew();
			line3.JI_CL = entryLine3.PK;
			testDec.Bills.AddNew();
			SetUpCertificatesAndBrokersLicence();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);
			Factory.Save();
			return line3;
		}

		JobDeclaration GetAmendableDeclaration(bool shouldSetUpCetificate)
		{
			var mockDeclaration = GetAmendableDeclarationMoq(shouldSetUpCetificate);
			return mockDeclaration.Object;
		}

		Mock<JobDeclaration> GetAmendableDeclarationMoq(bool shouldSetUpCetificate)
		{
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "AAA";
			var result = GetUnMergedDeclarationMoq();
			result.Setup(m => m.JE_MessageStatus).Returns(new ZString(CustomsEntryStatus.ClearFormalLodge.Code));   //recalculated after merge is done
																													//DeferredAmendmentSavingOptions savingOptions = new DeferredAmendmentSavingOptions();
																													//savingOptions.SendAmendment = true;
																													//result.ExpectAndReturnAlways("GetDeferredAmendmentSavingOptionsCore", savingOptions);
			var reason = new CMRAmendmentWithdrawalReason();
			result.Protected().Setup<Customs.Business.AmendmentWithdrawalReason>("GetAmendmentWithdrawalReasonCore")
				.Returns(reason);
			var declaration = result.Object;
			declaration.DoMerge();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			entry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			if (shouldSetUpCetificate)
			{
				SetUpCertificatesAndBrokersLicence();
			}

			Factory.Save();
			return result;
		}

		void SetUpCertificatesAndBrokersLicence()
		{
			GenRegCertAccredMaintList brokerLicence = GlbStaff.CurrentUser.Certificates.AddNew();
			brokerLicence.XZ_RefNumber = "54321";
			brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "AAA";
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "AAA";
			Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
		}

		void SetLodgementQuestions()
		{
			var q1 = CMRLodgementQuestion.New(Factory);
			q1.CQ_LodgementQuestionIdentifier = 1;
			q1.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			q1.CQ_LodgementQuestionType = "GLQ";
			var profile = CMRCommunityProtectionProfile.New(Factory);
			profile.CP_CommunityProtectionRiskIdentifier = 800;
			profile.CP_TariffClassificationNumberfield = "48131000";
			var risk = CMRCommunityProtectionRisk.New(Factory);
			risk.CK_Identifier = 800;
			risk.CK_StartDate = new ZDateTime(2005, 1, 1);
			risk.CK_LodgementQuestionIdentifier = 900;
			var q2 = CMRLodgementQuestion.New(Factory);
			q2.CQ_LodgementQuestionIdentifier = 900;
			q2.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			q2.CQ_LodgementQuestionType = "CPQ";
		}

		SendsMessagesToCustomsShutterUpperer TestSender
		{
			get
			{
				if (fTestSender == null)
				{
					fTestSender = new SendsMessagesToCustomsShutterUpperer(true);
					fTestSender.AnswerToContinueWithAction = true;
				}

				return fTestSender;
			}
		}

		SendsMessagesToCustomsShutterUpperer fTestSender;
	}

	sealed class ZAUCustomsDeclarationFormForTesting_AmendmentDetectionOnSavingForDeclaration : ZAUCustomsDeclarationForm
	{
		public ZAUCustomsDeclarationFormForTesting_AmendmentDetectionOnSavingForDeclaration(JobDeclaration declaration) : base(declaration)
		{
		}

		public Customs.GUI.SendsMessagesToCustomsGUI GetNewMessagingActionsControllerReturns { get; set; }
		protected override Customs.GUI.SendsMessagesToCustomsGUI GetNewMessagingActionsController()
		{
			return GetNewMessagingActionsControllerReturns;
		}
	}
}
