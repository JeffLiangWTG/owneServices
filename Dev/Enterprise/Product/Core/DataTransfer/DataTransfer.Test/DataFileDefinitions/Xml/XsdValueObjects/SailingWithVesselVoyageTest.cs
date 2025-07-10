using System.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.SailingWithVesselVoyage))]
	sealed class SailingWithVesselVoyageTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.SailingWithVesselVoyage sailing = new Xsd.SailingWithVesselVoyage();
			AssertEquals("Should not be specified by default", false, sailing.IsSpecified);

			sailing.VesselName = "VesselName";
			AssertEquals("Should be specified", true, sailing.IsSpecified);
			sailing.VesselName = "";

			sailing.VoyageNo = "VoyageNo";
			AssertEquals("Should be specified", true, sailing.IsSpecified);
			sailing.VoyageNo = "";

			sailing.LloydsNo = "LloydsNo";
			AssertEquals("Should be specified", true, sailing.IsSpecified);
			sailing.LloydsNo = "";

			sailing.CargoCarrierCode = "CCode";
			AssertEquals("Should be specified", true, sailing.IsSpecified);
			sailing.CargoCarrierCode = "";

			sailing.FCLDates.ReceivalCommencesDate = ZDateTime.Now;
			AssertEquals("Should be specified", true, sailing.IsSpecified);
			sailing.FCLDates.ReceivalCommencesDate = ZDateTime.Empty;

			sailing.LCLDates.ReceivalCommencesDate = ZDateTime.Now;
			AssertEquals("Should be specified", true, sailing.IsSpecified);
			sailing.LCLDates.ReceivalCommencesDate = ZDateTime.Empty;

			sailing.ETD = ZDateTime.BrettsBirthday;
			AssertEquals("Should be specified", true, sailing.IsSpecified);
			sailing.ETD = ZDateTime.Empty;

			sailing.ETA = ZDateTime.BrettsBirthday;
			AssertEquals("Should be specified", true, sailing.IsSpecified);

			sailing.IsSpecified = false;
			AssertEquals("Should not be specified when IsSpecified set to false explicitly", false, sailing.IsSpecified);
		}

		public void TestGetVesselName()
		{
			Xsd.SailingWithVesselVoyage sailing = new Xsd.SailingWithVesselVoyage();

			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);

			sailing.VesselName = "";
			sailing.LloydsNo = "8810449";
			AssertEquals("Expecting GetVesselName to get Vessel Name from Lloyds number if Vessel Name is empty.", "AOTEAROA CHIEF", sailing.GetVesselName(context));

			sailing.VesselName = "Clinty";
			AssertEquals("Expecting GetVesselName to return VesselName.", "Clinty", sailing.GetVesselName(context));
		}

		public void TestIfNewPropertiesAddedIsSpecifiedNeedsToChange()
		{
			AssertEquals(
				"If the number of properties has changed, make sure IsSpecified is still implemented correctly and doesn't need to take into account the new properties",
				72, TypeDescriptor.GetProperties(typeof(Xsd.SailingWithVesselVoyage)).Count);
			// Henry needs to fix this 
		}
	}
}
