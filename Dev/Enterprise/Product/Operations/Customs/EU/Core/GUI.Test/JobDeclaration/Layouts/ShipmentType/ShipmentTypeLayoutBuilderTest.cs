using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(ShipmentTypeLayoutBuilder<JobDeclaration>))]
	class ShipmentTypeLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ShipmentTypeLayoutBuilder<JobDeclaration>, JobDeclaration, Customs.GUI.ShipmentTypeControlBag>
	{
		protected override ShipmentTypeLayoutBuilder<JobDeclaration> GetColumnLayoutBuilderForTesting() => new ShipmentTypeLayoutBuilder<JobDeclaration>();

		protected override int ExpectedMaxColumns => 1;

		public void TestSpecificCircumstanceDropEditVisibility()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("SpecificCircumstanceDropEdit Visible, if messagetype " + Common.Shared.SharedJobMessageTypeList.Codes.Export, true, Layout.IsVisible(ShipmentTypeControlBag.Instance.SpecificCircumstanceDropEdit, declaration));

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("SpecificCircumstanceDropEdit Visible, if messagetype " + Common.Shared.SharedJobMessageTypeList.Codes.Import, false, Layout.IsVisible(ShipmentTypeControlBag.Instance.SpecificCircumstanceDropEdit, declaration));

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
				AssertEquals("SpecificCircumstanceDropEdit Visible, if messagetype " + Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms, true, Layout.IsVisible(ShipmentTypeControlBag.Instance.SpecificCircumstanceDropEdit, declaration));
			});
		}

		public void TestIsHighValueOvrdCheckBoxVisibility_Default()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("IsHighValueOvrdCheckBox Visible, if messagetype " + Common.Shared.SharedJobMessageTypeList.Codes.Import, false, Layout.IsVisible(ShipmentTypeControlBag.Instance.IsHighValueOvrdCheckBox, declaration));

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("IsHighValueOvrdCheckBox Visible, if messagetype " + Common.Shared.SharedJobMessageTypeList.Codes.Export, false, Layout.IsVisible(ShipmentTypeControlBag.Instance.IsHighValueOvrdCheckBox, declaration));

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("IsHighValueOvrdCheckBox Visible, if messagetype " + Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms, false, Layout.IsVisible(ShipmentTypeControlBag.Instance.IsHighValueOvrdCheckBox, declaration));
		}

		public void TestIsHighValueOvrdCheckBoxVisibility_Enabled()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.DV1DetailsSupport) + "Core", true))
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("IsHighValueOvrdCheckBox Visible, if messagetype " + Common.Shared.SharedJobMessageTypeList.Codes.Import, true, Layout.IsVisible(ShipmentTypeControlBag.Instance.IsHighValueOvrdCheckBox, declaration));

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("IsHighValueOvrdCheckBox Visible, if messagetype " + Common.Shared.SharedJobMessageTypeList.Codes.Export, false, Layout.IsVisible(ShipmentTypeControlBag.Instance.IsHighValueOvrdCheckBox, declaration));

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
				AssertEquals("IsHighValueOvrdCheckBox Visible, if messagetype " + Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms, false, Layout.IsVisible(ShipmentTypeControlBag.Instance.IsHighValueOvrdCheckBox, declaration));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;

		public PanelLayout Layout => layout ?? (layout = new ShipmentTypeLayouts().Layout);
		PanelLayout layout;
	}
}
