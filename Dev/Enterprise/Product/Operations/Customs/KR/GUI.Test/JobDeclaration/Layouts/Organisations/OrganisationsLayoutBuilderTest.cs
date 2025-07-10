using Enterprise.Customs.Common.KR;
using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(OrganisationsLayoutBuilder))]
	sealed class OrganisationsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<OrganisationsLayoutBuilder, JobDeclaration, CommonOrganisationsControlBag>
	{
		public void TestControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			var layout = ((IPanelLayoutProvider)new OrganisationsLayout()).Layout;
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
				AssertEquals(true, layout.IsVisible(OrganisationsControlBag.Instance.SupplierAddressControl, declaration));
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.ImporterAddressControl, declaration));
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.PayerGuidFindBox, declaration));
				AssertEquals(true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ManufacturerAddressControl, declaration));
				AssertEquals(true, layout.IsVisible(OrganisationsControlBag.Instance.IndustrialParkCodeCodeFindBox, declaration));
				AssertEquals(true, layout.IsVisible(OrganisationsControlBag.Instance.ExpoterAddressControl, declaration));
				AssertEquals(true, layout.IsVisible(OrganisationsControlBag.Instance.FinalBondedWarehouseCodeFindBox, declaration));
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.ManufacturerGuidFindBox, declaration));
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.ExporterGuidFindBox, declaration));
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.StevedoreAddressControl, declaration));
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.AuthorGroupBox, declaration));
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.ResponsiblePersonGroupBox, declaration));

				declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.SupplierAddressControl, declaration));
				AssertEquals(true, layout.IsVisible(OrganisationsControlBag.Instance.ImporterAddressControl, declaration));
				AssertEquals(true, layout.IsVisible(OrganisationsControlBag.Instance.PayerGuidFindBox, declaration));
				AssertEquals(false, layout.IsVisible(CommonOrganisationsControlBag.Instance.ManufacturerAddressControl, declaration));
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.IndustrialParkCodeCodeFindBox, declaration));
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.ExpoterAddressControl, declaration));
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.FinalBondedWarehouseCodeFindBox, declaration));
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.ManufacturerGuidFindBox, declaration));
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.ExporterGuidFindBox, declaration));
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.StevedoreAddressControl, declaration));
				AssertEquals(true, layout.IsVisible(OrganisationsControlBag.Instance.AuthorGroupBox, declaration));
				AssertEquals(true, layout.IsVisible(OrganisationsControlBag.Instance.ResponsiblePersonGroupBox, declaration));

				declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.SupplierAddressControl, declaration));
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.ImporterAddressControl, declaration));
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.PayerGuidFindBox, declaration));
				AssertEquals(false, layout.IsVisible(CommonOrganisationsControlBag.Instance.ManufacturerAddressControl, declaration));
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.IndustrialParkCodeCodeFindBox, declaration));
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.ExpoterAddressControl, declaration));
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.FinalBondedWarehouseCodeFindBox, declaration));
				AssertEquals(true, layout.IsVisible(OrganisationsControlBag.Instance.ManufacturerGuidFindBox, declaration));
				AssertEquals(true, layout.IsVisible(OrganisationsControlBag.Instance.ExporterGuidFindBox, declaration));
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.StevedoreAddressControl, declaration));
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.AuthorGroupBox, declaration));
				AssertEquals(false, layout.IsVisible(OrganisationsControlBag.Instance.ResponsiblePersonGroupBox, declaration));
				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
				AssertEquals(true, layout.IsVisible(OrganisationsControlBag.Instance.StevedoreAddressControl, declaration));
			});
		}

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override int ExpectedMaxColumns => 1;

		protected override OrganisationsLayoutBuilder GetColumnLayoutBuilderForTesting()
		{
			var builder = new OrganisationsLayoutBuilder();
			builder.AddControlBag(OrganisationsControlBag.Instance);
			return builder;
		}
	}
}
