using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test;

[TestedType(typeof(HeaderPartiesUserControl))]
sealed class HeaderPartiesUserControlTest : TestCaseWithFactory
{
	public void TestIAdditionalTabPage() => CombineAssertions(() =>
	{
		using var control = new HeaderPartiesUserControl();
		var additionalTabPage = (IAdditionalTabPage)control;

		AssertEquals("TabPageCaption", "Header Parties", additionalTabPage.AdditionalTabPageCaption.Caption);
		AssertEquals("Tab Page Sequence", 1, additionalTabPage.TabPageSequence);
		AssertEquals("Visibility", false, additionalTabPage.AdditionalControlVisibility.isVisible(null));
		AssertEquals("Control Reference", control, additionalTabPage.AdditionalTabPageUserControl);
	});

	public void TestVisibility()
	{
		var manifest = Factory.New<AsycudaManifestHeader>();

		using (var control = new HeaderPartiesUserControl() as IAdditionalTabPage)
		{
			CombineAssertions(() =>
			{
				manifest.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
				manifest.AMA_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;
				manifest.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F11;
				AssertEquals("Is visible on Carrier Manifest with SEA and F11", true, control.AdditionalControlVisibility.isVisible(manifest));

				manifest.AMA_TransportMode = Customs.Business.TransportTypeList.Codes.Road;
				manifest.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F11;
				AssertEquals("Is not visible on Carrier Manifest with Road and F11", false, control.AdditionalControlVisibility.isVisible(manifest));

				manifest.AMA_TransportMode = Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport;
				manifest.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F12;
				AssertEquals("Is visible on Carrier Manifest with IWT and F12", true, control.AdditionalControlVisibility.isVisible(manifest));

				manifest.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F10;
				AssertEquals("Is not visible on Carrier Manifest with IWT and F10", false, control.AdditionalControlVisibility.isVisible(manifest));

				manifest.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.Consolidator;
				manifest.AMA_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;
				manifest.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F11;
				AssertEquals("Is not visible on NVC (Forwarder) Manifest", false, control.AdditionalControlVisibility.isVisible(manifest));
			});
		}
	}

	public void TestControls()
	{
		using var userControl = new HeaderPartiesUserControl();
		_ = userControl.AssertContainsControl<DynamicLayoutPanel>(nameof(userControl.DynamicHeaderPartiesPanel),
				x => x.WithBindTo(nameof(AsycudaManifestHeader.MasterBill)));
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new HeaderPartiesUserControl();
	}
	HeaderPartiesUserControl control;

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
}
