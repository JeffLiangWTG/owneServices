using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SACMessageLine : BaseMessageLine
	{
		public SACMessageLine(CusEntryLine entryLine, SegmentGroup30 group30)
			: base(entryLine, group30)
		{
		}

		public override void Populate(int lineNumber, string lineActionCode)
		{
			PopulateCST(lineActionCode);
			PopulateFTX();
			PopulateMEA();
			PopulateGroup33();
			PopulateGroup35();
			PopulateGroup40();
		}

		#region Group 33

		protected internal override void PopulateGroup33()
		{
			SegmentGroup33 group33 = Group30.Group33.InstantiateAChildAndAddItToChildrenCollection();
			PopulateMOA(group33, EntryLine.CustomsValue, MonetaryAmountTypeCodeQualifierList.CustomsValue);
		}

		#endregion
	}
}
