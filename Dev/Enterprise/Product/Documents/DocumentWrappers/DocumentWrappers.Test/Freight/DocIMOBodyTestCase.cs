using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocIMOBody))]
	sealed class DocIMOBodyTestCase : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocIMOBody("", "", "", "", "");
		}

		public void TestMarksLine()
		{
			AssertEquals("MarksLine", docIMOBody.MarksAndNumbers);
		}

		public void TestGoodsDescLine()
		{
			AssertEquals("GoodsLine", docIMOBody.GoodsDescription);
		}

		public void TestGrossMassLine()
		{
			AssertEquals("GrossMassLine", docIMOBody.GrossMass);
		}

		public void TestCubeLine()
		{
			AssertEquals("CubeLine", docIMOBody.Volume);
		}

		public void TestNetMassLine()
		{
			AssertEquals("NetMassLine", docIMOBody.NetMassLine);
		}

		DocIMOBody docIMOBody;
		protected override void SetUp()
		{
			docIMOBody = new DocIMOBody("MarksLine", "GoodsLine", "GrossMassLine", "NetMassLine", "CubeLine");
			AssertNotNull("Precondition: new DocIMOBody not null", docIMOBody);
			base.SetUp();
		}
	}
}
