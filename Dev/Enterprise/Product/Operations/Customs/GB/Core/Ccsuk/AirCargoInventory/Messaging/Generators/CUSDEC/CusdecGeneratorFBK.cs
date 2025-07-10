using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Environment;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CUSDEC_2_912
{
	public class CusdecGeneratorFBK : CusdecGeneratorBase
	{
		public CusdecGeneratorFBK(ICcsukCusAwb awb, ErrorCollector ec, CusUnderbond fbkUnderbond)
			: base(awb, ec, fbkUnderbond)
		{
			if (!IsAllowedToMakeFallbacks)
			{
				errorCollector.AddError(FallbackNoSecurityRightsMessage, null);
			}
			if (awb.ShipmentDescriptionCode == ShipmentDescriptionCodes.Codes.CommunityStatusFromECAirport || awb.ShipmentDescriptionCode == ShipmentDescriptionCodes.Codes.CommunityStatusFromOutsideEC)
			{
				errorCollector.AddError(awb.ReferenceNumber + " has a European SDC, a fallback is not permitted", null);
			}
		}

		public const string FallbackNoSecurityRightsMessage = "You don't have security rights to send a new Fallback request message. Your administrator can send the request or grant you rights";

		public static bool IsAllowedToMakeFallbacks
		{
			get { return Env.Security.AirCcsukCreateFallbackUnderbondRequest.IsAllowed; }

#if DEBUG
			set { Env.Security.AirCcsukCreateFallbackUnderbondRequest.IsAllowed = value; }
#endif
		}

		protected override string AssociationAssignedCode
		{
			get { return "109601"; }
		}

		protected override string BgmDocumentName
		{
			get { return "FBK"; }
		}

		protected override LocationAndRelationsAsASingleElement MakeLOC85DestinationAirport()
		{
			return null;
		}

		protected override void MakesGISforLicenceRestricted()
		{
		}

		protected override void MakeTDT()
		{
		}
	}
}
