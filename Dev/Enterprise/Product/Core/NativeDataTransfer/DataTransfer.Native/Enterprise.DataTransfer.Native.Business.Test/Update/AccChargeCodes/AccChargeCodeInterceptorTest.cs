using System;
using System.Linq;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.AccChargeCodes
{
	public class AccChargeCodeInterceptorTest : TransactionedTestCase
	{
		public void TestWorksWithInvalidPK()
		{
			var sessionServices = new AncillaryImportServices();
			var chargeCodeCompany = new Entity(TestUtil.FindEntityDefinition("Order", "JobOrderHeader.LandedCostHeader.LandCostInput.ChargeCode.GlbCompany"), sessionServices);
			chargeCodeCompany["Code"] = "ABC";
			chargeCodeCompany["PK"] = "NotAValidGuid";

			var chargeCode = new Entity(TestUtil.FindEntityDefinition("Order", "JobOrderHeader.LandedCostHeader.LandCostInput.ChargeCode"), sessionServices);
			chargeCode["Code"] = "PLCFRT";
			chargeCode.ParentCollection.Add(chargeCodeCompany);

			var order = new Entity(TestUtil.FindEntityDefinition("Order", "JobOrderHeader"), sessionServices);
			order.ChildrenCollection.Add(chargeCode);

			var orderSet = new EntitySet("Order");
			orderSet.Root = order;
			var context = new UpdateContext(sessionServices, new FactoryProvider());
			var setting = new AccChargeCodeSetting { Context = context };

			var interceptor = new AccChargeCodeInterceptor(setting, sessionServices);
			interceptor.Function = e => { };
			interceptor.Invoke(orderSet);

			AssertEquals("GlbCompany should be replaced with current company PK", chargeCodeCompany["PK"], Env.CurrentCompany.PK);
			AssertEquals("GlbCompany should be replaced with current company PK", chargeCodeCompany.InternalPK, Env.CurrentCompany.PK);
		}

		public void TestWorksWithNoPKButAValidCompanyCode()
		{
			var sessionServices = new AncillaryImportServices();
			var chargeCodeCompany = new Entity(TestUtil.FindEntityDefinition("Order", "JobOrderHeader.LandedCostHeader.LandCostInput.ChargeCode.GlbCompany"), sessionServices);
			chargeCodeCompany["Code"] = "SIN"; // Singapore Company in standard test dataset
			chargeCodeCompany["PK"] = Guid.Empty;

			var chargeCode = new Entity(TestUtil.FindEntityDefinition("Order", "JobOrderHeader.LandedCostHeader.LandCostInput.ChargeCode"), sessionServices);
			chargeCode["Code"] = "CUSDEF";
			chargeCode.ParentCollection.Add(chargeCodeCompany);

			var order = new Entity(TestUtil.FindEntityDefinition("Order", "JobOrderHeader"), sessionServices);
			order.ChildrenCollection.Add(chargeCode);

			var orderSet = new EntitySet("Order");
			orderSet.Root = order;
			var context = new UpdateContext(sessionServices, new FactoryProvider());
			var setting = new AccChargeCodeSetting { Context = context };

			var interceptor = new AccChargeCodeInterceptor(setting, sessionServices);
			interceptor.Function = e => { };
			interceptor.Invoke(orderSet);

			AssertEquals("GlbCompany should not be replaced with current company PK", chargeCodeCompany["PK"], Guid.Empty);
			AssertEquals("GlbCompany should not be replaced with current company PK", chargeCodeCompany.InternalPK, Guid.Empty);
		}

		public void TestIfNoCompanySpecifiedInsertTheDefaultCompany()
		{
			var sessionServices = new AncillaryImportServices();
			var chargeCode = new Entity(TestUtil.FindEntityDefinition("Order", "JobOrderHeader.LandedCostHeader.LandCostInput.ChargeCode"), sessionServices);
			chargeCode["Code"] = "PLCFRT";

			var order = new Entity(TestUtil.FindEntityDefinition("Order", "JobOrderHeader"), sessionServices);
			order.ChildrenCollection.Add(chargeCode);

			var orderSet = new EntitySet("Order");
			orderSet.Root = order;
			var context = new UpdateContext(sessionServices, new FactoryProvider());
			var setting = new AccChargeCodeSetting { Context = context };

			var interceptor = new AccChargeCodeInterceptor(setting, sessionServices);
			interceptor.Function = e => { };
			interceptor.Invoke(orderSet);

			var chargeCodeCompany = chargeCode.Parents.FirstOrDefault(entity => entity.EntityName == "GlbCompany");
			AssertEquals("GlbCompany should be inserted with current company PK", chargeCodeCompany["PK"], Env.CurrentCompany.PK);
			AssertEquals("GlbCompany should be inserted with current company PK", chargeCodeCompany.InternalPK, Env.CurrentCompany.PK);
		}
	}
}
