using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Shared.GUI.Testing;

[TestedType(typeof(EditableMessageSendingForm<MockSendingObjectParent>))]
sealed class EditableMessageSendingFormTest : ZFormBasherTest
{
	public void TestConfirm_PopUpMessageForMessageError()
	{
		IMessageVisualObjectParentProvider provider = SendingObjectParent;

		var visualObject = provider.VisualObjectParent.VisualObjects.AddNew();
		var fieldBizObj = JPMessageTestHelper.CreateEditableFieldBizObject();
		visualObject.Header.Add(fieldBizObj);
		fieldBizObj.OverrideValue = "^";

		using (var form = GetFormToBash())
		{
			form.Show();
			var confirmButton = form.FindSingle<ZButton>("ConfirmButton");
			confirmButton.PerformClick();
		}
		AssertEquals("It is likely that your message(s) will be rejected by Customs, as they have the following message errors:\r\n\r\nOverride: The character '^' is not supported by NACCS.\r\n\r\nDo you want to send the message(s) despite these errors?", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	public void TestConfirm()
	{
		IMessageVisualObjectParentProvider provider = SendingObjectParent;

		using (var form = GetFormToBash())
		{
			Assert("Pre-Condition", !provider.UseVisualData);

			form.Show();

			var confirmButton = form.FindSingle<ZButton>("ConfirmButton");
			confirmButton.PerformClick();
		}

		Assert("Should use visual data now.", provider.UseVisualData);
	}

	protected override Form GetFormToBashCore()
	{
		var form = new EditableMessageSendingForm<MockSendingObjectParent>(SendingObjectParent);

		MissingResourceStringChecker.ExcludeFromTest(form.FindSingle<ZCalcEdit>("ObjectsCurrentNumberCalcEdit"));
		MissingResourceStringChecker.ExcludeFromTest(form.FindSingle<ZCalcEdit>("ObjectsNumberOfResultsCalcEdit"));
		MissingResourceStringChecker.ExcludeFromTest(form.FindSingle<ZCalcEdit>("ItemsCurrentNumberCalcEdit"));
		MissingResourceStringChecker.ExcludeFromTest(form.FindSingle<ZCalcEdit>("ItemsNumberOfResultsCalcEdit"));

		TypeDescriptor.AddAttributes(form.FindSingle<MessageVisualObjectUserControl>(), new SuppressControlRequiresTextBasherAttribute());

		return form;
	}

	protected override bool AllowHasChangesOnFormOpen => true;

	MockSendingObjectParent SendingObjectParent => sendingObjectParent ??= new MockSendingObjectParent(Factory);
	MockSendingObjectParent sendingObjectParent;

	sealed class MockSendingObjectParent : NonPersistentBusinessObject, IMessageVisualObjectParentProvider
	{
		public MockSendingObjectParent(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public IMessageSendingContext Context { get; }

		public bool UseVisualData { get; set; }

		public ZString PropertyForTest { get; set; }

		public ZPropertyInfo PropertyForTestInfo => GetZPropertyInfo(nameof(PropertyForTest));

		public MessageVisualObjectParent VisualObjectParent => visualObjectParent ??= new MessageVisualObjectParent(Factory);
		MessageVisualObjectParent visualObjectParent;

		public IEnumerable<IMessageContentProvider> GetContentProviders() => Enumerable.Empty<IMessageContentProvider>();

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		public MockSendingObjectParentValidation Validation => validation ??= new MockSendingObjectParentValidation(this);
		MockSendingObjectParentValidation validation;

		public class MockSendingObjectParentValidation : ZValidation
		{
			public MockSendingObjectParentValidation(MockSendingObjectParent parent)
				: base(parent)
			{
				this.parent = parent;
			}

			readonly MockSendingObjectParent parent;

			public override Type AutoValidationType => typeof(MockSendingObjectParentValidation);

			public override void ValidateAll()
			{
				parent.PropertyForTestInfo.AddMessageError("Test message error.");
			}
		}
	}
}
