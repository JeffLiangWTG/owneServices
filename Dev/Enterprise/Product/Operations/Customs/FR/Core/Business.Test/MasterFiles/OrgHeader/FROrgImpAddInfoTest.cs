using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MasterFiles.Testing
{
	[TestedType(typeof(FROrgImpAddInfo))]
	public class FROrgImpAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestZO_VatProcedureDateLimit()
		{
			var resourceStrings = DataBoundResourceStrings.GetDataForProperty(new FROrgImpAddInfo(Factory).ZO_VATProcedureDateLimitInfo);
			AssertEquals("VAT Procedure Date Limit", resourceStrings.Caption);
			AssertEquals("Date", resourceStrings.ShortCaption);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<OrgHeader>();
			var frOrgImpAddInfo = FROrgImpAddInfo.Get(header);
			return frOrgImpAddInfo;
		}

		public void TestSetDefaultValues()
		{
			var header = Factory.New<OrgHeader>();
			var frOrgImpAddInfo = FROrgImpAddInfo.Get(header);
			AssertEquals(VATProcedureList.Codes.S, frOrgImpAddInfo.ZO_VATDeferType);
		}
	}
}
