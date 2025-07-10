using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IExtendedOfficeHoursHeader<T> : IMessageDataProvider
		where T : IExtendedOfficeHoursEntry
	{
		ZString ApplicationNumber { get; }
		ZDateTime StartDateTime { get; }
		ZDateTime EndDateTime { get; }
		ZString DeclarationCustomsOffice { get; }
		ZString DeclarationCustomsDivision { get; }
		IOrganization Declarant { get; }
		ZString ApplicationReason { get; }
		IEnumerable<T> Entries { get; }
	}
}
