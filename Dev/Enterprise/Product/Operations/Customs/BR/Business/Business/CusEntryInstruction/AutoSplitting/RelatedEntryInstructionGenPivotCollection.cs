using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business
{
	public class RelatedEntryInstructionGenPivotCollection : CustomsGenPivotCollection<RelatedEntryInstructionGenPivot, CusEntryInstruction, CusEntryInstruction>
	{
		public RelatedEntryInstructionGenPivotCollection(CusEntryInstruction master)
			: base(master)
		{
		}

		protected override string RelationType
		{
			get { return GenPivotTypeDecider.Types.RelatedEntryInstructionGenPivot; }
		}
	}
}
