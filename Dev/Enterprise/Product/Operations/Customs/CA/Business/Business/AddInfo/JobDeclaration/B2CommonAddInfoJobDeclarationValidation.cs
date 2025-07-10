using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class B2CommonAddInfoJobDeclarationValidation : AddInfoJobDeclarationValidation
	{
		public B2CommonAddInfoJobDeclarationValidation(AddInfoJobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckCA_OriginalTransactionNo()
		{
			base.CheckCA_OriginalTransactionNo();
			var declaration = Parent.Parent;
			if (declaration != null && (declaration.IsB2Adjustments || declaration.IsB3X) && !declaration.IsBlanketB2)
			{
				MandatoryValidation.CheckEntered(Parent.CA_OriginalTransactionNoInfo);
			}
		}

		protected override void CheckCA_AmendmentTo()
		{
			base.CheckCA_AmendmentTo();
			if (Parent.CA_AmendmentTo.IsEmpty)
			{
				Parent.CA_AmendmentToInfo.AddError(Res.GetString("837A1A39-6EDE-48F8-A5AF-603754D479ED", "Please signify if You are adjusting and original B3 or a B2/B3X that has already adjusted the B3 by selecting one of those two options."));
			}
		}
	}
}
