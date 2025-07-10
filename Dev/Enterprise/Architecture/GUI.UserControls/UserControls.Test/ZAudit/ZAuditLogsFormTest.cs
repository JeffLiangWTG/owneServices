using System.Windows.Forms;
using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	internal class ZAuditLogsFormTest
	{
		[TestedType(typeof(ZAuditLogsForm))]
		sealed class ZAuditLogsFormBasherTest : ZFormBasherTest
		{
			public void TestFormVerb()
			{
				using (var form = (ZAuditLogsForm)GetFormToBashCore())
				{
					AssertEquals("FormVerb should be empty", string.Empty, form.FormVerb);
				}
			}

			public void TestFormOpen()
			{
				ZFormModaliser.ShowDialogsInTest = true;
				using (var form = (ZAuditLogsForm)GetFormToBashCore())
				{
					form.Show();
					AssertEquals("Audit server is valid.", expected: true, form.IsAuditServerValid);
				}
			}

			public void TestFormWithoutAuditServer()
			{
				BiServers.ClearBiServersCache();
				BiServers.TemporarilySetAuditServerToNull();
				using (var form = (ZAuditLogsForm)GetFormToBashCore())
				{
					AssertEquals("Audit server is not valid.", expected: false, form.IsAuditServerValid);
				}
			}

			public void TestFormWithoutInvalidAuditServer()
			{
				BiServers.ClearBiServersCache();
				BiServers.SaveAuditServer(Db.Connection, "NOT VALID");
				using (var form = (ZAuditLogsForm)GetFormToBashCore())
				{
					AssertEquals("Audit server is not valid.", expected: false, form.IsAuditServerValid);
				}
			}

			#region Implementation

			protected override Form GetFormToBashCore()
			{
				return new ZAuditLogsForm(Factory.New<DummyLogged>());
			}
			#endregion
		}
	}
}
