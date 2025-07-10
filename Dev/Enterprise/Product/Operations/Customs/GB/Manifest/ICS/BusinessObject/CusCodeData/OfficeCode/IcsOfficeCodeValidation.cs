using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Manifest.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.GB.ICS.Business
{
	public class IcsOfficeCodeValidation : EU.Manifest.Business.IcsOfficeCodeValidation
	{
		public IcsOfficeCodeValidation(IcsOfficeCode parent) : base(parent)
		{
		}

		protected override void CheckCY_Data()
		{
			var factory = Parent.Factory;
			var sourceValue = Parent.CY_Data;
			var targetInfo = Parent.CY_DataInfo;

			if (sourceValue.IsEmpty || sourceValue.Left(2) != Core.Constants.CountryCodes.UnitedKingdom)
			{
				targetInfo.AddNotification(NotificationTypeForEmptyCY_Data, Res.GetString("0BDF779C-7E4C-44B3-892F-D9C09358A0B0", "A valid GB office code is needed. Example: GB000010."));
			}
			else
			{
				var roles = Parent.CY_RoleCodes;
				var office = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, sourceValue, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today);
				if (office == null)
				{
					targetInfo.AddMessageError(ErrorMessageForInvalidCY_Data);
				}
				else if (roles != null && roles.Any((ZString x) => !office.HasAttribute(RefCusCodeListAttributeTypes.Codes.ROLE, x)))
				{
					targetInfo.AddMessageError(Res.GetString("9C194F01-FDCD-4B33-80FE-C6973DAD28B9", "According to reference data, this office does not fulfill this role"));
				}
			}
		}

		protected override void CheckCY_Date()
		{
			base.CheckCY_Date();
			if (Parent.CY_Code == OfficeCodes_ICS.Codes.OfficeOfFirstEntry && Parent.CY_Date.IsEmpty)
			{
				Parent.CY_DateInfo.AddMessageError(Res.GetString("BB23778D-69F6-4B65-8201-C8A8EC6E87B6", "The scheduled date and time of arrival of the means of transport at the Office of First Entry is required."));
			}
		}
	}
}
