using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class CUSCARGeneratorFCS : CUSCARGeneratorFRI
	{
		public CUSCARGeneratorFCS(ICcsukCusAwb awb, ErrorCollector ec, NonPersistentSplitLineCollection splitLines)
			: base(awb, ec)
		{
			this.splitLines = splitLines;
			if (splitLines.Count == 0)
			{
				ec.AddError("number of splits", new ErrorInfo("", "mandatory"));
			}

			DemandFieldsNotEmpty("Airport", iCuscar.AirportOfArrival,
								"AWB number", iCuscar.AirWaybillSerialNumber);
		}

		protected override string AssociationAssignedCode
		{
			get { return "109505"; }
		}

		protected override string BgmDocumentName
		{
			get { return "FCS"; }
		}

		protected override string BgmDocumentNameHuman
		{
			get { return "Split Manipulation"; }
		}

		protected override void MakeGroup5Lines()
		{
			// One line for each split
			foreach (NonPersistentSplitLine splitLine in splitLines)
			{
				var grp5 = result.Group5.InstantiateAChildAndAddItToChildrenCollection();
				MakeGroup5GID(splitLine.SplitNumber, grp5);
				MakeGroup6QTYNumberOfPieces(grp5, splitLine.NumberOfPieces);
				MakeGroup5MEAWeight(grp5, splitLine.WeightUQ, splitLine.Weight);
			}
		}

		protected override void MakeLOC(LOCSegmentWithLotsOfLocations lOCSegmentWithLotsOfLocations, string arrivalAirport, string shed, ZString originAirport, ZString destinationAirport)
		{
			MakeLOCAndAddType11ForAOA(lOCSegmentWithLotsOfLocations, arrivalAirport, shed);
		}

		protected override void MakeGISShipmentDescription(ZString sdc)
		{
			base.MakeGISShipmentDescription("P");
		}

		protected override void Group2Tdt20PutInFullTransportDetails(TDTSegmentWithDatetime tdt)
		{
		}

		protected override void MakeGroup3NAD()
		{
		}

		protected override void MakeGroup5FTXDescOfGoods(CUSCARSegmentGroup5 grp5)
		{
		}

		protected override ZString GetHtmlInterpretationForFieldsFromCusCarEdifact()
		{
			var table = new HtmlTableCreator(new string[] { "Split Reference", "Pieces", "Weight" });
			foreach (NonPersistentSplitLine splitLine in splitLines)
			{
				table.WriteRow(splitLine.SplitNumber, splitLine.NumberOfPieces, splitLine.Weight.ToStringTrimZeros() + splitLine.WeightUQ);
			}
			return table.ToHtml();
		}

		protected override void MakeCommunityHandlingCodes()
		{
		}

		protected override string GetCuscarMessageVersionTwoOrThree()
		{
			return "2";
		}

		readonly NonPersistentSplitLineCollection splitLines;
	}
}
