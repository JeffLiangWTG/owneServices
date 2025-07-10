using System;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ConsolCostDefaultApportionmentMethod))]
	public class ConsolCostDefaultApportionmentMethodTest : Enterprise.Registry.Business.Testing.RegistryBusinessObjectTemplateTestCase<ConsolCostDefaultApportionmentMethod>
	{
		protected override ConsolCostDefaultApportionmentMethod GetBusinessObjectToClone()
		{
			var result = new ConsolCostDefaultApportionmentMethod();

			return result;
		}

		public override void TestBizObjectFields()
		{
			Assert(true);
		}

		protected override ConsolCostDefaultApportionmentMethod GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new ConsolCostDefaultApportionmentMethod BizObj
		{
			get
			{
				return base.BizObj;
			}
		}

		public void TestConsolTypeSetToAllOnDTBSelection()
		{
			var method = new ConsolCostDefaultApportionmentMethod();
			method.ConsolType = "OTH";
			AssertEquals("Precondition", "OTH", method.ConsolType);
			method.Module = "DTB";
			AssertEquals("Consol Type should be set to all on module change to DTB", ApportionmentMethod.AllCode, method.ConsolType);
		}

		public void TestModuleSetToAllOnTRWSelection()
		{
			var method = new ConsolCostDefaultApportionmentMethod();
			method.ConsolType = "OTH";
			AssertEquals("Precondition", "OTH", method.ConsolType);
			method.Module = "TRW";
			AssertEquals("Consol Type should be set to all on module change to TRW", ApportionmentMethod.AllCode, method.ConsolType);
			AssertEquals("Transport Mode should be set to all on module change to TRW", ApportionmentMethod.AllCode, method.TransportMode);
			AssertEquals("Container Mode should be set to all on module change to TRW", ApportionmentMethod.AllCode, method.ContainerMode);
			AssertEquals("Direction should be set to all on module change to TRW", Constants.FreightShipmentDirection.Code.All, method.Direction);
			AssertContainsExactElementsInAnyOrder("ApportionmentList should be set to specific methods on module change to TRW",
				new[] {
					AllocationMethod.Manual,
					AllocationMethod.Shipment,
					AllocationMethod.GrossWeight,
					AllocationMethod.GrossVolume,
					AllocationMethod.OuterPackTotal,
				},
				method.Lookups.ApportionmentList.GetAllCodes());
		}

		public void TestDirectionSetToAllOnDTBSelection()
		{
			var method = new ConsolCostDefaultApportionmentMethod();
			method.Direction = "IMP";
			AssertEquals("Precondition", "IMP", method.Direction);
			method.Module = ApportionmentMethodModules.TransportBooking;
			AssertEquals("Direction should be set to all on module change to DTB", ApportionmentMethod.AllCode, method.Direction);
		}

		public void TestDirectionSetToAllOnTRWSelection()
		{
			var method = new ConsolCostDefaultApportionmentMethod();
			method.Direction = "IMP";
			AssertEquals("Precondition", "IMP", method.Direction);
			method.Module = ApportionmentMethodModules.TransitWarehouse;
			AssertEquals("Direction should be set to all on module change to TRW", ApportionmentMethod.AllCode, method.Direction);
		}

		public void TestValueReload()
		{
			var config = new ConsolCostDefaultApportionmentMethodConfiguration();
			config.ConsolCostDefaultApportionmentMethodCollection.RemoveAndDeleteAll();
			var newMethod = new ConsolCostDefaultApportionmentMethod
			{
				TransportMode = Constants.TransportModes.Air,
				ConsolType = Constants.AgentType.Direct,
				ContainerMode = Constants.ContainerModes.Loose,
				Module = ApportionmentMethodModules.Forwarding,
				Apportionment = AllocationMethod.CapacityPerContainer,
				Direction = Constants.CartageDirection.Import,
			};
			config.ConsolCostDefaultApportionmentMethodCollection.Add(newMethod);
			AccountingConfigurationRegistry.Instance.ConsolCostDefaultApportionmentMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, config);

			var registryReload = AccountingConfigurationRegistry.Instance.ConsolCostDefaultApportionmentMethod.Value.ConsolCostDefaultApportionmentMethodCollection;
			AssertEquals("PreCondition", 1, registryReload.Count);
			AssertEquals($"Reload value must be same as original,{nameof(ConsolCostDefaultApportionmentMethod.TransportMode)}", newMethod.TransportMode, registryReload[0].TransportMode);
			AssertEquals($"Reload value must be same as original,{nameof(ConsolCostDefaultApportionmentMethod.ConsolType)}", newMethod.ConsolType, registryReload[0].ConsolType);
			AssertEquals($"Reload value must be same as original,{nameof(ConsolCostDefaultApportionmentMethod.ContainerMode)}", newMethod.ContainerMode, registryReload[0].ContainerMode);
			AssertEquals($"Reload value must be same as original,{nameof(ConsolCostDefaultApportionmentMethod.Module)}", newMethod.Module, registryReload[0].Module);
			AssertEquals($"Reload value must be same as original,{nameof(ConsolCostDefaultApportionmentMethod.Apportionment)}", newMethod.Apportionment, registryReload[0].Apportionment);
			AssertEquals($"Reload value must be same as original,{nameof(ConsolCostDefaultApportionmentMethod.Direction)}", newMethod.Direction, registryReload[0].Direction);
		}

		public void TestIConsolCostApportionmentMethod()
		{
			var method = new ConsolCostDefaultApportionmentMethod()
			{
				TransportMode = Constants.TransportModes.Air,
				ConsolType = Constants.AgentType.Direct,
				ContainerMode = Constants.ContainerModes.Loose,
				Module = ApportionmentMethodModules.Forwarding,
				Apportionment = AllocationMethod.CapacityPerContainer,
				Direction = Constants.CartageDirection.Import,
			};

			IApportionmentMethodOverride consolCostApportionmentMethod = method;
			AssertEquals("PreCondition", ApportionmentMethodModules.Forwarding, consolCostApportionmentMethod.Module);
			AssertEquals("PreCondition", Constants.TransportModes.Air, consolCostApportionmentMethod.TransportMode);
			AssertEquals("PreCondition", Constants.AgentType.Direct, consolCostApportionmentMethod.ConsolType);
			AssertEquals("PreCondition", Constants.ContainerModes.Loose, consolCostApportionmentMethod.ContainerMode);
			AssertEquals("PreCondition", Constants.CartageDirection.Import, consolCostApportionmentMethod.Direction);
			AssertEquals("PreCondition", AllocationMethod.CapacityPerContainer, consolCostApportionmentMethod.ApportionmentMethod);

			method.TransportMode = Constants.TransportModes.Sea;
			AssertEquals(nameof(consolCostApportionmentMethod.TransportMode), Constants.TransportModes.Sea, consolCostApportionmentMethod.TransportMode);

			method.ConsolType = Constants.AgentType.Agent;
			AssertEquals(nameof(consolCostApportionmentMethod.ConsolType), Constants.AgentType.Agent, consolCostApportionmentMethod.ConsolType);

			method.ContainerMode = Constants.ContainerModes.FCL;
			AssertEquals(nameof(consolCostApportionmentMethod.ContainerMode), Constants.ContainerModes.FCL, consolCostApportionmentMethod.ContainerMode);

			method.Apportionment = AllocationMethod.GrossVolume;
			AssertEquals(nameof(consolCostApportionmentMethod.ApportionmentMethod), AllocationMethod.GrossVolume, consolCostApportionmentMethod.ApportionmentMethod);

			method.Direction = Constants.CartageDirection.Export;
			AssertEquals(nameof(consolCostApportionmentMethod.Direction), Constants.CartageDirection.Export, consolCostApportionmentMethod.Direction);

			method.Module = ApportionmentMethod.AllCode;
			AssertEquals(nameof(consolCostApportionmentMethod.Module), ApportionmentMethod.AllCode, consolCostApportionmentMethod.Module);
		}
	}
}
