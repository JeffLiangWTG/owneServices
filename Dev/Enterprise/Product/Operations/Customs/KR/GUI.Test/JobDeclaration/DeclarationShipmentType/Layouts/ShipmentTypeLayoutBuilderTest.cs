using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ShipmentTypeLayoutBuilder<JobDeclaration>))]
	sealed class ShipmentTypeLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ShipmentTypeLayoutBuilder<JobDeclaration>, JobDeclaration, Customs.GUI.ShipmentTypeControlBag>
	{
		protected override ShipmentTypeLayoutBuilder<JobDeclaration> GetColumnLayoutBuilderForTesting() => new ShipmentTypeLayoutBuilder<JobDeclaration>();

		protected override int ExpectedMaxColumns => 1;

		public void TestControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			var layout = new ShipmentTypeLayout().Layout;
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
				AssertEquals(true, layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.TransportModeDropEdit, declaration));

				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				AssertEquals(false, layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, declaration));

				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				AssertEquals(true, layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, declaration));

				AssertEquals(true, layout.IsVisible(ShipmentTypeControlBag.Instance.DeclarationTypeDropEdit, declaration));
				AssertEquals(true, layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.MessageSubTypeDropEdit, declaration));
				AssertEquals(true, layout.IsVisible(ShipmentTypeControlBag.Instance.TransactionTypeDropEdit, declaration));

				AssertEquals(true, layout.IsVisible(ShipmentTypeControlBag.Instance.ExporterTypeDropEdit, declaration));

				AssertEquals(false, layout.IsVisible(ShipmentTypeControlBag.Instance.TransactionDropEdit, declaration));
				AssertEquals(false, layout.IsVisible(ShipmentTypeControlBag.Instance.PaymentTypeDropEdit, declaration));
				AssertEquals(false, layout.IsVisible(ShipmentTypeControlBag.Instance.PlanTypeDropEdit, declaration));
				AssertEquals(false, layout.IsVisible(ShipmentTypeControlBag.Instance.ImporterTypeDropEdit, declaration));

				declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
				AssertEquals(true, layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.TransportModeDropEdit, declaration));

				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				AssertEquals(false, layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, declaration));

				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				AssertEquals(true, layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, declaration));

				AssertEquals(true, layout.IsVisible(ShipmentTypeControlBag.Instance.DeclarationTypeDropEdit, declaration));
				AssertEquals(true, layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.MessageSubTypeDropEdit, declaration));
				AssertEquals(false, layout.IsVisible(ShipmentTypeControlBag.Instance.TransactionTypeDropEdit, declaration));

				AssertEquals(false, layout.IsVisible(ShipmentTypeControlBag.Instance.ExporterTypeDropEdit, declaration));

				AssertEquals(true, layout.IsVisible(ShipmentTypeControlBag.Instance.TransactionDropEdit, declaration));
				AssertEquals(true, layout.IsVisible(ShipmentTypeControlBag.Instance.PaymentTypeDropEdit, declaration));
				AssertEquals(true, layout.IsVisible(ShipmentTypeControlBag.Instance.PlanTypeDropEdit, declaration));
				AssertEquals(true, layout.IsVisible(ShipmentTypeControlBag.Instance.ImporterTypeDropEdit, declaration));

				declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
				AssertEquals(false, layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.TransportModeDropEdit, declaration));

				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				AssertEquals(false, layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, declaration));

				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				AssertEquals(false, layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, declaration));

				AssertEquals(false, layout.IsVisible(ShipmentTypeControlBag.Instance.DeclarationTypeDropEdit, declaration));
				AssertEquals(true, layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.MessageSubTypeDropEdit, declaration));
				AssertEquals(true, layout.IsVisible(ShipmentTypeControlBag.Instance.TransactionTypeDropEdit, declaration));

				AssertEquals(false, layout.IsVisible(ShipmentTypeControlBag.Instance.ExporterTypeDropEdit, declaration));

				AssertEquals(false, layout.IsVisible(ShipmentTypeControlBag.Instance.TransactionDropEdit, declaration));
				AssertEquals(false, layout.IsVisible(ShipmentTypeControlBag.Instance.PaymentTypeDropEdit, declaration));
				AssertEquals(false, layout.IsVisible(ShipmentTypeControlBag.Instance.PlanTypeDropEdit, declaration));
				AssertEquals(false, layout.IsVisible(ShipmentTypeControlBag.Instance.ImporterTypeDropEdit, declaration));
			});
		}
	}
}
