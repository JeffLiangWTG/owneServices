using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class NBPreviousOperationInfo
{
	public NBPreviousOperationInfo(IPreviousOperationInfo previousOperationInfo)
	{
		this.previousOperationInfo = previousOperationInfo;
	}

	public NBPreviousOperationInfo()
	{
	}

	readonly IPreviousOperationInfo previousOperationInfo;

	[MessageLayout(Order = 0)]
	public NBPreviousAdministrativeReference PreviousAllibrament => new NBPreviousAdministrativeReference(previousOperationInfo?.PreviousAllibrament);

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 18, false)]
	public ZString MRN => previousOperationInfo?.MRN ?? ZString.Empty;

	[MessageLayout(Order = 2)]
	public NBPreviousAdministrativeReference PreviousProcedure => new NBPreviousAdministrativeReference(previousOperationInfo?.PreviousProcedure);

	[MessageLayout(Order = 3)]
	[MessageFieldIntegerRepresentation(7, false)]
	public ZInt? NumberOfPackages => previousOperationInfo?.NumberOfPackages;

	[MessageLayout(Order = 4)]
	[MessageFieldDecimalRepresentation(14, 5, false)]
	public ZDecimal? GrossMass => previousOperationInfo?.GrossMass;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 10, false)]
	public ZString CombinedNomenclature => previousOperationInfo?.CombinedNomenclature ?? ZString.Empty;

	[MessageLayout(Order = 6)]
	[MessageFieldDecimalRepresentation(14, 5, false)]
	public ZDecimal? NetMass => previousOperationInfo?.NetMass;

	[MessageLayout(Order = 7)]
	[MessageFieldDecimalRepresentation(11, 2, false)]
	public ZDecimal? SupplementaryUnit => previousOperationInfo?.SupplementaryUnit;
}
