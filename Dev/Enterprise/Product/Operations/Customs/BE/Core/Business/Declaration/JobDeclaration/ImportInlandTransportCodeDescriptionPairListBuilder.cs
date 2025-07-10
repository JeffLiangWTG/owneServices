using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Declaration;

public class ImportInlandTransportCodeDescriptionPairListBuilder : EU.Business.Declaration.InlandTransportCodeDescriptionPairListBuilder
{
	public ImportInlandTransportCodeDescriptionPairListBuilder(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
	{
	}

	protected override CodeDescriptionPairList GetDefaultList() => new InlandMeansOfTransportList();
}
