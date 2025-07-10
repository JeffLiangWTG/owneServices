using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(AdditionalInfo))]
	sealed class AdditionalInfoBaseOnlyTest : AdditionalInfoTest<AdditionalInfo>
	{
		public void TestMaxLength()
		{
			var additionalInfo = Factory.New<AdditionalInfo>();
			CombineAssertions(() =>
			{
				AssertEquals(5, additionalInfo.CSI_CodeInfo.MaxLength);
				AssertEquals(512, additionalInfo.CSI_DescriptionInfo.MaxLength);
			});
		}

		public void TestCaptionResourceString()
		{
			var additionalInfo = Factory.New<AdditionalInfo>();
			CombineAssertions(() =>
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(additionalInfo.CSI_CodeInfo,
					(string[])null, "Code");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(additionalInfo.AdditionalInfoDescriptionInfo,
					(string[])null, "Description");
			});
		}

		public void TestDataGrouping()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.FrenchGuyana))
			{
				var header = Factory.New<AsycudaManifestHeader>();
				var bill = header.Bills.AddNew();
				var additionalInfo = bill.AdditionalInfos.AddNew();
				AssertEquals("additionalInfo.DataGrouping", Constants.CountryCodes.France, additionalInfo.DataGrouping);
			}
		}
	}

	[TestsSubclassesOf(typeof(AdditionalInfo))]
	public abstract class AdditionalInfoTest<T> : CusSupportingInfoTest<T>
		where T : AdditionalInfo
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.AdditionalInfos.AddNew();
		}

		protected override IEnumerable<T> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			yield return (T)bill.AdditionalInfos.AddNew();
		}
	}
}
