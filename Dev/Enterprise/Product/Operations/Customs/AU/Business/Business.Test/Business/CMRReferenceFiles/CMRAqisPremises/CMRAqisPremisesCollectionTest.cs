using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRAqisPremisesCollection))]
	sealed class CMRAqisPremisesCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestOrderAfterLoad()
		{
			TestCaseHelper.ClearTable(CMRAqisPremises.Schema.TableName);

			CMRAqisPremises premises1 = CMRAqisPremises.New(Factory);
			premises1.QP_AQISPremisesIdentifier = "Code";
			premises1.QP_AQISPremisesName = "Premises";

			CMRAqisPremises premises2 = CMRAqisPremises.New(Factory);
			premises2.QP_AQISPremisesIdentifier = "A Code";
			premises2.QP_AQISPremisesName = "A Premises";

			CMRAqisPremisesCollection collection = new CMRAqisPremisesCollection(Factory);
			collection.Load();
			AssertEquals("First description", "A Premises", collection[0].QP_AQISPremisesName);
			AssertEquals("Second description", "Premises", collection[1].QP_AQISPremisesName);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CMRAqisPremisesCollection(Factory);
	}
}
