using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class IMHeaderEntryCustomsOfficeWrapper : IIMHeaderEntryCustomsOffice
{
	public IMHeaderEntryCustomsOfficeWrapper(JobDeclaration jobDeclaration)
	{
		this.jobDeclaration = Argument.NotNull(jobDeclaration, nameof(jobDeclaration));
	}
	readonly JobDeclaration jobDeclaration;

	EuOfficeCode EntryCustomsOffice => jobDeclaration.CustomsOffices?.Find(office => office.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent).FirstOrDefault();

	ZString EntryCustomsOfficeCode => EntryCustomsOffice?.CY_Data ?? ZString.Empty;

	public ZString Nationality => SADWrapperHelper.IsItalianCustomsOffice(EntryCustomsOfficeCode) ? (ZString)Core.Constants.CountryCodes.Italy : ZString.Empty;

	public ZString ReferenceNumber => SADWrapperHelper.RemoveIsoCodeIfIsItalianCustomsOffice(EntryCustomsOfficeCode);

	public ZString Name => EntryCustomsOffice?.CY_OfficeDescription.Left(SADConstants.CustomsFieldMaxLength.EntryCustomsOffice.Name) ?? ZString.Empty;
}
