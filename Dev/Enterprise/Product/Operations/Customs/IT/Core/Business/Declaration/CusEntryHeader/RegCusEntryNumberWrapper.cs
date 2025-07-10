using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.Business.Declaration;

public class RegCusEntryNumberWrapper
{
	RegCusEntryNumberWrapper(CusEntryNumber entryNumber)
	{
		this.entryNumber = entryNumber;
	}

	readonly CusEntryNumber entryNumber;

	public static RegCusEntryNumberWrapper Load(CusEntryNumber entryNumber) => new RegCusEntryNumberWrapper(entryNumber);

	public ZString Register => EntryNumSplit?.ElementAtOrDefault(0).Left(1) ?? ZString.Empty;
	public ZString RegistrationNumber => EntryNumSplit?.ElementAtOrDefault(1) ?? ZString.Empty;
	public ZString RegistrationNumberWithoutCin => RegistrationNumber.Left(RegistrationNumber.Length - 1);
	public ZString RegistrationNumberCin => RegistrationNumber.Right(1);
	public ZDate IssueDate => entryNumber?.CE_IssueDate.Date ?? ZDate.Empty;
	public ZString Series => EntryNumSplit?.ElementAtOrDefault(0).Trim().Length > 1 ? EntryNumSplit[0].Right(1) : ZString.Empty;
	public ZString RegisterIncludingSeries => FormattableString.Invariant($"{Register} {Series}").Trim();
	public ZString RegistrationNumberIncludingRegisterAndSeries => EntryNum;
	public ZBool IsEmpty => EntryNum.IsEmpty && IssueDate.IsEmpty && CustomsOfficeCode.IsEmpty;

	public ZString CustomsOfficeCode => entryNumber?.CE_EntryLineReference ?? ZString.Empty;
	public ZString CustomsOfficeDescription => customsOfficeDescription ?? (customsOfficeDescription = GetCustomsOfficeDescription());
	string customsOfficeDescription;

	#region Implementation

	ZString[] EntryNumSplit => entryNumSplit ?? (entryNumSplit = EntryNum.Split('-'));
	ZString[] entryNumSplit;

	ZString EntryNum => entryNumber?.CE_EntryNum ?? ZString.Empty;

	string GetCustomsOfficeDescription()
	{
		if (!CustomsOfficeCode.IsEmpty)
		{
			var cusCode = Core.Constants.CountryCodes.Italy + CustomsOfficeCode;
			return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(entryNumber.Factory, cusCode, Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Now)?.ZZD_Description ?? ZString.Empty;
		}
		return ZString.Empty;
	}

	#endregion
}
