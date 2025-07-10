using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
namespace Enterprise.Accounting.Business.CashBook.ExchangeDifference
{
	public class NewCashbookExchangeDiffCollection : DependentBusinessObjectCollection<NewCashbookExchangeDiff, NewCashbookExchangeDiffHeader>
	{
		public NewCashbookExchangeDiffCollection(NewCashbookExchangeDiffHeader master) : base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => new SchemaGuidColumn(Schema.GenericTableSchema, NewCashbookExchangeDiff.Schema.AH_NewCashbookExchangeDiff, 0, Guid.Empty, false);

		protected override bool AllowNewCore => false;
	}
}
