using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI.Forms;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZMainFormTestTransactioned : TransactionedTestCase
	{
		Mock<ZMainForm> MockForm;

		#region Setup

		protected override void SetUp()
		{
			MockForm = GetFormMock();
		}

		static Mock<ZMainForm> GetFormMock()
		{
			var mockForm = new Mock<ZMainForm>();
			mockForm.CallBase = true;
			AssertNotNull(mockForm.Object); // this is required to 'initialise' the mocked object once since lazy initialisation seems to be used
			mockForm.Invocations.Clear();
			return mockForm;
		}

		protected override void TearDown()
		{
			MockForm.Object.Dispose();
		}

		#endregion

		// see KForm.Text for why we need this
		public void TestFormTextChangedWithUser()
		{
			using (var form = new ZMainForm())
			{
				Env.Registry.ShowUserName = true;
				form.Text = "Hello World";
				AssertEquals("Hello World - Branch: BN - AUBNE - Company: Eagle Datamation International - Department: Branch - User: CargoWise Support", form.AppTitleText.Text);

				Env.Registry.ShowUserName = false;
				form.Text = "World Hello";
				AssertEquals("World Hello - Branch: BN - AUBNE - Company: Eagle Datamation International - Department: Branch", form.AppTitleText.Text);
			}
		}
	}
}
