using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort.InvoiceDocLineRollUpper;

namespace Enterprise.DocumentWrappers.Accounting.DocRollUpSort.RollUpper
{
	class RollupGrouperCCDWithTaxRate : BaseInvoiceDocRollUpper<ZString, ZString>
	{
		public RollupGrouperCCDWithTaxRate(BaseInvoiceDocLineRollUpper docLineRollUpper, DocARBaseInvoice docARInvoice, BusinessObjectFactory factory, DocARInvoiceLineCollection lines)
			: base(docLineRollUpper, docARInvoice, factory, lines)
		{
			IsGroupedByChargeCode = true;
		}

		protected override ZString GetGroupId(IDocLine line)
		{
			var docARInvoiceLine = (DocARInvoiceLine)line;

			var result = ZString.Empty;
			if (docARInvoiceLine != null)
			{
				var lineChargeCode = docARInvoiceLine.Line.ChargeCode;
				var lineGLAccount = docARInvoiceLine.Line.GLHeader;
				var lineTaxRate = docARInvoiceLine?.Line.TaxRate;

				if (docARInvoiceLine.PreventGrouping)
				{
					result = docARInvoiceLine.Line.PK.ToString();
				}
				else if (lineChargeCode != null)
				{
					result = lineChargeCode.PK.ToString();
				}
				else if (lineGLAccount != null)
				{
					result = lineGLAccount.PK.ToString();
				}

				if (lineTaxRate != null)
				{
					result += lineTaxRate.PK.ToString();
				}
				else
				{
					result += ZGuid.Empty.ToString();
				}
			}

			return result;
		}

		protected override ZString GetDescriptionForRolledUpLine(DocARInvoiceLineCollection group, ZString groupId) => group.CombinedGroupDescription;
	}
}
