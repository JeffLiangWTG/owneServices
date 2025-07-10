using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business;

public class PNTSDocumentProvider : IPNTSDocument
{
	public PNTSDocumentProvider(TemporaryStorageBill temporaryStorageBill)
	{
		this.temporaryStorageBill = Argument.NotNull(temporaryStorageBill, nameof(temporaryStorageBill));
	}
	readonly TemporaryStorageBill temporaryStorageBill;

	public string ReferenceNumber => temporaryStorageBill.ABL_BillNumber;

	public string Type => temporaryStorageBill.TypeOfBillDocument;
}
