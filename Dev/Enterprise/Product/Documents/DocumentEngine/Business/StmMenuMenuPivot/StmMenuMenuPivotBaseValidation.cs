using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Business
{
	public class StmMenuMenuPivotBaseValidation : StmMenuMenuPivotValidation
	{
		public StmMenuMenuPivotBaseValidation(StmMenuMenuPivotBase parent)
			: base(parent)
		{
		}

		protected override void CheckSF_SU_Inward()
		{
			ValidateIfInitialized(Parent.SF_SU_InwardInfo);
		}
		protected override void CheckSF_SU_Outward()
		{
			ValidateIfInitialized(Parent.SF_SU_OutwardInfo);
		}

		void ValidateIfInitialized(ZPropertyInfo property)
		{
			Validate(property);
		}

		void Validate(ZPropertyInfo property)
		{
			base.CheckSF_SU_Inward();
			var collection = new StmMenuMenuPivotBaseCollection(Parent.Factory);
			collection.Load(new ZQuery(StmMenuMenuPivotSchema.SF_SU_Inward, Parent.SF_SU_Inward));

			foreach (var other in collection)
			{
				var otherPivot = (AutoStmMenuMenuPivot)other;
				if (Parent.SF_SU_Outward == otherPivot.SF_SU_Outward &&
					Parent.SF_OverriddenBusinessContext == otherPivot.SF_OverriddenBusinessContext &&
					Parent.PK != otherPivot.PK)
				{
					property.AddError(Res.GetString("B7B59B44-9EF3-48BC-B1F3-E0EAE9790BF8", "Each document can only be used once"));
				}
			}
		}

		protected override void CheckSF_Filter()
		{
			base.CheckSF_Filter();

			if (!Parent.SF_Filter.IsEmpty)
			{
				string message;
				if (!ZExpressionEvaluator.IsValidOtherDocumentOrEDocsFilterName(Parent.SF_Filter, out message))
				{
					Parent.SF_FilterInfo.AddError(message);
				}
			}
		}
	}
}
