using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(LPCOViewCollection))]
	sealed class LPCOViewCollectionTest : NonPersistentBusinessObjectCollectionTestCase<LPCOViewCollection>
	{
		public void TestCopyValueFrom()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Centilitre;
			invoiceLine.CA_GACInd = "Y";

			var gacLpcoview = declaration.LPCOViews.AddNew();
			gacLpcoview.CLP_Type = "0002";

			var gacPGAHeader = invoiceLine.GACPGAHeader;
			var lpcoview = gacPGAHeader.LPCOViews.AddNew();
			lpcoview.CLP_Type = "0003";

			var newDeclaration = Factory.New<JobDeclaration>();

			var newGacLpcoview = newDeclaration.LPCOViews.AddNew();
			newGacLpcoview.CLP_Type = "0004";

			var newInvoiceLine = newDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			newInvoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Centilitre;
			newInvoiceLine.CA_GACInd = "Y";

			var newGacPGAHeader = newInvoiceLine.GACPGAHeader;
			var newLPCO = newGacPGAHeader.LPCOViews.AddNew();
			newLPCO.CLP_Type = "0005";
			((IPGAHeader)newGacPGAHeader).CopyPersistentValuesFrom(gacPGAHeader);

			var copiedLPCOList = newGacPGAHeader.LPCOViews.OfType<LPCOView>().ToList();
			AssertEquals(2, copiedLPCOList.Count);
			AssertEquals("Only the LPCO on original invoice line level PGA will be copied.", "0003", copiedLPCOList.First(x => !x.ReadOnly).CLP_Type);
			AssertEquals("LPCO on declaration remains.", "0004", copiedLPCOList.First(x => x.ReadOnly).CLP_Type);
		}

		public void TestHolderAndApplicantShouldBeCopied()
		{
			var factory = new BusinessObjectFactory();
			var importer1 = factory.NewWithValidTestData<OrgHeader>();
			var org1 = factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "TestOrgName1";
			org1.OH_Code = "TES1";
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "TestContact1";
			contact1.OC_Email = "Test1@1.com";
			contact1.OC_Phone = "02512345678";
			var org2 = factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "TestOrgName2";
			org2.OH_Code = "TES2";
			org2.MainAddress.OA_Address1 = "Address 1";
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "TestContact2";
			contact2.OC_Email = "Test2@1.com";
			contact2.OC_Phone = "02587654321";

			var part = factory.NewWithValidTestData<OrgSupplierPart>();
			part.FillWithValidTestData();
			part.OP_PartNum = "1234";
			var part1Relation1 = part.RelatedOrganisations.AddNew();
			part1Relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			part1Relation1.OU_OH = importer1.PK;

			CusClassPartPivot pivot = factory.New<CusClassPartPivot>();
			pivot.CI_OP = part.PK;
			pivot.CI_TariffNum = "1111.11.11";
			pivot.CCA_GACIndicator = YesNoList.Codes.Yes;

			var partGAC = pivot.GACPGAHeader;
			partGAC.CA_AllProgramInd = YesNoList.Codes.Yes;
			var partLPCOView = partGAC.LPCOViews.AddNew();
			partLPCOView.CLP_Type = "8000";
			partLPCOView.CLP_RefNo = "TEST";
			partLPCOView.CLP_HolderType = LPCOHolderPartyTypeCodes.Codes.Other;
			partLPCOView.LPCOHolderOrgPK = org1.PK;
			partLPCOView.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Other;
			partLPCOView.LPCOApplicantOrgPK = org2.PK;
			partLPCOView.CLP_IsApplicantOverridden = true;
			partLPCOView.CLP_ApplicantContactName = "TestContact3";
			partLPCOView.CLP_ApplicantContactEmail = "Test3@1.com";
			partLPCOView.CLP_ApplicantContactPhone = "02587651234";
			factory.Save();

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_OH_Importer = importer1.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "1234";

			var invGAC = invoiceLine.GACPGAHeader;
			AssertEquals("LPCOViews count should be 1", 1, invGAC.LPCOViews.Count);
			var invLPCOView = invGAC.LPCOViews[0];
			var lpco = invLPCOView.LPCO;

			AssertEquals(org1.MainAddress.PK, lpco.CLP_OA_Holder);
			Assert(!lpco.CLP_IsHolderOverridden);
			AssertEquals(ZString.Empty, lpco.CLP_HolderContactName);
			AssertEquals(ZString.Empty, lpco.CLP_HolderContactEmail);
			AssertEquals(ZString.Empty, lpco.CLP_HolderContactPhone);

			AssertEquals(org2.MainAddress.PK, lpco.CLP_OA_Applicant);
			Assert(lpco.CLP_IsApplicantOverridden);
			AssertEquals("TestContact3", lpco.CLP_ApplicantContactName);
			AssertEquals("Test3@1.com", lpco.CLP_ApplicantContactEmail);
			AssertEquals("02587651234", lpco.CLP_ApplicantContactPhone);
		}

		public void TestCollectionCopiedBeforeEnumeratingInCopyValueFrom()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Centilitre;
			invoiceLine.CA_GACInd = "Y";

			var gacLpcoview = declaration.LPCOViews.AddNew();
			gacLpcoview.CLP_Type = "1101";

			var gacPGAHeader = invoiceLine.GACPGAHeader;
			var lpcoview = gacPGAHeader.LPCOViews.AddNew();
			lpcoview.CLP_Type = "2202";

			var newDeclaration = Factory.New<JobDeclaration>();

			var newGacLpcoview = newDeclaration.LPCOViews.AddNew();
			newGacLpcoview.CLP_Type = "3303";

			var newInvoiceLine = newDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			newInvoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Centilitre;
			newInvoiceLine.CA_GACInd = "Y";

			var newGacPGAHeader = newInvoiceLine.GACPGAHeader;
			var newLPCO = newGacPGAHeader.LPCOViews.AddNew();
			newLPCO.CLP_Type = "4404";
			AssertNoExceptionThrown("No Exception on Collection beforfe Enumerating", () => ((IPGAHeader)newGacPGAHeader).CopyPersistentValuesFrom(gacPGAHeader));
		}

		public void TestAddNewIfNotExist()
		{
			var pgaHeader = Factory.New<ECCCPGAHeader>();
			pgaHeader.CA_WRMProgramInd = "Y";
			pgaHeader.LPCOViews.RemoveAndDeleteAll();

			pgaHeader.LPCOViews.AddDefaultLPCOs();
			AssertContainsExactElementsInAnyOrder(new[] { "8000", "8001" }, pgaHeader.LPCOViews.Cast<LPCOView>().Select(x => x.CLP_Type));

			pgaHeader.LPCOViews.AddNewIfNotExist("8000");
			AssertContainsExactElementsInAnyOrder(new[] { "8000", "8001" }, pgaHeader.LPCOViews.Cast<LPCOView>().Select(x => x.CLP_Type));

			pgaHeader.LPCOViews.AddNewIfNotExist("8002");
			AssertContainsExactElementsInAnyOrder(new[] { "8000", "8001", "8002" }, pgaHeader.LPCOViews.Cast<LPCOView>().Select(x => x.CLP_Type));
		}

		#region Implementation

		public void TestAddDefaultLPCOs()
		{
			var pgaHeader = Factory.New<ECCCPGAHeader>();
			pgaHeader.CA_WRMProgramInd = "Y";
			pgaHeader.LPCOViews.RemoveAndDeleteAll();

			pgaHeader.LPCOViews.AddDefaultLPCOs();
			AssertContainsExactElementsInAnyOrder(new[] { "8000", "8001" }, pgaHeader.LPCOViews.Cast<LPCOView>().Select(x => x.CLP_Type));
			pgaHeader.LPCOViews.RemoveAndDeleteAll();

			var lpco1 = pgaHeader.LPCOViews.AddNew();
			lpco1.CLP_Type = "8001";
			lpco1.CLP_RefNo = "AA";

			var lpco2 = pgaHeader.LPCOViews.AddNew();
			lpco2.CLP_Type = "8002";
			lpco2.CLP_RefNo = "XXX";

			var lpco3 = pgaHeader.LPCOViews.AddNew();
			lpco3.CLP_Type = "8003";
			lpco3.CLP_RefNo = "BB";

			pgaHeader.LPCOViews.AddDefaultLPCOs();
			AssertContainsExactElementsInAnyOrder(new[] { "8000", "8001", "8003" }, pgaHeader.LPCOViews.Cast<LPCOView>().Select(x => x.CLP_Type));
		}

		public void TestLPCOViewCollectionForEachPGA()
		{
			DummyBusinessObject bo1 = Factory.New<DummyBusinessObjectForTesting>();
			DummyBusinessObject bo2 = Factory.New<DummyBusinessObjectForTesting>();
			var headerCollection = new CusCALPCOCollection(bo1);
			var lpco1 = headerCollection.AddNew();
			lpco1.CLP_Type = "0001";
			var lpco2 = headerCollection.AddNew();
			lpco2.CLP_Type = "0002";
			var lpco3 = headerCollection.AddNew();
			lpco3.CLP_Type = "0003";

			var pgaCollection = new CusCALPCOCollection(bo2);
			var collectionView = new LPCOViewCollection(pgaCollection, headerCollection, new CFIADummyBusinessObject());
			AssertEquals(1, collectionView.Count);
			AssertEquals("0001", collectionView[0].CLP_Type);

			var collectionView2 = new LPCOViewCollection(pgaCollection, headerCollection, new GACDummyBusinessObject());
			AssertEquals(2, collectionView2.Count);

			var collectionView3 = new LPCOViewCollection(pgaCollection, headerCollection, null);
			AssertEquals(0, collectionView3.Count);

			var lpco4 = pgaCollection.AddNew();
			lpco4.CLP_Type = "0004";
			var collectionView4 = new LPCOViewCollection(pgaCollection, null, null);
			AssertEquals(1, collectionView4.Count);
			AssertEquals("0004", collectionView4[0].CLP_Type);

			var collectionView5 = new LPCOViewCollection(pgaCollection, headerCollection, null);
			AssertEquals(1, collectionView5.Count);
			AssertEquals("0004", collectionView5[0].CLP_Type);

			var collectionView6 = new LPCOViewCollection(pgaCollection, headerCollection, new CFIADummyBusinessObject());
			AssertEquals(2, collectionView6.Count);
		}

		public void TestRemoveLPCOView()
		{
			DummyBusinessObject bo1 = Factory.New<DummyBusinessObjectForTesting>();
			DummyBusinessObject bo2 = Factory.New<DummyBusinessObjectForTesting>();
			var headerCollection = new CusCALPCOCollection(bo1);
			var lpco1 = headerCollection.AddNew();
			lpco1.CLP_Type = "0001";

			var pgaCollection = new CusCALPCOCollection(bo2);
			var lpco4 = pgaCollection.AddNew();
			lpco4.CLP_Type = "0001";

			var collectionView = new LPCOViewCollection(pgaCollection, headerCollection, new CFIADummyBusinessObject());
			AssertEquals(2, collectionView.Count);
			collectionView.RemoveAll();
			AssertEquals("PGA level LPCOs should be deleted when PGA level LPCOViewCollection removed", 0, pgaCollection.Count);
			AssertEquals("Header level LPCOs should not be deleted when PGA level LPCOViewCollection removed", 1, headerCollection.Count);

			var collectionView2 = new LPCOViewCollection(headerCollection, null, null);
			AssertEquals(1, collectionView2.Count);
			collectionView2.RemoveAll();
			AssertEquals(0, collectionView2.Count);
			AssertEquals("Header level LPCOs should be deleted when header level LPCOViewCollection removed", 0, headerCollection.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var attributeNamePGA = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, "PGA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);

			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "0001", "0001 DESC", startDate, endDate);
			_ = helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.CFIA);

			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "0002", "0002 DESC", startDate, endDate);
			_ = helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.GAC);

			var code3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "0003", "0003 DESC", startDate, endDate);
			_ = helper.CreateNewOrGetExistingCusCodeListAttribute(code3.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.GAC);

			var code4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "0004", "0004 DESC", startDate, endDate);
			_ = helper.CreateNewOrGetExistingCusCodeListAttribute(code4.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.GAC);

			var code5 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "0005", "0005 DESC", startDate, endDate);
			_ = helper.CreateNewOrGetExistingCusCodeListAttribute(code5.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.GAC);
			Factory.Save();
		}

		protected override LPCOViewCollection GetCollectionToTest()
		{
			var dec = GetJobDeclaration();
			dec.LPCOs.AddNew();
			return new LPCOViewCollection(dec.LPCOs, null, null);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var dec = GetJobDeclaration();
			var lpco = dec.LPCOs.AddNew();
			return new LPCOView(lpco);
		}

		JobDeclaration GetJobDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.CA_ServiceOption = "IID";
			return dec;
		}
		#endregion
	}
}
