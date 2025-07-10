using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Chief.EdiFact.UKCINV
{
	public class ErsReport
	{
		public ErsReport()
		{
			Declarations = new List<EmrOrErsDeclarationReport>();
		}

		public ZString MasterUCR { get; set; } //MASTER-UCR
		public ZString MovementReference { get; set; } //MOVT-REF
		public ZString GoodsLocation { get; set; } //GDS-LOCN
		public ZString Shed { get; set; } //SHED-OP-ID
		public ZString EntryProcessingUnitNumber { get; set; } //EPU-NO
		public ZString EntryProcessingUnitID { get; set; } //EPU-ID
		public ZDateTime GoodsArrivalDateTime { get; set; } //GDS-ARR-DTM
		public List<EmrOrErsDeclarationReport> Declarations { get; private set; }
	}
}
