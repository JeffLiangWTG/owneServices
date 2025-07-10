using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

sealed class JobDeclarationValueSetStrategyTest : TestCaseWithFactory
{
	public void TestUpdateAddressesDependingOnDeclarantType_Self()
	{
		declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
		CombineAssertions(() =>
		{
			AssertEquals("Declarant Address equals to login broker", declaration.Branch.OrgProxy?.MainAddress?.PK, declaration.JE_OA_DeclarantAddress);
			AssertEquals("Exporter Address equals to login broker", declaration.Branch?.OrgProxy?.PK, declaration.ExporterDocAddress.OrganisationPK);
			AssertEquals("Representative Address is empty", ZGuid.Empty, declaration.JE_OA_Representative);
		});
	}

	public void TestUpdateAddressesDependingOnDeclarantType_Direct()
	{
		declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
		CombineAssertions(() =>
		{
			AssertEquals("Declarant Address equals to supplier", declaration.SupplierDocumentaryAddress.E2_OA_Address, declaration.JE_OA_DeclarantAddress);
			AssertEquals("Exporter Address equals to supplier", declaration.SupplierDocumentaryAddress.OrganisationPK, declaration.ExporterDocAddress.OrganisationPK);
			AssertEquals("Representative Address equals to login broker", declaration.Branch.OrgProxy?.MainAddress?.PK, declaration.JE_OA_Representative);
		});
	}

	public void TestUpdateAddressesDependingOnDeclarantType_Indirect()
	{
		declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
		CombineAssertions(() =>
		{
			AssertEquals("Declarant Address equals to login broker", declaration.Branch.OrgProxy?.MainAddress?.PK, declaration.JE_OA_DeclarantAddress);
			AssertEquals("Exporter Address equals to supplier", declaration.SupplierDocumentaryAddress.OrganisationPK, declaration.ExporterDocAddress.OrganisationPK);
			AssertEquals("Representative Address equals to login broker", declaration.Branch.OrgProxy?.MainAddress?.PK, declaration.JE_OA_Representative);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		supplier = Factory.NewWithValidTestData<OrgHeader>();
		declaration.JE_JS = shipment.PK;
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.JE_OH_Supplier = supplier.PK;
	}

	JobDeclaration declaration;
	OrgHeader supplier;
}
