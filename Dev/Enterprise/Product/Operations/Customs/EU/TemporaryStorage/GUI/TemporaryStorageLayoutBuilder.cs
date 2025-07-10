using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using TransportMeansList = Enterprise.Customs.Business.TransportMeansList;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class TemporaryStorageLayoutBuilder<T> : ColumnLayoutBuilder<T, TemporaryStorageUserControlBag>
		where T : TemporaryStorageHeader
	{
		public override TemporaryStorageUserControlBag CommonBag { get; } = TemporaryStorageUserControlBag.Instance;

		protected override int MaxColumns => 2;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override void SetDefaultCaptions()
		{
			base.SetDefaultCaptions();
			SetCaption(CommonBag.ArrivalTransportMeansCodeTextBox, GetArrivalTransportMeansCodeTextBoxCaption, l => l.TransportTypeInfo);
			SetCaption(CommonBag.PersonPresentingTheGoodsAddressControl, GetPersonPresentingTheGoodsAddressControlCaption, l => l.AMA_MessageTypeInfo);
		}

		static ResourceStringData GetPersonPresentingTheGoodsAddressControlCaption(TemporaryStorageHeader header)
		{
			switch (header.AMA_MessageType)
			{
				case PNTSMessageTypeList.Codes.Transfer:
					return Res.GetData("A850B206-30F4-4933-BB3C-D461D081E3CF", englishShortCaption: "Person notifying the arrival", englishCaption: "Person notifying the arrival", englishMediumCaption: "Person notifying the arrival", englishFullDescription: "Person notifying the arrival after movement");
				default:
					return null;
			}
		}

		protected virtual ResourceStringData GetArrivalTransportMeansCodeTextBoxCaption(TemporaryStorageHeader header)
		{
			switch (header.TransportType)
			{
				case TransportMeansList.Codes.ImoShipIdentificationNumber:
					return Res.GetData("D8450F86-0C32-4430-ABAB-EF6118D44769", englishCaption: "IMO Number", englishFullDescription: "[19 06 017 000] Arrival Transport Means > Identification Number");
				case TransportMeansList.Codes.WagonNumber:
					return Res.GetData("CE76997F-A878-4DE8-85D3-04B12D01FB45", englishCaption: "Wagon Number", englishFullDescription: "[19 06 017 000] Arrival Transport Means > Identification Number");
				case TransportMeansList.Codes.TrainNumber:
					return Res.GetData("AB633205-BD63-4B47-8146-A6BA9036887E", englishCaption: "Train Number", englishFullDescription: "[19 06 017 000] Arrival Transport Means > Identification Number");
				case TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer:
					return Res.GetData("58209377-0A2A-43AD-8F26-DE5847919D9F", englishCaption: "Road Trailer Reg. No.", englishFullDescription: "[19 06 017 000] Arrival Transport Means > Identification Number");
				case TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle:
					return Res.GetData("5B8DA249-2E5E-40FF-8F3A-BE3C2474BDD3", englishCaption: "Road Vehicle Reg. No.", englishFullDescription: "[19 06 017 000] Arrival Transport Means > Identification Number");
				case TransportMeansList.Codes.RegistrationNumberOfTheAircraft:
					return Res.GetData("C80AB667-A6DD-41BF-9B58-1FBE2825DCB4", englishCaption: "Aircraft Reg. No.", englishFullDescription: "[19 06 017 000] Arrival Transport Means > Identification Number");
				case TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode:
					return Res.GetData("CC2097C5-3DDC-4803-B74C-4FE9F8EEF76C", englishCaption: "ENI Code", englishFullDescription: "[19 06 017 000] Arrival Transport Means > Identification Number");
				default:
					return Res.GetData("C8413E5E-45C7-40B8-B68E-927BD7E83920", englishCaption: "Arrival Transport Means", englishFullDescription: "[19 06 017 000] Arrival Transport Means > Identification Number");
			}
		}
	}
}
