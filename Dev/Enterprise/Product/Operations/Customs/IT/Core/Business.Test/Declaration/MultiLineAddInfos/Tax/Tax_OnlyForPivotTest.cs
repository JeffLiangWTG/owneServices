using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(Tax_OnlyForPivot))]
sealed class Tax_OnlyForPivotTest : BusinessObjectBaseTestCase
{
	public void TestLookups()
	{
		var tax = pivot.Taxes.AddNew().Data;
		AssertNotNull("Lookups should not be null", tax.Lookups);
	}

	public void TestValidation()
	{
		var tax = pivot.Taxes.AddNew().Data;
		AssertType<ITAddInfoTaxValidation>("Validation", tax.Validation);
	}

	public void TestG4_PortTaxRate()
	{
		var tax = pivot.Taxes.AddNew().Data;

		tax.G4_Type = "921";
		tax.G4_PortTaxRate = "A1";

		tax.G4_Type = "";
		AssertEquals("G4_PortTaxRate", "", tax.G4_PortTaxRate);
	}

	public void TestPortTaxRateReadOnly()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.SetupHarbourRates();

		CombineAssertions(() =>
		{
			var tax = pivot.Taxes.AddNew().Data;
			AssertEquals("When Tax type is empty, PortTaxRateReadOnly", true, tax.PortTaxRateReadOnly);

			tax.G4_Type = "XYZ";
			AssertEquals("When Tax type is not a port tax, PortTaxRateReadOnly", true, tax.PortTaxRateReadOnly);

			tax.G4_Type = "9AA";
			AssertEquals("When Tax type is a port tax, PortTaxRateReadOnly", false, tax.PortTaxRateReadOnly);
		});
	}

	public void TestIsPortTax()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.SetupHarbourRates();

		var tax = pivot.Taxes.AddNew().Data;
		CombineAssertions(() =>
		{
			tax.G4_Type = "";
			AssertEquals("When Tax type is empty, IsPortTax", false, tax.IsPortTax);

			tax.G4_Type = "XYZ";
			AssertEquals("When Tax type is not a port tax, IsPortTax", false, tax.IsPortTax);

			tax.G4_Type = "9AA";
			AssertEquals("When Tax type is a port tax, IsPortTax", true, tax.IsPortTax);
		});
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return pivot.Taxes.AddNew().Data;
	}

	protected override void SetUp()
	{
		org = Factory.NewWithValidTestData<OrgHeader>();
		var product = (OrgSupplierPart)Enterprise.Customs.EU.Business.MasterFiles.OrgSupplierPart.New(Factory);
		product.OP_PartNum = "POOPY";
		var relationship = product.RelatedOrganisations.AddNew();
		relationship.OU_Relationship = "BTH";
		relationship.OU_OH = org.PK;
		pivot = product.PivotsForBinding.AddNew();
		pivot.CI_ChildType = Common.ClassificationType.Both;
	}

	OrgHeader org;
	CusClassPartPivot pivot;
}
