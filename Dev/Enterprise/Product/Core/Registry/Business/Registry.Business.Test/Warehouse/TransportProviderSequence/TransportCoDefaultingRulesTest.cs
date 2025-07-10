using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(TransportCoDefaultingRules))]
	sealed class TransportCoDefaultingRulesTest : RegistryBusinessObjectTemplateTestCase<TransportCoDefaultingRules>
	{
		public void TestDefaultValues()
		{
			AssertEquals("Consignee", (ZByte)1, BizObj.Consignee);
			AssertEquals("Client", (ZByte)2, BizObj.Client);
			AssertEquals("Warehouse", (ZByte)3, BizObj.Warehouse);
		}

		public void TestRunPreSaveValidation()
		{
			BizObj.Consignee = 9;
			BizObj.Client = 9;
			BizObj.Warehouse = 9;

			BizObj.ClearAllNotifications();
			BizObj.RunPreSaveValidation();

			AssertHasErrors(BizObj.ConsigneeInfo);
			AssertHasErrors(BizObj.ClientInfo);
			AssertHasErrors(BizObj.WarehouseInfo);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override TransportCoDefaultingRules GetBusinessObjectToClone()
		{
			var result = new TransportCoDefaultingRules();

			result.Consignee = 1;
			result.Client = 2;
			result.Warehouse = 3;

			return result;
		}

		protected override TransportCoDefaultingRules GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
