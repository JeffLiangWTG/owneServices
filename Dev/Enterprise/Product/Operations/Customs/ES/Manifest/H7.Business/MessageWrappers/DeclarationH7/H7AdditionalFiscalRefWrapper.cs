using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Manifest.H7.Business;

public class H7AdditionalFiscalRefWrapper(AsycudaBill bill) : IH7AdditionalFiscalRef
{
	public ZString Id => bill.ABL_SellerRegNo;
	public ZString Role => !bill.ABL_SellerRegNo.IsEmpty ? SellerRoleFr5 : ZString.Empty;

	const string SellerRoleFr5 = "FR5";
}
