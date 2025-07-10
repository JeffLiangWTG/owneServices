using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.MasterFiles.Testing;

[TestedType(typeof(CusClassification.Loader))]
public class ClassificationLoaderTest : LoaderTestCase
{
	protected override BusinessObject.Loader GetNewLoaderToTest()
	{
		return new CusClassification.Loader(Factory);
	}
}
