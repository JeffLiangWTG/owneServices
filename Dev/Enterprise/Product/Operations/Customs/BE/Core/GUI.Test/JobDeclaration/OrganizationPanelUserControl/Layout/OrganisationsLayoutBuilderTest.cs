using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(OrganisationsLayoutBuilder))]
sealed class OrganisationsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<OrganisationsLayoutBuilder, EU.Business.Declaration.JobDeclaration, CommonOrganisationsControlBag>
{
	public void TestExporterDocAddressControl_Visibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		var layout = new OrganisationsLayout().Layout;
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("Export", true, layout.IsVisible(EU.GUI.OrganisationsControlBag.Instance.ExporterDocAddressControl, declaration));
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("Import", false, layout.IsVisible(EU.GUI.OrganisationsControlBag.Instance.ExporterDocAddressControl, declaration));
		});
	}

	public void TestContractualPartnerDocAddressControl_Visibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		var layout = new OrganisationsLayout().Layout;
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("Export", true, layout.IsVisible(EU.GUI.OrganisationsControlBag.Instance.ContractualPartnerDocAddressControl, declaration));
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("Import", false, layout.IsVisible(EU.GUI.OrganisationsControlBag.Instance.ContractualPartnerDocAddressControl, declaration));
		});
	}

	public void TestCarrierEUBorderDocAddressControl_Visibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		var layout = new OrganisationsLayout().Layout;
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("Export", true, layout.IsVisible(EU.GUI.OrganisationsControlBag.Instance.CarrierEUBorderDocAddressControl, declaration));
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("Import", false, layout.IsVisible(EU.GUI.OrganisationsControlBag.Instance.CarrierEUBorderDocAddressControl, declaration));
		});
	}

	public void TestConsigneeAddressControl_Visibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		var layout = new OrganisationsLayout().Layout;
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("Export", false, layout.IsVisible(CommonOrganisationsControlBag.Instance.ConsigneeAddressControl, declaration));
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("Import", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ConsigneeAddressControl, declaration));
		});
	}

	public void TestSellerAddressControl_Caption()
	{
		var declaration = Factory.New<JobDeclaration>();
		var layout = new OrganisationsLayout().Layout;
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("Export Caption", "[2] Subcontractor", layout.GetCaption(CommonOrganisationsControlBag.Instance.SellerAddressControl, declaration));
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("Import Caption", "[UCC 3/24] Seller", layout.GetCaption(CommonOrganisationsControlBag.Instance.SellerAddressControl, declaration));
		});
	}

	public void TestCaption()
	{
		var declaration = Factory.New<JobDeclaration>();
		var layout = new OrganisationsLayout().Layout;

		CombineAssertions(() =>
		{
			layout.TryGetCaption(CommonOrganisationsControlBag.Instance.RepresentativeAddressControl, declaration, out var resourceStringData);
			AssertEquals("Representative caption", "[UCC 3/19] Representative", resourceStringData.Caption);

			layout.TryGetCaption(EU.GUI.OrganisationsControlBag.Instance.CarrierEUBorderDocAddressControl, declaration, out resourceStringData);
			AssertEquals("Carrier EU Border caption", "[UCC 3/31] Carrier EU border", resourceStringData.Caption);

			layout.TryGetCaption(EU.GUI.OrganisationsControlBag.Instance.ExporterDocAddressControl, declaration, out resourceStringData);
			AssertEquals("Exporter caption", "[UCC 3/1] Exporter", resourceStringData.Caption);

			layout.TryGetCaption(CommonOrganisationsControlBag.Instance.DeclarantOfficeAddressControl, declaration, out resourceStringData);
			AssertEquals("Declarant Office caption", "[UCC 3/17] Declarant", resourceStringData.Caption);
		});
	}

	protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

	protected override int ExpectedMaxColumns => 1;

	protected override OrganisationsLayoutBuilder GetColumnLayoutBuilderForTesting()
	{
		var builder = new OrganisationsLayoutBuilder();
		builder.AddControlBag(EU.GUI.OrganisationsControlBag.Instance);
		return builder;
	}
}
