using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public partial class JobDeclaration
{
	public ZBool IsLocationQualifierValid => !JE_LocationQualifier.IsEmpty && Lookups.LocationQualifierList.ContainsCode(JE_LocationQualifier);

	public ZBool IsLocationQualifierValidAndD => IsLocationQualifierValid && JE_LocationQualifier == GoodsLocationList.Codes.GoodsAreInCustomsAreaForInspection;

	public ZBool IsLocationQualifierValidAndF => IsLocationQualifierValid && JE_LocationQualifier == GoodsLocationList.Codes.GoodsAreOutOfCustomsCircuit;

	public ZBool IsLocationQualifierValidAndFC => IsLocationQualifierValid && JE_LocationQualifier == GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation;

	public ZBool IsLocationQualifierValidAndLBorLC => IsLocationQualifierValid
													  && (JE_LocationQualifier == ImportGoodsLocationQualifierWithAuthorisationList.Codes.AuthorizedPlace || JE_LocationQualifier == ImportGoodsLocationQualifierWithAuthorisationList.Codes.ApprovedPlace);
}
