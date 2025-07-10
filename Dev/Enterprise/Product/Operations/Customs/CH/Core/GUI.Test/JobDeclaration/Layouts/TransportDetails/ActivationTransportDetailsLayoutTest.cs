using System.Collections.Generic;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(ActivationTransportDetailsLayout))]
sealed class ActivationTransportDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestVisibility() => CombineAssertions(() =>
	{
		var controlBag = Customs.GUI.TransportDetailsControlBag.Instance;

		AssertVisibility(TransportModes.Rail, expectedTransportIDAndNationality: true);
		AssertVisibility(TransportModes.Road, expectedTransportIDAndNationality: true);
		AssertVisibility(TransportModes.Air, expectedMasterBillTextBox: true, expectedVoyageAndNationalityUserControl: true);
		AssertVisibility(TransportModes.FixedTransportInstallations, expectedTransportIDAndNationality: true);
		AssertVisibility(TransportModes.InlandWaterwayTransport, expectedTransportIDAndNationality: true);
		AssertVisibility(TransportModes.OwnPropulsion, expectedTransportMeansDropEdit: true, expectedTransportIDAndNationality: true);

		void AssertVisibility(string transportMode, bool expectedTransportIDAndNationality = false, bool expectedMasterBillTextBox = false, bool expectedVoyageAndNationalityUserControl = false, bool expectedTransportMeansDropEdit = false)
		{
			SendingObjectParent.SendingDeclaration.JE_TransportMode = transportMode;
			AssertEquals($"TransportMode={transportMode} TransportIDAndNationalityUserControl", expectedTransportIDAndNationality, Layout.IsVisible(controlBag.TransportIDAndNationalityUserControl, SendingObjectParent.SendingDeclaration));
			AssertEquals($"TransportMode={transportMode} MasterBillTextBox", expectedMasterBillTextBox, Layout.IsVisible(controlBag.MasterBillTextBox, SendingObjectParent.SendingDeclaration));
			AssertEquals($"TransportMode={transportMode} VoyageAndNationalityUserControl", expectedVoyageAndNationalityUserControl, Layout.IsVisible(controlBag.VoyageAndNationalityUserControl, SendingObjectParent.SendingDeclaration));
			AssertEquals($"TransportMode={transportMode} TransportMeansDropEdit", expectedTransportMeansDropEdit, Layout.IsVisible(controlBag.TransportMeansDropEdit, SendingObjectParent.SendingDeclaration));
		}
	});

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	PanelLayout Layout => layout ?? (layout = new ActivationTransportDetailsLayout().Layout);
	PanelLayout layout;

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (TransportDetailsControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportMeansDropEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, ControlWidthClass.Auto);
		}
	}

	protected override int ControlBagCount => 2;

	public ExportDeclarationMessageSendingObjectParent SendingObjectParent => sendingObjectParent ??= new ExportDeclarationMessageSendingObjectParent(Factory.New<JobDeclaration>());
	ExportDeclarationMessageSendingObjectParent sendingObjectParent;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new TransportDetailsLayoutBuilder<JobDeclaration>();
}
