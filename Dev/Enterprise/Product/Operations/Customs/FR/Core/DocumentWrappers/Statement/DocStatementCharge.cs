using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.FR.DocumentWrappers.Statement;

public class DocStatementCharge : DocBaseWrapper
{
	DocStatementCharge(CusStatementLineCharge cusStatementLineCharge, BusinessObjectFactory factoryToWrap)
		: base(cusStatementLineCharge, factoryToWrap)
	{ }

	public static DocStatementCharge New(CusStatementLineCharge cusStatementLineCharge, BusinessObjectFactory factoryToWrap) => new DocStatementCharge(cusStatementLineCharge, factoryToWrap);

	public CusStatementLineCharge StatementLineCharge => (CusStatementLineCharge)base.WrappedObject;

	public ZDecimal Amount => StatementLineCharge.B4_ChargeAmount;
	public ZString TaxCode => StatementLineCharge.B4_ChargeType;
	public ZString MethodOfPayment => StatementLineCharge.MethodOfPaymentDescription;
}
