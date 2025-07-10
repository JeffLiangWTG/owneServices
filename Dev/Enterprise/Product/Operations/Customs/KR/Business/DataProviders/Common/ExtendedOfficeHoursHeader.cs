using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ExtendedOfficeHoursHeader<T> : IExtendedOfficeHoursHeader<T>
		where T : IExtendedOfficeHoursEntry
	{
		public string ApplicationNumber { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public string DeclarationCustomsOffice { get; set; }
		public string DeclarationCustomsDivision { get; set; }
		public Organisation Declarant { get; set; }
		public string ApplicationReason { get; set; }
		public T[] Entries { get; set; }

		ZString IExtendedOfficeHoursHeader<T>.ApplicationNumber => ApplicationNumber;

		ZDateTime IExtendedOfficeHoursHeader<T>.StartDateTime => StartDateTime;

		ZDateTime IExtendedOfficeHoursHeader<T>.EndDateTime => EndDateTime;

		ZString IExtendedOfficeHoursHeader<T>.DeclarationCustomsOffice => DeclarationCustomsOffice;

		ZString IExtendedOfficeHoursHeader<T>.DeclarationCustomsDivision => DeclarationCustomsDivision;

		IOrganization IExtendedOfficeHoursHeader<T>.Declarant => Declarant;

		ZString IExtendedOfficeHoursHeader<T>.ApplicationReason => ApplicationReason;

		IEnumerable<T> IExtendedOfficeHoursHeader<T>.Entries => Entries;
	}
}
