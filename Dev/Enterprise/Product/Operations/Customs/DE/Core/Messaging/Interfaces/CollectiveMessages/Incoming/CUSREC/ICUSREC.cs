using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICUSREC : IDataProvider
	{
		ZString ReferencedMessageIdentifier { get; }

		string ReferenceNumber { get; }

		string MRN { get; }

		ZString LocalReferenceNumber { get; }

		ZDate RegistrationDate { get; }

		IReadOnlyCollection<ZString> NotificationSeverity { get; }

		IReadOnlyCollection<ICUSRECGoodsItem> GoodsItems { get; }
	}
}
