using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(ExperimentalSettingsForm))]
	public class ExperimentalSettingsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var system = Factory.New<BMSystem>();
			var provider = new ExperimentalSettingsProvider(system.PK, Factory);
			return new ExperimentalSettingsForm(provider);
		}

		public void TestClickSaveSavesViewModelThenClose()
		{
			var system = Factory.New<BMSystem>();
			system.FS_Name = "TestSystem";

			var provider = new ExperimentalSettingsProvider(system.PK, Factory);
			var setting = provider.ExperimentalSettings.AddNew();
			setting.Key = "DateTimeNow";
			setting.Value = ZDateTime.Now.ToString();

			var stmData = Factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Owner, system.PK));
			AssertEquals(null, stmData);

			using (var form = new ExperimentalSettingsForm(provider))
			{
				form.Show();

				var button = (ZButton)form.Controls.Find("SaveExperimentalSettingsButton", true).Single();
				button.PerformClick();

				stmData = Factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Owner, system.PK));
				AssertNotEquals(null, stmData);

				AssertEquals(true, form.IsDisposed);
			}
		}

		public void TestClickCancelDoesNotSaveViewModel()
		{
			var system = Factory.New<BMSystem>();
			var provider = new ExperimentalSettingsProvider(system.PK, Factory);
			var setting = provider.ExperimentalSettings.AddNew();
			setting.Key = "DateTimeNow";
			setting.Value = ZDateTime.Now.ToString();

			var stmData = Factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Owner, system.PK));
			AssertEquals(null, stmData);

			using (var form = new ExperimentalSettingsForm(provider))
			{
				form.Show();

				var button = (ZButton)form.Controls.Find("CloseExperimentalSettingsButton", true).Single();
				button.PerformClick();

				stmData = Factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Owner, system.PK));
				AssertEquals(null, stmData);
			}
		}
	}
}
