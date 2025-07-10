using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(JobDeclarationCustomsOfficeRequirementHelper))]
	public abstract class JobDeclarationCustomsOfficeRequirementHelperAbstractTest<T> : TestCaseWithFactory
		where T : JobDeclarationCustomsOfficeRequirementHelper
	{
		public void TestCacheKeyCombination()
		{
			var expectedCacheKeyCombination = SetupDeclarationForCacheKey();
			AssertEquals(expectedCacheKeyCombination, officeHelper.CacheKeyCombination);
		}

		public abstract void TestMainOffice_Import();

		public abstract void TestMainOffice_Export();

		public abstract void TestMainOffice_Miscellaneous();

		public abstract void TestOtherRequirements_Import();

		public abstract void TestOtherRequirements_Export();

		public abstract void TestOtherRequirements_Miscellaneous();

		protected abstract string SetupDeclarationForCacheKey();

		public virtual void AssertCustomsOfficeRequirementEquals(ZString shortComment, CustomsOfficeRequirement first, CustomsOfficeRequirement second)
		{
			CustomsOfficeRequirementHelperTest.AssertCustomsOfficeRequirementEquals(shortComment, first, second);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = GetJobDeclaration();
			officeHelper = (T)declaration.CustomsOfficeRequirementHelper;
		}
		protected JobDeclaration declaration;
		protected T officeHelper;

		protected abstract JobDeclaration GetJobDeclaration();
	}
}
