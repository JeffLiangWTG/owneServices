using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DeclarationDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestTILVAddInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_ICN = "1";
			invoiceLine.AddInfo.ZA_TILV = "100.00AUD";

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var shipment = writer.GetDataObject(declaration);
			var addInfos = shipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].AddInfoCollection;
			AssertNotNull(addInfos.FirstOrDefault(x => x.Key.HasValue && x.Value.HasValue && x.Key.Value == UniversalExtensions.TILV4Warehouse && x.Value.Value == "100.00"));
		}

		public void TestPopulateUNDG()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MessageSubType = "FRM";
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;

			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container.CO_ContainerNumber = "AAA";
			container.CO_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			container.CO_Weight = 150m;
			container.CO_WeightUQ = "KG";

			var masterBill1 = declaration.Bills.AddNew();
			masterBill1.CU_MasterBill = "Master Bill 1";

			var houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_HouseBill = "House Bill 1";
			houseBill1.CU_MasterBill = "Master Bill 1";

			var pack = declaration.Packages[0];
			pack.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			pack.CW_PackQty = 150;

			var owner = Factory.New<OrgHeader>();
			owner.OH_IsConsignee = true;
			owner.OH_Code = "OWNER";
			var contact1 = owner.Contacts.AddNew();
			contact1.OC_ContactName = "KNZ";
			contact1.OC_Email = "Test@qq.com";
			contact1.OC_Phone = "478457484";
			contact1.OC_Fax = "789456";

			var undg1 = pack.UNDGs.TryGetOrCreate("3208A", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			undg1.DI_DGFlashPoint = 10m;
			undg1.DI_OC_DGContact = contact1.PK;
			undg1.DI_TechnicalName = "TECH METALLIC SUBSTANCE";
			undg1.DI_MPMarinePollutant = Customs.Business.YesNoList.Codes.Yes;
			undg1.DI_DGWeight = 485m;
			undg1.DI_UnitOfWeight = "KG";
			undg1.DI_DGVolume = 141m;
			undg1.DI_UnitOfVolume = "M3";

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var shipment = writer.GetDataObject(declaration);
			AssertEquals(1, shipment.PackingLineCollection.Count);

			var packInfo = shipment.PackingLineCollection[0];
			AssertEquals(1, packInfo.UNDGCollection.Count);

			var undgInfo = packInfo.UNDGCollection[0];
			AssertEquals("3208a", undgInfo.UNDGCode.GetValueOrDefault());
			AssertEquals(undg1.DI_IMOClass, undgInfo.IMOClass.GetValueOrDefault());
			AssertEquals("10", undgInfo.FlashPoint.GetValueOrDefault());
			AssertEquals("Y", undgInfo.MarinePollutant.Code);
			AssertEquals("TECH METALLIC SUBSTANCE", undgInfo.TechicalName.GetValueOrDefault());
			AssertEquals(485m, undgInfo.Weight.GetValueOrDefault());
			AssertEquals("KG", undgInfo.WeightUQ.Code);
			AssertEquals(141m, undgInfo.Volume.GetValueOrDefault());
			AssertEquals("M3", undgInfo.VolumeUQ.Code);

			var contactInfo = undgInfo.Contact;
			AssertNotNull(contactInfo);
			AssertEquals("KNZ", contactInfo.FullName);
			AssertEquals("478457484", contactInfo.Phone);
		}

		public void TestPackingTypeExportedFromParent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MessageSubType = "FRM";
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			declaration.JE_TotalNoOfPacks = 50;
			declaration.JE_TotalNoOfPacksPackType = Core.Constants.PkgUnit.Pallet;

			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container.CO_ContainerNumber = "AAA";
			container.CO_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			container.CO_Weight = 150m;
			container.CO_WeightUQ = "KG";

			var masterBill1 = declaration.Bills.AddNew();
			masterBill1.CU_MasterBill = "Master Bill 1";

			var houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_HouseBill = "House Bill 1";
			houseBill1.CU_MasterBill = "Master Bill 1";
			houseBill1.CU_PackType = Core.Constants.PkgUnit.Pallet;

			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_HouseBill = "House Bill 2";
			houseBill2.CU_MasterBill = "Master Bill 1";
			houseBill2.CU_PackType = Core.Constants.PkgUnit.Box;

			var pack1_HB1 = declaration.Packages[0];
			pack1_HB1.CW_PackQty = 20;

			var packHB2 = declaration.Packages[1];
			packHB2.CW_PackQty = 30;
			packHB2.CW_PackType = Core.Constants.PkgUnit.Box;

			var pack2_HB1 = declaration.Packages.AddNew();
			pack2_HB1.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			pack2_HB1.CW_PackQty = 20;

			var pack3_HB1 = declaration.Packages.AddNew();
			pack3_HB1.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			pack3_HB1.CW_PackQty = 10;

			var owner = Factory.New<OrgHeader>();
			owner.OH_IsConsignee = true;
			owner.OH_Code = "OWNER";
			var contact1 = owner.Contacts.AddNew();
			contact1.OC_ContactName = "KNZ";
			contact1.OC_Email = "Test@qq.com";
			contact1.OC_Phone = "478457484";
			contact1.OC_Fax = "789456";

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var shipment = writer.GetDataObject(declaration);
			AssertEquals(4, shipment.PackingLineCollection.Count);

			var packingLine1 = shipment.PackingLineCollection[0];
			var packingLine2 = shipment.PackingLineCollection[1];
			var packingLine3 = shipment.PackingLineCollection[2];
			var packingLine4 = shipment.PackingLineCollection[3];

			AssertEquals("BillNumber", "House Bill 1", packingLine1.BillNumber);
			AssertEquals("PackQty should have been written", 20, packingLine1.PackQty.Value);
			AssertEquals("PackType.Code should have been written", "PLT", packingLine1.PackType.Code);

			AssertEquals("BillNumber", "House Bill 2", packingLine2.BillNumber);
			AssertEquals("PackQty should have been written", 30, packingLine2.PackQty.Value);
			AssertEquals("PackType.Code should have been written from value on Parent bill (HB2)", "BOX", packingLine2.PackType.Code);

			AssertEquals("BillNumber", "House Bill 1", packingLine3.BillNumber);
			AssertEquals("PackQty should have been written", 20, packingLine3.PackQty.Value);
			AssertEquals("PackType.Code should have been written from value on Parent bill (HB1)", "PLT", packingLine3.PackType.Code);

			AssertEquals("BillNumber", "House Bill 1", packingLine4.BillNumber);
			AssertEquals("PackQty should have been written", 10, packingLine4.PackQty.Value);
			AssertEquals("PackType.Code should have been written from value on Parent bill (HB1)", "PLT", packingLine4.PackType.Code);
		}

		public void TestPackingTypeIsDefaultedWhenParentUQNotEntered()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MessageSubType = "FRM";
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			declaration.JE_TotalNoOfPacks = 50;
			declaration.JE_TotalNoOfPacksPackType = Core.Constants.PkgUnit.Pallet;

			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container.CO_ContainerNumber = "AAA";
			container.CO_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			container.CO_Weight = 150m;
			container.CO_WeightUQ = "KG";

			var masterBill1 = declaration.Bills.AddNew();
			masterBill1.CU_MasterBill = "Master Bill 1";

			var houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_HouseBill = "House Bill 1";
			houseBill1.CU_MasterBill = "Master Bill 1";

			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_HouseBill = "House Bill 2";
			houseBill2.CU_MasterBill = "Master Bill 1";

			var pack1_HB1 = declaration.Packages[0];
			pack1_HB1.CW_PackQty = 20;

			var packHB2 = declaration.Packages[1];
			packHB2.CW_PackQty = 30;

			var pack2_HB1 = declaration.Packages.AddNew();
			pack2_HB1.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			pack2_HB1.CW_PackQty = 20;

			var pack3_HB1 = declaration.Packages.AddNew();
			pack3_HB1.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			pack3_HB1.CW_PackQty = 10;

			var owner = Factory.New<OrgHeader>();
			owner.OH_IsConsignee = true;
			owner.OH_Code = "OWNER";
			var contact1 = owner.Contacts.AddNew();
			contact1.OC_ContactName = "KNZ";
			contact1.OC_Email = "Test@qq.com";
			contact1.OC_Phone = "478457484";
			contact1.OC_Fax = "789456";

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var shipment = writer.GetDataObject(declaration);
			AssertEquals(4, shipment.PackingLineCollection.Count);

			var packingLine1 = shipment.PackingLineCollection[0];
			var packingLine2 = shipment.PackingLineCollection[1];
			var packingLine3 = shipment.PackingLineCollection[2];
			var packingLine4 = shipment.PackingLineCollection[3];

			AssertEquals("BillNumber", "House Bill 1", packingLine1.BillNumber);
			AssertEquals("PackQty should have been written", 20, packingLine1.PackQty.Value);
			AssertEquals("PackType.Code 1 defaults from Declaration pack type", "PLT", packingLine1.PackType.Code);

			AssertEquals("BillNumber", "House Bill 2", packingLine2.BillNumber);
			AssertEquals("PackQty should have been written", 30, packingLine2.PackQty.Value);
			AssertEquals("PackType.Code 2 should also have been defaulted as it was empty", "PLT", packingLine2.PackType.Code);

			AssertEquals("BillNumber", "House Bill 1", packingLine3.BillNumber);
			AssertEquals("PackQty should have been written", 20, packingLine3.PackQty.Value);
			AssertEquals("PackType.Code 3 should also have been defaulted as it was empty", "PLT", packingLine3.PackType.Code);

			AssertEquals("BillNumber", "House Bill 1", packingLine4.BillNumber);
			AssertEquals("PackQty should have been written", 10, packingLine4.PackQty.Value);
			AssertEquals("PackType.Code 4 should also have defaulted as it was empty", "PLT", packingLine4.PackType.Code);
		}

		public void TestOrganizationAddressCollectionHasARPATDWhenMessageTypeIsAQS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			declaration.Invoices.AddNew();
			var quarantineInvoice = declaration.QuarantineInvoice;
			var aQISResponsiblePerson = quarantineInvoice.AQISResponsiblePerson;
			var aQISTransitDestination = quarantineInvoice.AQISTransitDestination;
			aQISResponsiblePerson.E2_AddressOverride = true;
			aQISResponsiblePerson.E2_Address1 = "TestAddress1";
			aQISResponsiblePerson.E2_Contact = "TestContact";

			aQISTransitDestination.E2_AddressOverride = true;
			aQISTransitDestination.E2_Address1 = "TestAddress21";
			aQISTransitDestination.E2_CompanyName = "TestCompanyName";

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var shipment = writer.GetDataObject(declaration);
			AssertEquals(2, shipment.OrganizationAddressCollection.Count);

			var organizationAddressLine1 = shipment.OrganizationAddressCollection[0];
			var organizationAddressLine2 = shipment.OrganizationAddressCollection[1];

			AssertEquals("TestAddress1", organizationAddressLine1.Address1);
			AssertEquals("TestContact", organizationAddressLine1.Contact);

			AssertEquals("TestAddress21", organizationAddressLine2.Address1);
			AssertEquals("TestCompanyName", organizationAddressLine2.CompanyName);
		}

		public void TestOrganizationAddressCollection_AQISEUPlaceOfDestination()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			declaration.Invoices.AddNew();
			var quarantineInvoice = declaration.QuarantineInvoice;
			var aQISEUPlaceOfDestination = quarantineInvoice.AQISEUPlaceOfDestination;
			aQISEUPlaceOfDestination.E2_AddressOverride = true;
			aQISEUPlaceOfDestination.E2_Address1 = "TestAddress21";
			aQISEUPlaceOfDestination.E2_CompanyName = "TestCompanyName";
			aQISEUPlaceOfDestination.ApprovalNumber = "1234";

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var shipment = writer.GetDataObject(declaration);

			var aQISEUPlaceOfDestinationAddress = shipment.OrganizationAddressCollection[0];
			var approvalNumberOnShipment = aQISEUPlaceOfDestinationAddress.RegistrationNumberCollection[0];

			AssertEquals("TestAddress21", aQISEUPlaceOfDestinationAddress.Address1);
			AssertEquals("TestCompanyName", aQISEUPlaceOfDestinationAddress.CompanyName);
			AssertEquals("1234", approvalNumberOnShipment.Value);
		}

		protected override void SetUp()
		{
			setupCreator = ((IExternalFetchHintSupporter)Factory).SetupCreator();
			base.SetUp();
		}
		IDisposable setupCreator;

		protected override void TearDown()
		{
			base.TearDown();
			if (setupCreator != null)
			{
				setupCreator.Dispose();
				setupCreator = null;
			}
		}
	}
}
