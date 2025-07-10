using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(CartageAdviceHelper))]
	sealed class CartageAdviceHelperTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2006, 04, 17, 01, 02, 03)]
		public void TestDateAsUniqueIdentifier()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				OrgHeader header = Factory.New<OrgHeader>();
				GlbCompany.CurrentCompany.SetCountry("GB");

				DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
				MockIDocCartageAdvice cartageAdviceParent = new MockIDocCartageAdvice(dummy, Factory);
				cartageAdviceParent.IsAir = false;
				AssertEquals("UniqueIdentifier Should be empty", "", cartageAdviceParent.CartageAdvice.DateAsUniqueIdentifier);

				cartageAdviceParent = new MockIDocCartageAdvice(dummy, Factory);
				cartageAdviceParent.IsAir = true;
				AssertEquals("UniqueIdentifier Should be 20060417010203", "20060417010203", cartageAdviceParent.CartageAdvice.DateAsUniqueIdentifier);

				GlbCompany.CurrentCompany.SetCountry("AU");
				cartageAdviceParent = new MockIDocCartageAdvice(dummy, Factory);
				cartageAdviceParent.IsAir = true;
				AssertEquals("UniqueIdentifier Should be empty", "", cartageAdviceParent.CartageAdvice.DateAsUniqueIdentifier);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			MockIDocCartageAdvice mockObj = new MockIDocCartageAdvice(dummy, Factory);
			return new CartageAdviceHelper(mockObj, Factory);
		}

		#endregion
	}
}
