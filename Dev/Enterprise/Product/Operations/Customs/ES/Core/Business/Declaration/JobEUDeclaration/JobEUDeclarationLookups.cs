namespace Enterprise.Customs.ES.Business.Declaration;

public class JobEUDeclarationLookups : EU.Business.Declaration.JobEUDeclarationLookups
{
	public JobEUDeclarationLookups(EU.Business.Declaration.JobEUDeclaration parent) : base(parent)
	{
	}

	public System.Collections.ICollection RegionOrTerritoryOfDestinationList => ((JobDeclaration)Parent.Declaration).IsGoodsDestinationESOrXCOrXLOrEmpty ? LookupsHelper.RegionOfDestinationDropEditList(Parent.Factory) : LookupsHelper.RegionOfDestinationCodeFindBoxList(Parent.Factory);
}
