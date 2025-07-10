using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientLicenceFeeCollection))]
	internal class ClientLicenceFeeCollectionTest : ActiveBusinessObjectCollectionTestCase<ClientLicenceFeeCollection>
	{
		public void TestRelationshipDefaultsForNewElement()
		{
			ClientLicenceFee item = Collection.AddNew();
			AssertEquals("Master", Master.PK, item.L8_LC);
		}

		public void TestAllowNew()
		{
			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = true;
			AssertEquals(true, ((IBindingList)Collection).AllowNew);

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = false;
			AssertEquals(false, ((IBindingList)Collection).AllowNew);
		}

		public void TestGetMatched()
		{
			ZDateTime billingDate = new ZDateTime(2010, 12, 1);

			ClientLicenceFee matchedFee1 = CreateFee(Collection, billingDate, billingDate);
			ClientLicenceFee matchedFee2 = CreateFee(Collection, billingDate.AddMonths(-1), billingDate.AddMonths(1));
			ClientLicenceFee matchedFee3 = CreateFee(Collection, billingDate, billingDate.AddMonths(1));
			ClientLicenceFee matchedFee4 = CreateFee(Collection, ZDateTime.Empty, ZDateTime.Empty);

			ClientLicenceFee notMatchedFee1 = CreateFee(Collection, billingDate.AddMonths(1), billingDate);
			ClientLicenceFee notMatchedFee2 = CreateFee(Collection, billingDate.AddMonths(1), billingDate.AddMonths(2));
			ClientLicenceFee notMatchedFee3 = CreateFee(Collection, billingDate.AddMonths(-2), billingDate.AddMonths(-1));

			IEnumerable<ClientLicenceFee> matchedFees = Collection.GetMatched(billingDate);
			AssertContainsExactElementsInAnyOrder(new ClientLicenceFee[] { matchedFee1, matchedFee2, matchedFee3, matchedFee4 }, matchedFees);
		}

		ClientLicenceFee CreateFee(ClientLicenceFeeCollection collection, ZDateTime startDate, ZDateTime endDate)
		{
			ClientLicenceFee result = collection.AddNew();
			result.L8_StartDate = startDate;
			result.L8_EndDate = endDate;

			return result;
		}

		#region Implementation

		LicenceCompany Master;

		protected override ClientLicenceFeeCollection GetCollectionToTest()
		{
			Master = Factory.New<LicenceCompany>();
			return new ClientLicenceFeeCollection(Master);
		}

		#endregion
	}
}
