using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(CusClassificationForm))]
	sealed class CusClassificationFormBasherTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestIsPromptAuditOnSavedEnabled()
		{
			using (var form = new CusClassificationFormForTesting(Factory.New<CusClassification>()))
			{
				Assert("IsPromptAuditOnSavedEnabled", !form.IsPromptAuditOnSavedEnabledExposed);
				CACustomsDataRegistry.Instance.ReleaseLowValueProductAudit.SetTemporaryValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, ProductAuditActions.Codes.AddWarningValidation);
				Assert("IsPromptAuditOnSavedEnabled", form.IsPromptAuditOnSavedEnabledExposed);
				CACustomsDataRegistry.Instance.ReleaseLowValueProductAudit.SetTemporaryValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, ProductAuditActions.Codes.NoAction);
				Assert("IsPromptAuditOnSavedEnabled", !form.IsPromptAuditOnSavedEnabledExposed);
				CACustomsDataRegistry.Instance.EntryHighValueProductAudit.SetTemporaryValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, ProductAuditActions.Codes.AddMessageErrorValidation);
				Assert("IsPromptAuditOnSavedEnabled", form.IsPromptAuditOnSavedEnabledExposed);
			}
		}

		public void TestPGARequirements()
		{
			var cusClassification = Factory.New<CusClassification>();
			cusClassification.CC_ClassificationType = "IMP";
			using (var form = new CusClassificationForm(cusClassification))
			{
				form.Show();
				var mainTabControl = form.Controls.Find("MainTabControl", true)[0] as TabControl;
				mainTabControl.SelectedIndex = 0;
				AssertEquals(445, form.ClientSize.Height);
				AssertEquals(570, form.ClientSize.Width);

				mainTabControl.SelectedIndex = 1;
				AssertEquals(550, form.ClientSize.Height);
				AssertEquals(1200, form.ClientSize.Width);

				var pgaTabPage = mainTabControl.Controls.Find("PGATabPage", true)[0] as ZTabPage;
				Assert(pgaTabPage.TabVisible);
				Assert(form.pgaTabCollection.Visible);
			}

			cusClassification.CC_ClassificationType = "EXP";
			using (var form = new CusClassificationForm(cusClassification))
			{
				form.Show();
				var mainTabControl = form.Controls.Find("MainTabControl", true)[0] as TabControl;
				mainTabControl.SelectedIndex = 0;
				AssertEquals(230, form.ClientSize.Height);
				AssertEquals(570, form.ClientSize.Width);
				Assert(!form.pgaTabCollection.Visible);

				var pgaTabPage = mainTabControl.Controls.Find("PGATabPage", true).FirstOrDefault() as ZTabPage;
				AssertNull(pgaTabPage);
			}
		}

		public void TestOnRefreshSIMAMeasureEvent()
		{
			var cusClassification = Factory.New<CusClassification>();
			cusClassification.CC_ClassificationType = "IMP";
			AssertNull(cusClassification.OnRefreshSIMAMeasureEvent);
			using (var form = new CusClassificationForm(cusClassification))
			{
				form.Show();
				AssertNotNull(cusClassification.OnRefreshSIMAMeasureEvent);
			}
		}

		protected override Form GetFormToBashCore() => new CusClassificationForm(Factory.New<CusClassification>());

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
		}

		protected override void TearDown()
		{
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
			base.TearDown();
		}

		sealed class CusClassificationFormForTesting : CusClassificationForm
		{
			public CusClassificationFormForTesting(CusClassification classification)
				: base(classification)
			{ }

			internal bool IsPromptAuditOnSavedEnabledExposed => IsPromptAuditOnSavedEnabled;
		}
	}
}
