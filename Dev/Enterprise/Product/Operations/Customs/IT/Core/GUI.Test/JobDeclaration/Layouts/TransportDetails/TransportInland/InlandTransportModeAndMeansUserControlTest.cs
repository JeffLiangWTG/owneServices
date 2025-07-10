using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class InlandTransportModeAndMeansUserControlTest : TestCase
{
	public void TestCaptionRenderingEnabled()
	{
		AssertEquals("CaptionRenderingEnabled", true, control.CaptionRenderingEnabled);
	}

	public void TestInlandModeOfTransportDropEdit()
	{
		AssertEquals("BindTo", "JE_TransportModeInland", control.InlandModeOfTransportDropEdit.BindTo);
	}

	public void TestInlandTransportCodeDropEdit()
	{
		var inlandTransportMeansDropEdit = control.InlandTransportMeansDropEdit;
		var resString = inlandTransportMeansDropEdit.CaptionResourceString;
		CombineAssertions(() =>
		{
			AssertEquals("BindTo", "JE_TransportMeans", inlandTransportMeansDropEdit.BindTo);
			AssertEquals("Caption", "[18] Code", resString.Caption);
			AssertEquals("FullDescription", "[18] Inland Transport Code", resString.FullDescription);
			AssertEquals("PreBoundMaxLength", 2, inlandTransportMeansDropEdit.PreBoundMaxLength);
			AssertEquals("ShowDescriptionBox", false, inlandTransportMeansDropEdit.ShowDescriptionBox);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new InlandTransportModeAndMeansUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	InlandTransportModeAndMeansUserControl control;
}
