using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETLinePackage
{
	public ETLinePackage(IPackage iPackage)
	{
		this.iPackage = Argument.NotNull(iPackage, "iPackage");
	}
	readonly IPackage iPackage;

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(5, false)]
	[MessageFieldExportRules("D", "C60", "R020", "RN22")]
	[MessageFieldExportWithTransitRules("D", "C60", "R021", "RN22")]
	[MessageFieldTransitRules("D", "C60", "R021", "RN22")]
	[MessageFieldInternationalRoadTransportsRules("D", "C60", "R021", "RN22")]
	public ZInt? NumberOfPacks => iPackage.NumberOfPacks;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 42, false)]
	[MessageFieldExportRules("D", "C60")]
	[MessageFieldExportWithTransitRules("D", "C60")]
	[MessageFieldTransitRules("D", "C60")]
	[MessageFieldInternationalRoadTransportsRules("D", "C60")]
	public ZString MarksAndNumbers => iPackage.MarksAndNumbers;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	public ZString MarksAndNumbersLng => ZString.Empty;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 3, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString PackageType => iPackage.PackageType;

	[MessageLayout(Order = 4)]
	[MessageFieldIntegerRepresentation(5, false)]
	[MessageFieldExportRules("D", "C60")]
	[MessageFieldExportWithTransitRules("D", "C60")]
	[MessageFieldTransitRules("D", "C60")]
	[MessageFieldInternationalRoadTransportsRules("D", "C60")]
	public ZInt? NumberOfPieces => iPackage.NumberOfPieces;
}
