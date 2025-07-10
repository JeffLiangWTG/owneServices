using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business.FetchStrategies
{
	public class CusEntryInstructionFetchStrategy : Customs.Business.FetchStrategies.CusEntryInstructionFetchStrategy
	{
		public CusEntryInstructionFetchStrategy(CusEntryInstruction instruction)
			: base(instruction)
		{
		}

		protected new CusEntryInstruction BusinessObject => (CusEntryInstruction)base.BusinessObject;

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(CusCNEntryInstructionSchema.CNE_CEI, BusinessObject.PK);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			AddFetchHints(Factory);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			AddFetchHints(Factory);
		}

		void AddFetchHints(BusinessObjectFactory factory)
		{
			factory.AddFetchHint(CusStorageDocPivotSchema.CSD_ParentID, BusinessObject.PK);
			factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
			factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, BusinessObject.PK);
		}
	}
}
