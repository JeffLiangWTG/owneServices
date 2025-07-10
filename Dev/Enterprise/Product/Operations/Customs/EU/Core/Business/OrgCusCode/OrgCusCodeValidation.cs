using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business
{
	public class OrgCusCodeValidation : Enterprise.MasterFiles.Business.OrgCusCodeValidation, Integration.Customs.EU.IOrgCusCodeValidation
	{
		public OrgCusCodeValidation(OrgCusCode parent)
			: base(parent)
		{
		}

		protected override void CheckOK_OA_PremisesAddress()
		{
			base.CheckOK_OA_PremisesAddress();
			if (Parent.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID || (Parent.OK_CodeType == OrgCusCode.CodeTypes.CustomsOfficeForTransit && MultipleOrgCusCodesWithSameCodeTypeExists(OrgCusCode.CodeTypes.CustomsOfficeForTransit)))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.OK_OA_PremisesAddressInfo);
			}
		}

		ZBool MultipleOrgCusCodesWithSameCodeTypeExists(ZString codeType)
		{
			var query = new ZQuery(OrgCusCodeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			query.AddToFilter(OrgCusCodeSchema.OK_CodeType, codeType);
			return Parent.Header.CustomsCodes.Find(query).Any();
		}

		protected override void CheckOK_CustomsRegNo()
		{
			base.CheckOK_CustomsRegNo();
			var customsRegNo = Parent.OK_CustomsRegNo;

			if (!customsRegNo.IsEmpty)
			{
				var codeType = Parent.OK_CodeType;
				switch (codeType)
				{
					case OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID:
						var query = new ZQuery(OrgCusCodeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
						query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, customsRegNo);
						query.AddToFilter(OrgCusCodeSchema.OK_CodeType, codeType);
						if (Parent.Header.CustomsCodes.Find(query).Any())
						{
							Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("A861D3A3-29C8-4D87-8CBD-DFA738D09627", "Each 'TID' must have a unique code."));
						}
						break;
					case OrgCusCode.EuropeanUnionSharedCodeTypes.ImportOneStopShopVatRegistration:
						var pattern = $@"^IM{Parent.CodeCountry.RN_IsoNumericUNM49Code}[0-9]{{7}}$";
						if (!Regex.IsMatch(customsRegNo, pattern))
						{
							Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("F47C8F33-B24D-42C4-ACE4-27E09E488A81", "The format should be 'IMxxxyyyyyyz'. Where 'xxx' is the IOS three letter numeric code, 'yyyyyy' is the 6-digit number allocated by the member country, and 'z' is the check digit."));
						}
						break;
				}
			}
		}
	}
}
