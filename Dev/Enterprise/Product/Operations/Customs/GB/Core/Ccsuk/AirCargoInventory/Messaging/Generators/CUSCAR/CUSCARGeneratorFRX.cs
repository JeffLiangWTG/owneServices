using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Chief.EdiFact;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class CUSCARGeneratorFRX : CUSCARGeneratorBase
	{
		public CUSCARGeneratorFRX(ICcsukCusAwb awb, ErrorCollector ec)
			: base(awb, ec)
		{
		}

		protected override string AssociationAssignedCode
		{
			get { return "109504"; }
		}

		protected override string BgmDocumentName
		{
			get { return "FRX"; }
		}

		public override string MakeMessageText()
		{
			MakeUNH();
			MakeBGM();
			MakeGroup2TDTandLOC();
			MakeUNT();

			var charSet = new UkCharSet();
			return ReplaceFakeSegmentNames(charSet, result.ToString(charSet));
		}

		protected override void Group2Tdt20PutInFullTransportDetails(TDTSegmentWithDatetime tdt)
		{
		}

		protected override string BgmDocumentNameHuman
		{
			get { return "Delete Freight Record"; }
		}

		protected override string GetCuscarMessageVersionTwoOrThree()
		{
			return "2";
		}
	}
}
