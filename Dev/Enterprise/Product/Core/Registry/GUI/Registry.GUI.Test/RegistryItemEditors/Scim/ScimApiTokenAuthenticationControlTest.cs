using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ScimApiTokenAuthenticationControl))]
	sealed class ScimApiTokenAuthenticationControlTest : RegistryZUserControlTestCase
	{
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ScimApiTokenAuthenticationControl)control).ReadOnly;
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new ScimApiTokenAuthenticationControl();
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return null;
		}

		[RequiresSTA]
		public void TestInitialState_ValueIsEmpty()
		{
			using (var control = GetNewControl() as ScimApiTokenAuthenticationControl)
			{
				AssertNullOrEmpty(control.Value);
			}
		}

		public void TestGenerateToken_SetsValue()
		{
			using (var control = GetNewControl() as ScimApiTokenAuthenticationControl)
			{
				control.CreateToken();
				AssertNotNullOrEmpty(control.Value);
			}
		}

		public void TestTokenGenerated_NewControl_ValueEmpty()
		{
			using (var control = GetNewControl() as ScimApiTokenAuthenticationControl)
			{
				control.CreateToken();
				var token = control.Value;
				AssertNotNullOrEmpty(token);

				using (var newControl = new ScimApiTokenAuthenticationControl())
				{
					AssertNullOrEmpty(newControl.Value);
				}
			}
		}

		public void TestMultipleTokenGeneration_ReturnsNewValueEachTime()
		{
			using (var control = GetNewControl() as ScimApiTokenAuthenticationControl)
			{
				control.CreateToken();
				var firstToken = control.Value;
				control.CreateToken();

				var secondToken = control.Value;
				AssertNotEquals(firstToken, secondToken);
			}
		}

		[RequiresSTA]
		public void TestGenerateToken_NotifiesChanges()
		{
			using (var form = new TestRegistryForm())
			{
				using (var control = GetNewControl() as ScimApiTokenAuthenticationControl)
				{
					form.Controls.Add(control);
					form.Show();
					AssertEquals(0, form.UpdateHasChangesCallCount);
					control.CreateToken();
					AssertEquals(1, form.UpdateHasChangesCallCount);
				}
			}
		}

		public void TestGenerateToken_NotifiesChangesEachTime()
		{
			using (var form = new TestRegistryForm())
			{
				using (var control = GetNewControl() as ScimApiTokenAuthenticationControl)
				{
					form.Controls.Add(control);
					form.Show();
					AssertEquals(0, form.UpdateHasChangesCallCount);
					control.CreateToken();
					control.CreateToken();
					AssertEquals(2, form.UpdateHasChangesCallCount);
				}
			}
		}

		public void TestAPITokenTextBox()
		{
			using (var control = new ScimApiTokenAuthenticationControlForTest())
			{
				AssertEquals("Textbox must be enabled", true, control.TextBoxApiToken.Enabled);
				AssertEquals("Textbox must be readonly", true, control.TextBoxApiToken.ReadOnly);
			}
		}

		class ScimApiTokenAuthenticationControlForTest : ScimApiTokenAuthenticationControl
		{
			public ZTextBox TextBoxApiToken
			{
				get
				{
					return tbApiToken;
				}
			}
		}

		class TestRegistryForm : ZForm, IRegistryForm
		{
			public int UpdateHasChangesCallCount { get; private set; }

			public void UpdateHasChanges()
			{
				UpdateHasChangesCallCount++;
			}
		}
	}
}
