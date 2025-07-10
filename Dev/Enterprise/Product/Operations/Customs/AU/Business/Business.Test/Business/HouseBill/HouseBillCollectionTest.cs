using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(BillCollection))]
	sealed class HouseBillCollectionTest : Customs.Business.Testing.BaseHouseBillCollectionTest
	{
		public override void TestCloneHasChanges()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "123Test";
			houseBill.PackingGroups.AddNew();

			var declarationCloned = (JobDeclaration)declaration.Clone();
			AssertEquals("House bill copied", 0, declarationCloned.Bills.Count);
		}

		public override void TestCloneSingleHouseBill()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "123Test";
			houseBill.PackingGroups.AddNew();

			var declarationCloned = (JobDeclaration)declaration.Clone();
			AssertEquals("House bill copied", 0, declarationCloned.Bills.Count);
		}

		public void TestNotAddingNewPackRecord()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2004, 01, 01);
			declaration.JE_HouseBill = "HouseBill";
			AssertEquals("House Bill Count", 1, declaration.Bills.Count);
			AssertEquals("House Bill Packs Count", 0, declaration.PackingGroups.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = JobDeclaration.New(Factory);
			return declaration.Bills;
		}
	}
}
