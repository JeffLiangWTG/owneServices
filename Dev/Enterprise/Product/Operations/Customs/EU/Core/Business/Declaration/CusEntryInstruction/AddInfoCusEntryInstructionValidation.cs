using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class AddInfoCusEntryInstructionValidation : EUAddInfoValidation
	{
		public AddInfoCusEntryInstructionValidation(AddInfoCusEntryInstruction parent) : base(parent)
		{
		}

		protected new AddInfoCusEntryInstruction Parent => (AddInfoCusEntryInstruction)base.Parent;

		protected AddInfoCusEntryInstructionLookups Lookups => Parent.Lookups;

		#region ZG_SealsCount

		protected override void CheckZG_SealsCount()
		{
			base.CheckZG_SealsCount();

			var parent = Parent;
			var entryInstruction = parent.Parent;
			var declaration = entryInstruction.JobDeclaration;
			var entryInstructionConfiguration = declaration?.Configuration?.InstructionConfiguration;

			if (entryInstructionConfiguration?.SealsSupport(declaration) ?? false)
			{
				var targetPropertyInfo = parent.ZG_SealsCountInfo;
				MandatoryValidation.MessageErrorIfIsNegative(targetPropertyInfo);
				CheckSealsCountMatchesIndicatedSealsForTheEntryInstruction(parent, entryInstruction, targetPropertyInfo);
			}
		}

		void CheckSealsCountMatchesIndicatedSealsForTheEntryInstruction(AddInfoCusEntryInstruction addInfoEntryInstruction, CusEntryInstruction entryInstruction, ZPropertyInfo targetPropertyInfo)
		{
			var sealNumbers = entryInstruction.GetEffectiveSealNumbers();
			if (addInfoEntryInstruction.ZG_SealsCount < sealNumbers.Count)
			{
				targetPropertyInfo.AddMessageError(Res.GetString("8ABE641C-2FBE-43AF-BB30-6909D106BF34", "Seals Quantity is less than the sum of distinct seals entered in 'Seals' and 'Containers' tabs."));
			}
		}

		#endregion
	}
}
