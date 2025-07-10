using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort.InvoiceDocLineRollUpper;

namespace Enterprise.DocumentWrappers.Accounting.DocRollUpSort.RollUpper
{
	class RollUpGrouperByChargeAndTax : BaseInvoiceDocRollUpper<ZString, ZString>
	{
		public RollUpGrouperByChargeAndTax(BaseInvoiceDocLineRollUpper docLineRollUpper, DocARBaseInvoice docARInvoice, BusinessObjectFactory factory, DocARInvoiceLineCollection lines)
			: base(docLineRollUpper, docARInvoice, factory, lines)
		{
		}

		protected override ZString GetGroupId(IDocLine line)
		{
			var docARInvoiceLine = (DocARInvoiceLine)line;

			var lineChargeCode = docARInvoiceLine?.Line.ChargeCode;
			var lineTaxRate = docARInvoiceLine?.Line.TaxRate;

			return (lineChargeCode?.PK ?? ZGuid.Empty).ToString() +
				(lineTaxRate?.PK ?? ZGuid.Empty).ToString();
		}

		protected override ZString GetDescriptionForRolledUpLine(DocARInvoiceLineCollection group, ZString groupId)
		{
			var docARInvoiceLineCollection = group;

			var invoiceLine = ((DocumentWrapper)docARInvoiceLineCollection[0]).WrappedObject as InvoicingLineBase;

			return invoiceLine != null && invoiceLine.ChargeCode != null
				? DocARBaseInvoice.GetChargeDescription(invoiceLine)
				: "";
		}

		protected override bool IsRollUpGrouperByChargeAndTax => true;
	}
}
