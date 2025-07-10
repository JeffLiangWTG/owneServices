using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface INBHeader
{
	ZString MessageCodeEntry { get; }
	ZString ReferenceNumber { get; }
	ZString DeclarationCIN { get; }
	ZDate DeclarationDate { get; }
	ZInt ItemNumber { get; }
}
