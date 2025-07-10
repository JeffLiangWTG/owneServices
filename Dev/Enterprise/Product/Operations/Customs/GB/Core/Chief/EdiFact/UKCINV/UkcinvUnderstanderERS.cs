using CargoWise.Types;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Chief.EdiFact.UKCINV
{
	public class UkcinvUnderstanderERS : UkcinvUnderstander
	{
		public UkcinvUnderstanderERS(UkCinvMessage messageToUnderstand)
			: base(messageToUnderstand)
		{ }

		public ErsReport Parse()
		{
			ersReport = GetNewReport();
			ParseHeaderGEIs();
			ParseHeaderRFFs();
			GetLocationAndDateDetails();
			ParseDeclarations();
			return ersReport;
		}

		protected virtual ErsReport GetNewReport()
		{
			return new ErsReport();
		}

		void ParseDeclarations()
		{
			foreach (UkCinvSegmentGroup1 grp1 in ukCinv.Group1)
			{
				var declarationReport = new EmrOrErsDeclarationReport();
				ParseOneDeclarationReport(declarationReport, grp1);
				ersReport.Declarations.Add(declarationReport);
			}
		}

		void ParseOneDeclarationReport(EmrOrErsDeclarationReport declarationReport, UkCinvSegmentGroup1 grp1)
		{
			var ducrHelper = GetDucrForSingleGroup1(grp1);
			declarationReport.DeclarationUCR = ducrHelper.Ducr;
			declarationReport.DeclarationUcrPart = ducrHelper.PartWithoutCheckSum;
			ZString ics = ZString.Empty;
			ZString roe = ZString.Empty;
			ZString soe = ZString.Empty;
			GetGEIs(ref ics, ref soe, ref roe, ref throwAway, ref throwAway, grp1.GEI);
			declarationReport.ImportCustomsStatus = ics;
			declarationReport.RouteOfEntry = roe;
			declarationReport.StyleOfEntry = soe;
			foreach (CNTSegment cnt in grp1.CNT)
			{
				ZInt count;
				ZInt.TryParse(cnt.Control.ControlTotalValue, out count);
				declarationReport.TotalPackages = count;
				break;
			}

			foreach (MEASegment mea in grp1.MEA)
			{
				ZDecimal weight;
				ZDecimal.TryParse(mea.ValueRange.MeasurementValue, out weight);
				declarationReport.TotalNetMassKilos = weight;
				break;
			}

			foreach (CSTSegment cst in grp1.CST)
			{
				declarationReport.CommodityCode = cst.CustomsIdentityCodes1.CustomsGoodsIdentifier;
				break;
			}

			declarationReport.SubmittingRole = grp1.AUT[0].ValidationResultValue;
		}

		void GetLocationAndDateDetails()
		{
			ersReport.GoodsArrivalDateTime = AssignDateTime(ref throwAway, ukCinv.DTM[0]);
			ZString location = ZString.Empty;
			ZString shed = ZString.Empty;
			ZString epuId = ZString.Empty;
			ZString epuNo = ZString.Empty;
			GetLocations(ukCinv.LOC, ref location, ref shed, ref epuNo, ref epuId);
			ersReport.GoodsLocation = location;
			ersReport.Shed = shed;
			ersReport.EntryProcessingUnitID = epuId;
			ersReport.EntryProcessingUnitNumber = epuNo;
		}

		void ParseHeaderRFFs()
		{
			ZString ucn = ZString.Empty;
			ZString aes = ZString.Empty;
			AssignFromRff(ukCinv.RFF1, ChiefConstants.RffSegmentIdentifiers.UCN, ref ucn, ref throwAway, ref throwAway);
			AssignFromRff(ukCinv.RFF1, ChiefConstants.RffSegmentIdentifiers.AES, ref aes, ref throwAway, ref throwAway);
			ersReport.MovementReference = aes;
			ersReport.MasterUCR = ucn;
		}

		protected virtual void ParseHeaderGEIs()
		{
		}

		ErsReport ersReport;
	}
}
