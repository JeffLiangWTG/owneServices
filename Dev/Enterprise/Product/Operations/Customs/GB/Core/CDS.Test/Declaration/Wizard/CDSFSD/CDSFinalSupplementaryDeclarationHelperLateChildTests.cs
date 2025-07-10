using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSFinalSupplementaryDeclarationHelperLateChild))]
	public class CDSFinalSupplementaryDeclarationHelperLateChildTests : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CDSFinalSupplementaryDeclarationHelperLateChild(Factory);
		}

		[TestDate(2010, 11, 12)]
		public void TestProperties()
		{
			var helper = new CDSFinalSupplementaryDeclarationHelperLateChild(Factory);

			helper.DueDate = ZDate.Today;
			helper.NumberOfTypeYDeclarations = 1;
			helper.NumberOfTypeZDeclarations = 2;

			AssertEquals(new ZDate(2010, 11, 12), helper.DueDate);
			AssertEquals(1, helper.NumberOfTypeYDeclarations);
			AssertEquals(2, helper.NumberOfTypeZDeclarations);
		}
	}
}
