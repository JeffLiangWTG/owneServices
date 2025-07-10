using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class ConsolCostDefaultApportionmentMethodLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestConsolTypeList()
		{
			ApportionmentMethodOverrideLookupsTestBase.AssertTestConsolTypeList(
				() => Parent.Lookups.ModuleList,
				(module) =>
				{
					Parent.Module = module;
					return Parent.Lookups.TransportModeList;
				},
				(transport, module) =>
				{
					Parent.Module = module;
					Parent.TransportMode = transport;
					return Parent.Lookups.ConsolTypeList;
				}
			);
		}

		public void TestContainerModeList()
		{
			ApportionmentMethodOverrideLookupsTestBase.AssertTestContainerModeList(
				() => Parent.Lookups.ModuleList,
				(module) =>
				{
					Parent.Module = module;
					return Parent.Lookups.TransportModeList;
				},
				(transport, module) =>
				{
					Parent.Module = module;
					Parent.TransportMode = transport;
					return Parent.Lookups.ConsolTypeList;
				},
				(consolType, transport, module) =>
				{
					Parent.Module = module;
					Parent.TransportMode = transport;
					Parent.ConsolType = consolType;
					return Parent.Lookups.ContainerModeList;
				}
			);
		}

		public void TestModuleList()
		{
			ApportionmentMethodOverrideLookupsTestBase.AssertTestModuleList(
				() => Parent.Lookups.ModuleList
			);
		}

		public void TestTransportModeList()
		{
			ApportionmentMethodOverrideLookupsTestBase.AssertTestTransportModeList(
				() => Parent.Lookups.ModuleList,
				(module) =>
				{
					Parent.Module = module;
					return Parent.Lookups.TransportModeList;
				}
			);
		}

		public void TestDirectionList()
		{
			ApportionmentMethodOverrideLookupsTestBase.AssertTestDirectionList(
				() => Parent.Lookups.ModuleList,
				(module) =>
				{
					Parent.Module = module;
					return Parent.Lookups.DirectionList;
				}
			);
		}

		public void TestApportionmentMethodList()
		{
			ApportionmentMethodOverrideLookupsTestBase.AssertTestApportionmentMethodList(
				() => Parent.Lookups.ModuleList,
				(module) =>
				{
					Parent.Module = module;
					return Parent.Lookups.ApportionmentList;
				}
			);
		}

		ConsolCostDefaultApportionmentMethod Parent => parent ?? (parent = new ConsolCostDefaultApportionmentMethod());

		ConsolCostDefaultApportionmentMethod parent;
	}
}
