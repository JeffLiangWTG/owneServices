using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.DocDataObjects.Testing
{
	sealed class DocDataObjectValidationProviderTest : TestCase
	{
		class DumpAdHocDataObject : DocDataObject, IAdHocValidationProvider
		{
			public System.Func<IEnumerable<string>> Validator { get; set; }
		}

		class AdHocDummyWithDocDataObjectValidation : NonPersistentBusinessObject, IObsoleteValidation
		{
			public DumpAdHocDataObject AdHoc { get; set; }
		}

		public void TestAdHocValidationProvider_UsingRulesFromValidationProvider()
		{
			var provider = new DocDataObjectValidationProvider();

			var obj = new AdHocDummyWithDocDataObjectValidation();
			obj.AdHoc = new DumpAdHocDataObject();
			var dynamicData = obj.MakeDynamic(validationProvider: provider, dynamicDataFactory: new DocDataObjectDynamicDataFactory());
			var nameProperty = dynamicData.GetDynamicProperty(nameof(obj.AdHoc));
			AssertNoExceptionThrown(() => provider.GetValidationRules(nameProperty).ToList());
		}

		#region MandatoryAttribute

		public void TestMandatoryAttribute_UsingRulesFromValidationProvider()
		{
			var provider = new DocDataObjectValidationProvider();

			var obj = new DummyWithDocDataObjectValidation();
			var dynamicData = obj.MakeDynamic(validationProvider: provider, dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var nameProperty = dynamicData.GetDynamicProperty(nameof(obj.Name));
			var nameValidationRules = provider.GetValidationRules(nameProperty);

			AssertMultilineASCIIEquals("validation of name property",
				"MessageError|Value is required.",
				Validate(nameProperty, nameValidationRules));

			nameProperty.SetValue("John");

			AssertMultilineASCIIEquals("validation of name property",
				"",
				Validate(nameProperty, nameValidationRules));

			nameProperty.SetValue("    ");

			AssertMultilineASCIIEquals("validation of name property",
				"MessageError|Value is required.",
				Validate(nameProperty, nameValidationRules));
		}

		public void TestMandatoryAttribute_UsingValidationMethodOnDynamicData()
		{
			var obj = new DummyWithDocDataObjectValidation();
			var dynamicData = obj.MakeDynamic(validationProvider: new DocDataObjectValidationProvider(), dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var nameProperty = dynamicData.GetDynamicProperty(nameof(obj.Name));

			nameProperty.Validate();

			AssertMultilineASCIIEquals("validation of name property",
				"MessageError|Value is required.",
				FormatNotifications(nameProperty));

			nameProperty.SetValue("John");

			AssertMultilineASCIIEquals("validation of name property",
				"",
				FormatNotifications(nameProperty));

			nameProperty.SetValue("    ");

			AssertMultilineASCIIEquals("validation of name property",
				"MessageError|Value is required.",
				FormatNotifications(nameProperty));
		}

		#endregion

		#region MaxLengthAttribute

		public void TestMaxLengthAttribute_UsingRulesFromValidationProvider()
		{
			var provider = new DocDataObjectValidationProvider();

			var obj = new DummyWithDocDataObjectValidation();
			var dynamicData = obj.MakeDynamic(validationProvider: provider, dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var surnameProperty = dynamicData.GetDynamicProperty(nameof(obj.Surname));
			var surnameValidationRules = provider.GetValidationRules(surnameProperty);

			AssertMultilineASCIIEquals("validation of surname property",
				"",
				Validate(surnameProperty, surnameValidationRules));

			surnameProperty.SetValue("123456789012345");

			AssertMultilineASCIIEquals("validation of surname property",
				"Warning|Value exceeded max length of 10.",
				Validate(surnameProperty, surnameValidationRules));

			surnameProperty.SetValue("Puszkin");

			AssertMultilineASCIIEquals("validation of surname property",
				"",
				Validate(surnameProperty, surnameValidationRules));
		}

		public void TestMaxLengthAttribute_UsingValidationMethodOnDynamicData()
		{
			var obj = new DummyWithDocDataObjectValidation();
			var dynamicData = obj.MakeDynamic(validationProvider: new DocDataObjectValidationProvider(), dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var surnameProperty = dynamicData.GetDynamicProperty(nameof(obj.Surname));

			surnameProperty.Validate();

			AssertMultilineASCIIEquals("validation of surname property",
				"",
				FormatNotifications(surnameProperty));

			surnameProperty.SetValue("123456789012345");

			AssertMultilineASCIIEquals("validation of surname property",
				"Warning|Value exceeded max length of 10.",
				FormatNotifications(surnameProperty));

			surnameProperty.SetValue("Puszkin");

			AssertMultilineASCIIEquals("validation of surname property",
				"",
				FormatNotifications(surnameProperty));
		}

		#endregion

		#region ValidationMethodAttribute

		public void TestValidationMethodAttribute_UsingRulesFromValidationProvider()
		{
			AssertValidationMethodAttribute_UsingRulesFromValidationProvider(0m, "");
			AssertValidationMethodAttribute_UsingRulesFromValidationProvider(-30000m, "Error|Salary cannot be negative.");
			AssertValidationMethodAttribute_UsingRulesFromValidationProvider(20000m, "Warning|Salary is greater than 10000.");
			AssertValidationMethodAttribute_UsingRulesFromValidationProvider(60000m, "MessageError|Salary cannot be more than 50000.");
		}

		void AssertValidationMethodAttribute_UsingRulesFromValidationProvider(decimal salary, string expectedValidationMessage)
		{
			var provider = new DocDataObjectValidationProvider();

			var obj = new DummyWithDocDataObjectValidation
			{
				Salary = salary
			};

			var dynamicData = obj.MakeDynamic(validationProvider: provider, dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var salaryProperty = dynamicData.GetDynamicProperty(nameof(obj.Salary));
			var salaryValidationRules = provider.GetValidationRules(salaryProperty);

			AssertMultilineASCIIEquals("salary",
				expectedValidationMessage,
				Validate(salaryProperty, salaryValidationRules));
		}

		class DummyData : IAdHocValidationProvider
		{
			public Func<IEnumerable<string>> Validator => () => new string[] { "test dummy" };
		}

		public void TestValidationForObject_UsingRulesFromValidationProvider()
		{
			var provider = new DocDataObjectValidationProvider();

			var obj = new DummyData().MakeDynamic();
			var rules = provider.GetValidationRules(obj);
			AssertMultilineASCIIEquals("Object",
				"MessageError|test dummy",
				Validate(obj, rules));
		}

		public void TestValidationMethodAttribute_UsingValidationMethodOnDynamicData()
		{
			AssertValidationMethodAttribute_UsingValidationMethodOnDynamicData(0m, "");
			AssertValidationMethodAttribute_UsingValidationMethodOnDynamicData(-30000m, "Error|Salary cannot be negative.");
			AssertValidationMethodAttribute_UsingValidationMethodOnDynamicData(20000m, "Warning|Salary is greater than 10000.");
			AssertValidationMethodAttribute_UsingValidationMethodOnDynamicData(60000m, "MessageError|Salary cannot be more than 50000.");
		}

		void AssertValidationMethodAttribute_UsingValidationMethodOnDynamicData(decimal salary, string expectedValidationMessage)
		{
			var obj = new DummyWithDocDataObjectValidation
			{
				Salary = salary
			};

			var dynamicData = obj.MakeDynamic(validationProvider: new DocDataObjectValidationProvider(), dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var salaryProperty = dynamicData.GetDynamicProperty(nameof(obj.Salary));

			salaryProperty.Validate();

			AssertMultilineASCIIEquals("salary",
				expectedValidationMessage,
				FormatNotifications(salaryProperty));
		}

		#endregion

		#region CreateZPropertyInfoValidationRule

		public void TestCreateZPropertyInfoValidationRule_UsingMostSeriousNotification()
		{
			var obj = new DummyWithDocDataObjectValidation
			{
				IdentityCardNumber = "320123"
			};

			var dynamicData = obj.MakeDynamic(validationProvider: new DocDataObjectValidationProvider(), dynamicDataFactory: new DocDataObjectDynamicDataFactory());
			var identityCardNumberProperty = dynamicData.GetDynamicProperty(nameof(obj.IdentityCardNumber));

			identityCardNumberProperty.Validate();

			AssertMultilineASCIIEquals("IdentityCardNumber", "Error|Error for IdentityCardNumber.", FormatNotifications(identityCardNumberProperty));
		}

		public void TestCreateZPropertyInfoValidationRule_DeliveryError()
		{
			var obj = new DummyWithDocDataObjectValidation
			{
				Surname = "Cat"
			};

			var dynamicData = obj.MakeDynamic(validationProvider: new DocDataObjectValidationProvider(), dynamicDataFactory: new DocDataObjectDynamicDataFactory());
			var surnameProperty = dynamicData.GetDynamicProperty(nameof(obj.Surname));

			surnameProperty.Validate();

			AssertMultilineASCIIEquals("Surname", "DeliveryError|Surname should not be Cat.", FormatNotifications(surnameProperty));
		}

		#endregion

		#region Implementation

		string ConvertToString(INotification notification)
		{
			return $"{notification.Type}|{notification.Message}";
		}

		string FormatNotifications(INotificationProvider notificationProvider)
		{
			return string.Join("/r/n", notificationProvider.Notifications.Select(ConvertToString));
		}

		string Validate(IDynamicData dynamicData, IEnumerable<ValidationRule> rules)
		{
			var notifications = rules
				.Select(r => r(dynamicData))
				.Where(r => r != null)
				.Select(ConvertToString)
				.ToArray();

			return string.Join("/r/n", notifications);
		}

		#endregion
	}
}
