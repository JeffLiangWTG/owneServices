using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CDSFECChallengeValidation : CusCodeDataValidation
	{
		public CDSFECChallengeValidation(CusCodeData parent) : base(parent) { }

		protected FECChallenge fecChallenge => (FECChallenge)base.Parent;

		protected override void CheckCY_IsOverridden()
		{
			base.CheckCY_IsOverridden();
			if (!fecChallenge.CY_IsOverridden && !fecChallenge.IsNewValueInputed && Parent != null)
			{
				Parent.CY_IsOverriddenInfo.AddWarning(fecChallengeMustBeDealt);
			}
		}

		protected override void CheckCY_Type()
		{
		}

		protected override void CheckCY_Code()
		{
		}

		#region Error messages

		readonly ResourceString fecChallengeMustBeDealt = ResString.GetMultilingualString("3F2AA827-45F6-4B7D-B3FC-C357E7218CD3",
			"A FEC challenge has been received. You should either tick the confirm box or change the value. " +
			"When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly. ");

		#endregion
	}
}
