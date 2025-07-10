using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.Business.Declaration;

public class ExitAndReleaseCusEntryNumberWrapper
{
	ExitAndReleaseCusEntryNumberWrapper(CusEntryNumber entryNumber)
	{
		this.entryNumber = entryNumber;
	}

	readonly CusEntryNumber entryNumber;
	public static ExitAndReleaseCusEntryNumberWrapper Load(CusEntryNumber entryNumber) => new ExitAndReleaseCusEntryNumberWrapper(entryNumber);

	public ZDateTime Date => entryNumber?.CE_IssueDate ?? ZDate.Empty;
	public ZString Office => entryNumber?.CE_EntryLineReference ?? ZString.Empty;
	public ZString Status => entryNumber?.CE_EntryStatus ?? ZString.Empty;
	public ZString StatusDescription => GetStatusDescription();
	public ZString OfficeDescription => officeDescription ?? (officeDescription = GetOfficeDescription());
	string officeDescription;

	ZString GetOfficeDescription()
	{
		if (entryNumber != null)
		{
			var office = Office;
			var date = Date;
			var country = office.SubstringSafe(0, 2);
			return office.IsEmpty ? ZString.Empty : ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(entryNumber.Factory, office, country, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, date.IsValid ? date : ZDateTime.Today)?.ZZD_Description ?? ZString.Empty;
		}
		else
		{
			return ZString.Empty;
		}
	}

	ZString GetStatusDescription()
	{
		if (entryNumber != null)
		{
			var date = Date;
			var status = Status;
			return status.IsEmpty ? string.Empty : RefCusCodeListTypes.GetCachedList(entryNumber.Factory, Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, date.IsValid ? date : ZDateTime.Today).GetDescriptionFromCode(status);
		}
		else
		{
			return ZString.Empty;
		}
	}
}
