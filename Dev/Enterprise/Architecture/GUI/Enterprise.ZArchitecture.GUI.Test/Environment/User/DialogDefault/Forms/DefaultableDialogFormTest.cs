using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Data.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.ActivityLogging;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.Core.DialogDefault.Testing
{
	[TestedType(typeof(DialogDefaultForm))]
	sealed class DefaultableDialogFormTest : ZFormBasherTest
	{
		KUserControl DummyKUserControl
		{
			get
			{
				var container = new ZUserControl();
				container.Name = "DummyKUserControl";

				var label = new ZLabel();
				label.Name = "DummyKUserControl_Label";

				container.Controls.Add(label);
				container.Size = label.Size;
				container.CaptionRenderingEnabled = true;

				label.GetExtension<ILabelCaptionRenderer>().Options = StringRenderingOptions.Truncate;
				label.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("6FEEDD69-20BF-4336-AF1B-26ADDFA2B59E", "Something very important");

				return container;
			}
		}

		readonly DialogDefaultContext Context = new DialogDefaultContext(new ZGuid("327062AE-2020-4BDD-9239-CAC9760EED2C"),
					(NoResString)"Bashing form",
					ZMessageBoxButtons.AbortRetryIgnore, ZMessageBoxIcon.Asterisk);

		public override bool AllowUntranslatableFormTitle() { return true; }

		protected override bool AllowHasChangesOnFormOpen { get { return true; } }

		public void TestGetAdditionalOptions()
		{
			var checkboxOnly = new DialogDefaultContext(ZGuid.NewZGuid(), (NoResString)"", null, ZMessageBoxIcon.None, showCheckboxOnly: true);
			AssertNull("Since there is no more choices we can make, we should not have an additional options control", GetAdditionalOptionsType(checkboxOnly));

			var fullControl = new DialogDefaultContext(ZGuid.NewZGuid(), (NoResString)"", null, ZMessageBoxIcon.None, showCheckboxOnly: false);
			AssertEquals("Since we can modify them, the items should be shown", typeof(DialogDefaultFullOptions), GetAdditionalOptionsType(fullControl));

			DialogDefaultForm.CanCreateGlobalDefaultsCheckpoint.IsAllowed = false;
			AssertEquals("Since we dont have permission to modify, sysadmin controls should not be on the form", typeof(DialogDefaultUserOnlyOptions), GetAdditionalOptionsType(fullControl));
		}

		public Type GetAdditionalOptionsType(DialogDefaultContext context)
		{
			using (var form = new DialogDefaultForm(context, DummyKUserControl, DialogResult.OK))
			{
				var control = form.AdditionalOptionsControlExposed;
				return control == null ? null : control.GetType();
			}
		}

		public void TestCheckboxOnly()
		{
			var context = new DialogDefaultContext(new ZGuid("746CB8C5-8487-4691-8233-4E05D3ED0562"),
				(NoResString)"test",
				ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Asterisk, showCheckboxOnly: true);

			using (var form = new DialogDefaultForm(context, DummyKUserControl, DialogResult.Yes))
			{
				form.SaveDefaultCheckBoxExposed.Checked = true;

				Assert(!form.Controls.Contains(form.AdditionalOptionsControlExposed));
				AssertEquals("Its a bad move to allow people to unknowingly set defaults for other users", form.Options.Level, DialogDefaultLevel.Codes.User);
				AssertEquals(form.Options.KeepShowingDialog, false);
			}
		}

		public void TestGlobalOnly()
		{
			var context = new DialogDefaultContext(new ZGuid("51CDB684-CAD9-41C7-8E89-B6DCE548BFC5"), (NoResString)"GlobalOnly test", ZMessageBoxButtons.OK, ZMessageBoxIcon.Information);

			using (var form = new DialogDefaultForm(context, DummyKUserControl, DialogResult.Yes))
			{
				AssertEquals(form.Options.Level, DialogDefaultLevel.Codes.User);
			}

			context = new DialogDefaultContext(new ZGuid("51CDB684-CAD9-41C7-8E89-B6DCE548BFC5"), (NoResString)"GlobalOnly test", ZMessageBoxButtons.OK, ZMessageBoxIcon.Information, true, ZDialogResult.OK, true);

			using (var form = new DialogDefaultForm(context, DummyKUserControl, DialogResult.Yes))
			{
				AssertEquals(form.Options.Level, DialogDefaultLevel.Codes.Global);
			}
		}

		public void TestCheckboxCaption()
		{
			var checkboxCaption = Res.GetData("52A3C8C2-46D6-4502-B9DC-4DB7B8125C74", "Checkbox caption test");
			var context = new DialogDefaultContext(new ZGuid("0E8B7BB2-D5C6-443D-9A16-90B79D2CC944"), (NoResString)"CheckboxCaption test", ZMessageBoxButtons.OK, ZMessageBoxIcon.Information, true, ZDialogResult.OK, true, checkboxCaption);

			using (var form = new DialogDefaultForm(context, DummyKUserControl, DialogResult.Yes))
			{
				AssertEquals(form.SaveDefaultCheckBoxExposed.CaptionResourceString, checkboxCaption);
			}
		}

		public void TestAcceptButtonSet()
		{
			var context = new DialogDefaultContext(new ZGuid("A99C545C-E8C1-4082-A572-53093FD98458"), "Dogs are the Best",
				ZMessageBoxButtons.YesNoCancel, ZMessageBoxIcon.Information, false, ZDialogResult.Yes);

			using (var form = new DialogDefaultForm(context, DummyKUserControl, DialogResult.None))
			{
				AssertEquals(form.AcceptButton.DialogResult, DialogResult.Yes);
			}

			context = new DialogDefaultContext(new ZGuid("99BD0C72-F9C9-497C-982B-12FE24523D03"), "Praise Doggo",
				ZMessageBoxButtons.YesNoCancel, ZMessageBoxIcon.Information, false, ZDialogResult.Cancel);

			using (var form = new DialogDefaultForm(context, DummyKUserControl, DialogResult.None))
			{
				AssertEquals(form.AcceptButton.DialogResult, DialogResult.Cancel);
			}
		}

		public void TestWhyAmIReadOnlyLinkLabel()
		{
			var control = new DummyKUserControlWithDialogDefaultMembers();
			using (var form = new DialogDefaultForm(Context, control, DialogResult.Abort, isReadOnly: true))
			{
				form.Show();

				Assert("'Why read only' label should be on the control", ContainsControl(form, form.WhyReadOnlyLabelExposed));
				Assert("'Why read only' label should be visible", form.WhyReadOnlyLabelExposed.Visible);

				form.WhyReadOnlyLabelExposed.OnLinkClicked_Exposed(null);

				var userNotification = (UnitTestUserNotification)Globals.Message;
				AssertEquals("Should have shown the information dialog", "Why is this dialog read only?", userNotification.LastMessage.Caption);
			}
		}

		class DummyKUserControlWithDialogDefaultMembers : KUserControl, IDialogDefaultControlMembers
		{
			public bool IsReadOnly { get; private set; }
			public ZDialogResult Result { get; private set; }

			public void SetReadOnly(bool readOnly, ZDialogResult allowedResult)
			{
				IsReadOnly = readOnly;
				Result = allowedResult;
			}
		}

		public void TestReadOnly()
		{
			var control = new DummyKUserControlWithDialogDefaultMembers();
			using (var form = new DialogDefaultForm(Context, control, DialogResult.Abort, isReadOnly: true))
			{
				Assert("SetReadOnly should be called", control.IsReadOnly);
				AssertEquals("SetReadOnly should be called", ZDialogResult.Abort, control.Result);
			}
		}

		bool ContainsControl(Control haystack, Control needle)
		{
			return haystack == needle || haystack.Controls.Cast<Control>().Any(subControl => ContainsControl(subControl, needle));
		}

		public void TestHidesDefaultsWhenAppropriate()
		{
			var context = new DialogDefaultContext(ZGuid.NewZGuid(), (NoResString)"", null, ZMessageBoxIcon.None, showCheckboxOnly: false);

			using (var form = new DialogDefaultForm(context, DummyKUserControl, DialogResult.OK))
			{
				AssertEquals(form.SaveDefaultCheckBoxExposed.Checked, form.Controls.Contains(form.AdditionalOptionsControlExposed));
				form.SaveDefaultCheckBoxExposed.Checked = !form.SaveDefaultCheckBoxExposed.Checked;
				AssertEquals(form.SaveDefaultCheckBoxExposed.Checked, form.Controls.Contains(form.AdditionalOptionsControlExposed));
			}
		}

		public void TestGetsAppropriateButtons()
		{
			using (var form = (DialogDefaultForm)GetFormToBash())
			{
				var nameRegex = new Regex("(OK)|([A-Z][a-z]+)");

				foreach (MessageBoxButtons buttons in Enum.GetValues(typeof(MessageBoxButtons)))
				{
					var namesOfResultsForThisMessageBox = nameRegex.Matches(
						Enum.GetName(typeof(MessageBoxButtons), buttons))
						.Cast<Match>()
						.Select(match => match.Value)
						.OrderBy(dialogResultName => dialogResultName);

					var dialogResultNames = form.GetDialogResultsForExposed(buttons)
						.Select(dialogResult => Enum.GetName(typeof(DialogResult), dialogResult))
						.OrderBy(dialogResultName => dialogResultName);

					var errorMessage = string.Format("Expected [{0}], got [{1}]", string.Join(", ", namesOfResultsForThisMessageBox),
						string.Join(", ", dialogResultNames));
					Assert(errorMessage, namesOfResultsForThisMessageBox.SequenceEqual(dialogResultNames));
				}
			}
		}

		public void TestUsesMinimumSizeNotCurrentSize()
		{
			var stupidlyBigSize = new Size(10000, 10000);
			var dummyControl = new DialogDefaultAdditionalOptions(Context)
			{
				MinimumSize = new Size(100, 100),
				Size = stupidlyBigSize
			};

			using (var form = new DialogDefaultForm(Context, DummyKUserControl, DialogResult.OK, dummyControl))
			{
				Assert("Should build off the MinimumSize, not the Size", form.Width < stupidlyBigSize.Width);
			}
		}

		public void TestMakesReadOnlyWhenApplicable()
		{
			using (var form = new DialogDefaultForm(Context, DummyKUserControl, DialogResult.Ignore, isReadOnly: true))
			{
				AssertEquals("1 button should always be available on a dialog", 1,
					form.Controls.Cast<Control>()
						.Count(control => control is ZButton && control.Enabled));
			}

			using (var form = new DialogDefaultForm(Context, DummyKUserControl, DialogResult.Ignore, isReadOnly: false))
			{
				AssertEquals("All buttons should be available on a non-readonly dialog", 0,
					form.Controls.Cast<Control>()
						.Count(control => control is ZButton && !control.Enabled));
			}
		}

		#region Form Closing

		public void TestNoExceptionThrownOnFormClosing()
		{
			using (var form = new DialogDefaultForm(Context, DummyKUserControl, DialogResult.OK))
			{
				form.SetDataBinding(null, "");
				form.Show();
				AssertNoExceptionThrown(() => form.Close());
			}
		}

		public void TestShouldDisallowFormClosing_WhenCancellingIsNotAllowed()
		{
			AssertFormClose(shouldAllowClosing: true, forceOverridenDefaults: true, defaultResult: DialogResult.Cancel, isReadOnly: true);
			AssertFormClose(shouldAllowClosing: false, forceOverridenDefaults: true, defaultResult: DialogResult.OK, isReadOnly: true);
			AssertFormClose(shouldAllowClosing: false, forceOverridenDefaults: true, defaultResult: DialogResult.Yes, isReadOnly: true);
			AssertFormClose(shouldAllowClosing: false, forceOverridenDefaults: true, defaultResult: DialogResult.No, isReadOnly: true);

			AssertFormClose(shouldAllowClosing: true, forceOverridenDefaults: true, defaultResult: DialogResult.Cancel, isReadOnly: false);
			AssertFormClose(shouldAllowClosing: true, forceOverridenDefaults: true, defaultResult: DialogResult.OK, isReadOnly: false);
			AssertFormClose(shouldAllowClosing: true, forceOverridenDefaults: true, defaultResult: DialogResult.Yes, isReadOnly: false);
			AssertFormClose(shouldAllowClosing: true, forceOverridenDefaults: true, defaultResult: DialogResult.No, isReadOnly: false);

			AssertFormClose(shouldAllowClosing: true, forceOverridenDefaults: false, defaultResult: DialogResult.Cancel, isReadOnly: true);
			AssertFormClose(shouldAllowClosing: true, forceOverridenDefaults: false, defaultResult: DialogResult.OK, isReadOnly: true);
			AssertFormClose(shouldAllowClosing: true, forceOverridenDefaults: false, defaultResult: DialogResult.Yes, isReadOnly: true);
			AssertFormClose(shouldAllowClosing: true, forceOverridenDefaults: false, defaultResult: DialogResult.No, isReadOnly: true);

			AssertFormClose(shouldAllowClosing: true, forceOverridenDefaults: false, defaultResult: DialogResult.Cancel, isReadOnly: false);
			AssertFormClose(shouldAllowClosing: true, forceOverridenDefaults: false, defaultResult: DialogResult.OK, isReadOnly: false);
			AssertFormClose(shouldAllowClosing: true, forceOverridenDefaults: false, defaultResult: DialogResult.Yes, isReadOnly: false);
			AssertFormClose(shouldAllowClosing: true, forceOverridenDefaults: false, defaultResult: DialogResult.No, isReadOnly: false);
		}

		void AssertFormClose(bool shouldAllowClosing, bool forceOverridenDefaults, DialogResult defaultResult, bool isReadOnly)
		{
			var context = new DialogDefaultContext(new ZGuid("C8BBFCF0-4661-41D0-8D60-80D4EC74773F"),
					(NoResString)"Test form",
					ZMessageBoxButtons.AbortRetryIgnore, ZMessageBoxIcon.Asterisk,
					context: null,
					forceOverriddenDefaults: forceOverridenDefaults
					);

			using (var form = new DialogDefaultForm(context, DummyKUserControl, defaultResult, isReadOnly))
			{
				form.SetDataBinding(null, "");
				form.Show();
				var allowedClosing = false;
				form.FormClosing += (object sender, FormClosingEventArgs e) =>
				{
					allowedClosing = !e.Cancel;
					e.Cancel = false;
				};
				form.DialogResult = DialogResult.Cancel;
				form.Close();
				AssertEquals(shouldAllowClosing, allowedClosing);
			}
		}

		#endregion

		[UseSnapshotProtection]
		public void TestShouldAddFormActivityLog()
		{
			using (RawDataRegistry.Instance.UserEventTrackingEnterprise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ZFormActivityLogger.Instance.StatLogs.Clear();

				using (var testDialogDefaultForm = (DialogDefaultForm)GetFormToBashCore())
				{
					AssertEquals("Should have no activity logs", 0, ZFormActivityLogger.Instance.StatLogs.Count);
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new DialogDefaultForm(Context, DummyKUserControl, DialogResult.Cancel);
		}
	}
}
