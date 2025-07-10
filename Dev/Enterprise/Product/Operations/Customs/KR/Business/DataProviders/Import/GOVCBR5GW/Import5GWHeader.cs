using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import5GWHeader : ExtendedOfficeHoursHeader<Import5GWEntry>, IImport5GWHeader
	{
		ZString IExtendedOfficeHoursHeader<IImport5GWEntry>.ApplicationNumber => ApplicationNumber;

		ZDateTime IExtendedOfficeHoursHeader<IImport5GWEntry>.StartDateTime => StartDateTime;

		ZDateTime IExtendedOfficeHoursHeader<IImport5GWEntry>.EndDateTime => EndDateTime;

		ZString IExtendedOfficeHoursHeader<IImport5GWEntry>.DeclarationCustomsOffice => DeclarationCustomsOffice;

		ZString IExtendedOfficeHoursHeader<IImport5GWEntry>.DeclarationCustomsDivision => DeclarationCustomsDivision;

		IOrganization IExtendedOfficeHoursHeader<IImport5GWEntry>.Declarant => Declarant;

		ZString IExtendedOfficeHoursHeader<IImport5GWEntry>.ApplicationReason => ApplicationReason;

		IEnumerable<IImport5GWEntry> IExtendedOfficeHoursHeader<IImport5GWEntry>.Entries => Entries;
	}
}
