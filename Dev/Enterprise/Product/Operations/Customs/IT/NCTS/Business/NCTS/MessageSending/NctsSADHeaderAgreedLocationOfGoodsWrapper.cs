using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsSADHeaderAgreedLocationOfGoodsWrapper : IETHeaderAgreedLocationOfGoods
{
	public NctsSADHeaderAgreedLocationOfGoodsWrapper(NctsHeader nctsHeader)
	{
		this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		nctsMovementHeader = Argument.NotNull(nctsHeader.MovementHeader, nameof(nctsHeader.MovementHeader));
	}
	readonly NctsHeader nctsHeader;
	readonly NctsDepartureMovementHeader nctsMovementHeader;

	public ZString AgreedLocationOfGoodsCode => ZString.Empty;

	public ZString AgreedLocationOfGoodsDescription => nctsHeader.Authorization.IsEmpty ? nctsMovementHeader.BM_LocationOfGoodsCode : ZString.Empty;

	public ZString AuthorizedLocationOfGoodsCodeAndCin => GetAuthorizedLocationOfGoodsCodeAndCin();

	public ZString CustomsSubPlace => nctsMovementHeader.BM_CustomsSubPlace;

	#region Implementation

	ZString GetAuthorizedLocationOfGoodsCodeAndCin()
	{
		var effectiveLocationOfGoods = !nctsHeader.Authorization.IsEmpty ? nctsMovementHeader.BM_LocationOfGoodsCode : ZString.Empty;
		return SADWrapperHelper.AppendFEIfElectronicDocument(nctsMovementHeader, effectiveLocationOfGoods);
	}

	#endregion
}
