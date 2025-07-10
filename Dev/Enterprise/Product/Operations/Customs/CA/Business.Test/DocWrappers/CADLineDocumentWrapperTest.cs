using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Customs.CA.Business.CADImportDocumentWrapper;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CADLineDocumentWrapper))]
	public class CADLineDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			entryHeader.CH_JE = declaration.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			return new CADImportDocumentWrapper(entryHeader).CADSubHeaders.OfType<CADSubHeaderDocumentWrapper>().First().CADLines.OfType<CADLineDocumentWrapper>().First();
		}

		#endregion
	}
}
