using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class ValidationExtensionsTest : TestCaseWithFactory
	{
		public void TestGuardClause()
		{
			var bizObj = Factory.New<DummyBusinessObject>();

			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>(
					"When targetPtyInfo is null",
					() => ValidationExtensions.AddNotificationBasedOnChildValidationStatus(masterTargetPtyInfo: null, "Notification Message", () => { }, bizObj));

				AssertExceptionThrown<ArgumentException>(
					"When notificationMessage is empty",
					() => ValidationExtensions.AddNotificationBasedOnChildValidationStatus(bizObj.Z0_DescriptionInfo, masterNotificationMessage: "", () => { }, bizObj));

				AssertExceptionThrown<ArgumentNullException>(
					"When validateAction is null",
					() => ValidationExtensions.AddNotificationBasedOnChildValidationStatus(bizObj.Z0_DescriptionInfo, "Notification Message", childValidateAction: null, bizObj));

				AssertExceptionThrown<ArgumentNullException>(
					"When notificationProvider is null",
					() => ValidationExtensions.AddNotificationBasedOnChildValidationStatus(bizObj.Z0_DescriptionInfo, "Notification Message", () => { }, childNotificationProvider: null));
			});
		}

		public void TestErrorAddedToParentWhenChildHasError()
		{
			var bizObj = Factory.New<ParentDummyBusinessObjectForTest>();
			bizObj.Child.ValidationActionForTest = (childTargetPropertyInfo) => childTargetPropertyInfo.AddError("Notification on child object");

			bizObj.Validation.ValidateZ0_Description();

			AssertHasErrorContaining(bizObj.Z0_DescriptionInfo, "Summary Notification On Parent");
		}

		public void TestMessageErrorAddedToParentWhenChildHasMessageError()
		{
			var bizObj = Factory.New<ParentDummyBusinessObjectForTest>();
			bizObj.Child.ValidationActionForTest = (childTargetPropertyInfo) => childTargetPropertyInfo.AddMessageError("Notification on child object");

			bizObj.Validation.ValidateZ0_Description();

			AssertHasMessageErrorContaining(bizObj.Z0_DescriptionInfo, "Summary Notification On Parent");
		}

		public void TestWarningAddedToParentWhenChildHasWarning()
		{
			var bizObj = Factory.New<ParentDummyBusinessObjectForTest>();
			bizObj.Child.ValidationActionForTest = (childTargetPropertyInfo) => childTargetPropertyInfo.AddWarning("Notification on child object");

			bizObj.Validation.ValidateZ0_Description();

			AssertHasWarningContaining(bizObj.Z0_DescriptionInfo, "Summary Notification On Parent");
		}

		public void TestNoNotificationsAddedWhenChildHasNone()
		{
			var bizObj = Factory.New<ParentDummyBusinessObjectForTest>();
			bizObj.Child.ValidationActionForTest = (_) => { };

			bizObj.Validation.ValidateZ0_Description();

			AssertNoNotifications(bizObj.Z0_DescriptionInfo);
		}

		public void TestAddMostSevereNotificationType()
		{
			var bizObj = Factory.New<ParentDummyBusinessObjectForTest>();
			bizObj.Child.ValidationActionForTest = (childTargetPropertyInfo) =>
			{
				childTargetPropertyInfo.AddWarning("Not the most severe notification");
				childTargetPropertyInfo.AddError("The most severe notification");
			};

			bizObj.Validation.ValidateZ0_Description();

			AssertHasErrorContaining(bizObj.Z0_DescriptionInfo, "Summary Notification On Parent");
		}

		#region Parent + Child DummyBusinessObjects

		sealed class ParentDummyBusinessObjectForTest : DummyBusinessObject
		{
			public ParentDummyBusinessObjectForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ChildDummyBusinessObjectForTest Child
			{
				get
				{
					if (child is null)
					{
						child = Factory.New<ChildDummyBusinessObjectForTest>();
						RegisterEditableChildObject(child);
					}
					return child;
				}
			}

			ChildDummyBusinessObjectForTest child;

			protected override DummyBizoValidation GetNewValidation() => new ParentDummyBusinessObjectValidationForTest(this);
		}

		sealed class ParentDummyBusinessObjectValidationForTest : DummyBizoValidation
		{
			public ParentDummyBusinessObjectValidationForTest(ParentDummyBusinessObjectForTest parent) : base(parent)
			{
			}

			new ParentDummyBusinessObjectForTest Parent => (ParentDummyBusinessObjectForTest)base.Parent;

			protected override void CheckZ0_Description()
			{
				base.CheckZ0_Description();

				Parent.Z0_DescriptionInfo.AddNotificationBasedOnChildValidationStatus(
					"Summary Notification On Parent",
					() => Parent.Child.Validation.ValidateAll()
					, Parent.Child);
			}
		}

		sealed class ChildDummyBusinessObjectForTest : DummyBusinessObject
		{
			public ChildDummyBusinessObjectForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public Action<ZPropertyInfo> ValidationActionForTest { get; set; }

			protected override DummyBizoValidation GetNewValidation() => new ChildDummyBusinessObjectValidationForTest(this, ValidationActionForTest);
		}

		sealed class ChildDummyBusinessObjectValidationForTest : DummyBizoValidation
		{
			public ChildDummyBusinessObjectValidationForTest(ChildDummyBusinessObjectForTest parent, Action<ZPropertyInfo> validationActionForTest) : base(parent)
			{
				this.validationActionForTest = validationActionForTest;
			}

			protected override void CheckZ0_Code()
			{
				base.CheckZ0_Code();

				validationActionForTest?.Invoke(Parent.Z0_CodeInfo);
			}

			readonly Action<ZPropertyInfo> validationActionForTest;
		}

		#endregion
	}
}
