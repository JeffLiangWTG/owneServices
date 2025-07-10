using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class D87EntryCreationStrategy : EntryCreationStrategy
	{
		public D87EntryCreationStrategy(JobDeclaration declaration)
			: base(declaration, ElectronicDocumentTypeList.Codes._D87)
		{
		}

		protected override bool IsActiveCore => Declaration.JE_MessageType == ElectronicDocumentTypeList.Codes._D87;

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
