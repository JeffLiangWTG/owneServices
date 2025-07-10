using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.ServiceTask;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS
{
	[AllowPublicConstructor]
	public class SecurityNctsIE29DocumentWrapper : NctsIE29DocumentWrapper, INctsIE29DocumentWrapper
	{
		public SecurityNctsIE29DocumentWrapper(NctsEdiMessage ediMessage, BusinessObjectFactory factoryToWrap, NctsIE29CusdecResponseData cusdecResponseData)
			: base(ediMessage, factoryToWrap, cusdecResponseData)
		{
		}

		public static new SecurityNctsIE29DocumentWrapper New(NctsEdiMessage ediMessage, BusinessObjectFactory factoryToWrap)
		{
			return new SecurityNctsIE29DocumentWrapper(ediMessage, factoryToWrap, null);
		}

		public ZString BOXS32OTHERSCI
		{
			get { return Tools.ValueOrThreeDashes(cusdecResponseData.SpecificCircumstanceIndicator); }
		}

		public ZString BOXS12FIRSTARRIVALTIME
		{
			get { return "---"; } // The element was introduced in annex 30a CCIP and it is not part of the message IE15; when NOT used print "---"
		}

		public ZString BOXS29TRANSPORTCHARGESMOP
		{
			get { return Tools.ValueOrThreeDashes(cusdecResponseData.TransportChargesMoP); }
		}

		public ZString BOX7REFERENCENUMBERS
		{
			get { return cusdecResponseData.LocalReferenceNumber; }
		}

		public ZString BOX7UCR
		{
			get { return cusdecResponseData.ReferenceNumberUCR; }
		}

		public ZString BOX21BORDERTRANSPORTID
		{
			get { return Tools.ValueOrThreeDashes(cusdecResponseData.MeansOfTransportCrossingBorderIdentity); }
		}

		public ZString BOX21BORDERTRANSPORTFLAG
		{
			get { return Tools.ValueOrThreeDashes(cusdecResponseData.MeansOfTransportCrossingBorderNationality); }
		}

		public ZString BOX25BORDERTRANSPORTMODE
		{
			get { return Tools.ValueOrThreeDashes(cusdecResponseData.TransportModeAtBorder); }
		}

		public ZString BOX30LOCATIONOFGOODS
		{
			get { return (cusdecResponseData.AgreedLocationOfGoods + " " + cusdecResponseData.CustomsSubPlace).Trim(); }
		}

		public ZBool BOXS00SECURITY
		{
			get { return cusdecResponseData.IsSecurity; }
		}

		public ZString BOXS18PLACEOFUNLOADING
		{
			get { return Tools.ValueOrThreeDashes(cusdecResponseData.PlaceOfUnloadingCode); }
		}

		public ZString BOXS17PLACEOFLOADING
		{
			get { return Tools.ValueOrThreeDashes(cusdecResponseData.PlaceOfLoadingCode); }
		}

		public ZString BOXS10CONVEYANCE
		{
			get { return Tools.ValueOrThreeDashes(cusdecResponseData.ConveyanceReferenceNumber); }
		}

		public ZString BOXS13ROUTING
		{
			get { return cusdecResponseData.Itinerary; }
		}

		public ZString BOXS6SECURITYCONSIGNEE
		{
			get { return GetWrappedAddress(cusdecResponseData.SecurityConsignee); }
		}

		public ZString BOXS6SECURITYCONSIGNEEEORI
		{
			get { return cusdecResponseData.SecurityConsignee?.GovRegNum ?? ZString.Empty; }
		}

		public ZString BOXS4SECURITYCONSIGNOR
		{
			get { return GetWrappedAddress(cusdecResponseData.SecurityConsignor); }
		}

		public ZString BOXS4SECURITYCONSIGNOREORI
		{
			get { return cusdecResponseData.SecurityConsignor?.GovRegNum ?? ZString.Empty; }
		}

		public ZString BOXS7CARRIER
		{
			get { return GetWrappedAddress(cusdecResponseData.Carrier); }
		}

		public ZString BOXS7CARRIEREORI
		{
			get { return cusdecResponseData.Carrier?.GovRegNum ?? ZString.Empty; }
		}

		public ZString PRESENTATIONOFGOODSDATETIME
		{
			get { return IsPhase5 ? cusdecResponseData.PresentationOfTheGoodsDateAndTime.ToString("yyyy-MM-dd hh:mm:ss") : string.Empty; }
		}

		public ZString BOX44AUTHORISATIONS
		{
			get
			{
				if (IsPhase5)
				{
					var sb = new ZStringBuilder();
					cusdecResponseData.Authorisations?.ForEach(a => sb.Append($"{a.SequenceNumber} - {a.Type} - {a.ReferenceNumber}"));
					return sb.ToStringWithNewLineBetweenAppends();
				}
				else
				{
					return string.Empty;
				}
			}
		}

		public ZString BOX50REPRESENTATIVE
		{
			get { return IsPhase5 ? $"{cusdecResponseData.Representative.IdentificationNumber} - {cusdecResponseData.Representative.Status}" : string.Empty; }
		}

		public ZString BOXS28SEALSNUMBER => ConsecutiveSequenceCondenser.Condense(cusdecResponseData.Seals);

		public override ZString BOXDSEALSIDENTITY => IsPhase5 ? ZString.Empty : base.BOXDSEALSIDENTITY;
	}
}
