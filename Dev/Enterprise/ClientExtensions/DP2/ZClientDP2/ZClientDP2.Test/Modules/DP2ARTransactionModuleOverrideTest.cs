using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.GUI.XmlExport;
using Enterprise.Accounting.Module;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.DP2.Testing
{
	public class DP2ARTransactionModuleOverrideTest : TestCaseWithFactory
	{
		public void TestExportForm()
		{
			DP2DataRegistry.Instance.EnableARTransactionExport = true;
			using (DP2ARTransactionModuleOverride module = new DP2ARTransactionModuleOverride())
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

		public void TestARTransExportMenu()
		{
			SetupRegistryItems();
			Assert("Precondition: AR Export is enabled for Current Company", DP2DataRegistry.Instance.EnableARTransactionExport);
			using (var module = new DP2ARTransactionModuleOverride())
			{
				GetMenuItem(module, "Export To AWH File");
			}

			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>(TestBusinessObjectKind.MinimumRequiredToSave);
			anotherCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Nepal;
			var anotherBranch = anotherCompany.Branches.AddNew();
			Factory.Save();
			using (Env.Instance.SetTemporaryUserContext(Env.CurrentUserPK, anotherBranch.PK.ToGuid(), Guid.Empty))
				using (var module = new DP2ARTransactionModuleOverride())
				{
					GetMenuItem(module, "Export To AWH File", false);
				}
		}

#region Implementation
		MenuItem GetMenuItem(ARTransactionModuleStrip module, string menuName, bool assertNotNull = true)
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

			if (assertNotNull)
			{
				AssertNotNull("'" + menuName + "' should exist.", result);
			}
			else
			{
				AssertNull("'" + menuName + "' should exist.", result);
			}

			return result;
		}

#endregion
		void SetupRegistryItems()
		{
			DP2DataRegistry.Instance.ARTransExportDirectory = Env.TempPath;
			DP2DataRegistry.Instance.ExportFilePrefix = "Prefix";
			CodeDescriptionPairList brhList = new CodeDescriptionPairList();
			brhList.Add(new CodeDescriptionPair(Env.CurrentBranch.Code, "ABC"));
			DP2DataRegistry.Instance.BranchList = brhList;
			CodeDescriptionPairList deptList = new CodeDescriptionPairList();
			deptList.Add(new CodeDescriptionPair(Env.CurrentDepartment.Code, "1"));
			DP2DataRegistry.Instance.DepartmentList = deptList;
			DP2DataRegistry.Instance.EnableARTransactionExport = true;
		}
	}
}
