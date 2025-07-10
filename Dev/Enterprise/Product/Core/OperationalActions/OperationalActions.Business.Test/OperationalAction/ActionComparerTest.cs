using System.Collections;
using System.Text;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	sealed class ActionComparerTest : TestCaseWithFactory
	{
		public void TestCompare()
		{
			OperationalActionCollection actions = new OperationalActionCollection(Factory, new OperationalActionContext(Supporter, "Module Name"));
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
			actions.Sort((IComparer)new ActionComparer(false));
			AssertMultilineASCIIEquals("Normal", expectedText, OrderAsString(actions));
			actions.Sort((IComparer)new ActionComparer(true));
			AssertMultilineASCIIEquals("Inverted", expectedTextInverted, OrderAsString(actions));
		}

		#region Implementation
		string OrderAsString(OperationalActionCollection actions)
		{
			StringBuilder builder = new StringBuilder();
			foreach (OperationalAction action in actions)
			{
				builder.AppendFormat("{0}/{1}\n", action.SU_MenuPath, action.SU_MenuName);
			}

			return builder.ToString();
		}

		OperationalActionSupporter Supporter
		{
			get
			{
				if (supporter == null)
				{
					MockOperationalActionSupportable supportable = new MockOperationalActionSupportable();
					supportable.businessContext = (BusinessContext)(-1);
					supporter = supportable.OperationalActionSupporter;
				}

				return supporter;
			}
		}

		OperationalActionSupporter supporter;
		#endregion
	}
}
