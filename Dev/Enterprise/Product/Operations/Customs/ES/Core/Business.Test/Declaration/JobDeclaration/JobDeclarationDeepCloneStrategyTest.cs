using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	class JobDeclarationDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestDoNotPopulateGuaranteeAmountWhenCopyingCustomDeclarations()
		{
			JobDeclaration oldDec = Factory.New<JobDeclaration>();

			var guarantee1 = oldDec.Guarantees.AddNew();
			guarantee1.PW_BondAmount = 100;

			var guarantee2 = oldDec.Guarantees.AddNew();
			guarantee2.PW_BondAmount = 100;

			var cloneStrategy = new JobDeclarationDeepCloneStrategy(oldDec, CloneType.TemplateCopy);
			var newDec = (JobDeclaration)cloneStrategy.Clone(new BusinessObjectCloneArgs());

			CombineAssertions(() =>
			{
				AssertEquals("Guarantees count of clone should be the same as cloned Guarantees.", 2, newDec.Guarantees.Count);
				for (int i = 0; i < 2; i++)
				{
					var originalGuarantee = oldDec.Guarantees[i];
					var clonedGuarantee = newDec.Guarantees[i];
					AssertEquals("PW_BondAmount in original guarantee in position " + i, (decimal)100, originalGuarantee.PW_BondAmount);
					AssertEquals("PW_BondAmount in cloned guarantee in position " + i, (decimal)0, clonedGuarantee.PW_BondAmount);
				}
			});
		}
	}
}

