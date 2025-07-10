using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DocumentEventHandlerTest : TestCaseWithFactory
	{
		public void TestHandleEvent()
		{
			AssertHandleEvent(false);
		}

		public void TestHandleEvent_ScriptWithSemicolon()
		{
			AssertHandleEvent(true);
		}

		public void AssertHandleEvent(bool appendEachMacroLineWithSemicolon)
		{
			var companyProxy = Factory.NewWithValidTestData<OrgHeader>();
			companyProxy.OH_FullName = "EDI CUSTOMS BROKERS";
			Factory.Save();

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			GlbBranch.CurrentBranch.Factory.Save();
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = companyProxy.PK;
			GlbCompany.CurrentCompany.Factory.Save();

			var lineEnding = appendEachMacroLineWithSemicolon
				? ";"
				: "";

			var worksheet = DummyWorksheet.Parse(
$@"#Config:Name=""Test Document""
#Event:Type=""BeforeDocumentCreated""
	def val1 = @env.Company.Organization.Name{lineEnding}
	def val2 = ""hello""{lineEnding}
	def val3 = Z0_Description{lineEnding}
#End
#End
#Body
	<@val1> <@val2>
#End");

			var template = new StandardTemplate(worksheet);

			var handler = new DocumentEventHandler(DocumentEvents.BeforeDocumentCreated,
				template,
				GetContext());

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "some description";

			var notificationsHandler = new NotificationsHandler();

			using (var scope = new MacroScope(dummy))
			{
				scope.SetVariable(VariableNames.Environment, new Enterprise.MasterFiles.Business.Macros.Environment());

				var result = handler.Invoke(scope, notificationsHandler);

				AssertContainsExactElementsInAnyOrder("variables",
					new[]
					{
						"val1|EDI CUSTOMS BROKERS",
						"val2|hello",
						"val3|some description"
					},
					FormatVariables(result));

				AssertContainsExactElementsInAnyOrder("notifications",
					System.Array.Empty<string>(),
					FormatNotifications(notificationsHandler));
			}
		}

		public void TestHandleScriptWithErrors()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#Event:Type=""BeforeDocumentCreated""
	def val1 = ""hello""
	badfunctioncal(
	def val2 = Z0_Description
#End
#End
#Body
	<@val1> <@val2>
#End");

			var template = new StandardTemplate(worksheet);

			var handler = new DocumentEventHandler(DocumentEvents.BeforeDocumentCreated,
				template,
				GetContext());

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "some description";

			var notificationsHandler = new NotificationsHandler();

			using (var scope = new MacroScope(dummy))
			{
				scope.SetVariable(VariableNames.Environment, new Enterprise.MasterFiles.Business.Macros.Environment());

				var result = handler.Invoke(scope, notificationsHandler);

				AssertContainsExactElementsInAnyOrder("variables",
					new[]
					{
						"val1|hello",
						"val2|some description"
					},
					FormatVariables(result));

				AssertContainsExactElementsInAnyOrder("notifications",
					new[]
					{
						"Warning|Batch processing failed.",
						"Error|Syntax error at line 2 position 16.",
						"Error|badfunctioncal( has error: 'Syntax error at line 1 position 16.'"
					},
					FormatNotifications(notificationsHandler));
			}
		}

		IMacroEvaluationContext GetContext()
		{
			return new[]
			{
				new StandardLibrary()
			}.CreateContext();
		}

		IEnumerable<string> FormatVariables(IEnumerable<IVariable> variables)
		{
			foreach (var var in variables)
			{
				yield return string.Format("{0}|{1}", var.Name, var.Value);
			}
		}

		IEnumerable<string> FormatNotifications(INotificationProvider provider)
		{
			foreach (var notification in provider.Notifications)
			{
				yield return string.Format("{0}|{1}", notification.Type, notification.Message);
			}
		}
	}
}