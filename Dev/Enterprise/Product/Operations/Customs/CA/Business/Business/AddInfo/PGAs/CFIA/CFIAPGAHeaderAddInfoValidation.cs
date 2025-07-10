//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCFIAPGAHeaderAddInfoValidation
//
//    This class should be used for overriding validation in AutoCFIAPGAHeaderAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Customs.CA.Business
{
	public class CFIAPGAHeaderAddInfoValidation : AutoCFIAPGAHeaderAddInfoValidation
	{
		public CFIAPGAHeaderAddInfoValidation(AutoCFIAPGAHeaderAddInfo parent) : base(parent)
		{
		}

		protected new CFIAPGAHeaderAddInfo Parent => (CFIAPGAHeaderAddInfo)base.Parent;

		internal void CheckCFIAAccountNumber(ZPropertyInfo info)
		{
			var invoiceLine = ((Parent.Parent as CFIAPGAHeader).InvoiceLine);
			if (invoiceLine?.Declaration?.IsCFIAAccountNumberRequired ?? false)
			{
				info.AddWarning(OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnOrgProxyErrorText);
			}
		}

		#region AIRS details validation

		protected override void CheckCA_AIRSEndUse()
		{
			base.CheckCA_AIRSEndUse();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_AIRSEndUseInfo, Parent.Lookups.EndUseCodes);
		}

		protected override void CheckCA_AIRSMiscellaneous()
		{
			base.CheckCA_AIRSMiscellaneous();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_AIRSMiscellaneousInfo, (ICodeDescriptionPairList)Parent.Lookups.AirsMiscellaneous);
		}

		#endregion

	}
}
