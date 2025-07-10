using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class NotificationHelperTest : TestCaseWithFactory
{
	public void TestTurnMessageErrorsIntoErrors() => CombineAssertions(() =>
	{
		var bo = new BusinessObjectForTesting();

		bo.Property = "EADD-MESSAGE-ERROR";
		AssertEquals(bo.Property, 1, bo.PropertyInfo.Notifications.Count());
		AssertHasError(bo.Property, bo.PropertyInfo, MessageErrorText1);

		bo.Property = "EADD-TWO-MESSAGE-ERRORS";
		AssertEquals(bo.Property, 2, bo.PropertyInfo.Notifications.Count());
		AssertHasError(bo.Property, bo.PropertyInfo, MessageErrorText1);
		AssertHasError(bo.Property, bo.PropertyInfo, MessageErrorText2);

		bo.Property = "EADD-WARNING";
		AssertEquals(bo.Property, 1, bo.PropertyInfo.Notifications.Count());
		AssertHasWarning(bo.Property, bo.PropertyInfo, WarningText);

		bo.Property = "EADD-ERROR";
		AssertEquals(bo.Property, 1, bo.PropertyInfo.Notifications.Count());
		AssertHasError(bo.Property, bo.PropertyInfo, ErrorText);

		bo.Property = "EADD-TWO-MESSAGE-ERRORS-WITH-PREDICATE";
		AssertEquals(bo.Property, 2, bo.PropertyInfo.Notifications.Count());
		AssertHasMessageError(bo.Property, bo.PropertyInfo, MessageErrorText1);
		AssertHasError(bo.Property, bo.PropertyInfo, MessageErrorText2);
	});

	public void TestTurnMessageErrorsIntoWarning() => CombineAssertions(() =>
	{
		var bo = new BusinessObjectForTesting();

		bo.Property = "WADD-MESSAGE-ERROR";
		AssertEquals(bo.Property, 1, bo.PropertyInfo.Notifications.Count());
		AssertHasWarning(bo.Property, bo.PropertyInfo, MessageErrorText1);

		bo.Property = "WADD-TWO-MESSAGE-ERRORS";
		AssertEquals(bo.Property, 2, bo.PropertyInfo.Notifications.Count());
		AssertHasWarning(bo.Property, bo.PropertyInfo, MessageErrorText1);
		AssertHasWarning(bo.Property, bo.PropertyInfo, MessageErrorText2);

		bo.Property = "WADD-WARNING";
		AssertEquals(bo.Property, 1, bo.PropertyInfo.Notifications.Count());
		AssertHasWarning(bo.Property, bo.PropertyInfo, WarningText);

		bo.Property = "WADD-ERROR";
		AssertEquals(bo.Property, 1, bo.PropertyInfo.Notifications.Count());
		AssertHasError(bo.Property, bo.PropertyInfo, ErrorText);

		bo.Property = "WADD-TWO-MESSAGE-ERRORS-WITH-PREDICATE";
		AssertEquals(bo.Property, 2, bo.PropertyInfo.Notifications.Count());
		AssertHasMessageError(bo.Property, bo.PropertyInfo, MessageErrorText1);
		AssertHasWarning(bo.Property, bo.PropertyInfo, MessageErrorText2);
	});

	class BusinessObjectForTesting : NonPersistentBusinessObject
	{
		public ZString Property
		{
			get => property;
			set
			{
				property = value;
				Validation.ValidateProperty();
			}
		}
		ZString property;

		public ZPropertyInfo PropertyInfo => GetZPropertyInfo(nameof(Property));

		public BusinessObjectForTestingValidation Validation => new BusinessObjectForTestingValidation(this);
	}

	class BusinessObjectForTestingValidation : ZValidation
	{
		public BusinessObjectForTestingValidation(BusinessObjectForTesting parent) : base(parent)
		{
			this.parent = parent;
		}

		readonly BusinessObjectForTesting parent;

		public override void ValidateAll()
		{
			ValidateProperty();
		}

		public void ValidateProperty()
		{
			ValidateCalculatedProperty(parent.PropertyInfo);
		}

		protected virtual void CheckProperty()
		{
			Func<INotification, bool> predicate = null;
			switch (parent.Property.TrimStart(['E', 'W']))
			{
				case "ADD-MESSAGE-ERROR":
					parent.PropertyInfo.AddMessageError(MessageErrorText1);
					break;
				case "ADD-TWO-MESSAGE-ERRORS":
					parent.PropertyInfo.AddMessageError(MessageErrorText1);
					parent.PropertyInfo.AddMessageError(MessageErrorText2);
					break;
				case "ADD-WARNING":
					parent.PropertyInfo.AddWarning(WarningText);
					break;
				case "ADD-ERROR":
					parent.PropertyInfo.AddError(ErrorText);
					break;
				case "ADD-TWO-MESSAGE-ERRORS-WITH-PREDICATE":
					parent.PropertyInfo.AddMessageError(MessageErrorText1);
					parent.PropertyInfo.AddMessageError(MessageErrorText2);
					predicate = notification => notification.Message.Equals(MessageErrorText2);
					break;
			}

			if (parent.Property.StartsWith("E"))
			{
				NotificationHelper.TurnMessageErrorsIntoErrors(parent.PropertyInfo, predicate);
			}
			else if (parent.Property.StartsWith("W"))
			{
				NotificationHelper.TurnMessageErrorsIntoWarning(parent.PropertyInfo, predicate);
			}
		}

		public override Type AutoValidationType => null;
	}

	internal const string MessageErrorText1 = "message-error 1";
	internal const string MessageErrorText2 = "message-error 2";
	internal const string WarningText = "warning";
	internal const string ErrorText = "error";
}
