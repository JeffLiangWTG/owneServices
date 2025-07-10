using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class OfficeCodeValidation : EuOfficeCodeValidation
	{
		public OfficeCodeValidation(OfficeCode parent) : base(parent)
		{
		}

		protected new OfficeCode Parent => (OfficeCode)base.Parent;

		protected EMCSJobDeclaration Declaration => (EMCSJobDeclaration)Parent.Parent;

		protected override void CheckCY_Code()
		{
			base.CheckCY_Code();

			var sourceValue = Parent.CY_Code;
			var targetInfo = Parent.CY_CodeInfo;
			if (!sourceValue.IsEmpty)
			{
				var atLeastOneCADOfficeIsRequiredMessage = Res.GetString("b08335b5-6c59-4b14-be74-6ef4d056d6a3", "At least one office of type {0} is required", OfficeCodes_EMCS.Codes.CompetentAuthorityOfDispatch);
				CheckAtLeastOneOfficeTypeExists(OfficeCodes_EMCS.Codes.CompetentAuthorityOfDispatch, atLeastOneCADOfficeIsRequiredMessage, sourceValue, targetInfo);
				if (!Parent.OfficeCodesUseDesInsteadOfCaa && Declaration.JE_DeclarantType == EMCSEntryTypeList.Codes.Consignee)
				{
					var atLeastOneCAAOfficeIsRequiredMessage = Res.GetString("9d5603aa-a17e-4b84-9efd-8a0574209aad", "At least one office of type {0} is required", OfficeCodes_EMCS.Codes.CompetentAuthorityOfArrival);
					CheckAtLeastOneOfficeTypeExists(OfficeCodes_EMCS.Codes.CompetentAuthorityOfArrival, atLeastOneCAAOfficeIsRequiredMessage, sourceValue, targetInfo);
				}
			}
		}

		void CheckAtLeastOneOfficeTypeExists(string code, string messageError, string sourceValue, ZPropertyInfo targetInfo)
		{
			if (sourceValue != code)
			{
				var hasCode = OfficeCodeProvider.CustomsOffices.Any(office => office.CY_Code == code);
				if (!hasCode)
				{
					targetInfo.AddMessageError(messageError);
				}
			}
		}
	}
}
