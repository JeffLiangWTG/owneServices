using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(WebRefCommodityCodeFilterBusinessObject))]
	sealed class WebRefCommodityCodeFilterBusinessObjectTest : FilterBusinessObjectTestCase
	{
		RefCommodityCode testCommodityCode;

		public void TestFilterDoesNotReturnInactiveCodes()
		{
			TestCaseHelper.ClearTable(RefCommodityCodeSchema.Constants.TableName);

			// Make the test commodity code inactive as well
			testCommodityCode.RH_IsActive = ZBool.False;

			RefCommodityCode code1 = Factory.New<RefCommodityCode>();
			code1.RH_IsActive = ZBool.False;
			RefCommodityCode code2 = Factory.New<RefCommodityCode>();
			RefCommodityCode code3 = Factory.New<RefCommodityCode>();
			code3.RH_IsActive = ZBool.False;
			RefCommodityCode code4 = Factory.New<RefCommodityCode>();
			RefCommodityCode code5 = Factory.New<RefCommodityCode>();
			code5.RH_IsActive = ZBool.False;
			RefCommodityCode code6 = Factory.New<RefCommodityCode>();

			WebRefCommodityCodeFilterBusinessObject filterBizO = GetNewBusinessObject() as WebRefCommodityCodeFilterBusinessObject;
			RefCommodityCodeCollection collection = new RefCommodityCodeCollection(Factory);
			collection.AdditionalFilter = filterBizO.Filter;

			AssertEquals("Collection should contain 3 codes", 3, collection.Count);
			Assert("Collection should contain Code2", collection.Contains(code2));
			Assert("Collection should contain Code4", collection.Contains(code4));
			Assert("Collection should contain Code6", collection.Contains(code6));
		}

		public void TestFilter_DescriptionDoesNotExceedCodeLength()
		{
			var code = Factory.New<RefCommodityCode>();
			code.RH_Description = "LONGDESCRIPTION";
			code.RH_Code = "ABCD";
			var filterBizO = GetNewBusinessObject() as WebRefCommodityCodeFilterBusinessObject;
			var collection = new RefCommodityCodeCollection(Factory);

			filterBizO.RH_Description = "SOMETHINGELSE";
			collection.AdditionalFilter = filterBizO.Filter;
			Assert("Collection does not contain code", !collection.Contains(code));

			filterBizO.RH_Description = "ABCD";
			collection.AdditionalFilter = filterBizO.Filter;
			Assert("Collection should contain code", collection.Contains(code));

			filterBizO.RH_Description = "LONGDESCRIPTION";
			collection.AdditionalFilter = filterBizO.Filter;
			Assert("Collection should contain code", collection.Contains(code));
		}

		protected override void SetUp()
		{
			base.SetUp();
			testCommodityCode = Factory.New<RefCommodityCode>();
			testCommodityCode.RH_Code = "CAN";
			testCommodityCode.RH_Description = "CANDIES";
			Factory.Save();
		}
	}
}
