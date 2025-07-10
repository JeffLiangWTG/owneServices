using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.MultiLineAddInfos.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(CusAddInfo<MaritimeUcnThatIsHeld>))]
	class CusAddInfo_MaritimeUcnThatIsHeldTest : CusAddInfoTest<CusAddInfo<MaritimeUcnThatIsHeld>>
	{
		protected override IEnumerable<CusAddInfo<MaritimeUcnThatIsHeld>> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			yield return entry.MaritimeUcnsThatAreHeld.AddNew();
		}
	}

	[TestedType(typeof(MaritimeUcnThatIsHeld))]
	public class MaritimeUcnThatIsHeldTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<CusEntryHeader>().MaritimeUcnsThatAreHeld.AddNew().Data;
		}

		public void TestProperties()
		{
			var ucn = (MaritimeUcnThatIsHeld)GetNewBusinessObject();
			ucn.NW_UCN = "2345678901234";
			ucn.NW_HoldTypeComments = "Scanner hold exists";
			AssertEquals("2345678901234", ucn.NW_UCN);
			AssertEquals("Scanner hold exists", ucn.NW_HoldTypeComments);
			AssertEquals("2345678901234", ucn.KeyToDeterimeUniqueness);
		}
	}

	internal class MaritimeUcnThatIsHeldLookupsTest : BusinessObjectLookupsTestCase
	{
	}

	internal class MaritimeUcnThatIsHeldValidationTest : BusinessObjectValidationTestCase
	{
	}
}
