using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Customs.CN.Business.Testing
{
	class ValidationExtensionsTest : TestCaseWithFactory
	{
		public void TestAddNotification()
		{
			var bizObj = Factory.New<DummyBizObjForTesting>();
			AssertAddNotification(bizObj, bizObj.Z0_DescriptionInfo, new ZString("AddNotification"), "Notification on Description");
		}

		public void TestAddNotificationIfInvalidCodeOrEmptyWithPairList()
		{
			var bizObj = Factory.New<DummyBizObjForTesting>();
			AssertAddNotification(bizObj, bizObj.Z0_CodeInfo, new ZString("Code"), "Enter a valid Code.", "The code you have selected is not in the list.", "You have not entered a valid code.");

			bizObj = Factory.New<DummyBizObjForTesting>();
			AssertAddNotification(bizObj, bizObj.Z0_CodeInfo, ZString.Empty, "Please enter a Code.", "You have not entered a Code.", "You have not entered a Code.");
		}

		public void TestAddNotificationIfInvalidCodeOrEmpty()
		{
			var bizObj = Factory.New<DummyBizObjForTesting>();
			AssertAddNotification(bizObj, bizObj.Z0_DescriptionInfo, new ZString("AddNotificationIfInvalidCode"), "Enter a valid Description.", "The code you have selected is not in the list.", "You have not entered a valid code.");

			bizObj = Factory.New<DummyBizObjForTesting>();
			AssertAddNotification(bizObj, bizObj.Z0_DescriptionInfo, ZString.Empty, "Please enter a Description.", "You have not entered a Description.", "You have not entered a Description.");
		}

		public void TestAddNotificationIfInvalidCode()
		{
			var bizObj = Factory.New<DummyBizObjForTesting>();
			AssertAddNotification(bizObj, bizObj.Z0_DescriptionInfo, new ZString("AddNotificationIfInvalidCode"), "Enter a valid Description.", "The code you have selected is not in the list.", "You have not entered a valid code.");
		}

		public void TestAddNotificationIfInvalidCodeWithPairList()
		{
			var bizObj = Factory.New<DummyBizObjForTesting>();
			AssertAddNotification(bizObj, bizObj.Z0_CodeInfo, new ZString("XXX"), "Enter a valid Code.", "The code you have selected is not in the list.", "You have not entered a valid code.");
		}

		public void TestAddNotificationIfIsEntered()
		{
			var bizObj = Factory.New<DummyBizObjForTesting>();
			AssertAddNotification(bizObj, bizObj.Z0_DescriptionInfo, new ZString("AddNotificationIfIsEntered"), "Please do not enter a Description.");
		}

		public void TestAddNotificationIfNotEntered()
		{
			var bizObj = Factory.New<DummyBizObjForTesting>();
			AssertAddNotification(bizObj, bizObj.Z0_DescriptionInfo, ZString.Empty, "Please enter a Description.", "You have not entered a Description.", "You have not entered a Description.");
		}

		public void TestAddNotificationIfNotEnteredWithPropertyDescription()
		{
			var bizObj = Factory.New<DummyBizObjForTesting>();
			AssertAddNotification(bizObj, bizObj.Z0_DescriptionInfo, ZString.Empty, "Please enter a Description.", "You have not entered a Z0_Description.", "You have not entered a Description.");
		}

		public void TestAddNotificationIfIsZero()
		{
			var bizObj = Factory.New<DummyBizObjForTesting>();
			AssertAddNotification(bizObj, bizObj.Z0_DecimalInfo, new ZDecimal(0m), "Decimal cannot be zero.");
		}

		public void TestAddNotificationIfIsZeroWithPropertyDescription()
		{
			var bizObj = Factory.New<DummyBizObjForTesting>();
			AssertAddNotification(bizObj, bizObj.Z0_DecimalInfo, new ZDecimal(0m), "Z0_Decimal cannot be zero.");
		}

		public void TestAddNotificationIfIsNegative()
		{
			var bizObj = Factory.New<DummyBizObjForTesting>();
			AssertAddNotification(bizObj, bizObj.Z0_DecimalInfo, new ZDecimal(-1m), "Decimal cannot be negative.");
		}

		public void TestAddNotificationIfIsNegativeWithPropertyDescription()
		{
			var bizObj = Factory.New<DummyBizObjForTesting>();
			AssertAddNotification(bizObj, bizObj.Z0_DecimalInfo, new ZDecimal(-1m), "Z0_Decimal cannot be negative.");
		}

		public void TestAddNotificationIfLessThanOrEqualToZero()
		{
			var bizObj = Factory.New<DummyBizObjForTesting>();
			AssertAddNotification(bizObj, bizObj.Z0_DecimalInfo, new ZDecimal(-1m), "Decimal cannot be negative.", "Please enter a 'Decimal' greater than 0.", "Decimal cannot be negative.");
			bizObj = Factory.New<DummyBizObjForTesting>();
			AssertAddNotification(bizObj, bizObj.Z0_DecimalInfo, new ZDecimal(0m), "Decimal cannot be zero.", "Please enter a 'Decimal' greater than 0.", "Decimal cannot be zero.");
		}

		public void TestAddRowNotification()
		{
			var exceptedMsg = "Add row message";
			var bizObj = Factory.New<DummyBizObjForTesting>();

			CombineAssertions("Add Row Notification", () =>
			{
				bizObj.ValidationMode = ValidationModes.ErrorForTesting;
				bizObj.Validation.ValidateAll();
				AssertHasRowError("Add Error when ValidationMode=ErrorForTesting", bizObj, exceptedMsg);
				bizObj.ValidationMode = ValidationModes.Full;
				bizObj.Validation.ValidateAll();
				AssertHasRowMessageError("Add Message Error when ValidationMode=Full", bizObj, exceptedMsg);
				bizObj.ValidationMode = ValidationModes.Preliminary;
				bizObj.Validation.ValidateAll();
				AssertHasRowWarning("Add Warning when ValidationMode=Preliminary", bizObj, exceptedMsg);
				bizObj.TestingNullValidationModeProvider = true;
				bizObj.Validation.ValidateAll();
				AssertHasRowMessageError("Add Error when ValidationModeProvider=NULL", bizObj, exceptedMsg);
			});
		}

		public void TestGetNotificationType()
		{
			JobDeclaration declaration = null;
			AssertEquals(NotificationType.MessageError, declaration.GetNotificationType());
			declaration = Factory.New<JobDeclaration>();
			declaration.ValidationMode = ValidationModes.Full;
			AssertEquals(NotificationType.MessageError, declaration.GetNotificationType());
			declaration.ValidationMode = ValidationModes.Preliminary;
			AssertEquals(NotificationType.Warning, declaration.GetNotificationType());
		}

		public static void AssertValidationModeProvider(JobDeclaration declaration, IValidationModeProvider validationModeProvider)
		{
			declaration.ValidationMode = ValidationModes.Full;
			AssertEquals(ValidationModes.Full, validationModeProvider.ValidationMode);
			declaration.ValidationMode = ValidationModes.Preliminary;
			AssertEquals(ValidationModes.Preliminary, validationModeProvider.ValidationMode);
		}

		void AssertAddNotification(DummyBizObjForTesting bizObj, ZPropertyInfo propertyInfo, IZType propertyValue, string errorMessage)
		{
			AssertAddNotification(bizObj, propertyInfo, propertyValue, errorMessage, errorMessage, errorMessage);
		}

		void AssertAddNotification(DummyBizObjForTesting bizObj, ZPropertyInfo propertyInfo, IZType propertyValue, string errorMessage, string messageErrorMessage, string warningMessage)
		{
			CombineAssertions($"{propertyInfo.Name} = {propertyValue}", () =>
			{
				bizObj.ValidationMode = ValidationModes.ErrorForTesting;
				propertyInfo.Value = propertyValue;
				AssertHasError("Add Error when ValidationMode=ErrorForTesting", propertyInfo, errorMessage);
				bizObj.ValidationMode = ValidationModes.Full;
				propertyInfo.Value = propertyValue;
				AssertHasMessageError("Add Message Error when ValidationMode=Full", propertyInfo, messageErrorMessage);
				bizObj.ValidationMode = ValidationModes.Preliminary;
				propertyInfo.Value = propertyValue;
				AssertHasWarning("Add warning when ValidationMode=Preliminary", propertyInfo, warningMessage);
				bizObj.TestingNullValidationModeProvider = true;
				propertyInfo.Value = propertyValue;
				AssertHasMessageError("Add Message Error when ValidationModeProvider=NULL", propertyInfo, messageErrorMessage);
			});
		}

		class DummyBizObjForTesting : DummyBusinessObject, IValidationModeProvider
		{
			public DummyBizObjForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ValidationModes ValidationMode { get; set; } = ValidationModes.Full;

			public bool TestingNullValidationModeProvider;

			[List(nameof(Z0_DescriptionList))]
			public override ZString Z0_Description
			{
				get => base.Z0_Description; set => base.Z0_Description = value;
			}

			protected override DummyBizoValidation GetNewValidation() => new DummyBizObjForTestingValidation(this);

			public CodeDescriptionPairList Z0_DescriptionList => Factory.GetCachedValue<JobMessageStatusList>();
		}

		class DummyBizObjForTestingValidation : DummyBizoValidation
		{
			public DummyBizObjForTestingValidation(AutoDummyBizo parent) : base(parent)
			{
			}

			new DummyBizObjForTesting Parent => base.Parent as DummyBizObjForTesting;

			IValidationModeProvider ValidationModeProvider => Parent.TestingNullValidationModeProvider ? null : Parent;

			protected override void CheckZ0_Description()
			{
				base.CheckZ0_Description();
				Parent.Z0_DescriptionInfo.AddNotification("Notification on Description", ValidationModeProvider);
				Parent.Z0_DescriptionInfo.AddNotificationIfIsEntered(ValidationModeProvider);
				Parent.Z0_DescriptionInfo.AddNotificationIfNotEntered(ValidationModeProvider);
				Parent.Z0_DescriptionInfo.AddNotificationIfNotEntered("Z0_Description", ValidationModeProvider);
				Parent.Z0_DescriptionInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);
				Parent.Z0_DescriptionInfo.AddNotificationIfInvalidCode(ValidationModeProvider);
				Parent.Z0_DescriptionInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);
			}

			protected override void CheckZ0_Decimal()
			{
				base.CheckZ0_Decimal();
				Parent.Z0_DecimalInfo.AddNotificationIfIsZero("Z0_Decimal", ValidationModeProvider);
				Parent.Z0_DecimalInfo.AddNotificationIfIsZero(ValidationModeProvider);
				Parent.Z0_DecimalInfo.AddNotificationIfLessThanOrEqualToZero(ValidationModeProvider);
				Parent.Z0_DecimalInfo.AddNotificationIfIsNegative("Z0_Decimal", ValidationModeProvider);
				Parent.Z0_DecimalInfo.AddNotificationIfIsNegative(ValidationModeProvider);
				Parent.Z0_DecimalInfo.AddNotificationIfLessThanOrEqualToZero(ValidationModeProvider);
			}

			protected override void CheckZ0_Code()
			{
				base.CheckZ0_Code();
				Parent.Z0_CodeInfo.AddNotificationIfInvalidCodeOrEmpty(Parent.Z0_DescriptionList, ValidationModeProvider);
			}

			public override void ValidateAll()
			{
				base.ValidateAll();
				Parent.AddRowNotification("Add row message", ValidationModeProvider);
			}
		}
	}
}
