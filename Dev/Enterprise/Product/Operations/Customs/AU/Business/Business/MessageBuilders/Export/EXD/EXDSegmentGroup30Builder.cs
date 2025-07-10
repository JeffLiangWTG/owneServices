using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDSegmentGroup30Builder : CMRInvoiceLineDetailsBuilder
	{
		public EXDSegmentGroup30Builder(JobComInvoiceLine jobComInvLine, SegmentGroup30 group30, int lineNumber)
			: base(jobComInvLine)
		{
			this.group30 = group30;
			this.lineNumber = lineNumber;
		}

		protected override CSTSegmentMessageSection CSTSection
		{
			get
			{
				return group30.CST;
			}
		}

		protected override FTXSegmentMessageSection FTXSection
		{
			get
			{
				return group30.FTX;
			}
		}

		protected override LOCSegmentMessageSection LOCSection
		{
			get
			{
				return group30.LOC;
			}
		}

		protected override MEASegmentMessageSection MEASection
		{
			get
			{
				return group30.MEA;
			}
		}

		protected override MOASegmentMessageSection MOASection
		{
			get
			{
				return group30.Group33[0].MOA;
			}
		}

		protected override RFFSegment GetNextRFFSegment()
		{
			return group30.Group35.InstantiateAChildAndAddItToChildrenCollection().RFF[0];
		}

		protected SegmentGroup30 group30;
		protected int lineNumber;
		public void PopulateSegment()
		{
			PopulateCSTSegment(lineNumber);
			PopulateAHECC();
			PopulatePermits();
			PopulateTemporaryImportNumber();
			PopulateGoodsDescription();
			PopulateMeasurements();
			PopulateOriginOfGoods();
			PopulateFOBValue();
		}
	}
}
