using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class CUSCARGeneratorFRIUFO : CUSCARGeneratorFRI
	{
		public CUSCARGeneratorFRIUFO(ICcsukCusAwb awb, ErrorCollector ec)
			: base(awb, ec)
		{
		}

		protected override string BgmDocumentNameHuman
		{
			get { return "Insert Unidentified Freight Object Record"; }
		}

		protected override void MakesGISes()
		{
		}

		protected override void MakeCommunityHandlingCodes()
		{
		}

		protected override void MakeLOCForAirports(CUSCARSegmentGroup2 grp2)
		{
			var loc = grp2.LOC.InstantiateAChildAndAddItToChildrenCollection();
			MakeLOCAndAddType11ForAOA(loc, iCuscar.AirportOfArrival, iCuscar.CargoTerminalOperator);
		}

		protected override void MakeGroup3NAD()
		{
		}

		protected override void MakeGroup5FTXDescOfGoods(CUSCARSegmentGroup5 grp5)
		{
		}

		protected override void MakeGroup5MEAWeight(CUSCARSegmentGroup5 grp5, string weightCode, ZDecimal weight)
		{
		}
	}
}
