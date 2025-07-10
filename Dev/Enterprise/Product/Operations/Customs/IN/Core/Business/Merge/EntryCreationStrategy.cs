using Enterprise.Customs.Business;

namespace Enterprise.Customs.IN.Business;

public class EntryCreationStrategy : Customs.Business.EntryCreationStrategy
{
	public EntryCreationStrategy(BaseJobDeclaration declaration) : base(declaration)
	{
	}

	public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
	{
		var result = new MergeKey();
		result.Add(baseInvoiceLine.PK);
		return result;
	}
}
