using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineExDocShipsCompartment))]
	sealed class QuarantineExDocShipsCompartmentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestQuarantineExDocHeader()
		{
			var compartment = quarantineHeader.Compartments.AddNew();
			AssertNotNull("Compartment Exdocheader is not null", compartment.QuarantineExDocHeader);
			AssertEquals("Exdocheader loaded from compartment is correct exdocheader", quarantineHeader.PK, compartment.QuarantineExDocHeader.PK);
		}

		public void TestAmendPermissionMatrix()
		{
			var compartment = quarantineHeader.Compartments.AddNew();
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			QuarantineExDocHeaderTest.AssertAmendPermissionReadOnlyStatus(quarantineHeader, compartment.QC_CompartmentsInfo);
		}

		protected override BusinessObject GetNewBusinessObject() => quarantineHeader.Compartments.AddNew();

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => quarantineHeader.Compartments.AddNew();

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			quarantineHeader = helper.Header1.QuarantineExDocHeader;
		}
		QuarantineExDocHeader quarantineHeader;
	}
}
