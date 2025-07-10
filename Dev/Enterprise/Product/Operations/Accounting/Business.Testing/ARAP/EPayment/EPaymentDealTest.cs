using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(EPaymentDeal))]
	public class EPaymentDealTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCurrentDealIsResetWhenANewDealIsSaved()
		{
			var deal1 = TestObjectCreator.CreateValidEPaymentDealForStatus(EPaymentStatusCodes.Deal.Queued);
			var paymentApprovalPK = deal1.Quote.PaymentApproval.PK;
			var paymentApprovalAsPaymentApprovalBase = Factory.Load<PaymentApprovalBase>(paymentApprovalPK);
			var paymentApprovalAsPaymentApprovalWithAuthorisation = Factory.Load<PaymentApprovalWithAuthorisation>(paymentApprovalPK);
			var paymentApprovalAsAPPaymentApprovalWithoutAuthorisation = Factory.Load<APPaymentApprovalWithoutAuthorisation>(paymentApprovalPK);

			var paymentApprovalObejctsAlreadyLoadedInFactory = Factory.GetBizOsForPK(deal1.Quote.PaymentApproval.PK.ToGuid());
			AssertEquals("Same approval should be loaded as 3 different business object types", 3, paymentApprovalObejctsAlreadyLoadedInFactory.Length);

			AssertNull("Deal is not saved yet, current deal must be null for PaymentApprovalBase.", paymentApprovalAsPaymentApprovalBase.CurrentDeal);
			AssertNull("Deal is not saved yet, current deal must be null for PaymentApprovalWithAuthorisation.", paymentApprovalAsPaymentApprovalWithAuthorisation.CurrentDeal);
			AssertNull("Deal is not saved yet, current deal must be null for APPaymentApprovalWithoutAuthorisation.", paymentApprovalAsAPPaymentApprovalWithoutAuthorisation.CurrentDeal);

			Factory.Save();

			AssertNull("Deal is saved, current deal is reset to null for PaymentApprovalBase.", paymentApprovalAsPaymentApprovalBase.CurrentDeal_ForTestOnly);
			AssertNull("Deal is saved, current deal is reset to null for PaymentApprovalWithAuthorisation.", paymentApprovalAsPaymentApprovalWithAuthorisation.CurrentDeal_ForTestOnly);
			AssertNull("Deal is saved, current deal is reset to null for APPaymentApprovalWithoutAuthorisation.", paymentApprovalAsAPPaymentApprovalWithoutAuthorisation.CurrentDeal_ForTestOnly);

			AssertEquals("Current deal should be loaded from DB.", deal1.PK, paymentApprovalAsPaymentApprovalBase.CurrentDeal.PK);
			AssertEquals("Current deal should be loaded from DB.", deal1.PK, paymentApprovalAsPaymentApprovalWithAuthorisation.CurrentDeal.PK);
			AssertEquals("Current deal should be loaded from DB.", deal1.PK, paymentApprovalAsAPPaymentApprovalWithoutAuthorisation.CurrentDeal.PK);

			deal1.AED_Status = EPaymentStatusCodes.Deal.Cancelled;
			Factory.Save();

			AssertNotNull("Saving the same deal with a different status should not reset current deal for PaymentApprovalBase.", paymentApprovalAsPaymentApprovalBase.CurrentDeal_ForTestOnly);
			AssertNotNull("Saving the same deal with a different status should not reset current deal for PaymentApprovalWithAuthorisation.", paymentApprovalAsPaymentApprovalWithAuthorisation.CurrentDeal_ForTestOnly);
			AssertNotNull("Saving the same deal with a different status should not reset current deal for APPaymentApprovalWithoutAuthorisation.", paymentApprovalAsAPPaymentApprovalWithoutAuthorisation.CurrentDeal_ForTestOnly);

			AssertEquals("Current deal should remain the same for PaymentApprovalBase.", deal1.PK, paymentApprovalAsPaymentApprovalBase.CurrentDeal.PK);
			AssertEquals("Current deal should remain the same for PaymentApprovalWithAuthorisation.", deal1.PK, paymentApprovalAsPaymentApprovalWithAuthorisation.CurrentDeal.PK);
			AssertEquals("Current deal should remain the same for APPaymentApprovalWithoutAuthorisation.", deal1.PK, paymentApprovalAsAPPaymentApprovalWithoutAuthorisation.CurrentDeal.PK);

			var deal2 = TestObjectCreator.CreateValidEPaymentDealForStatus(EPaymentStatusCodes.Deal.Queued, paymentApprovalAsPaymentApprovalBase);
			deal2.AED_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(10); // to ensure this is newer than previous deal.
			Factory.Save();

			AssertNull("Saving a new deal with QUE status should reset current deal for PaymentApprovalBase.", paymentApprovalAsPaymentApprovalBase.CurrentDeal_ForTestOnly);
			AssertNull("Saving a new deal with QUE status should reset current deal for PaymentApprovalWithAuthorisation.", paymentApprovalAsPaymentApprovalWithAuthorisation.CurrentDeal_ForTestOnly);
			AssertNull("Saving a new deal with QUE status should reset current deal for APPaymentApprovalWithoutAuthorisation.", paymentApprovalAsAPPaymentApprovalWithoutAuthorisation.CurrentDeal_ForTestOnly);

			AssertEquals("New current deal should be loaded from DB for PaymentApprovalBase.", deal2.PK, paymentApprovalAsPaymentApprovalBase.CurrentDeal.PK);
			AssertEquals("New current deal should be loaded from DB for PaymentApprovalWithAuthorisation.", deal2.PK, paymentApprovalAsPaymentApprovalWithAuthorisation.CurrentDeal.PK);
			AssertEquals("New current deal should be loaded from DB for APPaymentApprovalWithoutAuthorisation.", deal2.PK, paymentApprovalAsAPPaymentApprovalWithoutAuthorisation.CurrentDeal.PK);
		}

		public void TestDealReferenceIsSetOnSaving()
		{
			var company1 = TestObjectCreator.CreateNewCompany("CC1");
			var branch1 = TestObjectCreator.CreateBranch("BB1", company1);
			var branch2 = TestObjectCreator.CreateBranch("BB2", company1);

			var company2 = TestObjectCreator.CreateNewCompany("CC2");
			var branch3 = TestObjectCreator.CreateBranch("BB3", company2);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var deal1 = TestObjectCreator.CreateValidEPaymentDealForStatus(EPaymentStatusCodes.Deal.Queued);
				var deal2 = TestObjectCreator.CreateValidEPaymentDealForStatus(EPaymentStatusCodes.Deal.Queued);
				Factory.Save();
				AssertContainsExactElementsInAnyOrder(new[] { "00001000", "00001001" }, new[] { deal1, deal2 }.Select(x => x.AED_InternalReference));
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var deal3 = TestObjectCreator.CreateValidEPaymentDealForStatus(EPaymentStatusCodes.Deal.Queued);
				var deal4 = TestObjectCreator.CreateValidEPaymentDealForStatus(EPaymentStatusCodes.Deal.Queued);
				Factory.Save();
				AssertContainsExactElementsInAnyOrder(new[] { "00001002", "00001003" }, new[] { deal3, deal4 }.Select(x => x.AED_InternalReference));
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch3.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var deal5 = TestObjectCreator.CreateValidEPaymentDealForStatus(EPaymentStatusCodes.Deal.Queued);
				var deal6 = TestObjectCreator.CreateValidEPaymentDealForStatus(EPaymentStatusCodes.Deal.Queued);
				Factory.Save();
				AssertContainsExactElementsInAnyOrder(new[] { "00001000", "00001001" }, new[] { deal5, deal6 }.Select(x => x.AED_InternalReference));
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return new TestObjectCreator(factory).CreateValidEPaymentDealForStatus(EPaymentStatusCodes.Deal.Queued);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => TestObjectCreator.CreateValidEPaymentDealForStatus(EPaymentStatusCodes.Deal.Queued);

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
