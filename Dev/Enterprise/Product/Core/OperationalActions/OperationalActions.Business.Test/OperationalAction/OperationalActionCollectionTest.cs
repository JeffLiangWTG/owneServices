using System.ComponentModel;
using System.Text;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(OperationalActionCollection))]
	internal sealed class OperationalActionCollectionTest : MenuEditableBusinessObjectCollectionTestCase<OperationalActionCollection>
	{
		public void TestCompare()
		{
			OperationalActionCollection actions = new OperationalActionCollection(Factory, Context);
			OperationalAction action1 = actions.AddNew();
			action1.SU_MenuName = "X - Field";
			action1.SU_MenuPath = "B - Folder";
			OperationalAction action2 = actions.AddNew();
			action2.SU_MenuName = "Y - Field";
			action2.SU_MenuPath = "A - Folder";
			OperationalAction action3 = actions.AddNew();
			action3.SU_MenuName = "X - Field";
			action3.SU_MenuPath = "A - Folder";
			OperationalAction action4 = actions.AddNew();
			action4.SU_MenuName = "A - Field";
			action4.SU_MenuIndex = 2;
			OperationalAction action5 = actions.AddNew();
			action5.SU_MenuName = "B - Field";
			action5.SU_MenuIndex = 2;
			OperationalAction action6 = actions.AddNew();
			action6.SU_MenuName = "C - Field";
			action6.SU_MenuIndex = 1;
			const string expectedText = "A - Folder/X - Field\n" + "A - Folder/Y - Field\n" + "B - Folder/X - Field\n" + "/C - Field\n" + "/A - Field\n" + "/B - Field\n" + "";
			const string expectedTextInverted = "/B - Field\n" + "/A - Field\n" + "/C - Field\n" + "B - Folder/X - Field\n" + "A - Folder/Y - Field\n" + "A - Folder/X - Field\n" + "";
			actions.Sort(StmMenuItemSchema.Constants.SU_MenuPath, ListSortDirection.Ascending);
			AssertMultilineASCIIEquals("Ascending", expectedText, OrderAsString(actions));
			actions.Sort(StmMenuItemSchema.Constants.SU_MenuPath, ListSortDirection.Descending);
			AssertMultilineASCIIEquals("Descending", expectedTextInverted, OrderAsString(actions));
		}

		public void TestActionSupportableForChildren()
		{
			OperationalActionCollection actions = new OperationalActionCollection(Factory, Context);
			AssertEquals("AddNew().Context", Context, actions.AddNew().Context);
		}

		public void TestRelationshipFilter()
		{
			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			OperationalAction action1 = anotherFactory.New<OperationalAction>();
			OperationalAction action2 = anotherFactory.New<OperationalAction>();
			DocumentCommand document = anotherFactory.New<DocumentCommand>();
			action1.SU_BusinessContext = OperationalAction.GetFullBusinessContext(BusinessContext.Order);
			action2.SU_BusinessContext = OperationalAction.GetFullBusinessContext(MockOperationalActionSupportable.BusinessContext);
			document.SU_BusinessContext = OperationalAction.GetFullBusinessContext(MockOperationalActionSupportable.BusinessContext);
			anotherFactory.Save();
			Collection.Load();
			AssertEquals("Contains(action1.PK)", false, Collection.Contains(action1.PK));
			AssertEquals("Contains(action2.PK)", true, Collection.Contains(action2.PK));
			AssertEquals("Contains(document.PK)", false, Collection.Contains(document.PK));
			foreach (OperationalAction action in Collection)
			{
				AssertEquals("ActionSupportable", Context, action.Context);
			}
		}

		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OperationalActionCollection(Factory, Context);
		}

		string OrderAsString(OperationalActionCollection actions)
		{
			StringBuilder builder = new StringBuilder();
			foreach (OperationalAction action in actions)
			{
				builder.AppendFormat("{0}/{1}\n", action.SU_MenuPath, action.SU_MenuName);
			}

			return builder.ToString();
		}

		OperationalActionContext Context
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return context ?? (context = new OperationalActionContext(ActionSupporter, "Module Name"));
			}
		}

		OperationalActionContext context;
		OperationalActionSupporter ActionSupporter
		{
			get
			{
				return actionSupporter ?? (actionSupporter = new MockOperationalActionSupportable().OperationalActionSupporter);
			}
		}

		OperationalActionSupporter actionSupporter;
		#endregion
	}
}
