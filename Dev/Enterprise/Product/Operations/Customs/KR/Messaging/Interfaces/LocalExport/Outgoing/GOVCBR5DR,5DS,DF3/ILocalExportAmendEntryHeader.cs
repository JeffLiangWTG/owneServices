using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface ILocalExportAmendEntryHeader : IMessageDataProvider
	{
		ZString DeclarationNumber { get; }
		ZString CustomsReceiptNumber { get; }
		ZString DeclarationCustomsOfficeAndDivision { get; }
		ZDateTime LoadingDate { get; }
		IOrganization Supplier { get; }
		IEnumerable<ILocalExportAmendItem> AmendedItems { get; }
		IEnumerable<ILocalExportStevedore> Stevedores { get; }
	}
}
