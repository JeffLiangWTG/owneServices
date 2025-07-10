using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class TemporaryStorageRegisterTransactionData : NonPersistentBusinessObject
{
	public TemporaryStorageRegisterTransactionData(BusinessObjectFactory factory) : base(factory)
	{
	}

	public TemporaryStorageRegisterTransactionData()
	{
	}

	public CusTempStorageRegHeader PreviousRegisterHeader { get; set; }
	public ZString CustomsReferenceNumber { get; set; }
	public ZDecimal GrossMass { get; set; }
	public ZInt PackageQuantity { get; set; }
	public ZString ReferenceType { get; set; }
	public ZString InternalReferenceNumber { get; set; }
	public ZString InternalReferenceType { get; set; }
	public ZString Comments { get; set; }
	public ZInt RegisterLineNo { get; set; }
	public List<ZString> ErrorMessages = new ();
}
