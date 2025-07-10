using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(ShipmentTypeLayoutBuilder))]
sealed class ShipmentTypeLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ShipmentTypeLayoutBuilder, JobDeclaration, Customs.GUI.ShipmentTypeControlBag>
{
	protected override ShipmentTypeLayoutBuilder GetColumnLayoutBuilderForTesting() => new ShipmentTypeLayoutBuilder();

	protected override int ExpectedMaxColumns => 1;

	public void TestControlsVisibility()
	{
		CombineAssertions(() =>
		{
			var controlReference = ShipmentTypeControlBag.Instance.BorderTransportMeansDropEdit;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertControlVisibility(controlReference, true, "EXP");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertControlVisibility(controlReference, false, "IMP");
		});
	}

	void AssertControlVisibility(ControlReference controlReference, bool isVisible, string reference)
	{
		AssertEquals($"{controlReference.Name} Visible, for " + reference, isVisible, Layout.IsVisible(controlReference, declaration));
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;

	public PanelLayout Layout => layout ?? (layout = new ShipmentTypeLayout().Layout);
	PanelLayout layout;
}
