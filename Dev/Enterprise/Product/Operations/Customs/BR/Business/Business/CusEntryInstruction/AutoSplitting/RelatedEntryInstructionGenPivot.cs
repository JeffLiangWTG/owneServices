using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class RelatedEntryInstructionGenPivot : CustomsGenPivot, Integration.Customs.BR.IRelatedEntryInstructionGenPivot
	{
		public RelatedEntryInstructionGenPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			XX_RelationType = GenPivotTypeDecider.Types.RelatedEntryInstructionGenPivot;
			XX_Relation1TableCode = CusEntryInstructionSchema.Constants.Prefix;
			XX_Relation2TableCode = CusEntryInstructionSchema.Constants.Prefix;
		}

		public CusEntryInstruction ParentEntryInstruction => Relation2Object as CusEntryInstruction;
	}
}
