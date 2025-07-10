using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public class CusCNEntryInstructionValidation : AutoCusCNEntryInstructionValidation
	{
		public CusCNEntryInstructionValidation(AutoCusCNEntryInstruction parent) : base(parent) { }

		internal IValidationModeProvider ValidationModeProvider => (Parent as CusCNEntryInstruction).EntryInstruction?.JobDeclaration;

		protected override void CheckCNE_CEI()
		{
			base.CheckCNE_CEI();
			var parentID = Parent.CNE_CEI;
			if (parentID.IsValid && !Parent.IsInDatabase)
			{
				var query = new ZQuery(CusCNEntryInstructionSchema.CNE_CEI, parentID);
				query.AddToFilter(CusCNEntryInstructionSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				if (Parent.Factory.ExistsInDatabase(CusCNEntryInstructionSchema.Constants.TableName, query))
				{
					Parent.CNE_CEIInfo.AddError(Res.GetString("FE58705C-825E-4DA8-A816-EB9E9CE39A1E", "There is another record linked to the same Entry Instruction."));
				}
			}
		}

		protected override void CheckCNE_TransitionSite()
		{
			base.CheckCNE_TransitionSite();

			var targetInfo = Parent.CNE_TransitionSiteInfo;
			if (Parent.CNE_ApplyForTransition)
			{
				targetInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);
			}
			else if (!Parent.CNE_TransitionSite.IsEmpty)
			{
				targetInfo.AddNotification(Res.GetString("43B5055F-4AB7-4ACC-ADF9-E9CE3B948ECB", "Transition Site is not required when Apply for Transition is unchecked."), ValidationModeProvider);
			}
		}
	}
}
