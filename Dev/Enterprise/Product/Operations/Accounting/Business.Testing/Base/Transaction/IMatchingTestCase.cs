using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[TestsSubclassesOf(typeof(IMatching))]
	public abstract class IMatchingTestCase : TestCaseWithFactory
	{
		protected abstract IMatching GetNewIMatching(ZGuid branchPK, ZGuid organisationPK, ZString currencyCode);

		protected virtual bool IsShownOnMatchingForm
		{
			get { return true; }
		}

		public void TestBranchAndOrganisationPropertiesHaveListAttributeSetupCorrectly()
		{
			if (IsShownOnMatchingForm)
			{
				TestObjectCreator testObjCreator = new TestObjectCreator(Factory);

				ZGuid branchPK = testObjCreator.NonCurrentBranch.PK;
				ZGuid organisationPK = testObjCreator.LocalClient.PK;
				ZString currencyCode = testObjCreator.USD.RX_Code;
				ZString currencyCodeDesc = testObjCreator.USD.RX_Desc;

				IMatching transaction = GetNewIMatching(branchPK, organisationPK, currencyCode);

				AssertEquals("Precondition: Transaction Branch should be set correctly", branchPK, transaction.BranchGuid);
				AssertEquals("Precondition: Transaction Organisation should be set correctly", organisationPK, transaction.Organisation);
				AssertEquals("Precondition: Transaction Currency should be set correctly", currencyCode, transaction.CurrencyCode);

				AssertCodeFromPrimarykey("Branch", transaction.BranchGuidInfo, testObjCreator.NonCurrentBranch.GB_Code);
				AssertCodeFromPrimarykey("Organisation", transaction.OrganisationInfo, testObjCreator.LocalClient.OH_Code);
				AssertCodeFromNaturalkey("Currency", transaction.PaymentCurrencyCodeInfo, currencyCodeDesc);
			}
			else
			{
				Assert("Transfer and Contra should not have list attributes", !IsShownOnMatchingForm);
			}
		}

		public void TestWHTColumnsReadonlyness()
		{
			var instance = GetNewIMatching(ZGuid.Empty, ZGuid.Empty, ZString.Empty);
			Assert(instance.NotionalWHTTaxInfo.ReadOnly);
			Assert(instance.RealizedWTHTaxInfo.ReadOnly);
		}

		static void AssertCodeFromPrimarykey(ZString description, ZPropertyInfo propertyInfo, ZString expectedCode)
		{
			var listProvider = MetaData.GetListDataSource(propertyInfo.BizObj, propertyInfo.PropertyDescriptor) as IFindBoxListProvider;
			Assert("List Attribute has not been applied properly for this property: [" + description + "]", listProvider != null);
			string branchCode = listProvider.CodeFromPrimaryKey((ZGuid)propertyInfo.Value);
			AssertEquals(description, expectedCode, branchCode);
		}

		static void AssertCodeFromNaturalkey(ZString description, ZPropertyInfo propertyInfo, ZString expectedCodeDesc)
		{
			var listProvider = MetaData.GetListDataSource(propertyInfo.BizObj, propertyInfo.PropertyDescriptor) as IFindBoxListProvider;
			Assert("List Attribute has not been applied properly for this property: [" + description + "]", listProvider != null);
			string currencyCodeDesc = listProvider.DescriptionFromCode((ZString)propertyInfo.Value);
			AssertEquals(description, expectedCodeDesc, currencyCodeDesc);
		}
	}
}
