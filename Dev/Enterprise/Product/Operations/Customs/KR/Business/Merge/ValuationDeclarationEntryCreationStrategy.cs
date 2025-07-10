using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ValuationDeclarationEntryCreationStrategy : EntryCreationStrategy
	{
		public ValuationDeclarationEntryCreationStrategy(JobDeclaration declaration)
			: base(declaration, ElectronicDocumentTypeList.Codes._5SM)
		{
		}

		protected override bool IsActiveCore => Declaration.JE_MessageType == ElectronicDocumentTypeList.Codes._5SM;

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var result = new MergeKey();
			result.Add(baseInvoiceLine.PK);
			return result;
		}

		protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine invoiceLine)
		{
			return new MergeKey();
		}
	}
}
