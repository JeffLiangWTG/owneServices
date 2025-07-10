using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.GUI;

static class LayoutTestHelper
{
	public static void AssertControlsVisiblilityDependOnTransportMode(this PanelLayout layout, AsycudaManifestHeader header, BusinessObject dataItem,
		bool visiblilityForSea, bool visiblilityForAir, ControlReference[] controls)
	{
		AssertControlsVisiblility(TransportTypeList.Codes.Sea, expected: visiblilityForSea);
		AssertControlsVisiblility(TransportTypeList.Codes.Air, expected: visiblilityForAir);
		AssertControlsVisiblility("XXX", expected: false);

		void AssertControlsVisiblility(string transportMode, bool expected)
		{
			Assertion.CombineAssertions($"Transport Mode - {transportMode}", () =>
			{
				header.AMA_TransportMode = transportMode;
				foreach (var control in controls)
				{
					Assertion.AssertEquals(control.ToString(), expected: expected, layout.IsVisible(control, dataItem));
				}
			});
		}
	}

	public static void AssertCaption(this PanelLayout layout, BusinessObject dataItem, ControlReference controlReference, string expectedCaption)
	{
		Assertion.Assert($"ResourceData for {controlReference.Name}", layout.TryGetCaption(controlReference, dataItem, out var resourceStringData));
		Assertion.AssertEquals($"Caption for {controlReference.Name}", expectedCaption, resourceStringData.Caption);
	}
}
