using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DrawbackAddInfoDeclarationValidation : AUAddInfoValidation
	{
		public DrawbackAddInfoDeclarationValidation(AUAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckZA_DARC_Hidden()
		{
			base.CheckZA_DARC_Hidden();
			JobDeclaration jobDeclaration = this.JobDeclaration;
			ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_DARC_HiddenInfo, AddInfo.Lookups.DrawbackAmberCodeList);
			if (!jobDeclaration.JE_AmberStatement.IsEmpty && (AddInfo.ZA_DARC_Hidden.IsEmpty && !jobDeclaration.InvoiceLines.HasADrawbackAmberReason))
			{
				AddInfo.ZA_DARC_HiddenInfo.AddMessageError("There is no amber reason type for Header or Line and thus, the amber statement is not required.");
			}
			if (!AddInfo.ZA_DARC_Hidden.IsEmpty && jobDeclaration.InvoiceLines.HasADrawbackAmberReason)
			{
				AddInfo.ZA_DARC_HiddenInfo.AddMessageError("Amber reason type may not be specified at both Header and Line.");
			}
			((JobDeclarationValidation)jobDeclaration.Validation).ValidateJE_AmberStatement();
		}

		protected override void CheckZA_DAM_Hidden()
		{
			base.CheckZA_DAM_Hidden();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(AddInfo.ZA_DAM_HiddenInfo, AddInfo.Lookups.ZA_DAM_List);
		}

		protected override void CheckZA_PRF()
		{
		}

		protected override void CheckZA_EDN_Hidden()
		{
			base.CheckZA_EDN_Hidden();
			if (!Parent.ZA_EDN_Hidden.IsEmpty)
			{
				new Common.AU.CMR.CANValidation().ValidateCANField(Parent.ZA_EDN_HiddenInfo);
			}
		}
	}
}
