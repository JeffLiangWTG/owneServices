using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public static class CusEntryNumExtensionMethods
	{
		public static void Generate5ULEntryNumber(this CusEntryNumber entryNumber, ZGuid companyPK)
		{
			ZString uniPASSDeclarantID = KRCustomsRegistry.Instance.UNIPASSDeclarantID.GetValueWithoutFallback(companyPK.ToGuid(), Guid.Empty, Guid.Empty);
			if (!uniPASSDeclarantID.IsEmpty && entryNumber != null && entryNumber.CE_EntryType == ElectronicDocumentTypeList.Codes._5UL)
			{
				var messageInterchange = Enterprise.Core.Constants.CountryCodes.KoreaSouth + ElectronicDocumentTypeList.Codes._5UL;
				var year2digit = ZDate.Today.ToString(Constants.DateFormatType.Year).Substring(2, 2);
				var maxParamValue = NumberFountainMaxValues._5digit;
				var formatDigit = Constants.NumberFormatDigit.D5;
				var checkDigit = Constants.EntryNumberCheckDigit.U;

				var fountainNumber = Env.NumberFountains.KREntryNumberFountain(messageInterchange, uniPASSDeclarantID, year2digit, maxParamValue).GetNextFormatted(entryNumber.Factory);
				var formattedNumber = ZInt.ParseSafe(fountainNumber, 0).ToString(formatDigit);

				entryNumber.CE_EntryNum = uniPASSDeclarantID + year2digit + formattedNumber + checkDigit;
			}
		}
	}
}
