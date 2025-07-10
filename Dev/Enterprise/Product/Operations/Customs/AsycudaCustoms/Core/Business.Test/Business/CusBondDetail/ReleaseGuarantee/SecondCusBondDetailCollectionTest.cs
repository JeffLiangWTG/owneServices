using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(SecondCusBondDetailCollection))]
	class SecondCusBondDetailCollectionTest : ActiveBusinessObjectCollectionTestCase<SecondCusBondDetailCollection>
	{
		public void TestOnAdded()
		{
			var collection = GetCollectionToTest();
			var added = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("PW_ParentTableCode", CusEntryInstructionSchema.Constants.Prefix, added.PW_ParentTableCode);
				AssertEquals("PW_CPH_Guarantee", added.Instruction.Guarantee.PW_CPH_Guarantee, added.PW_CPH_Guarantee);
				AssertEquals("PW_Status", added.Instruction.Guarantee.PW_Status, added.PW_Status);
			});
		}

		public void TestOnAdded_LockGuaranteeManagementMutex()
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			using (var mutex = new ZGlobalMutex(MutexIDs.GuaranteeManagement, guarantee.Instruction.PK.ToString()))
			{
				mutex.Lock();
				var releaseGuarantees = guarantee.Instruction.ReleaseGuarantees;
				var secondCusBondDetail = (SecondCusBondDetail)((IBindingList)releaseGuarantees).AddNew();
				secondCusBondDetail.PW_BondAmount = 1m;
				((ICancelAddNew)releaseGuarantees).EndNew(releaseGuarantees.IndexOf(secondCusBondDetail));
				CombineAssertions(() =>
				{
					AssertContains("Warning", "will be deleted.", ((SendsMessagesToCustomsShutterUpperer)guarantee.Instruction.JobDeclaration.MessageInitiator).Warning);
					AssertEquals(true, secondCusBondDetail.IsDeleted);
				});
				mutex.Unlock();
			}
		}

		protected override SecondCusBondDetailCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var guarantee = instruction.Guarantee;
			guarantee.PW_BondAmount = 1m;
			guarantee.PW_BondNumber2 = "VWG";
			guarantee.PW_BondEffectiveDate = ZDateTime.Now;
			return new SecondCusBondDetailCollection(instruction);
		}
	}
}
