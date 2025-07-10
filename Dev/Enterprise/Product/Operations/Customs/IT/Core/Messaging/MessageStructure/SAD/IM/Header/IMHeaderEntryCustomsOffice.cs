using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMHeaderEntryCustomsOffice
{
	public IMHeaderEntryCustomsOffice(IIMHeaderEntryCustomsOffice entryCustomsOffice)
	{
		this.entryCustomsOffice = Argument.NotNull(entryCustomsOffice, nameof(entryCustomsOffice));
	}

	readonly IIMHeaderEntryCustomsOffice entryCustomsOffice;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldImportRules("D", "CN2")]
	public ZString Nationality => entryCustomsOffice.Nationality;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 6, false)]
	[MessageFieldImportRules("D", "CN2")]
	public ZString ReferenceNumber => entryCustomsOffice.ReferenceNumber;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 30, false)]
	[MessageFieldImportRules("D", "CN2a")]
	public ZString Name => entryCustomsOffice.Name;
}
