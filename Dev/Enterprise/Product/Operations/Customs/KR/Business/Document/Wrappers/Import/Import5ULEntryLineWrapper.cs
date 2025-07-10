using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import5ULEntryLineWrapper : NonPersistentBusinessObject
	{
		public Import5ULEntryLineWrapper(Import5ULEntryLine entryLine, ZBool isFirstItem)
		{
			EntryLine = entryLine;
			this.IsFirstItem = isFirstItem;
		}
		public IImport5ULEntryLine EntryLine { get; }
		public ZBool IsFirstItem { get; }

		public ZString FormattedImportDeclarationNumber
		{
			get
			{
				var result = ZString.Empty;
				if (EntryLine.ImportDeclarationNumber != ZString.Empty)
				{
					result = MessageFunctions.DeclarationNumberFormat(EntryLine.ImportDeclarationNumber);
				}
				return result;
			}
		}

		public ZDecimal RefundQuantity
		{
			get
			{
				var result = ZDecimal.Zero;
				var invoiceLines = EntryLine.InvoiceLines;
				if (invoiceLines != null)
				{
					foreach (var invoiceLineData in invoiceLines)
					{
						result += invoiceLineData.RefundQuantity;
					}
				}
				return result;
			}
		}

		public ZString ShortGoodsLocationDescription
		{
			get
			{
				return EntryLine.GoodsLocationDescription.SubstringSafe(0, 50);
			}
		}

		public ZString ShortDamageSituation
		{
			get
			{
				return EntryLine.DamageSituation.SubstringSafe(0, 100);
			}
		}

		public ZString HSDescription
		{
			get
			{
				var reuslt = ZString.Empty;
				if (EntryLine.InvoiceLines.Any())
				{
					reuslt = EntryLine.InvoiceLines.FirstOrDefault().HSDescription;
				}
				return reuslt;
			}
		}
	}
}
