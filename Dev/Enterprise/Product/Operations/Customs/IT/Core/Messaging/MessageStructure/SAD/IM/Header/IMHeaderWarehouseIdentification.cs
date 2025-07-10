using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMHeaderWarehouseIdentification
{
	public IMHeaderWarehouseIdentification(IWarehouseIdentification warehouseIdentification)
	{
		this.warehouseIdentification = Argument.NotNull(warehouseIdentification, "warehouseIdentification");
	}

	readonly IWarehouseIdentification warehouseIdentification;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	public ZString AssessmentProcedure => ZString.Empty;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString Type => warehouseIdentification.Type;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 14, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString Identification => warehouseIdentification.Identification;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString CinIdentification => warehouseIdentification.CinIdentification;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString AuthorizingCountry => warehouseIdentification.AuthorizingCountry;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 8, false)]
	public ZString ControlCustomsOffice => ZString.Empty;
}
