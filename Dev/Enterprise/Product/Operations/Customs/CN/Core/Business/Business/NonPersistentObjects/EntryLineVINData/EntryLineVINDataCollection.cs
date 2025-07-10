using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business
{
	public class EntryLineVINDataCollection : NonPersistentBusinessObjectCollection<EntryLineVINData>
	{
		public EntryLineVINDataCollection(EntryLineProductQualification productQualification) : base(productQualification.Factory)
		{
			this.productQualification = Argument.NotNull(productQualification, nameof(productQualification));
			entryLine = productQualification.EntryLine;
		}
		readonly CusEntryLine entryLine;
		readonly EntryLineProductQualification productQualification;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;

		public override void Load()
		{
			RemoveAndDeleteAll();

			if (productQualification.SupportsVIN)
			{
				var randomLine = entryLine.RandomLine;
				var invoiceLines = entryLine.InvoiceLines.Cast<JobComInvoiceLine>();
				bool useInvoiceQuantity = invoiceLines.All(x => x.JI_InvoiceQuantity > 0) && invoiceLines.AllSame(x => x.JI_InvoiceUQ);

				var billOfLadingDate = randomLine?.EntryInstruction?.BillOfLadingDate ?? ZDateTime.Empty;
				var invoiceNumber = randomLine?.InvoiceHeader?.JZ_InvoiceNumber ?? ZString.Empty;
				var invoiceQuantity = useInvoiceQuantity ? invoiceLines.Sum(x => x.JI_InvoiceQuantity) : invoiceLines.Sum(x => x.JI_TradeQuantity);
				var unitPrice = useInvoiceQuantity ? randomLine.UnitPrice : randomLine.TradeUnitPrice;

				foreach (VINData vinData in invoiceLines.SelectMany(l => l.VINDataCollection))
				{
					Add(new EntryLineVINData(productQualification, vinData)
					{
						BillOfLadingDate = billOfLadingDate,
						InvoiceNumber = invoiceNumber,
						InvoiceQuantity = invoiceQuantity,
						UnitPrice = unitPrice
					});
				}
			}
		}
	}
}
