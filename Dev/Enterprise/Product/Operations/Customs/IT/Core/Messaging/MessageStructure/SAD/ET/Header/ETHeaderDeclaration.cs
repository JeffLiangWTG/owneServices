using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETHeaderDeclaration : IDeclaration
{
	readonly IDeclaration iDeclaration;

	public ETHeaderDeclaration(IDeclaration iDeclaration)
	{
		this.iDeclaration = Argument.NotNull(iDeclaration, "iDeclaration");
	}

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 3, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	public ZString TypeDeclarationSubType1 => iDeclaration.TypeDeclarationSubType1;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	public ZString TypeDeclarationSubType2 => iDeclaration.TypeDeclarationSubType2;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 5, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R", "RN5")]
	public ZString TypeDeclarationSubType3 => iDeclaration.TypeDeclarationSubType3;
}
