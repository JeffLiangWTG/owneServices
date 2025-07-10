using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class CusFiscalReferenceValidation : EU.Business.Declaration.CusFiscalReferenceValidation
	{
		public CusFiscalReferenceValidation(EU.Business.Declaration.CusFiscalReference parent)
			: base(parent)
		{
		}

		protected override void CheckCFR_Reference()
		{
			base.CheckCFR_Reference();

			var parent = Parent;
			var referenceNumber = parent.CFR_Reference;
			var code = parent.CFR_Code;
			if (!code.IsEmpty && !referenceNumber.IsEmpty)
			{
				var referenceNumberInfo = parent.CFR_ReferenceInfo;

				var country = CountryCodes.Germany;
				var referenceNumberLength = referenceNumber.Length;
				var caption = DataBoundResourceStrings.GetDataForProperty(referenceNumberInfo).Caption;
				if (code == FiscalReferenceCodeList.Codes.FR1 || code == FiscalReferenceCodeList.Codes.FR3)
				{
					if (!referenceNumber.StartsWith(country, StringComparison.OrdinalIgnoreCase))
					{
						referenceNumberInfo.AddMessageError(NumberRequiresPrefix(caption, code, country));
					}

					if (referenceNumberLength != ReferenceExactLengthForTypeFR1AndFR3)
					{
						referenceNumberInfo.AddMessageError(NumberExactLength(caption, ReferenceExactLengthForTypeFR1AndFR3));
					}
				}
				else if (code == FiscalReferenceCodeList.Codes.FR2)
				{
					if (referenceNumber.StartsWith(country, StringComparison.OrdinalIgnoreCase))
					{
						referenceNumberInfo.AddMessageError(Res.GetString("8435356A-921A-43DE-A51B-43063828ABA3", "{0} for Type {1} can't be issued in DE.", caption, code));
					}

					if (referenceNumberLength > ReferenceMaxLengthForTypeFR2)
					{
						referenceNumberInfo.AddMessageError(Res.GetString("7474361A-958E-4E10-8D7A-E639C4C3EFC1", "The max length of '{0}' must be {1}.", caption, ReferenceMaxLengthForTypeFR2));
					}
				}
				else if (code == FiscalReferenceCodeList.Codes.FR5)
				{
					var prefixForFR5 = "IM";
					if (!referenceNumber.StartsWith(prefixForFR5, StringComparison.OrdinalIgnoreCase))
					{
						referenceNumberInfo.AddMessageError(NumberRequiresPrefix(caption, code, prefixForFR5));
					}

					if (referenceNumberLength != ReferenceExactLengthForTypeFR5)
					{
						referenceNumberInfo.AddMessageError(NumberExactLength(caption, ReferenceExactLengthForTypeFR5));
					}
				}
			}
		}
		protected override void CheckCFR_OA_Owner()
		{
			base.CheckCFR_OA_Owner();

			var parent = Parent;
			var code = parent.CFR_Code;

			if (code == FiscalReferenceCodeList.Codes.FR1 || code == FiscalReferenceCodeList.Codes.FR2 || code == FiscalReferenceCodeList.Codes.FR3)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CFR_OA_OwnerInfo);
			}
		}

		ZString NumberRequiresPrefix(ZString caption, ZString type, ZString prefix) => Res.GetString("B883938B-9D5E-4816-8D8B-4892BD253996", "{0} for Code '{1}' must start with '{2}'.", caption, type, prefix);

		ZString NumberExactLength(ZString caption, ZInt lenght) => Res.GetString("74BC1270-E954-471A-A0B8-7F8737A8B1D3", "The length of {0} has to be {1} characters.", caption, lenght);

		const int ReferenceExactLengthForTypeFR1AndFR3 = 11;

		const int ReferenceMaxLengthForTypeFR2 = 14;

		const int ReferenceExactLengthForTypeFR5 = 12;
	}
}
