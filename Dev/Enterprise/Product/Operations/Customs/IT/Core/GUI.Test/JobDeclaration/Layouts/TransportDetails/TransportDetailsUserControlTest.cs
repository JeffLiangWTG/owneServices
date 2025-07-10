using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class TransportDetailsUserControlTest : TestCase
{
	public void TestCaptionRenderingEnabled()
	{
		AssertEquals("CaptionRenderingEnabled", true, control.CaptionRenderingEnabled);
	}

	public void TestTransportInlandAirUserControl()
	{
		AssertType<TransportInlandAirUserControl>("Type", control.TransportInlandAirUserControl);
	}

	public void TestTransportInlandRoadUserControl()
	{
		AssertType<TransportInlandRoadUserControl>("Type", control.TransportInlandRoadUserControl);
	}

	public void TestInlandTransportModeAndMeansUserControl()
	{
		AssertType<InlandTransportModeAndMeansUserControl>("Type", control.InlandTransportModeAndMeansUserControl);
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new TransportDetailsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	TransportDetailsUserControl control;
}
