using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.Core.DialogDefault.Testing
{
	sealed class DialogDefaultAdditionalOptionsTestCase : TestCaseWithFactory
	{
		class DialogDefaultOptionsWithContextsCheckbox : DialogDefaultAdditionalOptions
		{
			public DialogDefaultOptionsWithContextsCheckbox(DialogDefaultContext context)
				: base(context)
			{
				nullContextCheckbox = new ZCheckBox();
				Controls.Add(nullContextCheckbox);
			}

			public readonly ZCheckBox nullContextCheckbox;
			protected internal override ZCheckBox SaveForAllContextsCheckbox
			{
				get { return nullContextCheckbox; }
			}
		}

		public void TestSetsSaveForAllContextsCheckpoint_WithNullableContext()
		{
			var noContextDescription = Res.GetData("3090EECD-14D7-49CA-AA09-E12A3DE2F4DC", "dsjkbasdk");
			var ctx = new DialogDefaultContext(ZGuid.NewZGuid(), (NoResString)"", null, ZMessageBoxIcon.None, ZGuid.NewZGuid(), ZGuid.NewZGuid(), noContextDescription);
			using (var form = new ZForm())
			{
				var control = new DialogDefaultOptionsWithContextsCheckbox(ctx);
				form.Controls.Add(control);
				form.Show();

				AssertEquals(noContextDescription, control.nullContextCheckbox.CaptionResourceString);
				Assert(control.nullContextCheckbox.Visible);
			}
		}

		public void TestSetsSaveForAllContextsCheckpoint_WithNoContext()
		{
			var ctx = new DialogDefaultContext(ZGuid.NewZGuid(), (NoResString)"", null, ZMessageBoxIcon.None);
			using (var form = new ZForm())
			{
				var control = new DialogDefaultOptionsWithContextsCheckbox(ctx);
				form.Controls.Add(control);
				form.Show();

				Assert("Should not display the checkbox", !control.nullContextCheckbox.Visible);
			}
		}
	}
}
