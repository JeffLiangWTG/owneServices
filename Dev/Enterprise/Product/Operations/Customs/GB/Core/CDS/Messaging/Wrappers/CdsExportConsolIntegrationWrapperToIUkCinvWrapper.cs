using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;

namespace Enterprise.Customs.GB.CDS
{
	public class CdsExportConsolIntegrationWrapperToIUkCinvWrapper : IUkCinvWrapperConsol
	{
		public CdsExportConsolIntegrationWrapperToIUkCinvWrapper(CustomsExportConsolIntegrationWrapper wrapper)
		{
			consolWrapper = wrapper;
		}

		public ZString MasterUniqueConsignmentReference => consolWrapper.MawbExportHelper.ME_MasterUCR;

		public ZString LocationOfGoods => new CcsUkToCdsLocationConverter(consolWrapper.MawbExportHelper.ME_ExportLocation, ShedCode).CalculateCdsLocation(consolWrapper.Factory, true).Replace(" ", "");

		public ZString ShedCode => consolWrapper.MawbExportHelper.ME_ExportShed;

		public ZString DeclarationUniqueConsignmentReference => "";

		public ZString DeclarationUniqueConsignmentReferencePartSuffix => "";

		public ZDateTime DateAndTimeTheGoodsWillBeAvailableForInspectionAtLCPPremises => consolWrapper.MawbExportHelper.ME_MovementDate;

		public ZDateTime DateAndTimeTheGoodsWillBeLeavingLCPPremises => consolWrapper.MawbExportHelper.ME_MovementDate;

		public ZString TransportModeAtTheBorderBox25 => new TransportModeTranslator().TranslateToWCOCode(consolWrapper.MawbExportHelper.ME_TransportMode);

		public ZString TransportNationalityAtTheBorderBox21 => consolWrapper.MawbExportHelper.ME_TransportCountry;

		public ZString TransportIdentityAtTheBorderBox21 => consolWrapper.MawbExportHelper.ME_TransportID;

		public ZString MasterOpt => consolWrapper.MawbExportHelper.ME_MasterOpt;

		public ZString OriginAirport => consolWrapper.ForwardingConsol.JK_RL_NKLoadPort.Right(3);

		public ZString DestinationAirport => consolWrapper.ForwardingConsol.JK_RL_NKDischargePort.Right(3);

		public ZString DestinationCountry => consolWrapper.ForwardingConsol.JK_RL_NKDischargePort.Left(2);

		public ZBool PartMovement => consolWrapper.MawbExportHelper.ME_PartMovementIndicator;

		public ZBool UseAntiSmugglingTrptId => consolWrapper.MawbExportHelper.ME_UseAntiSmugglingTrptid;

		public ZString CommunityTransitStatus => consolWrapper.MawbExportHelper.ME_CommunityTransitStatus;

		public ZString MovementReference
		{
			get
			{
				var result = ZString.Empty;
				var arriveDateTime = DateAndTimeTheGoodsWillBeAvailableForInspectionAtLCPPremises;
				if (!arriveDateTime.IsEmpty)
				{
					result = arriveDateTime.ToString("ddMMMHHmm");
					if (PartMovement)
					{
						result += "PART";
					}
				}
				return result;
			}
		}

		public ZString CDSDeclarationUniqueConsignmentReference => "";

		public ZString CDSDeclarationUniqueConsignmentReferencePartSuffix => "";

		readonly CustomsExportConsolIntegrationWrapper consolWrapper;
	}
}
