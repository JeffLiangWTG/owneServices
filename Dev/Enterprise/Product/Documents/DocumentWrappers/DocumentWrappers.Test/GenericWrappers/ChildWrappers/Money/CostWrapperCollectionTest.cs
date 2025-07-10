using CargoWise.Application;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Money;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CostWrapperCollection))]
	sealed class CostWrapperCollectionTest : GenericWrapperCollectionTest<CostWrapperCollection>
	{
		public void TestLoadFromJobConsolCostCollection()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			JobConsolCostCollection costs = new JobConsolCostCollection(Factory, consol);
			JobConsolCost cost1 = costs.TryAddNew();
			JobConsolCost cost2 = costs.TryAddNew();

			cost1.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			cost2.E6_AC_ChargeCode = ((GuidRegistryItem)transportRegistry.TransportDriversGroup).Value;

			CostWrapperCollection wrappers = new CostWrapperCollection(costs, Factory);
			AssertEquals("wrappers.Count", 2, wrappers.Count);
			AssertEquals("wrappers[0].WrappedObject", cost1, wrappers[0].WrappedObject);
			AssertEquals("wrappers[1].WrappedObject", cost2, wrappers[1].WrappedObject);
		}

		#region Implementation

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			JobConsolCost cost = Factory.New<JobConsolCost>();

			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_RX_NKCurrency = Constants.CurrencyCodes.Australia;
			cost.E6_OSCostAmount = 12.00m;

			return new CostWrapper(cost, Factory);
		}

		protected override CostWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new CostWrapperCollection(Factory);
		}

		#endregion
	}
}
