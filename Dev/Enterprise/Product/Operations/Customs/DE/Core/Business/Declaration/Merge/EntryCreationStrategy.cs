using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class EntryCreationStrategy : EU.Business.Declaration.EntryCreationStrategy
	{
		public EntryCreationStrategy(JobDeclaration declaration, ZString entryHeaderMessageTypeToNewEntryHeader)
			: base(declaration, entryHeaderMessageTypeToNewEntryHeader)
		{
		}

		public EntryCreationStrategy(JobDeclaration declaration)
			: this(declaration, declaration.JE_ApplicationCode.IsEmpty ? declaration.JE_MessageType : declaration.JE_ApplicationCode)
		{
		}

		public override bool LineIsValidForMerge(BaseJobComInvoiceLine invoiceLine)
		{
			var result = base.LineIsValidForMerge(invoiceLine);
			if (result)
			{
				result = ((JobComInvoiceLine)invoiceLine).EntryInstruction != null;
			}
			return result;
		}

		protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine invoiceLine)
		{
			var key = base.GetKeyForHeaderCore(invoiceLine);
			var header = invoiceLine.InvoiceHeader;
			key.Add(header.JZ_OA_SellerAddress);
			key.Add(header.JZ_OA_BuyerAddress);

			return key;
		}
	}
}
