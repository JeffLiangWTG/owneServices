using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.TNT.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TNT.NZ.Testing
{
	[TestedType(typeof(NZQuantumMawbCollection))]
	class NZQuantumMawbCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NZQuantumMawbCollection>
	{
		protected override NZQuantumMawbCollection GetCollectionToTest()
		{
			return new NZQuantumMawbCollection(Factory, TestUtils.TinyFileFullName);
		}

		TNTTestUtils TestUtils;
		protected override void SetUp()
		{
			base.SetUp();
			TestUtils = new TNTTestUtils();
		}

		protected override void TearDown()
		{
			base.TearDown();
			TestUtils.Dispose();
		}
		public void TestNZQuantumMawbIsAddedWhenLoadedFromFile()
		{
			NZQuantumMawbCollection collection = GetCollectionToTest();
			collection.LoadFromFile();
			AssertEquals("1 Quantum mawb in the collection", 1, collection.Count);
			AssertEquals("Should be a NZQuantumMawb", typeof(NZQuantumMawb), collection[0].GetType());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			NZQuantumMawbCollection collection = GetCollectionToTest();
			collection.LoadFromFile();
			return collection[0];
		}

		public void TestIsX2()
		{
			var bneX2 = TestUtils.CopyResourceToFile("BNE.x2.20070528.210700.ok", "Enterprise.Client.TNT.Testing.DataManipulation.DataImportExport.Testing.");
			var sydX2 = TestUtils.CopyResourceToFile("SYD.X2.20040204.110259.ok", "Enterprise.Client.TNT.Testing.DataManipulation.DataImportExport.Testing.");
			var bneX1 = TestUtils.CopyResourceToFile("BNE.X1.20040601.095320.ok", "Enterprise.Client.TNT.Testing.DataManipulation.DataImportExport.Testing.");
			var bneX1_2 = TestUtils.CopyResourceToFile("BNE.x1.20070528.210700.ok", "Enterprise.Client.TNT.Testing.DataManipulation.DataImportExport.Testing.");
			var akl = TestUtils.CopyResourceToFile("AKL.IND.20060713.150553.OK", "Enterprise.Client.TNT.Testing.");
			AssertEquals(true, new NZQuantumMawbCollectionTestForX2(Factory, bneX2).IsX2);
			AssertEquals(true, new NZQuantumMawbCollectionTestForX2(Factory, sydX2).IsX2);
			AssertEquals(false, new NZQuantumMawbCollectionTestForX2(Factory, bneX1).IsX2);
			AssertEquals(false, new NZQuantumMawbCollectionTestForX2(Factory, bneX1_2).IsX2);
			AssertEquals(false, new NZQuantumMawbCollectionTestForX2(Factory, akl).IsX2);
		}

		class NZQuantumMawbCollectionTestForX2 : NZQuantumMawbCollection
		{
			public NZQuantumMawbCollectionTestForX2(BusinessObjectFactory factory, string fullFileName) : base(factory, fullFileName)
			{
			}

			public bool IsX2
			{
				get
				{
					return isX2;
				}
			}
		}
	}
}
