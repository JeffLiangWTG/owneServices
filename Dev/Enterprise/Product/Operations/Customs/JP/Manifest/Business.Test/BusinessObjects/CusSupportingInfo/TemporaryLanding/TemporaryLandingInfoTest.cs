using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(TemporaryLandingInfo))]
	sealed class TemporaryLandingInfoTest : Customs.Business.Testing.CusSupportingInfoTest<TemporaryLandingInfo>
	{
		public void TestCSI_ReferenceNumber()
		{
			var supporting = Factory.New<TemporaryLandingInfo>();
			AssertHasCustomAttribute<ListAttribute>(supporting.GetType(), TemporaryLandingInfo.Schema.CSI_ReferenceNumber, false, attrib => attrib.ListDataSourceMember == "Lookups.BondedTransportList");
		}

		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<TemporaryLandingInfo>();
			CombineAssertions(() =>
			{
				AssertEquals(CusSupportingInfoTypeList.Codes.ApprovalCertificate, supporting.CSI_Type);
				AssertEquals(AsycudaBillSchema.Constants.Prefix, supporting.CSI_ParentTableCode);
			});
		}

		public void TestLookups()
		{
			AssertType<TemporaryLandingInfoLookups>(((TemporaryLandingInfo)GetNewBusinessObject()).Lookups);
		}

		public void TestValidation()
		{
			AssertType<TemporaryLandingInfoValidation>(((TemporaryLandingInfo)GetNewBusinessObject()).Validation);
		}

		public void TestDeleteEmptyRowIfNeeded()
		{
			var supportingInfo = GetBizObjsForCorrectlyTypeDecideTest(Factory).First();
			supportingInfo.CSI_Code = "POS";
			supportingInfo.CSI_ItemNumber = 21;
			supportingInfo.CSI_DateOfIssue = ZDate.Today;
			supportingInfo.CSI_DateOfExpiry = ZDate.Today.AddDays(20);
			supportingInfo.CSI_ReferenceNumber = "6";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var parentPK = supportingInfo.Parent.PK;
			var query = new ZDBOnlyQuery(typeof(CusSupportingInfo));
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, parentPK);
			query.AddToFilter(CusSupportingInfoSchema.CSI_Type, supportingInfo.CSI_Type);
			var supportingInfoInDB = newFactory.Load(typeof(CusSupportingInfo), query);
			AssertEquals(1, supportingInfoInDB.Length);
			AssertEquals(supportingInfo.PK, supportingInfoInDB.Single().PK);

			supportingInfo.CSI_CodeInfo.ClearValue();
			supportingInfo.CSI_ItemNumberInfo.ClearValue();
			supportingInfo.CSI_DateOfIssueInfo.ClearValue();
			supportingInfo.CSI_DateOfExpiryInfo.ClearValue();
			supportingInfo.CSI_ReferenceNumberInfo.ClearValue();
			Factory.Save();
			newFactory = new BusinessObjectFactory();
			supportingInfoInDB = newFactory.Load(typeof(CusSupportingInfo), query);
			AssertEquals("The empty row was deleted", 0, supportingInfoInDB.Length);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetBizObjsForCorrectlyTypeDecideTest(Factory).First();
		}

		protected override IEnumerable<TemporaryLandingInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var testObj = bill.TemporaryLandingInfoCollection.AddNew();
			testObj.CSI_Code = "POS";
			yield return testObj;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetBizObjsForCorrectlyTypeDecideTest(factory).First();
		}
	}
}
