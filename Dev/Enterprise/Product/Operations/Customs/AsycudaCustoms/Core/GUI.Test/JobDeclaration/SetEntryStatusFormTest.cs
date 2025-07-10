using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.AsycudaCustoms.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	[TestedType(typeof(SetEntryStatusForm))]
	public class SetEntryStatusFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var helper = new SetEntryStatusDetail(declaration);
			return new SetEntryStatusForm(helper);
		}

		public void TestOkButton()
		{
			Factory.SetupEntryStatusList();
			var declaration = Factory.New<JobDeclaration>();
			var entry1 = declaration.ActiveEntryHeaders.AddNew();
			entry1.CH_EntryStatus = "ST1";
			var entry2 = declaration.ActiveEntryHeaders.AddNew();
			entry2.CH_EntryStatus = "ST2";
			SetEntryStatusDetail helperBO = new SetEntryStatusDetail(declaration);
			AssertEquals("no default value", true, helperBO.CusEntryHeaderPK.IsEmpty);
			using (var form = new SetEntryStatusForm(helperBO))
			{
				form.Show();
				var addEventButton = form.FindSingle<ZButton>("AddButton");
				addEventButton.PerformClick();
				bool hasLog = entry1.Logs.HasLogWith(log => true);
				Assert("No logs generated since not pass validation", !hasLog);
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				helperBO.CusEntryHeaderPK = entry1.PK;
				helperBO.EventTime = new ZDateTime(2021, 11, 11);
				helperBO.EntryStatus = "ST2";
				addEventButton.PerformClick();
				hasLog = entry1.Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.CustomsEntryStatusCode && log.SL_Reference == "ST2" && log.SL_EventTime == helperBO.EventTime);
				Assert("log generated ", hasLog);
			}
		}

		public void TestFormCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			SetEntryStatusDetail helperBO = new SetEntryStatusDetail(declaration);
			using (var form = new SetEntryStatusForm(helperBO))
			{
				AssertEquals("Set Customs Entry Status", form.FormCaption);
				AssertEquals("Set Customs Entry Status", form.FormHeading);
			}
		}
	}
}
