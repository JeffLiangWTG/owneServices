using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.GUI.Testing
{
	[TestedType(typeof(SendAccessCodeForm))]
	sealed class SendAccessCodeFormTest : ZFormBasherTest
	{
		public void TestSend()
		{
			using (var form = GetAndShowNewForm())
			{
				AssertNull("EdiMessage is empty", Factory.LoadTop1<AtlasEDIMessage>(new ZQuery()));

				form.BusinessEntity.OfficeOfGuarantee = "AAA";
				AssertEquals(false, form.BusinessEntity.HasErrors);

				var button = form.FindSingle<ZButton>("SendButton");
				button.PerformClick();
				Application.DoEvents();

				AssertNotNull("message created", Factory.LoadTop1<AtlasEDIMessage>(new ZQuery()));
				AssertEquals("In the future this will actually do something, but for now it will just close the form.", true, form.IsDisposed);
			}
		}

		public void TestCancel()
		{
			using (var form = GetAndShowNewForm())
			{
				form.BusinessEntity.RunPreSaveValidation();
				AssertEquals(true, form.BusinessEntity.HasErrors);

				var button = form.FindSingle<ZButton>("CancelButton");
				button.PerformClick();
				Application.DoEvents();

				AssertEquals("The form should close even though there are validation errors present.", true, form.IsDisposed);
			}
		}

		public void TestValidation_ShouldBlockSend()
		{
			using (var form = GetAndShowNewForm())
			{
				form.BusinessEntity.RunPreSaveValidation();
				AssertEquals(true, form.BusinessEntity.HasErrors);

				var button = form.FindSingle<ZButton>("SendButton");
				button.PerformClick();
				Application.DoEvents();

				CombineAssertions(() =>
				{
					AssertEquals("Send should be blocked and the form should not close because there are validation errors present.", false, form.IsDisposed);
					AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestOfficeOfGuaranteeFormat()
		{
			using (var form = GetAndShowNewForm())
			{
				var officeOfGuaranteeBox = form.FindSingle<ZCodeFindBox>().CodeBox;
				AssertEquals(CharacterCasing.Upper, officeOfGuaranteeBox.CharacterCasing);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return GetNewForm();
		}

		SendAccessCodeForm GetNewForm()
		{
			var header = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			var viewModel = new SendAccessCodeViewModel(header);
			return new SendAccessCodeForm(viewModel);
		}

		SendAccessCodeForm GetAndShowNewForm()
		{
			var form = GetNewForm();
			form.Show();
			Application.DoEvents();

			return form;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "AAA", "AAA Office",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, "GUA");
			Factory.Save();
		}
	}
}
