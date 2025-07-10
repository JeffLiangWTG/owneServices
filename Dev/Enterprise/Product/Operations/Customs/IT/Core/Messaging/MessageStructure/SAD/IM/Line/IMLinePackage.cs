using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMLinePackage
{
	public IMLinePackage(IPackage package)
	{
		this.package = Argument.NotNull(package, "package");
	}

	readonly IPackage package;

	[MessageLayout(Order = 0)]
	[MessageFieldImportRules("D", "C60")]
	[MessageFieldDepositoRules("D", "C60")]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 42, false)]
	public ZString MarksAndNumbers => package.MarksAndNumbers;

	[MessageLayout(Order = 1)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 3, false)]
	public ZString PackageType => package.PackageType;

	[MessageLayout(Order = 2)]
	[MessageFieldImportRules("D", "C60")]
	[MessageFieldDepositoRules("D", "C60")]
	[MessageFieldIntegerRepresentation(3, false)]
	public ZInt? NumberOfPieces => package.NumberOfPieces;
}
