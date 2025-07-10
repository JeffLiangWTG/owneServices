using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(OrganisationsLayoutBuilder))]
	class OrganisationsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<OrganisationsLayoutBuilder, JobDeclaration, CommonOrganisationsControlBag>
	{
		public void TestDefermentPartyDocAddressControlVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			var layout = ((IPanelLayoutProvider)new OrganisationsLayout()).Layout;
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("Not Import", false, layout.IsVisible(OrganisationsControlBag.Instance.DefermentPartyDocAddressControl, declaration));
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("Import", true, layout.IsVisible(OrganisationsControlBag.Instance.DefermentPartyDocAddressControl, declaration));
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
