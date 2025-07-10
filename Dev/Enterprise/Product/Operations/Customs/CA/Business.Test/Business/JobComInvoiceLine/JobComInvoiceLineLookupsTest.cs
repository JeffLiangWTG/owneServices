using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProperties()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals(RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory), invoiceLine.Lookups.InvoiceUQList);
			AssertEquals(typeof(AmountTypes), invoiceLine.Lookups.AmountTypes.GetType());
			AssertEquals(typeof(CalculationMethods), invoiceLine.Lookups.CalculationMethods.GetType());
			AssertEquals(false, invoiceLine.Lookups.CalculationMethods.ContainsCode(CalculationMethods.Codes.DeliveredDutyPaid));

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invHeader2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invHeader2.JobComInvoiceLines.AddNew();
			AssertEquals(true, invoiceLine2.Lookups.CalculationMethods.ContainsCode(CalculationMethods.Codes.RepairsRemission));
			AssertEquals(true, invoiceLine2.Lookups.CalculationMethods.ContainsCode(CalculationMethods.Codes.DutyDeferral));
			AssertEquals(true, invoiceLine2.Lookups.CalculationMethods.ContainsCode(CalculationMethods.Codes.WarrantyRepairsRemission));

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;

			var invHeader3 = declaration3.Invoices.AddNew();
			var invoiceLine3 = invHeader3.JobComInvoiceLines.AddNew();
			AssertEquals(false, invoiceLine3.Lookups.CalculationMethods.ContainsCode(CalculationMethods.Codes.RepairsRemission));
			AssertEquals(false, invoiceLine3.Lookups.CalculationMethods.ContainsCode(CalculationMethods.Codes.DutyDeferral));
			AssertEquals(false, invoiceLine3.Lookups.CalculationMethods.ContainsCode(CalculationMethods.Codes.WarrantyRepairsRemission));

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration4.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var invHeader4 = declaration4.Invoices.AddNew();
			var invoiceLine4 = invHeader4.JobComInvoiceLines.AddNew();
			AssertEquals(typeof(IIDUnitOfCountCodeList), invoiceLine4.Lookups.InvoiceUQList.GetType());
		}

		public void TestStateCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			declaration.JE_MessageType = "IMP";

			var invoiceLine = header.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			var stateCodes = invoiceLine.Lookups.StateCodesList;
			int i = 0;
			foreach (ICodeDescription code in new USStatesList())
			{
				AssertEquals("StateCodes code ", code.Code, stateCodes[i].Code);
				AssertEquals("StateCodes description ", code.Description, stateCodes[i++].Description);
			}

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Canada;
			stateCodes = invoiceLine.Lookups.StateCodesList;
			i = 0;
			foreach (ICodeDescription code in new CanadianProvinceList())
			{
				AssertEquals("StateCodes code ", code.Code, stateCodes[i].Code);
				AssertEquals("StateCodes description ", code.Description, stateCodes[i++].Description);
			}

			declaration.JE_MessageType = "EXP";
			invoiceLine = header.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			stateCodes = invoiceLine.Lookups.StateCodesList;
			i = 0;
			foreach (ICodeDescription code in new CanadianProvinceList())
			{
				AssertEquals("StateCodes code ", code.Code, stateCodes[i].Code);
				AssertEquals("StateCodes description ", code.Description, stateCodes[i++].Description);
			}

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Canada;
			stateCodes = invoiceLine.Lookups.StateCodesList;
			i = 0;
			foreach (ICodeDescription code in new CanadianProvinceList())
			{
				AssertEquals("StateCodes code ", code.Code, stateCodes[i].Code);
				AssertEquals("StateCodes description ", code.Description, stateCodes[i++].Description);
			}
		}

		public void TestInvoiceLine()
		{
			var parent = Factory.New<JobComInvoiceLine>();
			AssertEquals(parent.Lookups.InvoiceLine, parent);
		}

		public void TestTariffs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invHeader = declaration.Invoices.AddNew();
			var invoiceLine = invHeader.JobComInvoiceLines.AddNew();
			using (CACustomsDataRegistry.Instance.SendG7ExportMessages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var tariffs = invoiceLine.Lookups.Tariffs;
				AssertEquals(typeof(TariffViewCollection), invoiceLine.Lookups.Tariffs.GetType());
				var filterBusinessObjectDefaults = tariffs.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>();
				AssertNotNull(filterBusinessObjectDefaults.FirstOrDefault(x => IsMatch(x, "Tariff Type", "Property1", Core.Constants.CountryCodes.Canada)));
				AssertNotNull(filterBusinessObjectDefaults.FirstOrDefault(x => IsMatch(x, "Tariff Type", "Property2", Universal.Constants.TariffTypes.HarmonizedSystem)));
			}
			using (CACustomsDataRegistry.Instance.SendG7ExportMessages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var tariffs = invoiceLine.Lookups.Tariffs;
				AssertEquals(typeof(TariffViewCollection), invoiceLine.Lookups.Tariffs.GetType());
				var filterBusinessObjectDefaults = tariffs.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>();
				AssertNotNull(filterBusinessObjectDefaults.FirstOrDefault(x => IsMatch(x, "Tariff Type", "Property1", Core.Constants.CountryCodes.Canada)));
				AssertNotNull(filterBusinessObjectDefaults.FirstOrDefault(x => IsMatch(x, "Tariff Type", "Property2", Universal.Constants.TariffTypes.HarmonizedSystem)));
			}
		}

		bool IsMatch(FilterBusinessObjectDefault bizObjDefault, ZString filterName, ZString propertyName, ZString value)
		{
			return bizObjDefault.FilterName == filterName && bizObjDefault.PropertyName == propertyName && (ZString)bizObjDefault.Value == value;
		}

		public void TestClassificationList()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invHeader = declaration.Invoices.AddNew();
			var invoiceLine = invHeader.JobComInvoiceLines.AddNew();
			AssertEquals(typeof(ExportClassificationCollection), invoiceLine.Lookups.ClassificationList.GetType());
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(typeof(HTSClassificationCollection), invoiceLine.Lookups.ClassificationList.GetType());
		}

		public void TestCustomsUQListForDLM()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invHeader = declaration.Invoices.AddNew();
			var invoiceLine = invHeader.JobComInvoiceLines.AddNew();
			AssertEquals(typeof(UnitOfMeasureListForDLM), invoiceLine.Lookups.CustomsUQList.GetType());
		}

		public void TestCustomsUQListForG7Export()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invHeader = declaration.Invoices.AddNew();
			var invoiceLine = invHeader.JobComInvoiceLines.AddNew();
			AssertEquals(typeof(CustomsUnitOfMeasureList), invoiceLine.Lookups.CustomsUQList.GetType());
		}

		public void TestCustomsUQListForNonExport()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "";
			var invHeader = declaration.Invoices.AddNew();
			var invoiceLine = invHeader.JobComInvoiceLines.AddNew();
			AssertEquals(typeof(CustomsUnitOfMeasureList), invoiceLine.Lookups.CustomsUQList.GetType());
		}

		public void TestAuthorityNumberList()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var importerOfRecord = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;

			var invHeader = declaration.Invoices.AddNew();
			var invoiceLine = invHeader.JobComInvoiceLines.AddNew();

			invoiceLine.CA_AuthorityNumber = "C0001";
			AssertType<CACusRulingFindBoxCollection>(invoiceLine.Lookups.AuthorityNumberList);
			var appliesToOrgFilter = invoiceLine.Lookups.AuthorityNumberList.FilterBusinessObjectDefaults[Constants.ZZRefCusRulingFilters.AppliesToOrg + ":Property"];
			var rullingFilter = invoiceLine.Lookups.AuthorityNumberList.FilterBusinessObjectDefaults[Constants.ZZRefCusRulingFilters.RulingNumber + ":Property"];

			AssertEquals(importer.PK, appliesToOrgFilter.Value);
			AssertEquals("C0001", rullingFilter.Value);

			declaration.ImporterOfRecordAddress.OrganisationPK = importerOfRecord.PK;
			appliesToOrgFilter = invoiceLine.Lookups.AuthorityNumberList.FilterBusinessObjectDefaults[Constants.ZZRefCusRulingFilters.AppliesToOrg + ":Property"];

			AssertEquals(importerOfRecord.PK, appliesToOrgFilter.Value);
		}

		public void TestPartsListGetsEffectiveImporterAndSupplierForLVS()
		{
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_OH_Supplier = supplier1.PK;
			declaration.JE_OH_Importer = importer1.PK;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_OH_Supplier = supplier2.PK;
			invoiceHeader.JZ_OH_Buyer = importer2.PK;

			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "AAA";

			var part1 = Factory.New<OrgSupplierPart>();
			part1.FillWithValidTestData();
			part1.OP_PartNum = "AAA";
			var part1Relation1 = part1.RelatedOrganisations.AddNew();
			part1Relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			part1Relation1.OU_OH = importer1.PK;
			var part1Relation2 = part1.RelatedOrganisations.AddNew();
			part1Relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			part1Relation2.OU_OH = supplier1.PK;

			var part2 = Factory.New<OrgSupplierPart>();
			part2.FillWithValidTestData();
			part2.OP_PartNum = "AAA";
			var part2Relation1 = part2.RelatedOrganisations.AddNew();
			part2Relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			part2Relation1.OU_OH = importer2.PK;
			var part2Relation2 = part2.RelatedOrganisations.AddNew();
			part2Relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			part2Relation2.OU_OH = supplier2.PK;

			Factory.Save();

			var parts = invoiceLine.Lookups.PartsList;
			var collection = new OrgSupplierPartCollection(Factory);
			collection.Load(((IBusinessObjectCollectionTestingMembers)parts).GetAdditionalFilter());

			AssertEquals("Should NOT pick up part with declaration Importer/Supplier relationship", false, collection.Contains(part1));
			AssertEquals("Should pick up part with invoice header Importer/Supplier relationship", true, collection.Contains(part2));

			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			invoiceHeader.JZ_OH_Buyer = ZGuid.Empty;

			parts = invoiceLine.Lookups.PartsList;
			collection.Load(((IBusinessObjectCollectionTestingMembers)parts).GetAdditionalFilter());

			AssertEquals("Should pick up part with declaration Importer/Supplier relationship", true, collection.Contains(part1));
			AssertEquals("Should NOT pick up part with invoice header Importer/Supplier relationship", false, collection.Contains(part2));
		}

		public void TestVolumeUQList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			IEnumerable<string> expected = Core.Constants.Volume.Codes;
			IEnumerable<string> actual = invoiceLine.Lookups.VolumeUQList.GetAllCodes();
			AssertContainsExactElementsInAnyOrder("It should be the full list.", expected, actual);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			Assert("Precondition", declaration.IsIID);
			expected = Core.Constants.Volume.Codes.Where(x => x != Core.Constants.Volume.TeaChest);
			actual = invoiceLine.Lookups.VolumeUQList.GetAllCodes();
			AssertContainsExactElementsInAnyOrder("It should not contain TE code if the declaration is IID.", expected, actual);
		}

		public void TestParentIDList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;

			var asAccountedInvoice1 = declaration.B2AsAccountedForInvoices.AddNew();
			asAccountedInvoice1.JZ_InvoiceNumber = "1";
			var asAccountedInvoiceLine1 = asAccountedInvoice1.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedInvoiceLine1.CA_OriginalLineNo = "1";
			asAccountedInvoice1.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedInvoice1.AsAccountForFilteredInvoiceLines.AddNew();
			var asClaimedInvoiceLine1 = asAccountedInvoiceLine1.CorrespondingAsClaimedForInvoiceLine;

			AssertEquals(3, asClaimedInvoiceLine1.Lookups.ParentIDList.Count);
			Assert(asClaimedInvoiceLine1.Lookups.ParentIDList.ContainsCode("1 - 1"));
			Assert(asClaimedInvoiceLine1.Lookups.ParentIDList.ContainsCode("1 - 2"));
			Assert(asClaimedInvoiceLine1.Lookups.ParentIDList.ContainsCode("1 - 3"));

			var asClaimedInvoice = asClaimedInvoiceLine1.InvoiceHeader;
			asClaimedInvoice.JZ_InvoiceNumber = "2";
			AssertEquals(0, asClaimedInvoiceLine1.Lookups.ParentIDList.Count);
		}

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
	}
}
