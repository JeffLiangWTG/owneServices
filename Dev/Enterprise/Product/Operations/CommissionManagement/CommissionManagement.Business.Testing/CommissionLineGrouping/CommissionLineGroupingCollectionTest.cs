using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CommissionLineGroupingCollectionForTest))]
	internal class CommissionLineSourceGroupingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CommissionLineGroupingCollectionForTest>
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

		public void TestAddNotifications()
		{
			#region SetUp

			var collection1 = new CommissionLineGroupingCollectionForTest(Factory);
			var collection2 = new CommissionLineGroupingCollectionForTest(Factory);
			var company1 = Factory.New<GlbCompany>();
			var company2 = Factory.New<GlbCompany>();
			var fullPaymentMessageWhenRegistrySetToYes = "AR invoice(s) have to be fully paid prior to payment process action";
			var fullPaymentMessageWhenRegistrySetToNo = "AR Invoice(s) are not fully paid";

			var line1 = Factory.New<ViewCommissionLine>();
			line1.VCL_GC_Company = company1.PK;
			line1.VCL_Ledger = "AR";
			line1.VCL_TransactionType = "INV";
			line1.VCL_TransactionFullyPaidDate = ZDateTime.Empty;

			var line2 = Factory.New<ViewCommissionLine>();
			line2.VCL_GC_Company = company2.PK;

			var companyGrouper = new ViewCommissionLineLocalCompanyGrouper<ViewCommissionLine>();
			var genericGrouper = (ViewCommissionLineGrouper<ViewCommissionLine>)companyGrouper;

			collection1.AddNew(new[] { line1, line2 }, new[] { genericGrouper });
			collection2.AddNew(new[] { line1, line2 }, new[] { genericGrouper });

			var group1 = collection1[0];
			var group2 = collection2[0];
			AssertEquals(false, group1.IsLeaf);
			AssertEquals(false, group2.IsLeaf);

			#endregion

			collection1.AddFullPaymentNotifications(CargoWise.ComponentModel.NotificationType.Warning);
			AssertEquals("Warnings should appear in group level if it exists in line level.", true, group1.HasRowWarnings);

			var hasFullPaymentWarningWhenRegistrySetToYes = group1.RowWarnings.Cast<INotification>().Any(warning => warning.Message == fullPaymentMessageWhenRegistrySetToYes);
			Assert(hasFullPaymentWarningWhenRegistrySetToYes);

			using (OrganisationsDataRegistry.Instance.DisallowCommissionPaymentIfARInvoiceNotFullyPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				collection2.AddFullPaymentNotifications(CargoWise.ComponentModel.NotificationType.Warning);
				AssertEquals("Warnings should appear in group level if it exists in line level", true, group2.HasRowWarnings);

				var hasFullPaymentWarningWhenRegistrySetToNo = group2.RowWarnings.Cast<INotification>().Any(warning => warning.Message == fullPaymentMessageWhenRegistrySetToNo);
				Assert(hasFullPaymentWarningWhenRegistrySetToNo);
			}
		}

		#region Implementation

		protected override CommissionLineGroupingCollectionForTest GetCollectionToTest()
		{
			return new CommissionLineGroupingCollectionForTest(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var grouping = new CommissionLineGroupingForTest(Factory);
			grouping.Init(new[] { Factory.New<ViewCommissionLine>() });
			return grouping;
		}

		#endregion
	}

	class CommissionLineGroupingCollectionForTest : CommissionLineGroupingCollection<CommissionLineGroupingForTest, ViewCommissionLine>
	{
		public CommissionLineGroupingCollectionForTest(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override CommissionLineGroupingForTest CreateNew(ViewCommissionLineGrouper<ViewCommissionLine>[] subGroupers)
		{
			return new CommissionLineGroupingForTest(Factory, subGroupers);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CommissionLineGroupingForTest(Factory);
		}
	}
}
