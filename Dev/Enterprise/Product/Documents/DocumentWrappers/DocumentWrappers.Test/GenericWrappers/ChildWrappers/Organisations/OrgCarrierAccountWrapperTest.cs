using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(OrgCarrierAccountWrapper))]
	sealed class OrgCarrierAccountWrapperTest : GenericWrapperTest
	{
		#region TestMerchantID

		public void TestMerchantID()
		{
			var carrierAccount = Factory.NewWithValidTestData<OrgCarrierAccount>();
			carrierAccount.OAN_MerchantNumber = "2000";

			var wrapper = new OrgCarrierAccountWrapper(carrierAccount, Factory);
			AssertEquals("MerchangID", "2000", wrapper.MerchantID);
		}

		#endregion

		#region TestMerchantLocationID

		public void TestMerchantLocationID()
		{
			var carrierAccount = Factory.NewWithValidTestData<OrgCarrierAccount>();
			carrierAccount.OAN_DepotID = "MEL";

			var wrapper = new OrgCarrierAccountWrapper(carrierAccount, Factory);
			AssertEquals("MerchangLocationID", "MEL", wrapper.MerchantLocationID);
		}

		#endregion

		#region TestAccountNumber

		public void TestAccountNumber()
		{
			var carrierAccount = Factory.NewWithValidTestData<OrgCarrierAccount>();
			carrierAccount.OAN_AccountNumber = "12345678";

			var wrapper = new OrgCarrierAccountWrapper(carrierAccount, Factory);
			AssertEquals("AccountNumber", "12345678", wrapper.AccountNumber);
		}

		#endregion

		#region Implementation

		public override void TestWrapperMappingsEmpty()
		{
			var wrapper = (OrgCarrierAccountWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapper.MerchantID", "", wrapper.MerchantID);
			AssertEquals("wrapper.MerchantLocationID", "", wrapper.MerchantLocationID);
			AssertEquals("wrapper.AccountNumber", "", wrapper.AccountNumber);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var carrierAccount = Factory.NewWithValidTestData<OrgCarrierAccount>();
			return new OrgCarrierAccountWrapper(carrierAccount, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
OrgCarrierAccount
======================================================================
Name                                    Type
----------------------------------------------------------------------
AccountNumber                           String
MerchantID                              String
MerchantLocationID                      String
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var carrierAccount = Factory.New<OrgCarrierAccount>();
			return new OrgCarrierAccountWrapper(carrierAccount, Factory);
		}

		#endregion
	}
}
