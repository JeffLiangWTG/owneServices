using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETHeaderControlResult
{
	public ETHeaderControlResult(IETHeaderControlResult iETHeaderControlResult)
	{
		this.iETHeaderControlResult = Argument.NotNull(iETHeaderControlResult, "iETHeaderControlResult");
	}
	readonly IETHeaderControlResult iETHeaderControlResult;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 2, false)]
	public ZString Code => ZString.Empty;

	[MessageLayout(Order = 1)]
	[MessageFieldDateDDMMYYYYRepresentation]
	[MessageFieldExportWithTransitRules("O", "CN93")]
	[MessageFieldTransitRules("O", "CN93")]
	[MessageFieldInternationalRoadTransportsRules("O", "CN93")]
	public ZDate DateLimitOfArrivalNotification => iETHeaderControlResult.DateLimitOfArrivalNotification;

	[MessageLayout(Order = 2)]
	[MessageFieldDateDDMMYYYYRepresentation]
	[MessageFieldExportRules("O", "CN94")]
	public ZDate DateLimitForTheExitFromEC => iETHeaderControlResult.DateLimitForTheExitFromEC;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	public ZString DeclarationPlace => ZString.Empty;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	public ZString DeclarationPlaceLng => ZString.Empty;
}
