using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.GUI.XmlExport;
using Enterprise.Accounting.Module;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.AWH.Testing
{
	public class AWHARTransactionModuleOverrideTest : TestCaseWithFactory
	{
		public void TestExportForm()
		{
			using (AWHARTransactionModuleOverride module = new AWHARTransactionModuleOverride())
			{
				MenuItem item = GetMenuItem(module, "Export To AWH File");
				item.PerformClick();
				AssertNull("Export Form shouldn't be shown as registry are not set", ZFormModaliser.ActiveForm);
				ZString dirEmptyError = "- The Export Directory is not set.";
				AssertEquals("Error Mesg", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains(dirEmptyError));
				ZString dirNotExistError = "- The Export Directory doesn't exist.";
				AssertEquals("Error Mesg", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains(dirNotExistError));
				ZString prefixError = "- The Output File Prefix is not set.";
				AssertEquals("Error Mesg", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains(prefixError));
				ZString deptError = "- The Department Code Mapping is not set.";
				AssertEquals("Error Mesg", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains(deptError));
				ZString branchError = "- The Branch Code Mapping is not set.";
				AssertEquals("Error Mesg", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains(branchError));
				SetupRegistryItems();
				item.PerformClick();
				AssertNotNull("Export form should be shown", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Export form type", typeof(FlatFileXmlExportForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

#region Implementation
		MenuItem GetMenuItem(ARTransactionModuleStrip module, string menuName)
		{
			MenuItem result = null;
			foreach (MenuItem button in module.FormActionMenu.FindByText("&Actions").MenuItems)
			{
				if (button.Text == "D&ata Transfer")
				{
					foreach (MenuItem item in button.MenuItems)
					{
						if (item.Text == menuName)
						{
							result = item;
							break;
						}
					}

					break;
				}
			}

			AssertNotNull("'" + menuName + "' should exist.", result);
			return result;
		}

#endregion
		void SetupRegistryItems()
		{
			AWHDataRegistry.Instance.ARTransExportDirectory = Env.TempPath;
			AWHDataRegistry.Instance.ExportFilePrefix = "Prefix";
			CodeDescriptionPairList brhList = new CodeDescriptionPairList();
			brhList.Add(new CodeDescriptionPair(Env.CurrentBranch.Code, "ABC"));
			AWHDataRegistry.Instance.BranchList = brhList;
			CodeDescriptionPairList deptList = new CodeDescriptionPairList();
			deptList.Add(new CodeDescriptionPair(Env.CurrentDepartment.Code, "1"));
			AWHDataRegistry.Instance.DepartmentList = deptList;
		}
	}
}
