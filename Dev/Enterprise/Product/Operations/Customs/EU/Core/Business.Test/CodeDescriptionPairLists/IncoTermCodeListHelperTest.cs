using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business.Testing
{
	class IncoTermCodeListHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetCachedIncoTermListEU()
		{
			var list1 = Factory.GetCachedIncoTermListEU(false);
			var list2 = Factory.GetCachedIncoTermListEU(false);
			var list3 = Factory.GetCachedIncoTermListEU(true);
			var list4 = Factory.GetCachedIncoTermListEU(true);
			NUnit.Framework.Assert.That(list2, NUnit.Framework.Is.SameAs(list1), "IsUCC6 false");
			NUnit.Framework.Assert.That(object.ReferenceEquals(list1, list3), NUnit.Framework.Is.EqualTo(false));
			NUnit.Framework.Assert.That(list4, NUnit.Framework.Is.SameAs(list3), "IsUCC6 true");
		}

		[ExpectNoExceptions]
		public void TestIncoTermListEU()
		{
			var codeDescriptionPairList = IncoTermCodeListHelper.IncoTermListEU(false);
			NUnit.Framework.Assert.That(codeDescriptionPairList.CodesAsString, NUnit.Framework.Does.Not.Contain(Constants.IncoTerms.Other));

			codeDescriptionPairList = IncoTermCodeListHelper.IncoTermListEU(true);
			NUnit.Framework.Assert.That(codeDescriptionPairList.CodesAsString, NUnit.Framework.Does.Contain(Constants.IncoTerms.Other));
		}
	}
}
