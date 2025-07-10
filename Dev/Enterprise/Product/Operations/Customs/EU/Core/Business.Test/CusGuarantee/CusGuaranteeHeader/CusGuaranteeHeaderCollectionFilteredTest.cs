using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusGuaranteeHeaderCollectionFiltered))]
	public class CusGuaranteeHeaderCollectionFilteredTest : ActiveBusinessObjectCollectionTestCase<CusGuaranteeHeaderCollectionFiltered>
	{
		[ExpectNoExceptions]
		public void TestModuleIdAttribute()
		{
			var moduleIDAttribute = typeof(CusGuaranteeHeaderCollectionFiltered).GetCustomAttribute<ModuleIDAttribute>();
			NUnit.Framework.Assert.That(moduleIDAttribute.ModuleId, NUnit.Framework.Is.EqualTo(ModuleId.Guarantees));
		}
	}
}
