using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class FECChallengeValidation : CusCodeDataValidation
	{
		public FECChallengeValidation(FECChallenge parent) : base(parent) { }

		protected FECChallenge fecChallenge => (FECChallenge)base.Parent;

		protected override void CheckCY_IsOverridden()
		{
			base.CheckCY_IsOverridden();
			if (!fecChallenge.CY_IsOverridden && !fecChallenge.IsNewValueInputed && Parent != null)
			{
				JobDeclaration parentDec = fecChallenge.IsParentEntryHeader ? ((CusEntryHeader)(fecChallenge.Parent)).Declaration : ((CusEntryLine)(fecChallenge.Parent)).Header.Declaration;

				if (parentDec != null && !parentDec.JE_RouteFRequested)
				{
					Parent.CY_IsOverriddenInfo.AddMessageError(fecChallengeMustBeDealt);
				}
			}
		}

		protected override void CheckCY_Type()
		{
		}

		protected override void CheckCY_Code()
		{
		}

		#region Error Messages

		readonly ResourceString fecChallengeMustBeDealt = ResString.GetMultilingualString("CF3980F3-8C93-464B-87BA-6CA3A008BE1F", "A FEC challenge has been received. " +
			"You should either tick the confirm box or change the value or tick 'request route F' on the Misc tab. " +
			"When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly. ");

		#endregion
	}
}
