using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.HelperClasses.DocRollUpSort;

namespace Enterprise.DocumentWrappers.Accounting.DocRollUpSort
{
	public class InvoiceDocRollUpGroupList<TID> : BaseDocRollUpGroupList<TID, DocARInvoiceLineCollection>
		where TID : IZType
	{
		protected override DocARInvoiceLineCollection GetNewLine(BusinessObjectFactory factory)
			=> DocARInvoiceLineCollection.New(factory);
	}
}
