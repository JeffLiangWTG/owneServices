using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public class OrganizationsForCreditCheckWithAmountsCollectionTest : TestCase
	{
		public void TestIsSubsetOf()
		{
			var pk1 = ZGuid.NewZGuid();
			var pk2 = ZGuid.NewZGuid();

			var baseLineCollection = new OrganizationsForCreditCheckWithAmountsCollection();
			var baseLineItem1 = new OrganizationsForCreditCheckWithAmounts();
			baseLineItem1.OrgPK = pk1;
			baseLineItem1.PostedAmount = 100M;
			baseLineItem1.UnpostedAmount = 200M;

			var baseLineItem2 = new OrganizationsForCreditCheckWithAmounts();
			baseLineItem2.OrgPK = pk2;
			baseLineItem2.PostedAmount = 100M;
			baseLineItem2.UnpostedAmount = 200M;

			baseLineCollection.Add(baseLineItem1);
			baseLineCollection.Add(baseLineItem2);

			var collectionToMatch = new OrganizationsForCreditCheckWithAmountsCollection();

			AssertEquals("CollectionToMatch is not a subset as it is empty (onlyIfAmountIncreases = ture)", false, collectionToMatch.IsSubsetOf(baseLineCollection, true));
			AssertEquals("CollectionToMatch is not a subset as it is empty (onlyIfAmountIncreases = false)", false, collectionToMatch.IsSubsetOf(baseLineCollection, false));

			var itemToMatch1 = new OrganizationsForCreditCheckWithAmounts();
			itemToMatch1.OrgPK = pk1;
			itemToMatch1.PostedAmount = 50M;
			itemToMatch1.UnpostedAmount = 200M;

			collectionToMatch.Add(itemToMatch1);

			AssertEquals("CollectionToMatch is a subset as total amount for baseLineItem1 is more than the amount in itemToMatch1 (onlyIfAmountIncreases = true)", true, collectionToMatch.IsSubsetOf(baseLineCollection, true));
			AssertEquals("CollectionToMatch is not a subset as the amounts need to match to the cent for the same OrgPK (onlyIfAmountIncreases = false)", false, collectionToMatch.IsSubsetOf(baseLineCollection, false));

			itemToMatch1.UnpostedAmount = 250M;

			AssertEquals("CollectionToMatch is a subset as total amount for baseLineItem1 is exactly same for total amount in itemToMatch1 (onlyIfAmountIncreases = true)", true, collectionToMatch.IsSubsetOf(baseLineCollection, true));
			AssertEquals("CollectionToMatch is a subset as total amount for baseLineItem1 is exactly same for total amount in itemToMatch1 (onlyIfAmountIncreases = false)", true, collectionToMatch.IsSubsetOf(baseLineCollection, false));

			itemToMatch1.UnpostedAmount = 350M;

			AssertEquals("CollectionToMatch is not a subset as total amount for baseLineItem1 is less than total amount in itemToMatch1 (onlyIfAmountIncreases = true)", false, collectionToMatch.IsSubsetOf(baseLineCollection, true));
			AssertEquals("CollectionToMatch is not a subset as the amounts need to match to the cent for the same OrgPK (onlyIfAmountIncreases = false)", false, collectionToMatch.IsSubsetOf(baseLineCollection, false));

			var itemToMatch2 = new OrganizationsForCreditCheckWithAmounts();
			itemToMatch2.OrgPK = ZGuid.NewZGuid();
			itemToMatch2.PostedAmount = 100M;
			itemToMatch2.UnpostedAmount = 200M;

			AssertEquals("CollectionToMatch is not a subset as it contains item which is not in the base line collection (onlyIfAmountIncreases = true)", false, collectionToMatch.IsSubsetOf(baseLineCollection, true));
			AssertEquals("CollectionToMatch is not a subset as it contains item which is not in the base line collection (onlyIfAmountIncreases = false)", false, collectionToMatch.IsSubsetOf(baseLineCollection, false));
		}
	}
}