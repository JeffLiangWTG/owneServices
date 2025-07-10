using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.AWB
{
	[XsdSchema(Placement.Inner)]
	public partial class AWBHeader : IDataObject
	{
		public AWBHeader()
		{
		}

		public AWBHeader(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		public CodeDescriptionPair AWBType { get; set; }
		[MaxLength(35)]
		public ZString? AWBNumber { get; set; }
		[MaxLength(20)]
		public ZString? ForwardingAgentReference { get; set; }
		[MaxLength(3)]
		public ZString? OriginCode { get; set; }

		public AWBParty Shipper { get; set; }
		public AWBParty Consignee { get; set; }
		public AWBParty AlsoNotify { get; set; }

		#region Issuing Carrier's Agent

		[MaxLength(35)]
		public ZString? AgentName { get; set; }
		[MaxLength(17)]
		public ZString? AgentPlace { get; set; }
		[MaxLength(14)]
		public ZString? AgentIATACode { get; set; }
		[MaxLength(14)]
		public ZString? AgentAccountNo { get; set; }

		#endregion

		#region Issued By

		[MaxLength(50)]
		public ZString? IssuedByName { get; set; }
		[MaxLength(50)]
		public ZString? IssuedByAddress1 { get; set; }
		[MaxLength(50)]
		public ZString? IssuedByAddress2 { get; set; }

		#endregion

		public List<AWBAccountingInfo> AccountingInfoCollection { get; private set; }
		public List<CodeDescriptionPair> SpecialHandlingCollection { get; private set; }

		#region Routing

		[MaxLength(35)]
		public ZString? AirportOfDepartureAndRequestRouteText { get; set; }

		[MaxLength(3)]
		public ZString? Routing1stTo { get; set; }
		[MaxLength(2)]
		public ZString? Routing1stBy { get; set; }
		[MaxLength(3)]
		public ZString? Routing2ndTo { get; set; }
		[MaxLength(2)]
		public ZString? Routing2ndBy { get; set; }
		[MaxLength(3)]
		public ZString? Routing3rdTo { get; set; }
		[MaxLength(2)]
		public ZString? Routing3rdBy { get; set; }

		[MaxLength(3)]
		public ZString? AirportOfDestinationCode { get; set; }
		[MaxLength(35)]
		public ZString? AirportOfDestinationText { get; set; }

		[MaxLength(2)]
		public ZString? Requested1stCarrier { get; set; }
		[MaxLength(5)]
		public ZString? Requested1stFlight { get; set; }
		[MaxLength(2)]
		public ZString? Requested1stFlightDate { get; set; }
		[MaxLength(2)]
		public ZString? Requested2ndCarrier { get; set; }
		[MaxLength(5)]
		public ZString? Requested2ndFlight { get; set; }
		[MaxLength(2)]
		public ZString? Requested2ndFlightDate { get; set; }

		#endregion

		#region Handling

		[MaxLength(UniversalXmlInfo.MaxStringLength), AllowLineControlWhiteSpace]
		public ZString? HandlingInformation { get; set; }
		public ZBool? AsAgreedOn1stAWBSet { get; set; }
		public ZBool? AsAgreedOn2ndAWBSet { get; set; }
		[MaxLength(3)]
		public ZString? AsAgreedTypeOn1stAWBSet { get; set; }
		[MaxLength(3)]
		public ZString? AsAgreedTypeOn2ndAWBSet { get; set; }
		[MaxLength(3)]
		public ZString? SpecialHandlingCode { get; set; }
		[MaxLength(70)]
		public ZString? SpecialServiceRequest { get; set; }
		[MaxLength(70)]
		public ZString? OtherServiceInformation { get; set; }
		public ZInt? ShippersLoadAndCount { get; set; }

		#endregion

		#region Valuation

		[MaxLength(10)]
		public ZString? NetRateCode { get; set; }

		[MaxLength(15)]
		public ZString? ManifestDescriptionOfGoods { get; set; }

		[MaxLength(15)]
		public ZString? OptionalShippingInformation1 { get; set; }
		[MaxLength(15)]
		public ZString? OptionalShippingInformation2 { get; set; }

		public Currency Currency { get; set; }
		public CodeDescriptionPair2Char ChargesPayment { get; set; }
		public CodeDescriptionPair WeightChargesPayment { get; set; }
		public CodeDescriptionPair1Char OtherChargesPayment { get; set; }
		public ZDecimal? ValueForCarriage { get; set; }
		public ZDecimal? ValueForCustoms { get; set; }
		public ZDecimal? AmountOfInsurance { get; set; }

		public List<AWBRateLine> RateLineCollection { get; private set; }
		public List<AWBOtherCharges> OtherChargesCollection { get; private set; }

		public ZDecimal? TotalValuationPrepaid { get; set; }
		public ZDecimal? TotalValuationCollect { get; set; }
		public ZDecimal? TotalTaxesPrepaid { get; set; }
		public ZDecimal? TotalTaxesCollect { get; set; }

		#endregion

		#region Place and Date of Issue

		[MaxLength(64)]
		public ZString? ShipperExtraInfoLine1 { get; set; }
		[MaxLength(64)]
		public ZString? ShipperExtraInfoLine2 { get; set; }
		[MaxLength(35)]
		public ZString? ShippersSignature { get; set; }

		[MaxLength(64)]
		public ZString? AWBIssuerExtraInfo { get; set; }
		public ZDateTime? AWBIssueDate { get; set; }
		[MaxLength(17)]
		public ZString? AWBIssuePlace { get; set; }
		[MaxLength(35)]
		public ZString? AWBIssuerSignature { get; set; }
		[MaxLength(70)]
		public ZString? AWBIssuerApprovedExporterNumber { get; set; }

		#endregion

		public CargoSecurityDeclaration CargoSecurityDeclaration { get; set; }

		#region Pelican (Air Messaging) API required properties
		public List<CodeDescriptionPair35Char> AESExportCollection { get; private set; }

		public ACAS ACAS { get; set; }

		[MaxLength(7)]
		public ZString? KnownConsignor { get; set; }
		#endregion
	}
}
