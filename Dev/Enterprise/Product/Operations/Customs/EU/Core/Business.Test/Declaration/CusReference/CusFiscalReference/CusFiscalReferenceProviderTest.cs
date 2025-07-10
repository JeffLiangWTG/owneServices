using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class CusFiscalReferenceProviderBaseOnlyTest : TestCaseWithFactory
	{
		public void TestGetByDataGroupingCode()
		{
			var provider = CusFiscalReferenceProvider.GetByDataGroupingCode(Core.Constants.CountryCodes.Latvia);
			CombineAssertions(() =>
			{
				AssertNotNull("Not Null", provider);
				AssertEquals("DataGroupingCode", Core.Constants.CountryCodes.Latvia, provider.DataGroupingCode);
			});
		}

		public void TestGetByDataGroupingCode_DataGroupingCodeIsEmpty()
		{
			var provider = CusFiscalReferenceProvider.GetByDataGroupingCode(ZString.Empty);
			CombineAssertions(() =>
			{
				AssertNotNull("Not Null", provider);
				AssertEquals("DataGroupingCode", ZString.Empty, provider.DataGroupingCode);
			});
		}

		public void TestGetNewLookups()
		{
			AssertType<CusFiscalReferenceLookups>(cusFiscalReference.Provider.GetNewLookups(cusFiscalReference));
		}

		public void TestGetNewValidation()
		{
			AssertType<CusFiscalReferenceValidation>(cusFiscalReference.Provider.GetNewValidation(cusFiscalReference));
		}

		public void TestRecalculateOwnerIfNeeded()
		{
			cusFiscalReference.CFR_Reference = "12345";
			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;

			AssertEquals("Reference not changed", "12345", cusFiscalReference.CFR_Reference);
		}

		public void TestRecalculateReferenceIfNeeded()
		{
			var organisation = Factory.New<OrgHeader>();
			var orgAddress = organisation.Addresses.AddNew();
			cusFiscalReference.CFR_OA_Owner = orgAddress.PK;
			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;

			AssertEquals("Owner not changed", orgAddress.PK, cusFiscalReference.CFR_OA_Owner);
		}

		public void TestReferenceIsReadOnly()
		{
			AssertEquals("Not ReadOnly", false, cusFiscalReference.CFR_ReferenceInfo.ReadOnly);
		}

		public void TestOwnerIsReadOnly()
		{
			AssertEquals("Not ReadOnly", false, cusFiscalReference.CFR_OA_OwnerInfo.ReadOnly);
		}

		public void TestGetReferenceMaxLength()
		{
			AssertEquals("CFR_Reference MaxLength", 35, cusFiscalReference.CFR_ReferenceInfo.MaxLength);
		}

		protected override void SetUp()
		{
			base.SetUp();

			cusFiscalReference = Factory.New<CusFiscalReference>();
		}
		CusFiscalReference cusFiscalReference;
	}
}
