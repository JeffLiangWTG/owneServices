using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(JobDeclaration))]
	sealed class JobDeclarationExtendingBaseBehaviourTest : Customs.Business.Testing.BaseJobDeclarationTest<JobDeclaration>
	{
		public new void TestDeleteAnyNewMessages()
		{
			var testDec = Factory.New<CanNotBeSavedDeclaration>();
			testDec.CanNotBeSaved = false;
			var message1 = Factory.New<EDIMessage>();
			testDec.Messages.Add(message1);
			message1.EM_ReceiveTransmit = "RCV";
			Factory.Save();
			var message2 = Factory.New<EDIMessage>();
			testDec.Messages.Add(message2);
			message2.EM_ReceiveTransmit = "TRX";
			var message3 = Factory.New<EDIMessage>();
			testDec.Messages.Add(message3);
			message3.EM_ReceiveTransmit = "RCV";
			AssertEquals(3, testDec.Messages.Count);
			Assert(!message1.IsDeleted);
			Assert(!message2.IsDeleted);
			Assert(!message3.IsDeleted);
			testDec.JE_TransportMode = "ABC";
			testDec.CanNotBeSaved = true;
			AssertExceptionThrown<Exception>(() =>
			{
				Factory.Save();
			});
			AssertEquals(2, testDec.Messages.Count);
			Assert("Only outgoing message will be deleted", !message1.IsDeleted);
			Assert("Only outgoing message will be deleted", message2.IsDeleted);
			Assert("Only outgoing message will be deleted", !message3.IsDeleted);
		}

		public override void TestAreMultipleEntryInstructionsAllowed()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(false, dec.AreMultipleEntryInstructionsAllowed);
		}

		public void TestWarehouseAddressOnInvoiceLineIsClearedForNoneExWarehouseJobs()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			var declarationBefore = Factory.New<JobDeclaration>();
			declarationBefore.JE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-2);
			declarationBefore.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			var invoiceLineBefore = declarationBefore.InvoiceLines.AddNew();
			invoiceLineBefore.AddInfo.ZA_OA_WarehouseAddress_Hidden = orgProxy.MainAddress.PK;
			declarationBefore.JE_MessageType = JobMessageTypeList.Codes.Import;
			var declarationAfter = Factory.New<JobDeclaration>();
			declarationAfter.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			var invoiceLineAfter = declarationAfter.InvoiceLines.AddNew();
			invoiceLineAfter.AddInfo.ZA_OA_WarehouseAddress_Hidden = orgProxy.MainAddress.PK;
			declarationAfter.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();
			AssertEquals(orgProxy.MainAddress.PK, invoiceLineBefore.AddInfo.ZA_OA_WarehouseAddress_Hidden);
			AssertEquals(ZGuid.Empty, invoiceLineAfter.AddInfo.ZA_OA_WarehouseAddress_Hidden);
			declarationBefore.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declarationAfter.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			invoiceLineAfter.AddInfo.ZA_OA_WarehouseAddress_Hidden = orgProxy.MainAddress.PK;
			Factory.Save();
			AssertEquals(orgProxy.MainAddress.PK, invoiceLineBefore.AddInfo.ZA_OA_WarehouseAddress_Hidden);
			AssertEquals(orgProxy.MainAddress.PK, invoiceLineAfter.AddInfo.ZA_OA_WarehouseAddress_Hidden);
			declarationBefore.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationAfter.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLineAfter.AddInfo.ZA_OA_WarehouseAddress_Hidden = orgProxy.MainAddress.PK;
			Factory.Save();
			AssertEquals(orgProxy.MainAddress.PK, invoiceLineBefore.AddInfo.ZA_OA_WarehouseAddress_Hidden);
			AssertEquals(ZGuid.Empty, invoiceLineAfter.AddInfo.ZA_OA_WarehouseAddress_Hidden);
		}

		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.AUJobDeclaration);
			}
		}

		#endregion

		public void TestReciprocalRates()
		{
			Assert(!Factory.New<JobDeclaration>().IsReciprocalRates);
		}

		public override void TestLocalCurrencyCoreOverride()
		{
			AssertEquals(Enterprise.Core.Constants.CurrencyCodes.Australia, GetJobDeclarationForTesting().LocalCurrencyCodeCoreExposed);
		}

		public void TestIsConsignmentReferenceActive()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Not for export job", false, declaration.IsConsignmentReferenceActive);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Not for sea job", false, declaration.IsConsignmentReferenceActive);

			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Not when registry is inactive", false, declaration.IsConsignmentReferenceActive);

			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Active for Import / Air / When registry is set", true, declaration.IsConsignmentReferenceActive);
		}

		public void TestCanCancel()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = ZString.Empty;
			Assert(string.IsNullOrEmpty(declaration.CanCancel()));

			declaration.JE_EntryStatus = CustomsEntryStatus.NotSent.Code;
			Assert(string.IsNullOrEmpty(declaration.CanCancel()));

			declaration.JE_EntryStatus = CustomsEntryStatus.ClearWithdrawal.Code;
			Assert(string.IsNullOrEmpty(declaration.CanCancel()));

			declaration.JE_EntryStatus = CustomsEntryStatus.FailOriginal.Code;
			Assert(string.IsNullOrEmpty(declaration.CanCancel()));

			declaration.JE_EntryStatus = CustomsEntryStatus.AwaitingOriginal.Code;
			AssertEquals(BaseJobDeclaration.cancellationMessage, declaration.CanCancel());

			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			AssertEquals(BaseJobDeclaration.cancellationMessage, declaration.CanCancel());

			declaration.JE_EntryStatus = CustomsEntryStatus.ClearReplacement.Code;
			AssertEquals(BaseJobDeclaration.cancellationMessage, declaration.CanCancel());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = ZString.Empty;
			declaration.JE_MessageStatus = ZString.Empty;
			Assert(string.IsNullOrEmpty(declaration.CanCancel()));

			declaration.JE_EntryStatus = CustomsEntryStatus.NotSent.Code;
			Assert(string.IsNullOrEmpty(declaration.CanCancel()));

			declaration.JE_EntryStatus = CMRImportEntryAdvice.Withdrawn.Code;
			Assert(string.IsNullOrEmpty(declaration.CanCancel()));

			declaration.JE_MessageStatus = CustomsEntryStatus.AwaitingOriginal.Code;
			AssertEquals(BaseJobDeclaration.cancellationMessage, declaration.CanCancel());

			declaration.JE_EntryStatus = ZString.Empty;
			declaration.JE_MessageStatus = CustomsEntryStatus.ClearOriginal.Code;
			Assert(string.IsNullOrEmpty(declaration.CanCancel()));

			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			declaration.JE_MessageStatus = CustomsEntryStatus.ClearOriginal.Code;
			AssertEquals(BaseJobDeclaration.cancellationMessage, declaration.CanCancel());

			declaration.JE_EntryStatus = CustomsEntryStatus.Lodged.Code;
			declaration.JE_MessageStatus = ZString.Empty;
			AssertEquals(BaseJobDeclaration.cancellationMessage, declaration.CanCancel());

			declaration.JE_EntryStatus = CustomsEntryStatus.ClearPayment.Code;
			AssertEquals(BaseJobDeclaration.cancellationMessage, declaration.CanCancel());
		}

		public void TestVesselNumber()
		{
			var testVessel1 = RefVessel.New(Factory);
			testVessel1.RV_Code = "TEST VESSEL";
			testVessel1.RV_LloydsNumber = "777777";
			var testVessel2 = RefVessel.New(Factory);
			testVessel2.RV_Code = "TEST VESSEL WITHOUT NO";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals("No vessel yet", ZString.Empty, declaration.VesselNumber);
			declaration.JE_VesselName = "XXX";
			AssertEquals("Invalid vessel", ZString.Empty, declaration.VesselNumber);
			declaration.JE_VesselName = "TEST VESSEL WITHOUT NO";
			AssertEquals("No lloyds no on vessel", ZString.Empty, declaration.VesselNumber);
			declaration.JE_VesselName = "TEST VESSEL";
			AssertEquals("LLoyds num", "777777", declaration.VesselNumber);
			declaration.ZA_CustShipNo_Hidden = "X123456";
			AssertEquals("Customs Ship Number", "X123456", declaration.VesselNumber);
			declaration.JE_VesselName = ZString.Empty;
			AssertEquals("Still Customs Ship Number", "X123456", declaration.VesselNumber);
			declaration.ZA_CustShipNo_Hidden = ZString.Empty;
			AssertEquals("Back to no number", ZString.Empty, declaration.VesselNumber);
		}

		public void TestVesselReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Assert("Vessel is not readonly", !declaration.JE_VesselNameInfo.ReadOnly);
			declaration.ZA_CustShipNoOverride_Hidden = true;
			Assert("Vessel is readonly", declaration.JE_VesselNameInfo.ReadOnly);
		}

		public void TestJE_ContainerModeDoesntTouchJobDocAddressesTooEarlyButStillMarksForValidationIfInitialised()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			AssertEquals(false, declaration1.IsImporterDeliveryAddressInitialised);
			AssertEquals(false, declaration1.IsSupplierPickupAddressInitialised);
			declaration1.JE_ContainerMode = "XYZ";
			AssertEquals(true, declaration1.IsImporterDeliveryAddressInitialised);
			AssertEquals(true, declaration1.IsSupplierPickupAddressInitialised);
			Factory.Save();
			declaration1.JE_ContainerMode = "ABC";
			AssertEquals(true, declaration1.IsImporterDeliveryAddressInitialised);
			var throwAway = declaration1.SupplierPickupAddress;
			AssertEquals(true, declaration1.IsSupplierPickupAddressInitialised);

			var declaration2 = Factory.New<JobDeclaration>();
			var wasMarkedForValidation = false;
			Factory.MarkedAsNeedingValidation += new BusinessObjectFactory.MarkedAsNeedingValidationEventHandler((BusinessObject obj) =>
			{ wasMarkedForValidation = obj is JobDocAddress; });
			declaration2.JE_ContainerMode = "ABC";
			Assert(wasMarkedForValidation);
			var throwAwayAgain = declaration2.ImporterDeliveryAddress;
			declaration2.JE_ContainerMode = "XYZ";
			Assert(wasMarkedForValidation);
		}

		public void TestJE_ContainerCount_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("Container Count is readonly", declaration.JE_ContainerCountInfo.ReadOnly);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert("Container Count is Not readonly", !declaration.JE_ContainerCountInfo.ReadOnly);
		}

		public void TestUNDGsAreDefaultedFromProduct()
		{
			var owner = Factory.New<OrgHeader>();
			owner.OH_IsConsignee = true;
			owner.OH_Code = "OWNER";
			var contact1 = owner.Contacts.AddNew();
			contact1.OC_ContactName = "BOB";
			var contact2 = owner.Contacts.AddNew();
			contact2.OC_ContactName = "JOE";

			var part = Factory.New<Customs.Business.OrgSupplierPart>();
			part.OP_PartNum = "PARTZ234";
			part.OP_Desc = "Description";
			OrgPartRelation relation = part.RelatedOrganisations.AddOrganisationIfNotExist(owner.PK, OrgPartRelation.RelationshipTypes.Both);
			var undg1 = part.UNDGs.TryGetOrCreate("3208A", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			undg1.DI_DGFlashPoint = 10m;
			undg1.DI_OC_DGContact = contact1.PK;
			undg1.DI_TechnicalName = "TECH METALLIC SUBSTANCE";
			undg1.DI_MPMarinePollutant = YesNoList.Codes.Yes;
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = owner.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(0, invoiceLine.UNDGs.Count);
			invoiceLine.JI_PartNo = "PARTZ234";
			AssertEquals(0, invoiceLine.UNDGs.Count);

			invoiceLine.JI_PartNo = "";
			AssertEquals(0, invoiceLine.UNDGs.Count);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_PartNo = "PARTZ234";
			AssertNoWarning(invoiceLine.JI_PartNoInfo, JobComInvoiceLineValidation.WarningPartHasMultipleUNDGRecords);
			AssertEquals(1, invoiceLine.UNDGs.Count);
			var undg = invoiceLine.UNDGs[0];
			AssertEquals("3208a", undg.UNDGSubstance.DG_Code);
			AssertEquals(10m, undg.DI_DGFlashPoint);
			AssertEquals(contact1.PK, undg.DI_OC_DGContact);
			AssertEquals("TECH METALLIC SUBSTANCE", undg.DI_TechnicalName);
			AssertEquals(YesNoList.Codes.Yes, undg.DI_MPMarinePollutant);

			invoiceLine.JI_PartNo = "";
			invoiceLine.UNDGs.DeleteAll();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			invoiceLine.JI_PartNo = "PARTZ234";
			AssertNoWarning(invoiceLine.JI_PartNoInfo, JobComInvoiceLineValidation.WarningPartHasMultipleUNDGRecords);
			AssertEquals(1, invoiceLine.UNDGs.Count);
			undg = invoiceLine.UNDGs[0];
			AssertEquals("3208a", undg.UNDGSubstance.DG_Code);
			AssertEquals(10m, undg.DI_DGFlashPoint);
			AssertEquals(contact1.PK, undg.DI_OC_DGContact);
			AssertEquals("TECH METALLIC SUBSTANCE", undg.DI_TechnicalName);
			AssertEquals(YesNoList.Codes.Yes, undg.DI_MPMarinePollutant);

			invoiceLine.JI_PartNo = "";
			invoiceLine.UNDGs.DeleteAll();
			var undg2 = part.UNDGs.TryGetOrCreate("2015A", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			undg2.DI_DGFlashPoint = -14m;
			undg2.DI_OC_DGContact = contact2.PK;
			Factory.Save();
			invoiceLine.JI_PartNo = "PARTZ234";
			AssertEquals(0, invoiceLine.UNDGs.Count);
			AssertHasWarning(invoiceLine.JI_PartNoInfo, JobComInvoiceLineValidation.WarningPartHasMultipleUNDGRecords);

			invoiceLine.JI_PartNo = "";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_PartNo = "PARTZ234";
			AssertEquals(0, invoiceLine.UNDGs.Count);
			AssertHasWarning(invoiceLine.JI_PartNoInfo, JobComInvoiceLineValidation.WarningPartHasMultipleUNDGRecords);
		}

		public override void TestHouseBillLabel()
		{
			var declaration = Factory.New<JobDeclaration>();
			var dataBoundBO = new DataBoundBusinessObject(declaration);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_HouseBillInfo, dataBoundBO);
			AssertEquals("PP House Bill Label", "Parcel Post Number", resourceStringData.Caption);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_HouseBillInfo, dataBoundBO);
			AssertEquals("Default Label", "House Bill", resourceStringData.Caption);
		}

		public void TestIsImportAndIsPost()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;

			CombineAssertions(() =>
			{
				AssertEquals("IsImport and IsPost", true, declaration.IsImportAndIsPost);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("IsImport and Is Not Post", false, declaration.IsImportAndIsPost);

				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
				AssertEquals("Is Not Import and Is Post", false, declaration.IsImportAndIsPost);
			});
		}

		public override void TestLogCustomsCommencedIfNeeded()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			SetCurrentUserAsBroker("ZXZ", true);
			DoTestLogCustomsCommencedIfNeeded(Events.CustomsCommenced, declaration);
			AssertEquals("TestDec.JE_GS_NKCusAgent", "ZXZ", declaration.JE_GS_NKCusAgent);
		}

		public void TestLogCustomsCommencedIfNeeded_WhenCurrentUserIsNotABroker()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			SetCurrentUserAsBroker("ZXZ", false);
			declaration.LogCustomsClearedIfNeeded();
			AssertEquals("TestDec.JE_GS_NKCusAgent", ZString.Empty, declaration.JE_GS_NKCusAgent);
		}

		public override void TestLogCustomsCommencedIfNeeded_ForExport()
		{
			SetCurrentUserAsBroker("ZXZ", false);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			Assert("Precondition-No certificate", GlbStaff.CurrentUser.Certificates.GetFirstCertificateNumber(CertificateTypePairList.Codes.BR1).IsEmpty);
			DoTestLogCustomsCommencedIfNeeded(Events.ExportCustomsCommenced, declaration);
			AssertEquals("TestDec.JE_GS_NKCusAgent", "ZXZ", declaration.JE_GS_NKCusAgent);
		}

		public void TestCurrentUserIsABrokerOverride()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			AssertEquals("Broker should always be set with broker", "", declaration.JE_GS_NKCusAgent);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			Assert("Precondition-CurrentUser.GS_Code is not blank", !GlbStaff.CurrentUser.GS_Code.IsEmpty);
			Assert("Precondition-No certificate", GlbStaff.CurrentUser.Certificates.GetFirstCertificateNumber(CertificateTypePairList.Codes.BR1).IsEmpty);

			SetCurrentUserAsBroker(GlbStaff.CurrentUser.GS_Code, true);
			Assert("Now is a certificate", !GlbStaff.CurrentUser.Certificates.GetFirstCertificateNumber(CertificateTypePairList.Codes.BR1).IsEmpty);
			declaration.LogCustomsCommencedIfNeeded();
			AssertEquals("Broker should now be set", GlbStaff.CurrentUser.GS_Code, declaration.JE_GS_NKCusAgent);
		}

		public override void TestCloneHouseBills()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			Bill masterBill1 = declaration.Bills.AddNew();
			masterBill1.CU_MasterBill = "Master Bill 1";

			Bill masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill2.CU_MasterBill = "Master Bill 2";

			Bill houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_HouseBill = "House Bill 1";
			houseBill1.CU_MasterBill = "Master Bill 1";

			Bill houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_MasterBill = "Master Bill 2";
			houseBill2.CU_HouseBill = "House Bill 2";

			Package pack1 = declaration.Packages.AddNew();
			pack1.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			pack1.CW_PackQty = 150;

			Package pack2 = declaration.Packages.AddNew();
			pack2.CW_HouseBill = houseBill2.CU_BillUniqueCode;
			pack2.CW_PackQty = 250;

			JobDeclaration clonedDeclaration = (JobDeclaration)declaration.Clone();
			AssertEquals("Has no house bills", 0, clonedDeclaration.Bills.Count);
			AssertEquals("Has no Packing records", 0, clonedDeclaration.PackingGroups.Count);

			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });
		}

		public void TestAdjustingIncoTermsWhenMessageTypeChanges()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.PackedAtFactory;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("Terms should not have changed", Core.Constants.IncoTerms.CostInsuranceAndFreight, testDec.JE_ShipmentIncoTerm);
			AssertEquals("Terms should not have changed", Core.Constants.IncoTerms.PackedAtFactory, invoice.JZ_IncoTerm);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("Terms should now be FOB", Core.Constants.IncoTerms.FreeOnBoard, testDec.JE_ShipmentIncoTerm);
			AssertEquals("Terms should now be FOB", Core.Constants.IncoTerms.FreeOnBoard, invoice.JZ_IncoTerm);
		}

		protected override void AfterInitialise(BaseJobDeclaration declaration)
		{
			foreach (JobComInvoiceHeader inv in declaration.Invoices)
			{
				QuarantineExDocHeader x = inv.QuarantineExDocHeader;
			}
			foreach (JobComInvoiceLine invLine in declaration.InvoiceLines)
			{
				QuarantineExDocLine y = invLine.QuarantineExDocLine;
			}
		}

		public void TestAQISProcessingEstablishmentAddressRequirement()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertCollectionNotContains(DocAddressType.AQISProcessingEstablishment, ((IDocAddresses)declaration).SupportedAddressTypes);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			AssertCollectionContains(DocAddressType.AQISProcessingEstablishment, ((IDocAddresses)declaration).SupportedAddressTypes);

			var docAddresses = (IDocAddresses)declaration;
			var requirement = docAddresses.GetDocAddressRequirement(DocAddressType.AQISProcessingEstablishment);
			AssertEquals("Unlimited addresses", 0, requirement.DefaultMax);
		}

		public override void TestSetDefaultPackagesType()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("default value for JE_TotalNoOfPacksPackType", "PKG", declaration.JE_TotalNoOfPacksPackType);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("JE_TotalNoOfPacksPackType should be clear for DRW", "", declaration.JE_TotalNoOfPacksPackType);
		}

		public override void TestGetContainerModeForDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(Core.Constants.ContainerModes.FCLMixedShipper, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.BuyersConsol));
			AssertEquals(Core.Constants.ContainerModes.FCL, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL));
			AssertEquals(Core.Constants.ContainerModes.LCL, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.LCL));
			AssertEquals(Core.Constants.ContainerModes.LCL, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.ShippersConsol));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.ShippersConsol));
		}

		#region Test ICusEntryNumFilterProvider

		protected override void CancelEntryHeader(Customs.Business.CusEntryHeader entryHeader)
		{
			entryHeader.CH_Status = CustomsEntryStatus.ClearWithdrawal.Code;
		}

		#endregion

		#region Implementation

		JobDeclarationForTesting GetJobDeclarationForTesting()
		{
			var dec = Factory.New<JobDeclarationForTesting>();
			dec.DisableDefaultPackingInformation = true;
			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return dec;
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return testDec;
		}

		protected override void PrepareDeclarationForMerge(BaseJobDeclaration declaration)
		{
			base.PrepareDeclarationForMerge(declaration);
			((JobDeclaration)declaration).InitialiseDeclarationForMergeForTest();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return JobDeclaration.New(factory);
		}

		void SetCurrentUserAsBroker(string code, bool isBroker)
		{
			var user = GlbStaff.CurrentUser;
			user.GS_Code = code;
			user.GS_IsSystemAccount = false;
			if (isBroker)
			{
				var brokerLicence = user.Certificates.AddNew();
				brokerLicence.XZ_RefNumber = "54321";
				brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;
			}
		}

		#endregion

		#region CanNotBeSavedDeclaration

		class CanNotBeSavedDeclaration : JobDeclaration
		{
			public CanNotBeSavedDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override void OnSaving()
			{
				base.OnSaving();
				if (CanNotBeSaved)
				{
					throw new Exception("Do not save.");
				}
			}

			public bool CanNotBeSaved { get; set; }
		}

		#endregion
	}
}
