using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DocumentMenu.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using DocDummyBusinessObject = Enterprise.DocumentEngine.DocumentMenu.Testing.DocumentCommandTest.DocDummyBusinessObject;

namespace Enterprise.DocumentEngine.Business.Testing
{
	[TestedType(typeof(ProxyStmMenuItemBase))]
	sealed class ProxyStmMenuItemBaseTest : EnterpriseBusinessObjectTestCase
	{
		#region Properties

		public void TestProxyStmMenuItemProperties()
		{
			var proxyStmMenuItem = Factory.New<ProxyStmMenuItemBase>();

			AssertEquals(ZString.Empty, proxyStmMenuItem.SU_MenuName);
			AssertEquals(ZString.Empty, proxyStmMenuItem.SU_BusinessContext);
			AssertEquals(ZString.Empty, proxyStmMenuItem.SU_DocumentDirection);
			AssertEquals(ZString.Empty, proxyStmMenuItem.SU_FilterList);
			AssertEquals(ZString.Empty, proxyStmMenuItem.SU_MenuPath);
			AssertEquals(ZBool.False, proxyStmMenuItem.SU_IsSystemDefined);
			AssertEquals(ZBool.False, proxyStmMenuItem.SU_IsClientSpecific);

			var stmMenuItem = GetStmMenuItemBase();
			proxyStmMenuItem = ProxyStmMenuItemBase.New(stmMenuItem, nameof(BusinessContext.SubShipment));

			AssertEquals("some menu name", proxyStmMenuItem.SU_MenuName);
			AssertEquals(nameof(BusinessContext.SubShipment), proxyStmMenuItem.SU_BusinessContext);
			AssertEquals(nameof(DocumentDirection.DEP), proxyStmMenuItem.SU_DocumentDirection);
			AssertEquals("BUY=" + GlbStaff.CurrentUser.PK, proxyStmMenuItem.SU_FilterList);
			AssertEquals("Import", proxyStmMenuItem.SU_MenuPath);
			AssertEquals(ZBool.True, proxyStmMenuItem.SU_IsSystemDefined);
			AssertEquals(ZBool.True, proxyStmMenuItem.SU_IsClientSpecific);
		}

		StmMenuItemBase GetStmMenuItemBase()
		{
			var stmMenuItem = Factory.New<StmMenuItemBase>();
			stmMenuItem.SU_MenuName = "some menu name";
			stmMenuItem.SU_BusinessContext = nameof(BusinessContext.Shipment);
			stmMenuItem.SU_DocumentDirection = nameof(DocumentDirection.DEP);
			stmMenuItem.SU_FilterList = "BUY=" + GlbStaff.CurrentUser.PK;
			stmMenuItem.SU_MenuPath = "Import";
			stmMenuItem.SU_IsSystemDefined = true;
			stmMenuItem.SU_IsClientSpecific = true;

			return stmMenuItem;
		}

		#endregion

		public override void TestBizObjectFields()
		{
			Assert("This BizObj is used as a proxy for StmMenuItemBase, these BusinessObject fields will never be saved.", true);
		}

		public void TestIsSavedByFactory()
		{
			var proxyStmMenuItem = Factory.New<ProxyStmMenuItemBase>();
			AssertEquals("Not saved by factory", false, proxyStmMenuItem.IsSavedByFactory);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			// Not applicable to this class
		}

		public void TestDeleteParentShipmentMenuItem()
		{
			var testMenuName = "Test Menu Item";

			var shipmentMenu = Factory.New<StmMenuItemBase>();
			shipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			shipmentMenu.SU_MenuName = testMenuName;

			var template = Factory.New<StmTemplate>();
			template.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			template.SO_IsSystemDefined = true;
			template.SO_Name = "System Shipment Template";

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = shipmentMenu.PK;
			pivot.SI_DocumentTitle = "Pub System Ship Doc1 Pivot";

			Factory.Save();

			var docDummy = Factory.New<SubShipmentDocDummyBusinessObject>();
			AssertEquals("Pre-condition", BusinessContext.Shipment, docDummy.DocumentSupporter.BusinessContext);
			Assert("Pre-condition: docDummy.DocumentSupporter.SupportedChildBusinessContexts supports SubShipment Business Contexts",
				docDummy.DocumentSupporter.SupportedChildBusinessContexts.Contains(BusinessContext.SubShipment));

			var customisation = DocumentMenuCustomisation.New(docDummy, null, Factory);

			var subShipmentMenu = (ProxyStmMenuItemBase)customisation.AvailableChildMenus.FirstOrDefault(childMenuItem =>
			{
				if (!(childMenuItem is ProxyStmMenuItemBase))
				{
					return false;
				}

				var childProxyMenuItem = (ProxyStmMenuItemBase)childMenuItem;
				return childProxyMenuItem.SU_MenuName == testMenuName && childProxyMenuItem.SU_BusinessContext == nameof(BusinessContext.SubShipment);
			});

			AssertNotNull("Pre-condition: Sub shipment menu should have been created for the shipment menu when AvailableChildMenus was accessed", subShipmentMenu);

			AssertCollectionContains("Pre-condition: Shipment menu should be in AvailableChildMenus", shipmentMenu, customisation.AvailableChildMenus);
			AssertCollectionContains("Pre-condition: Sub shipment menu should be in AvailableChildMenus", subShipmentMenu, customisation.AvailableChildMenus);

			shipmentMenu.Delete();

			AssertCollectionNotContains("Shipment menu should not be in AvailableChildMenus", shipmentMenu, customisation.AvailableChildMenus);
			AssertCollectionNotContains("Sub shipment menu should not be in AvailableChildMenus", subShipmentMenu, customisation.AvailableChildMenus);

			var temp = subShipmentMenu.SU_IsSystemDefined;
			AssertContains("Developer error should still have been thrown", "Developer Error: Should not be accessing a property on a deleted business object", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public class SubShipmentDocDummyBusinessObject : DocDummyBusinessObject
		{
			public SubShipmentDocDummyBusinessObject(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			public override DocumentSupporter DocumentSupporter
			{
				get { return new SubShipmentDummyBusinessObjectDocumentSupporter(this); }
			}
		}

		public class SubShipmentDummyBusinessObjectDocumentSupporter : DocumentCommandTest.DummyBusinessObjectDocumentSupporter
		{
			public SubShipmentDummyBusinessObjectDocumentSupporter(DocDummyBusinessObject docDummyBusinessObject)
				: base(docDummyBusinessObject)
			{
			}

			public override BusinessContext[] SupportedChildBusinessContexts
			{
				get
				{
					return new BusinessContext[] { CargoWise.Definitions.BusinessContext.SubShipment };
				}
			}
		}
	}
}
