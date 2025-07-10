using CargoWise.Common;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class DocumentWrapper : CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces.IDocument
	{
		DocumentWrapper(TemporaryStorageBill bill)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
		}
		readonly TemporaryStorageBill bill;

		public string ReferenceNumber => referenceNumber ?? (referenceNumber = bill.ABL_BillNumber);
		string referenceNumber;

		public string Type => type ?? (type = bill.TypeOfBillDocument);
		string type;

		public static DocumentWrapper New(TemporaryStorageBill bill) => bill == null ? null : new DocumentWrapper(bill);
	}
}
