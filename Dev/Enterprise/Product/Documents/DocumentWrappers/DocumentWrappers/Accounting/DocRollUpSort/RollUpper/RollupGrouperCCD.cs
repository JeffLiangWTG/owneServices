using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort.InvoiceDocLineRollUpper;

namespace Enterprise.DocumentWrappers.Accounting.DocRollUpSort.RollUpper
{
	class RollupGrouperCCD : BaseInvoiceDocRollUpper<ZGuid, ZString>
	{
		public RollupGrouperCCD(BaseInvoiceDocLineRollUpper docLineRollUpper, DocARBaseInvoice docARInvoice, BusinessObjectFactory factory, DocARInvoiceLineCollection lines)
			: base(docLineRollUpper, docARInvoice, factory, lines)
		{
			IsGroupedByChargeCode = true;
		}

		protected override ZGuid GetGroupId(IDocLine line)
		{
			var docARInvoiceLine = (DocARInvoiceLine)line;

			var result = ZGuid.Empty;
			if (line != null)
			{
				var lineChargeCode = docARInvoiceLine.Line.ChargeCode;
				var lineGLAccount = docARInvoiceLine.Line.GLHeader;
				if (docARInvoiceLine.PreventGrouping)
				{
					result = docARInvoiceLine.Line.PK;
				}
				else if (lineChargeCode != null)
				{
					result = lineChargeCode.PK;
				}
				else if (lineGLAccount != null)
				{
					result = lineGLAccount.PK;
				}
			}
			return result;
		}

		protected override ZString GetDescriptionForRolledUpLine(DocARInvoiceLineCollection group, ZGuid groupId) => group.CombinedGroupDescription;
	}
}
