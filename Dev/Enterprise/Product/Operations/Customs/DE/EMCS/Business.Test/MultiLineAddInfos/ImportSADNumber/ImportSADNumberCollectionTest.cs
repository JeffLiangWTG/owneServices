using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ImportSADNumberCollection))]
	public class ImportSADNumberCollectionTest : EU.EMCS.Business.Testing.ImportSADNumberCollectionTest
	{
		protected override CusSupportingInfoCollection<EU.EMCS.Business.ImportSADNumber> GetCusSupportingInfoCollection() => new ImportSADNumberCollection(Factory.New<EMCSJobDeclaration>());
	}
}
