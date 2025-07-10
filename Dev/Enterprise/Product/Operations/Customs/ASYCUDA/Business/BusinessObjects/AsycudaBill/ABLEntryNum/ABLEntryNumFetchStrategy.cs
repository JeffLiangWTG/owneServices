using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class ABLEntryNumFetchStrategy : AsycudaFetchStrategy
	{
		public ABLEntryNumFetchStrategy(ABLEntryNum ablEntryNum)
			: base(ablEntryNum)
		{
		}

		new protected ABLEntryNum BusinessObject => (ABLEntryNum)base.BusinessObject;

		protected override void AddFetchHintsForDeleteCore()
		{
			base.AddFetchHintsForDeleteCore();
			Factory.AddFetchHint(typeof(GenPivot), GenPivotSchema.XX_Relation1ID, BusinessObject.PK);
		}

		protected override void AddFetchHintsForLoadChildEditableObjectsCore()
		{
			base.AddFetchHintsForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(typeof(GenPivot), GenPivotSchema.XX_Relation1ID, BusinessObject.PK);
		}

		protected override void AddFetchHintsForValidateCore()
		{
			base.AddFetchHintsForValidateCore();
			if (BusinessObject.Bill?.Header?.SupportAssociatedPacks ?? false)
			{
				Factory.AddFetchHint(typeof(GenPivot), GenPivotSchema.XX_Relation1ID, BusinessObject.PK);
			}
		}
	}
}
