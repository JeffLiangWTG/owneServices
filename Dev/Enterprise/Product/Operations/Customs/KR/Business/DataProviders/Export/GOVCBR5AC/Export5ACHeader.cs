using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Export5ACHeader : ExtendedOfficeHoursHeader<Export5ACEntry>, IExport5ACHeader
	{
		ZString IExtendedOfficeHoursHeader<IExport5ACEntry>.ApplicationNumber => ApplicationNumber;

		ZDateTime IExtendedOfficeHoursHeader<IExport5ACEntry>.StartDateTime => StartDateTime;

		ZDateTime IExtendedOfficeHoursHeader<IExport5ACEntry>.EndDateTime => EndDateTime;

		ZString IExtendedOfficeHoursHeader<IExport5ACEntry>.DeclarationCustomsOffice => DeclarationCustomsOffice;

		ZString IExtendedOfficeHoursHeader<IExport5ACEntry>.DeclarationCustomsDivision => DeclarationCustomsDivision;

		IOrganization IExtendedOfficeHoursHeader<IExport5ACEntry>.Declarant => Declarant;

		ZString IExtendedOfficeHoursHeader<IExport5ACEntry>.ApplicationReason => ApplicationReason;

		IEnumerable<IExport5ACEntry> IExtendedOfficeHoursHeader<IExport5ACEntry>.Entries => Entries;
	}
}
