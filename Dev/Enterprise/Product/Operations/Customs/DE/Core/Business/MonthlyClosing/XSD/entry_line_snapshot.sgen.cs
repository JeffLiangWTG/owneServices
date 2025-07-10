namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
#pragma warning disable CW1154
	[CargoWise.Customs.Shared.MessageDefinitions.EmbeddedResource("Enterprise.Customs.DE.Business.MonthlyClosing.XSD.cusrecon_snapshot.xsd")]
	[System.Xml.Serialization.XmlSerializerAssembly("Enterprise.Customs.DE.Business.XmlSerializers")]
	public partial class DEMonthlyClosingEntryLineSnapshot : IMonthlyClosingXmlObject
	{
		public bool IsEmpty =>
			AdditionalProcedure == null &&
			Assessment == null &&
			BorderTransportMeans == null &&
			CessionManagementFlag == null &&
			CommodityCode == null &&
			DepartureCountry == null &&
			Document == null &&
			ExciseDuty == null &&
			ForeignTradeFlag == null &&
			ForeignTradeStatistics == null &&
			InwardMovementAmount == null &&
			!NetMassMeasureSpecified &&
			OriginCountry == null &&
			PreferentialCountry == null &&
			PreferentialTreatment == null &&
			SupplementaryCodes == null &&
			SupplementaryInformation == null &&
			TobaccoRevenueStampNumber == null;
	}

	public partial class Amount : CargoWise.Customs.DE.MessageDefinitions.IAmount
	{
	}
#pragma warning restore CW1154
}
