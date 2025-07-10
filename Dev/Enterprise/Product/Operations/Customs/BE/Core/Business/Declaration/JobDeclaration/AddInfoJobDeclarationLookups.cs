using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Declaration;

public partial class JobDeclarationLookups
{
	public override CodeDescriptionPairList DeferTypeList => new VatDeferTypeList();

	public override CodeDescriptionPairList SpecificCircumstanceIndicatorList => Factory.GetCachedValue<BESpecificCircumstanceIndicatorList>();

	protected override EU.Business.Declaration.InlandTransportCodeDescriptionPairListBuilder GetImportInlandTransportCodeDescriptionPairListBuilder(EU.Business.Declaration.JobDeclaration declaration)
		=> new ImportInlandTransportCodeDescriptionPairListBuilder(declaration);

	protected override CodeDescriptionPairList RegionOfDestinationListCore => Factory.GetCachedValue<BERegionList>();
}
