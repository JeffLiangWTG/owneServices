using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Common.AU
{
	public interface IPRAMessagingData
	{
		#region Consol Level FieldMappers

		string PortOfLoading { get; }
		string PortOfDischarge { get; }
		string VesselName { get; }
		string Voyage { get; }
		string LloydsNumber { get; }
		string ECNorCRN { get; }
		string ShippingLineBookingReference { get; }
		string ConsignorName { get; }
		string ShippingLine1StopCode { get; }
		string LoadTerminal1StopCode { get; }
		string PortOfFinalDischarge { get; }

		#endregion

		#region GlbStaff Field Mappers

		string SenderCompanyName { get; }
		string SenderContactName { get; }
		string SenderPhone { get; }
		string SenderFax { get; }
		string SenderEmail { get; }

		#endregion

		#region RefContainer FieldMappers

		bool HasTynes { get; }
		string ISOContainerType { get; }
		ContainerISOType ISOType { get; }

		#endregion

		#region Container FieldMappers;

		bool ArrivingAtCTOByRail { get; }
		string ContainerNumber { get; }
		string SealNumber { get; }
		string Commodity1StopCode { get; }
		decimal ContainerGrossWeight { get; }
		decimal ContainerTareWeight { get; }
		decimal ContainerNetWeight { get; }
		int AirVentSetting { get; }
		int HumidityPercentage { get; }
		string AirVentSettingUnit { get; }
		string TemperatureSettingFormatted { get; }
		bool IsTempControlled { get; }
		bool IsEmptyContainer { get; }
		int Temperature { get; }
		int OverhangBackInCM { get; }
		int OverhangFrontInCM { get; }
		int OverhangLeftInCM { get; }
		int OverhangRightInCM { get; }
		int OverhangHeightInCM { get; }
		string FlatRackID { get; }
		string ReeferGeneratorID { get; }
		string TerminalVBSBooking { get; }
		string TruckRegoNumber { get; }
		string RoadOrig1StopCode { get; }
		string RoadDest1StopCode { get; }
		ZDateTime RoadScheduledDeparture { get; }
		ZDateTime RoadScheduledArrival { get; }
		bool ContainerIsWaitingForResponse { get; }
		string GrossWeightVerifiedType { get; }
		ZDateTime GrossWeightVerifiedDateTime { get; }
		string GrossWeightVerifiedByAddress { get; }
		string GrossWeightVerifiedDeclarantContact { get; }
		string GrossWeightVerifiedDeclarantSignature { get; }

		#endregion

		#region Container Cartage Company FieldMappers

		string CartageCompanyABN { get; }
		string CartageBookingReference { get; }

		#endregion

		#region Other DataMappers

		string MessageReference { get; }
		string SenderID { get; }
		string GoodsDescription { get; }
		string DateTimeStringForMessage { get; }
		ZString GetErrorText();
		ZString GetWarningText();
		EDIMessage GetEDIMessage();
		DangerousGoodsCollection DangerousGoodsList { get; }
		void Save();

		#endregion
	}
}
