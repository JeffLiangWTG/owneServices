using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort.InvoiceDocLineRollUpper;
using Enterprise.DocumentWrappers.HelperClasses.DocRollUpSort;

namespace Enterprise.DocumentWrappers.Accounting.DocRollUpSort.RollUpper
{
	class RollupGrouperCLC : BaseInvoiceDocRollUpper<ZString, ZString>
	{
		public RollupGrouperCLC(BaseInvoiceDocLineRollUpper docLineRollUpper, DocARBaseInvoice docARInvoice, BusinessObjectFactory factory, DocARInvoiceLineCollection lines)
			: base(docLineRollUpper, docARInvoice, factory, lines)
		{
			IsGroupedByChargeCode = true;
		}

		protected override ZString GetGroupId(IDocLine line)
		{
			var docARInvoiceLine = (DocARInvoiceLine)line;

			var result = ZString.Empty;
			if (line != null)
			{
				var lineChargeCode = docARInvoiceLine.Line.ChargeCode;
				var lineGLAccount = docARInvoiceLine.Line.GLHeader;
				var lineCurrency = docARInvoiceLine.ChargeCurrency;

				if (docARInvoiceLine.PreventGrouping)
				{
					result = docARInvoiceLine.Line.PK.ToString();
				}
				else
				{
					if (lineChargeCode != null)
					{
						result = lineChargeCode.PK.ToString();
					}
					else if (lineGLAccount != null)
					{
						result = lineGLAccount.PK.ToString();
					}
					result += lineCurrency;
				}
			}

			return result;
		}

		protected override ZString GetDescriptionForRolledUpLine(DocARInvoiceLineCollection group, ZString groupId) => group.CombinedGroupDescriptionWithChargeAndGLAccount;

		protected override void ProcessRolledUpLine(IRolledUpDocLine rolledUpLine, DocRollUpGroup<ZString, DocARInvoiceLineCollection> group)
		{
			var docARInvoiceLineCollection = group.Collection;

			var docARInvoiceLineForRollUp = (DocARInvoiceLineForRollUp)rolledUpLine;

			docARInvoiceLineForRollUp.ChargeCurrency = docARInvoiceLineCollection[0].ChargeCurrency;
			docARInvoiceLineForRollUp.ChargeOSAmountForCLC = docARInvoiceLineCollection.Cast<IDocARInvoiceLine>().Sum(x => x.ChargeOSAmountForCLC);
			docARInvoiceLineForRollUp.ChargeExchangeRate = docARInvoiceLineCollection[0].ChargeExchangeRate;
			docARInvoiceLineForRollUp.Sequence = docARInvoiceLineCollection.Cast<IDocARInvoiceLine>().Min(x => x.Sequence);
			base.ProcessRolledUpLine(docARInvoiceLineForRollUp, group);
		}
	}
}
