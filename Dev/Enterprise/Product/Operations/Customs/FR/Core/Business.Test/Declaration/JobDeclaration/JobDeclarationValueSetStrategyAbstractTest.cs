using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(JobDeclarationValueSetStrategy))]
	public abstract class JobDeclarationValueSetStrategyAbstractTest : TestCaseWithFactory
	{
		public abstract void TestDefaultJE_DeltaMode();

		public abstract void TestDefaultJE_DeclarationLanguage();

		public abstract void TestDefaultJE_CustomsProfile();
	}
}
