using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	public class DLMHeader : MessageBlock
	{
		public DLMHeader()
			: base("H")
		{
		}

		//[MessageBlockString(2, 6)]
		//public ZString NotUsed1;

		[MessageBlockString(8, 6)]
		public ZString ExporterAuthorizationId;

		//[MessageBlockString(14, 4)]
		//public ZString NotUsed2;

		[MessageBlockString(18, 11)]
		public ZString FormKey;

		#region Exporter
		[MessageBlockString(29, 70)]
		public ZString ExporterName;

		[MessageBlockString(99, 15)]
		public ZString ExporterBusinessNumber;

		[MessageBlockString(114, 70)]
		public ZString ExporterStreet;

		[MessageBlockString(184, 35)]
		public ZString ExporterCity;

		//[MessageBlockString(219, 3)]
		//public ZString NotUsed3;

		[MessageBlockString(222, 30)]
		public ZString ExporterProvinceState;

		//[MessageBlockString(252, 2)]
		//public ZString NotUsed4;

		[MessageBlockString(254, 20)]
		public ZString ExporterCountry;

		[MessageBlockString(274, 15)]
		public ZString ExporterPostalZipCode;

		[MessageBlockString(289, 10)]
		public ZString ExporterTelephone;

		[MessageBlockString(299, 4)]
		public ZString ExporterTelephoneExtension;

		[MessageBlockString(303, 10)]
		public ZString ExporterFax;
		#endregion

		#region Consignee
		[MessageBlockString(313, 70)]
		public ZString ConsigneeName;

		[MessageBlockString(383, 70)]
		public ZString ConsigneeStreet;

		[MessageBlockString(453, 35)]
		public ZString ConsigneeCity;

		[MessageBlockString(488, 30)]
		public ZString ConsigneeProvinceState;

		//[MessageBlockString(518, 2)]
		//public ZString NotUsed5;

		[MessageBlockString(520, 20)]
		public ZString ConsigneeCountry;
		#endregion

		#region Service Provider
		[MessageBlockString(540, 6)]
		public ZString ServiceProviderAuthorizationId;

		[MessageBlockString(546, 70)]
		public ZString ServiceProviderName;

		[MessageBlockString(616, 70)]
		public ZString ServiceProviderStreet;

		[MessageBlockString(686, 35)]
		public ZString ServiceProviderCity;

		//[MessageBlockString(721, 3)]
		//public ZString NotUsed6;

		[MessageBlockString(724, 30)]
		public ZString ServiceProviderProvinceState;

		//[MessageBlockString(754, 2)]
		//public ZString NotUsed7;

		[MessageBlockString(756, 20)]
		public ZString ServiceProviderCountry;

		[MessageBlockString(776, 15)]
		public ZString ServiceProviderPostalZipCode;

		[MessageBlockString(791, 10)]
		public ZString ServiceProviderTelephone;

		[MessageBlockString(801, 4)]
		public ZString ServiceProviderTelephoneExtension;
		#endregion

		#region Certifier
		[MessageBlockString(805, 70)]
		public ZString CertifierName;

		[MessageBlockString(875, 70)]
		public ZString CertifierStreet;

		[MessageBlockString(945, 35)]
		public ZString CertifierCity;

		//[MessageBlockString(980, 3)]
		//public ZString NotUsed8;

		[MessageBlockString(983, 30)]
		public ZString CertifierProvinceState;

		//[MessageBlockString(1013, 2)]
		//public ZString NotUsed9;

		[MessageBlockString(1015, 20)]
		public ZString CertifierCountry;

		[MessageBlockString(1035, 15)]
		public ZString CertifierPostalZipCode;

		[MessageBlockString(1050, 10)]
		public ZString CertifierTelephone;

		[MessageBlockString(1060, 4)]
		public ZString CertifierTelephoneExtension;

		[MessageBlockString(1064, 10)]
		public ZString CertifierFax;

		[MessageBlockString(1074, 70)]
		public ZString CertifierCompanyName;

		[MessageBlockString(1144, 1)]
		public ZString CertifierStatus;
		#endregion

		[MessageBlockDecimal(1145, 10)]
		public ZDecimal CommodityGrossWeight;

		//[MessageBlockString(1155, 3)]
		//public ZString NotUsed10;

		[MessageBlockString(1158, 50)]
		public ZString CommodityGrossWeightUnitOfMeasure;

		//[MessageBlockString(1208, 16)]
		//public ZString NotUsed11;

		[MessageBlockDecimal(1224, 16)]
		public ZDecimal FreightCharges;

		//[MessageBlockString(1240, 3)]
		//public ZString NotUsed12;

		[MessageBlockString(1243, 50)]
		public ZString CommodityCurrencyOfDeclaredValue;

		//[MessageBlockString(1293, 1)]
		//public ZString NotUsed13;

		[MessageBlockString(1294, 20)]
		public ZString ModeOfTransport;

		//[MessageBlockString(1314, 2)]
		//public ZString NotUsed14;

		[MessageBlockString(1316, 50)]
		public ZString ReasonForExport;

		[MessageBlockString(1366, 30)]
		public ZString VesselName;

		//[MessageBlockString(1396, 2)]
		//public ZString NotUsed15;

		[MessageBlockString(1398, 20)]
		public ZString CountryOfFinalDestination;

		[MessageBlockDate(1418)]
		public ZDate DateOfExportation;

		//[MessageBlockString(1426, 4)]
		//public ZString NotUsed16;

		[MessageBlockString(1430, 50)]
		public ZString PortOfExit;

		//[MessageBlockString(1480, 4)]
		//public ZString NotUsed17;

		[MessageBlockString(1484, 50)]
		public ZString PlaceOfReport;

		[MessageBlockInt(1534, 4)]
		public ZInt NumberOfPackages;

		//[MessageBlockString(1538, 3)]
		//public ZString NotUsed18;

		[MessageBlockString(1541, 20)]
		public ZString KindOfPackages;

		[MessageBlockString(1561, 35)]
		public ZString NameOfExportingCompany;

		[MessageBlockString(1596, 35)]
		public ZString TransportationDocumentNumber;

		//[MessageBlockString(1631, 19)]
		//public ZString NotUsed19;

		//[MessageBlockString(1650, 3)]
		//public ZString NotUsed20;
	}
}
