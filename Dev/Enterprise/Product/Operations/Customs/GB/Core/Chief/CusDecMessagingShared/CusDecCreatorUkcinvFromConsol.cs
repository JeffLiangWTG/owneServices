using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Edifact.D00A.Elements;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Chief.CusDec
{
	public class CusDecCreatorUkcinvFromConsol : CusDecCreatorUkcinv
	{
		public CusDecCreatorUkcinvFromConsol(CustomsExportConsolIntegrationWrapper wrapper, GbDes242MessageFunction mucrMessageFunction, ErrorCollector errorCollector)
		{
			gbDes242MessageFunction = mucrMessageFunction;
			ukCinvWrapperConsol = new ChiefExportConsolIntegrationWrapperToIUkCinvWrapper(wrapper);
			iUkCinvWrapper = ukCinvWrapperConsol;
			hmrc109 = CodeListResponsibleAgencyCodeList.GetFromString("109");
			this.errorCollector = errorCollector;
		}

		protected override void HeaderGroup1RffABO()
		{
			if (gbDes242MessageFunction is GbDes242MessageFunction.MasterUcrWithChildUcrFunction mucrWithDucrFunction)
			{
				HeaderGroup1RffABO(mucrWithDucrFunction.ChildUCR, mucrWithDucrFunction.ChildUCRPartNo);
			}
			else
			{
				base.HeaderGroup1RffABO();
			}
		}

		protected override RFFSegment HeaderGroup1RffUCN()
		{
			if (iUkCinvWrapper.MasterUniqueConsignmentReference.IsEmpty)
			{
				errorCollector.AddError("Master UCR", new ErrorInfo("", "Mandatory"));
			}
			return base.HeaderGroup1RffUCN();
		}

		protected override void MakeLOCLocationOfGoods()
		{
			if (gbDes242MessageFunction is GbInventoryManagementMessageFunction.ArrivalActual && iUkCinvWrapper.ShedCode.IsEmpty)
			{
				errorCollector.AddError("Shed", new ErrorInfo("", "Mandatory")); // see page 10 of "NES for CCSUK" document
			}
			base.MakeLOCLocationOfGoods();
		}

		protected override void MakeTDTTransportModeAndId()
		{
			if (ukCinvWrapperConsol.UseAntiSmugglingTrptId && gbDes242MessageFunction is GbInventoryManagementMessageFunction.ArrivalActual)
			{
				var tdt = ukCinvMessage.TDT.InstantiateAChildAndAddItToChildrenCollection();
				tdt.TransportStageCodeQualifier = TransportStageCodeQualifierList.GetFromString("13");
				var origin = ukCinvWrapperConsol.OriginAirport;
				var destPort = ukCinvWrapperConsol.DestinationAirport;
				var destCountry = ukCinvWrapperConsol.DestinationCountry;
				var maybeCTStatus = ukCinvWrapperConsol.CommunityTransitStatus.IsEmpty ? "" : string.Format("/T={0}", ukCinvWrapperConsol.CommunityTransitStatus);
				var maybePartIndicator = ukCinvWrapperConsol.PartMovement ? "/P" : "";
				tdt.TransportIdentification.TransportMeansIdentificationName = string.Format("O={0}/D={1}/C={2}{3}{4}", origin, destPort, destCountry, maybeCTStatus, maybePartIndicator);
			}
			else
			{
				base.MakeTDTTransportModeAndId();
			}
		}

		protected override void MakeGroup1RffAES()
		{
			if (gbDes242MessageFunction is GbInventoryManagementMessageFunction.ArrivalActual || gbDes242MessageFunction is GbInventoryManagementMessageFunction.ArrivalAnticipated)
			{
				var moveRef = ukCinvWrapperConsol.MovementReference;
				if (!moveRef.IsEmpty)
				{
					var rffAes = ukCinvMessage.RFF1.InstantiateAChildAndAddItToChildrenCollection();
					rffAes.Reference.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.GetFromString(ChiefConstants.RffSegmentIdentifiers.AES);
					rffAes.Reference.ReferenceIdentifier = ukCinvWrapperConsol.MovementReference.GetCusDecValue(ChiefDataElementsLengths.Codes.MOVT_REF);
				}
			}
		}

		readonly IUkCinvWrapperConsol ukCinvWrapperConsol;
	}
}
