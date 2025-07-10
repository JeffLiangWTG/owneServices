using System.Windows.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(EvvManualRequestForm))]
internal class EvvManualRequestFormTest : MessageSendingObjectFormTest
{
	protected override Form GetFormToBashCore()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		Factory.Save();
		return new EvvManualRequestForm(new EvvRequestSendingObjectParent(entryHeader));
	}

	public void TestFormHeading()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		using (var form = new EvvManualRequestForm(new EvvRequestSendingObjectParent(entryHeader)))
		{
			AssertEquals("EVV Documents Manual Request", form.FormHeading);
		}
	}

	public void TestSendButtonAvailability()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		using (var form = new EvvManualRequestFormForTesting(new EvvRequestSendingObjectParent(entryHeader)))
		{
			CombineAssertions(() =>
			{
				form.CustomsDutiesCheckBox.Checked = true;
				form.VatCheckBox.Checked = true;
				form.ChangeSendButtonAvailabilityExposed();
				AssertEquals("Default value, Send Button enabled", true, form.SendButton.Enabled);

				form.CustomsDutiesCheckBox.Checked = false;
				form.VatCheckBox.Checked = false;
				form.ChangeSendButtonAvailabilityExposed();
				AssertEquals("Send Button disabled when all checkboxes are disabled", false, form.SendButton.Enabled);

				form.CustomsDutiesCheckBox.Checked = true;
				form.BusinessEntity.MrnVersion = -1;
				form.ChangeSendButtonAvailabilityExposed();
				AssertEquals("Send Button disabled when SendingObject has errors", false, form.SendButton.Enabled);
			});
		}
	}

	public void TestControls()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		using (var form = new EvvManualRequestForm(new EvvRequestSendingObjectParent(entryHeader)))
		{
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("MrnTextBox", form.MrnTextBox);
				AssertType<ZTextBox>("VersionTextBox", form.VersionTextBox);
				AssertType<ZCheckBox>("CustomsDutiesCheckBox", form.CustomsDutiesCheckBox);
				AssertType<ZCheckBox>("VatCheckBox", form.VatCheckBox);
				AssertType<ZCheckBox>("ReimbursementCustomsDutiesCheckBox", form.ReimbursementCustomsDutiesCheckBox);
				AssertType<ZCheckBox>("ReimbursementVatCheckBox", form.ReimbursementVatCheckBox);
			});
		}
	}

	class EvvManualRequestFormForTesting : EvvManualRequestForm
	{
		internal EvvManualRequestFormForTesting(EvvRequestSendingObjectParent sendingObjectParent) : base(sendingObjectParent)
		{
		}

		public void ChangeSendButtonAvailabilityExposed() => base.ChangeSendButtonAvailability();
	}
}
