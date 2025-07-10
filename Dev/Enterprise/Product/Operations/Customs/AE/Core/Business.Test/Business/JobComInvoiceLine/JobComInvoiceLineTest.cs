using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(JobComInvoiceLine))]
sealed class JobComInvoiceLineTest : Customs.Business.Testing.BaseJobComInvoiceLineAbstractTest
{
	public void TestJI_NewUsed()
	{
		var info = DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_NewUsedInfo);
		AssertEquals("Goods Condition", info.Caption);
	}

	public void TestCustomsCountryCode()
	{
		AssertEquals("CustomsCountryCodeCore should be AE", Core.Constants.CountryCodes.UnitedArabEmirates, InvoiceLine.CustomsCountryCode);
	}

	public void TestIInvoiceLinePartDetailsMembers() => CombineAssertions(() =>
	{
		IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
		AssertEquals(Core.Constants.CountryCodes.UnitedArabEmirates, partDetails.CustomsCountryCode);
		AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
	});

	public void TestPartType() => CombineAssertions(() =>
	{
		var factory2 = new BusinessObjectFactory();
		var importer = factory2.New<OrgHeader>();
		importer.FillWithValidTestData();
		var product = (MasterFiles.Business.OrgSupplierPart)factory2.New<Integration.Customs.AU.IOrgSupplierPart>();
		product.OP_PartNum = "TestTEST";
		product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
		factory2.Save();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_OH_Importer = importer.PK;
		declaration.JE_GB = GlbCompany.CurrentCompany.FirstActiveBranch.PK;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_PartNo = "TestTEST";
		AssertType<OrgSupplierPart>("Product type gets changed depending on who is requesting", invoiceLine.Part);
		Factory.Save();
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
		{
			var factory3 = new BusinessObjectFactory();
			var declarationLoaded = factory3.Load<JobDeclaration>(declaration.PK);
			AssertType<OrgSupplierPart>("product type still the right type", declarationLoaded.InvoiceLines[0].Part);
		}
	});

	public void TestTypeDeciderGetsConcreteType()
	{
		AssertEquals("Update BaseJobComInvoiceLineTypeDecider to include a decider for this class", GetExpectedBusinessObjectType(), Factory.New(typeof(BaseJobComInvoiceLine)).GetType());
	}

	public void TestIsGoingIntoBondedWarehouse()
	{
		AssertEquals("InvoiceLine.IsGoingIntoBondedWarehouse", false, InvoiceLine.IsGoingIntoBondedWarehouse);
	}

	public void TestSetDefaults() => CombineAssertions(() =>
	{
		AssertEquals("InvoiceUQ should default to box.", InvoiceUQList.Codes.Boxes, InvoiceLine.JI_InvoiceUQ);
		AssertEquals("Goods Condition should default to New.", AEGoodsConditionList.Codes.New, InvoiceLine.JI_NewUsed);
	});

	public void TestLookups()
	{
		AssertType<JobComInvoiceLineLookups>("InvoiceLine.Lookups.GetType()", InvoiceLine.Lookups);
	}

	public new void TestEffectiveCountryOfOrigin() => CombineAssertions(() =>
	{
		InvoiceLine.JI_CountryOfOrigin = "AU";
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("InvoiceLine.EffectiveCountryOfOrigin", "AU", InvoiceLine.EffectiveCountryOfOrigin);
		InvoiceHeader.JZ_RN_NKDefaultOrigin = "NZ";
		AssertEquals("InvoiceLine.EffectiveCountryOfOrigin", "AU", InvoiceLine.EffectiveCountryOfOrigin);
		InvoiceLine.JI_CountryOfOrigin = "";
		AssertEquals("InvoiceLine.EffectiveCountryOfOrigin", "NZ", InvoiceLine.EffectiveCountryOfOrigin);
		InvoiceLine.JI_CountryOfOrigin = "AU";
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("InvoiceLine.EffectiveCountryOfOrigin", "AU", InvoiceLine.EffectiveCountryOfOrigin);
		InvoiceHeader.JZ_RN_NKDefaultOrigin = "NZ";
		AssertEquals("InvoiceLine.EffectiveCountryOfOrigin", "AU", InvoiceLine.EffectiveCountryOfOrigin);
		InvoiceLine.JI_CountryOfOrigin = "";
		AssertEquals("InvoiceLine.EffectiveCountryOfOrigin", "NZ", InvoiceLine.EffectiveCountryOfOrigin);
	});

	public void TestGetTariffDescription() => CombineAssertions(() =>
	{
		InvoiceLine.JI_Tariff = "";
		AssertEquals("InvoiceLine.TariffDescription", ZString.Empty, InvoiceLine.TariffDescription);
		InvoiceLine.JI_Tariff = "0103.91.00";
		AssertEquals("InvoiceLine.TariffDescription", ZString.Empty, InvoiceLine.TariffDescription);
	});

	public override void TestDefaultDataGroupingCode()
	{
		AssertEquals("Data grouping", AEConstants.DefaultDataGroupingForTariffs, InvoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));
	}

	public void TestVehiclesType()
	{
		AssertType(typeof(CusVehicleCollection<CusVehicle, JobComInvoiceLine>), InvoiceLine.Vehicles);
	}

	public void TestVehicleRelationship()
	{
		AssertEquals(VehicleRelationshipType.Many, InvoiceLine.VehicleRelationship);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		return invoiceLine;
	}

	protected override bool RatesAreReciprocal => true;

	protected override ZString TariffDataGrouping => Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO;

	new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;
}
