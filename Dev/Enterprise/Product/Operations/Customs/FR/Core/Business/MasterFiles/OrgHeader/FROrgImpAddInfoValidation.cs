//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoFROrgImpAddInfoValidation
//
//    This class should be used for overriding validation in AutoFROrgImpAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MasterFiles
{
	public class FROrgImpAddInfoValidation : AutoFROrgImpAddInfoValidation
	{
		public FROrgImpAddInfoValidation(AutoFROrgImpAddInfo parent) : base(parent)
		{
		}

		protected override void CheckZO_DeltaG1SubProcedure()
		{
			base.CheckZO_DeltaG1SubProcedure();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_DeltaG1SubProcedureInfo);
			var deltaAgreementNumberCollection = ((FROrgImpAddInfo)Parent).OrgHeader?.DeltaAgreementNumberCollection;
			if (Parent.ZO_DeltaG1SubProcedure.IsEmpty && deltaAgreementNumberCollection != null && deltaAgreementNumberCollection.Cast<OrgCusAccount>().Any(account => account.CZ_Type == OrgCusAccountDeltaGTypeList.Codes.G1))
			{
				Parent.ZO_DeltaG1SubProcedureInfo.AddMessageError(Res.GetString("C4F8E5BB-4772-499B-ADE5-B1BFC886DA85", "This value is mandatory when Delta Mode G1 is selected."));
			}
		}

		protected override void CheckZO_VATDeferType()
		{
			base.CheckZO_VATDeferType();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_VATDeferTypeInfo);
		}

		protected override void CheckZO_VATProcedureDateLimit()
		{
			base.CheckZO_VATProcedureDateLimit();
			if (Parent.ZO_VATDeferType == VATProcedureList.Codes.L)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ZO_VATProcedureDateLimitInfo);
			}
			if (Parent.ZO_VATProcedureDateLimit.IsInTheFuture())
			{
				Parent.ZO_VATProcedureDateLimitInfo.AddMessageError(Res.GetString("1FE575F4-8355-42F5-8DD0-256F3A25832E", "Date should be in the past."));
			}
		}
	}
}
