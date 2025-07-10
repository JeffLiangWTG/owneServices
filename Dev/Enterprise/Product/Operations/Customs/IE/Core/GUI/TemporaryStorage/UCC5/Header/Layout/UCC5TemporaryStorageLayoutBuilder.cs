using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.TemporaryStorage.GUI;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.GUI
{
	public class UCC5TemporaryStorageLayoutBuilder : TemporaryStorageLayoutBuilder<TemporaryStorageHeader>
	{
		protected override ResourceStringData GetArrivalTransportMeansCodeTextBoxCaption(EU.Business.CusTempStorage.TemporaryStorageHeader header)
		{
			switch (header.TransportType)
			{
				case TransportMeansList.Codes.NameOfTheSeaGoingVessel:
				case TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel:
					return Res.GetData("5213A980-81C3-4D62-9C5A-07B114EED5C1", englishCaption: "Vessel Name", englishFullDescription: "[19 06 017 000] Arrival Transport Means > Identification Number");
				case TransportMeansList.Codes.IataFlightNumber:
					return Res.GetData("9C914534-334F-4115-B0BB-2DA4CD4E9CE8", englishCaption: "Flight Number", englishFullDescription: "[19 06 017 000] Arrival Transport Means > Identification Number");
				default:
					return base.GetArrivalTransportMeansCodeTextBoxCaption(header);
			}
		}
	}
}
