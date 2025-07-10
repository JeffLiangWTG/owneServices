using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CommissionAgreementApprovalItemCollection))]
	internal class CommissionAgreementApprovalItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CommissionAgreementApprovalItemCollection>
	{
		#region Add / Remove

		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.AllowRemove);
		}

		#endregion

		#region Load

		public void TestLoad()
		{
			var commissionAgreements = new OrgCommissionAgreementCollection(Factory);
			var agreement1 = commissionAgreements.AddNew();
			var agreement2 = commissionAgreements.AddNew();
			var agreement3 = commissionAgreements.AddNew();

			var collection = new CommissionAgreementApprovalItemCollection(new CommissionAgreementApprovalWizard(Factory));
			collection.Load(commissionAgreements);

			AssertContainsExactElementsInAnyOrder(
				new[] { agreement1, agreement2, agreement3 },
				collection.Cast<CommissionAgreementApprovalItem>().Select(x => x.CommissionAgreement));
		}

		#endregion

		#region Delete

		public void TestItemIsRemovedFromCollectionWhenWrappedAgreementIsDeleted()
		{
			var agreementCollection = new OrgCommissionAgreementCollection(Factory);
			var agreement1 = agreementCollection.AddNew();
			var agreement2 = agreementCollection.AddNew();

			var wizard = new CommissionAgreementApprovalWizard(Factory);
			var itemCollection = new CommissionAgreementApprovalItemCollection(wizard);
			itemCollection.Load(agreementCollection);

			AssertEquals(2, itemCollection.Count);
			var item1 = itemCollection.Cast<CommissionAgreementApprovalItem>().Single(x => x.CommissionAgreement == agreement1);
			var item2 = itemCollection.Cast<CommissionAgreementApprovalItem>().Single(x => x.CommissionAgreement == agreement2);

			agreement1.Delete();

			AssertEquals(true, item1.IsDeleted);
			AssertEquals(false, item2.IsDeleted);
			AssertEquals(1, itemCollection.Count);
		}

		#endregion

		#region Implementation

		protected override CommissionAgreementApprovalItemCollection GetCollectionToTest()
		{
			return new CommissionAgreementApprovalItemCollection(new CommissionAgreementApprovalWizard(Factory));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CommissionAgreementApprovalItem(Wizard, Factory.New<OrgCommissionAgreement>());
		}

		CommissionAgreementApprovalWizard Wizard
		{
			get { return wizard ?? (wizard = new CommissionAgreementApprovalWizard(Factory)); }
		}
		CommissionAgreementApprovalWizard wizard;

		#endregion
	}
}
