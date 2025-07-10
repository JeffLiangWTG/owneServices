using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AUExportClassificationFilterBusinessObject))]
	sealed class AUExportClassificationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestTariffFilter()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var exportClass = Factory.New<Classification>();
				exportClass.CC_ClassificationType = "EXP";
				exportClass.CC_TariffNum = "1234.56.78";
				AUTariffModuleFilter tariffFilter = (AUTariffModuleFilter)filterBO["Tariff No"];
				tariffFilter.CC_TariffNum = "12345678";
				tariffFilter.IsActive = true;
				AssertEquals(tariffFilter.CC_TariffNum, "1234.56.78");
				Classification[] found = (Classification[])Factory.Load(typeof(Classification), filterBO.Filter);
				AssertEquals("There should be one record found", 1, found.Length);
				AssertEquals("1234.56.78", found[0].CC_TariffNum);
				tariffFilter.CC_TariffNum = "1234.56.78";
				found = (Classification[])Factory.Load(typeof(Classification), filterBO.Filter);
				AssertEquals("There should be one record found", 1, found.Length);
				AssertEquals("1234.56.78", found[0].CC_TariffNum);
				tariffFilter.CC_TariffNum = "1234";
				found = (Classification[])Factory.Load(typeof(Classification), filterBO.Filter);
				AssertEquals("There should be one record found", 1, found.Length);
				AssertEquals("1234.56.78", found[0].CC_TariffNum);
				tariffFilter.CC_TariffNum = "1235";
				found = (Classification[])Factory.Load(typeof(Classification), filterBO.Filter);
				AssertEquals("There should be no records found", 0, found.Length);
			}
		}

		public void TestTariffFilter_AHECC()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var exportClass = Factory.New<Classification>();
				exportClass.CC_ClassificationType = "EXP";
				exportClass.CC_TariffNum = "1234.56.78";
				AUTariffModuleFilter tariffFilter = (AUTariffModuleFilter)filterBO["Tariff No"];
				tariffFilter.CC_TariffNum = "12345678";
				tariffFilter.IsActive = true;
				AssertEquals(tariffFilter.CC_TariffNum, "1234.56.78");
				Classification[] found = (Classification[])Factory.Load(typeof(Classification), filterBO.Filter);
				AssertEquals("There should be one record found", 1, found.Length);
				AssertEquals("1234.56.78", found[0].CC_TariffNum);
				tariffFilter.CC_TariffNum = "1234.56.78";
				found = (Classification[])Factory.Load(typeof(Classification), filterBO.Filter);
				AssertEquals("There should be one record found", 1, found.Length);
				AssertEquals("1234.56.78", found[0].CC_TariffNum);
				tariffFilter.CC_TariffNum = "1234";
				found = (Classification[])Factory.Load(typeof(Classification), filterBO.Filter);
				AssertEquals("There should be one record found", 1, found.Length);
				AssertEquals("1234.56.78", found[0].CC_TariffNum);
				tariffFilter.CC_TariffNum = "1235";
				found = (Classification[])Factory.Load(typeof(Classification), filterBO.Filter);
				AssertEquals("There should be no records found", 0, found.Length);
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new AUExportClassificationFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			filterBO = new AUExportClassificationFilterBusinessObject();
		}

		AUExportClassificationFilterBusinessObject filterBO;
	}
}
