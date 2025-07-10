using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMHeaderDeclaration
{
	public IMHeaderDeclaration(IDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}

	readonly IDeclaration declaration;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 3, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString TypeDeclarationSubType1 => declaration.TypeDeclarationSubType1;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, true)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString TypeDeclarationSubType2 => declaration.TypeDeclarationSubType2;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 5, false)]
	public ZString TypeDeclarationSubType3 => declaration.TypeDeclarationSubType3;
}
