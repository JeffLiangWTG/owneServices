using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class TransportInlandAirUserControlTest : TestCase
{
	public void TestCaptionRenderingEnabled()
	{
		AssertEquals("CaptionRenderingEnabled", true, control.CaptionRenderingEnabled);
	}

	public void TestFlightTextBox()
	{
		CombineAssertions(() =>
		{
			var flightTextBox = control.FlightTextBox;
			var resStringData = flightTextBox.CaptionResourceString;
			AssertEquals("BindTo", "JE_TransportIDInland", flightTextBox.BindTo);
			AssertEquals("Caption", "Flight Number", resStringData.Caption);
			AssertEquals("MediumCaption", "Flight No.", resStringData.MediumCaption);
			AssertEquals("ShortCaption", "Flight", resStringData.ShortCaption);
		});
	}

	public void TestTransportNationalityCodeFindBox()
	{
		CombineAssertions(() =>
		{
			var transportNationalityCodeFindBox = control.TransportNationalityCodeFindBox;
			AssertEquals("BindTo", "JE_RN_NKTransportNationalityInland", transportNationalityCodeFindBox.BindTo);
			AssertEquals("Caption", "[18] Nationality", transportNationalityCodeFindBox.CaptionResourceString.Caption);
		});
	}

	public void TestAircraftIDTextBox()
	{
		CombineAssertions(() =>
		{
			AssertEquals("BindTo", "JE_AircraftRegistrationInland", control.AircraftIDTextBox.BindTo);
		});
	}

	public void TestTrailer1NationalityCodeFindBox()
	{
		CombineAssertions(() =>
		{
			var trailer1NationalityCodeFindBox = control.Trailer1NationalityCodeFindBox;
			AssertEquals("BindTo", "JE_RN_NKTrailer1Nationality", trailer1NationalityCodeFindBox.BindTo);
			AssertEquals("Caption", "[18] Nationality", trailer1NationalityCodeFindBox.CaptionResourceString.Caption);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new TransportInlandAirUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	TransportInlandAirUserControl control;
}
