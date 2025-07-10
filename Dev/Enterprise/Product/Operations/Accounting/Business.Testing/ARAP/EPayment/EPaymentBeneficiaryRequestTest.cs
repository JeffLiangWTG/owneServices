using System.Linq;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(EPaymentBeneficiaryRequest))]
	public class EPaymentBeneficiaryRequestTest : EnterpriseBusinessObjectTestCase
	{
		public void TestRequestReferenceIsSetOnSaving()
		{
			var company1 = TestObjectCreator.CreateNewCompany("CC1");
			var branch1 = TestObjectCreator.CreateBranch("BB1", company1);
			var branch2 = TestObjectCreator.CreateBranch("BB2", company1);

			var company2 = TestObjectCreator.CreateNewCompany("CC2");
			var branch3 = TestObjectCreator.CreateBranch("BB3", company2);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var request1 = Factory.NewWithValidTestData<EPaymentBeneficiaryRequest>();
				var request2 = Factory.NewWithValidTestData<EPaymentBeneficiaryRequest>();
				Factory.Save();
				AssertContainsExactElementsInAnyOrder(new[] { "00001000", "00001001" }, new[] { request1, request2 }.Select(x => x.ABR_InternalReference));
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var request3 = Factory.NewWithValidTestData<EPaymentBeneficiaryRequest>();
				var request4 = Factory.NewWithValidTestData<EPaymentBeneficiaryRequest>();
				Factory.Save();
				AssertContainsExactElementsInAnyOrder(new[] { "00001002", "00001003" }, new[] { request3, request4 }.Select(x => x.ABR_InternalReference));
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch3.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var request5 = Factory.NewWithValidTestData<EPaymentBeneficiaryRequest>();
				var request6 = Factory.NewWithValidTestData<EPaymentBeneficiaryRequest>();
				Factory.Save();
				AssertContainsExactElementsInAnyOrder(new[] { "00001000", "00001001" }, new[] { request5, request6 }.Select(x => x.ABR_InternalReference));
			}
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
