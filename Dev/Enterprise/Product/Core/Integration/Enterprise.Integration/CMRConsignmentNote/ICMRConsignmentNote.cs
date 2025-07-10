using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface ICMRConsignmentNote
	{
		ZString SupplierAddress { get; }
		ZString ImporterAddress { get; }
		ZString InternationalConsignmentNote { get; }
		ZString PlaceOfDelivery { get; }
		ZString CityCountryDateOfGoodsTakingOver { get; }
		ZString GoodsAttachedDocuments { get; }
		ZString CarrierAddress { get; }
		ZString LineDetailsBox6_7_8_9 { get; }
		ZString LineDetailsTariffCodeBox10 { get; }
		ZString LineDetailsGrossWeightInKGBox11 { get; }
		ZString LineDetailsVolumeInM3Box12 { get; }
		ZString IncotermAndTextBox14 { get; }
		ZString TransportIDBox23 { get; }
		ZString JobNumber { get; }
		ZString Box14PaymentCarriage { get; }
		ZString Box19SpecialAgreements { get; }
		ZString Box20ToBePaidBy { get; }
		ZString Box15CashOnDelivery { get; }
		ZString Box23TransportAndTrailerID { get; }
		ZString SendersInstructions { get; }
		ZString SpecialAgreements { get; }
		ZString EstablishedInPlace { get; }
		ZString EstablishedInDate { get; }
		ZString DangerousGoodsClass { get; }
		ZString DangerousGoodsNumber { get; }
		ZString DangerousGoodsLetter { get; }
		ITextLimitCalculator TextLimitCalculator { get; }
	}
}
