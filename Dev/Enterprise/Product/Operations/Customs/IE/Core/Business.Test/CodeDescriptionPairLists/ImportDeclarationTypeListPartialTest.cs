using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	class ImportDeclarationTypeListPartialTest : TestCase
	{
		public void TestDeclarationTypeListBR2074()
		{
			var expected = new List<string>
			{
				ImportDeclarationTypeList.Codes.H1,
				ImportDeclarationTypeList.Codes.H5,
				ImportDeclarationTypeList.Codes.H6,
				ImportDeclarationTypeList.Codes.I1
			};

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Declaration Type List for Validation BR2074", expected, ImportDeclarationTypeList.DeclarationTypeListBR2074);
				AssertEquals("Number of elements in list", 4, ImportDeclarationTypeList.DeclarationTypeListBR2074.Count);
			});
		}
	}
}
