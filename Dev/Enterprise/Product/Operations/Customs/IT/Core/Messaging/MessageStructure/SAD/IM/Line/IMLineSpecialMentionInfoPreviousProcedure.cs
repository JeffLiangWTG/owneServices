using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMLineSpecialMentionInfoPreviousProcedure
{
	public IMLineSpecialMentionInfoPreviousProcedure(IPreviousAdministrativeReference previousAdministrativeReference)
	{
		this.previousAdministrativeReference = Argument.NotNull(previousAdministrativeReference, nameof(previousAdministrativeReference));
	}
	readonly IPreviousAdministrativeReference previousAdministrativeReference;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 4, false)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZString Register => previousAdministrativeReference.Register;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 8, false)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZString Reference => previousAdministrativeReference.ReferenceNumber;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZString ReferenceCIN => previousAdministrativeReference.ReferenceCIN;

	[MessageLayout(Order = 3)]
	[MessageFieldDateDDMMYYYYRepresentation]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZDate Date => previousAdministrativeReference.Date;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 2, false)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZString Series => previousAdministrativeReference.Series;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 8, false)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZString CustomsOffice => previousAdministrativeReference.CustomsOffice;

	[MessageLayout(Order = 6)]
	[MessageFieldIntegerRepresentation(3, false)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZInt? ItemNumber => previousAdministrativeReference.ItemNumber;
}
