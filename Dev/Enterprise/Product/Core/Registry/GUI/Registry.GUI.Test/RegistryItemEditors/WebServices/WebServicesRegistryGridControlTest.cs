using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Test.RegistryItemEditors.WebServices
{
	[TestedType(typeof(WebServicesRegistryGridControl))]
	sealed class WebServicesRegistryGridControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new WebServicesConfigCollection();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((WebServicesRegistryGridControl)control).ReadOnly;

		public void TestPropertiesAreNotVisible_WhenCurrentUserIsNotCWSupport()
		{
			var nonSupportStaff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, SQLComparisonOperator.NotEqual, User.SupportUserName));
			using (Env.SetTemporaryUserContext(nonSupportStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals("Current User is not a Support User", false, Env.CurrentUser.IsSupportUser);

				using (var form = new ZForm())
				using (var control = new WebServicesRegistryGridControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(GetNewBusinessEntity(), null);

					var isAutoManagedColumn = control.WebServicesConfigItemsGrid.ColumnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(col => col.ColumnName == WebServicesConfig.Schema.IsAutoManaged);
					var numberOfServerClustersColumn = control.WebServicesConfigItemsGrid.ColumnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(col => col.ColumnName == WebServicesConfig.Schema.IsAutoManaged);

					AssertNull(isAutoManagedColumn);
					AssertNull(numberOfServerClustersColumn);
				}
			}
		}

		public void TestPropertiesAreVisible_WhenCurrentUserIsCWSupport()
		{
			using (Env.SetTemporaryUserContext("CWSupport", GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("Current User is Support User", true, Env.CurrentUser.IsSupportUser);

				using (var form = new ZForm())
				using (var control = new WebServicesRegistryGridControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(GetNewBusinessEntity(), null);

					var isAutoManagedColumn = control.WebServicesConfigItemsGrid.ColumnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(col => col.ColumnName == WebServicesConfig.Schema.IsAutoManaged);
					var numberOfServerClustersColumn = control.WebServicesConfigItemsGrid.ColumnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(col => col.ColumnName == WebServicesConfig.Schema.IsAutoManaged);

					AssertNotNull(isAutoManagedColumn);
					AssertNotNull(numberOfServerClustersColumn);
				}
			}
		}

		public void TestColumnNames()
		{
			using (Env.SetTemporaryUserContext("CWSupport", GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("Current User is Support User", true, Env.CurrentUser.IsSupportUser);

				using (var form = new ZForm())
				using (var control = new WebServicesRegistryGridControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(GetNewBusinessEntity(), null);

					var columns = control.WebServicesConfigItemsGrid.ColumnStyles;
					CombineAssertions(() =>
					{
						AssertEquals(typeof(ZTextBoxColumnStyleInfo), columns[0].GetType());
						AssertEquals(WebServicesConfig.Schema.Name, ((ZTextBoxColumnStyleInfo)columns[0]).ColumnName);
						AssertContains("Name", ((ZTextBoxColumnStyleInfo)columns[0]).CaptionResourceString.ToString());
						AssertEquals(typeof(ZCheckBoxColumnStyleInfo), columns[1].GetType());
						AssertEquals(WebServicesConfig.Schema.IsEnabled, ((ZCheckBoxColumnStyleInfo)columns[1]).ColumnName);
						AssertContains("Is Enabled", ((ZCheckBoxColumnStyleInfo)columns[1]).CaptionResourceString.ToString());
						AssertEquals(typeof(ZCheckBoxColumnStyleInfo), columns[2].GetType());
						AssertEquals(WebServicesConfig.Schema.IsCustomURL, ((ZCheckBoxColumnStyleInfo)columns[2]).ColumnName);
						AssertContains("Is Custom URL", ((ZCheckBoxColumnStyleInfo)columns[2]).CaptionResourceString.ToString());
						AssertEquals(typeof(ZTextBoxColumnStyleInfo), columns[3].GetType());
						AssertEquals(WebServicesConfig.Schema.URL, ((ZTextBoxColumnStyleInfo)columns[3]).ColumnName);
						AssertContains("URL", ((ZTextBoxColumnStyleInfo)columns[3]).CaptionResourceString.ToString());
						AssertEquals(typeof(ZCheckBoxColumnStyleInfo), columns[4].GetType());
						AssertEquals(WebServicesConfig.Schema.IsAutoManaged, ((ZCheckBoxColumnStyleInfo)columns[4]).ColumnName);
						AssertContains("Is Auto Managed", ((ZCheckBoxColumnStyleInfo)columns[4]).CaptionResourceString.ToString());
						AssertEquals(typeof(ZTextBoxColumnStyleInfo), columns[5].GetType());
						AssertEquals(WebServicesConfig.Schema.NumberOfServerClusters, ((ZTextBoxColumnStyleInfo)columns[5]).ColumnName);
						AssertContains("Number of Server Clusters", ((ZTextBoxColumnStyleInfo)columns[5]).CaptionResourceString.ToString());
					});
				}
			}
		}
	}
}
