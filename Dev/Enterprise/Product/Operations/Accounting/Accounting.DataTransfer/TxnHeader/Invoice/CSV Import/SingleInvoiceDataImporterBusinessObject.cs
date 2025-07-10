using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Accounting.DataTransfer.Invoices.FlatFile
{
	public partial class SingleInvoiceDataImporterBusinessObject : DataImporterBusinessObject
	{
		public SingleInvoiceDataImporterBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZString ProgressMessageForFatalError
		{
			// This is the comment that is shown if Factory.Save() is not called 
			// in the base DataImporterBusinessObject.
			// We dont want the factory to save because we are showing the form.
			// This is the message that will always be shown.
			get { return Res.GetString("479c15a5-b5c7-42f8-a335-5ef2a86763fe", "The import process is complete, please close this form to edit the transaction"); }
		}
	}
}