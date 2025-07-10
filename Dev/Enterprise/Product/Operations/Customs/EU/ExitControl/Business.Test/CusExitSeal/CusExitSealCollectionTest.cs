using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitSealCollection))]
	sealed class CusExitSealCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDataIsLoadedInSequenceNumber()
		{
			var header = Factory.New<CusExitHeader>();
			header.CXH_JobReference = header.PK.ToString().Substring(0, 35);
			var container = header.CusExitContainers.AddNew();
			for (var i = 5; i > 0; i--)
			{
				var seal = container.AllSealNumbers.AddNew();
				seal.BK_SealNumber = "SL" + i.ToString();
			}
			var seal12 = container.AllSealNumbers.AddNew();
			seal12.BK_SealNumber = "12SL";
			seal12.BK_SequenceNumber = 30;
			var seal13 = container.AllSealNumbers.AddNew();
			seal13.BK_SealNumber = "13SL";
			seal13.BK_SequenceNumber = 20;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			container = newFactory.Load<CusExitContainer>(container.PK);
			var allSealNumbers = container.AllSealNumbers;
			CombineAssertions(() =>
			{
				AssertEquals("additionalSealNumbers.Count", 7, allSealNumbers.Count);
				AssertArrayEqualsByElements(new ZString[] { "SL5", "SL4", "SL3", "SL2", "SL1", "13SL", "12SL" }, allSealNumbers.Cast<CusExitSeal>().Select(x => x.BK_SealNumber).ToArray());
			});
		}

		public void TestSequenceNumberisFixedAndRemainsUnchanged()
		{
			var header = Factory.New<CusExitHeader>();
			header.CXH_JobReference = header.PK.ToString().Substring(0, 35);
			var container = header.CusExitContainers.AddNew();
			CombineAssertions(() =>
			{
				var seal1 = container.AllSealNumbers.AddNew();
				AssertEquals("seal1.BK_SequenceNumber", (ZShort)1, seal1.BK_SequenceNumber);
				var seal2 = container.AllSealNumbers.AddNew();
				AssertEquals("seal1.BK_SequenceNumber", (ZShort)1, seal1.BK_SequenceNumber);
				AssertEquals("seal2.BK_SequenceNumber", (ZShort)2, seal2.BK_SequenceNumber);
				seal2.BK_SequenceNumber = 10;
				var seal3 = container.AllSealNumbers.AddNew();
				AssertEquals("seal1.BK_SequenceNumber", (ZShort)1, seal1.BK_SequenceNumber);
				AssertEquals("seal2.BK_SequenceNumber", (ZShort)10, seal2.BK_SequenceNumber);
				AssertEquals("seal3.BK_SequenceNumber", (ZShort)11, seal3.BK_SequenceNumber);
				seal2.Delete();
				AssertEquals("seal2.BK_SequenceNumber", (ZShort)1, seal1.BK_SequenceNumber);
				AssertEquals("seal3.BK_SequenceNumber", (ZShort)11, seal3.BK_SequenceNumber);
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			(var container, var header) = CusExitContainerTest.GetNewBusinessObject(Factory);
			return container.AllSealNumbers;
		}
	}
}
