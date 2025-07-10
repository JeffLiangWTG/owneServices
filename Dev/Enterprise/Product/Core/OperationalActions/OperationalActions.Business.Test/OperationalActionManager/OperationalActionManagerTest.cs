using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(OperationalActionManager))]
	internal sealed class OperationalActionManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TsetModuleName()
		{
			AssertEquals("Module Name", Manager.ModuleName);
		}

		public void TestDocumentSupporter()
		{
			AssertType(typeof(ManagerDocumentSupporter), Manager.DocumentSupporter);
		}

		public void TestActions()
		{
			OperationalAction action1 = Factory.New<OperationalAction>();
			OperationalAction action2 = Factory.New<OperationalAction>();
			action1.SU_BusinessContext = OperationalAction.GetFullBusinessContext(MockOperationalActionSupportable.BusinessContext);
			action2.SU_BusinessContext = OperationalAction.GetFullBusinessContext(BusinessContext.Order);
			AssertEquals("IsRegisteredEditableChildObject(Actions)", true, Manager.IsRegisteredEditableChildObject(manager.Actions));
			AssertEquals("Actions.Contains(action1)", true, Manager.Actions.Contains(action1));
			AssertEquals("Actions.Contains(action2)", false, Manager.Actions.Contains(action2));
		}

		public void TestEditingMode()
		{
			AssertEquals("EditingMode", MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, Manager.EditingMode);
			Manager.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("EditingMode", MenuEditingMode.AllowEditingOfClientSpecificOnly, Manager.EditingMode);
			AssertEquals("Actions.EditingMode", MenuEditingMode.AllowEditingOfClientSpecificOnly, Manager.Actions.EditingMode);
			Manager.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("EditingMode", MenuEditingMode.AllowEditingOfSystemDefinedOnly, Manager.EditingMode);
			AssertEquals("Actions.EditingMode", MenuEditingMode.AllowEditingOfSystemDefinedOnly, Manager.Actions.EditingMode);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new OperationalActionManager(Factory, Context);
		}

		OperationalActionContext Context
		{
			get
			{
				return context ?? (context = new OperationalActionContext(new MockOperationalActionSupportable().OperationalActionSupporter, "Module Name"));
			}
		}

		OperationalActionContext context;
		OperationalActionManager Manager
		{
			get
			{
				return manager ?? (manager = (OperationalActionManager)GetNewBusinessObject());
			}
		}

		OperationalActionManager manager;
		#endregion
	}
}
