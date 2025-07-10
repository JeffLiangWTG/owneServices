using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security.Provider;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.Testing
{
	sealed class VisualizerFormsSecurityInfoProviderTest : StmMenuItemsSecurityInfoProviderTest<VisualizerFormsSecurityInfoProvider>
	{
		public void TestMenuItems_HideAdditionalHBLFromRegistry()
		{
			var security = new SecurityCore(new GlbSecurityCollection(Factory), Factory.New<GlbStaff>(), Guid.Empty, Guid.Empty, Guid.Empty);
			using (var shipmentModule = ObjectFactory.Get<IModuleFactory>().Create(ModuleIDs.JobShipment))
			{
				var provider = new ModuleSecurityInfoProvider(new RootSecurityInfoProvider(security), shipmentModule);

				var item1 = GetNewMenuItem();
				item1.SU_BusinessContext = "Shipment";
				item1.SU_IsPublished = true;
				item1.SU_MenuName = "Yusen HBL";
				item1.SU_MenuPath = "AndThatIsAMenuPath";
				item1.SU_FilterList = "Filter1";
				item1.SU_Hint = "Description for item1";
				item1.SU_AddressCategory = OrgAddressCategory.Codes.Office;

				Factory.Save();

				var filter = GetNewMenuItemQuery();
				filter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, "Shipment");
				filter.AddToFilter(StmMenuItemSchema.SU_IsPublished, true);
				filter.AddToFilter(StmMenuItemSchema.SU_MenuName, "Yusen HBL");
				filter.AddToFilter(StmMenuItemSchema.SU_MenuPath, "AndThatIsAMenuPath");
				var menuItems = new StmMenuItemsForSecurity(new BusinessObjectFactory().Load<StmMenuItem>(filter));

				AssertEquals(1, menuItems.Count);

				var stmMenuItemsProvider = new VisualizerFormsSecurityInfoProvider(provider, shipmentModule, menuItems);
				var menuItemProviders = stmMenuItemsProvider.GetChildren().ToArray();

				AssertEquals("Yusen HBL is enabled", 1, menuItemProviders.Length);

				var defaultValuesOverride = new AdditionalHouseBillOfLadingTypeCollection();
				var yusType = new AdditionalHouseBillOfLadingType
				{
					Code = Constants.AddtionalHouseBillTypeMenu.Code.YusenHBL,
					Description = (NoResString)"Yusen HBL",
					Enable = false,
					EnableMessaging = true
				};
				defaultValuesOverride.Add(yusType);

				FreightDataRegistry.Instance.AddtionalHouseBillOfLadingTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValuesOverride);
				menuItemProviders = stmMenuItemsProvider.GetChildren().ToArray();

				AssertEquals("Yusen HBL registry is disabled", 0, menuItemProviders.Length);
			}
		}

		protected override StmMenuItem GetNewMenuItem()
		{
			var menuItem = base.GetNewMenuItem();
			menuItem.SU_MenuType = Constants.StmMenuItemTypes.Forms;
			return menuItem;
		}

		protected override ZQuery GetNewMenuItemQuery()
		{
			var query = new ZQuery();
			query.AddToFilter(StmMenuItemSchema.SU_MenuType, Constants.StmMenuItemTypes.Forms);
			return query;
		}

		protected override void AssertSecurityInfo(ISecurityInfo securityInfo, StmMenuItem item, string moduleID)
		{
			var expectedCheckpoints = new[]
			{
				new AssertionCheckpoint { Name = "Modify", Code = "FormModify" + moduleID, Guid = item.PK.ToGuid() },
				new AssertionCheckpoint { Name = "Deliver Document", Code = "FormDelivery" + moduleID, Guid = item.PK.ToGuid() },
				new AssertionCheckpoint { Name = "Send Message", Code = "FormSendMessage" + moduleID, Guid = item.PK.ToGuid() }
			};

			var actualCheckpoints = new List<AssertionCheckpoint>();

			foreach (var node in securityInfo.Nodes)
			{
				var checkpoint = new AssertionCheckpoint { Name = node.Name, Code = node.Checkpoint.Code, Guid = node.Checkpoint.ItemGuid };
				actualCheckpoints.Add(checkpoint);
			}

			AssertContainsExactElementsInAnyOrder(expectedCheckpoints, actualCheckpoints);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var defaultValuesOverride = new AdditionalHouseBillOfLadingTypeCollection();
			var yusType = new AdditionalHouseBillOfLadingType
			{
				Code = Constants.AddtionalHouseBillTypeMenu.Code.YusenHBL,
				Description = (NoResString)"Yusen HBL",
				Enable = true,
				EnableMessaging = true
			};
			defaultValuesOverride.Add(yusType);

			FreightDataRegistry.Instance.AddtionalHouseBillOfLadingTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValuesOverride);
		}

		class AssertionCheckpoint
		{
			public string Name { get; set; }
			public string Code { get; set; }
			public Guid Guid { get; set; }

			public override bool Equals(object obj)
			{
				var other = (AssertionCheckpoint)obj;
				return other.Name == Name && other.Code == Code && other.Guid == Guid;
			}

			public override int GetHashCode()
			{
				return Name.GetHashCode() ^ Code.GetHashCode() ^ Guid.GetHashCode();
			}

			public override string ToString()
			{
				return string.Format("Name: {0}\r\nCode: {1}\r\nGuid: {2}", Name, Code, Guid);
			}
		}

		protected override SecurityCheckpoint GetMenuItemCheckpoint(SecurityCore security, Guid guid, string menuName, ModuleIdentifier moduleId, ISecurityCheckpoint parent)
		{
			return security.FindOrCreateVisualizerFormCheckpoint(guid, (NoResString)menuName, moduleId, parent);
		}

		protected override StmMenuItemCheckpointHelper CheckpointHelper
		{
			get { return new VisualizerFormsCheckpointHelper(); }
		}
	}
}
