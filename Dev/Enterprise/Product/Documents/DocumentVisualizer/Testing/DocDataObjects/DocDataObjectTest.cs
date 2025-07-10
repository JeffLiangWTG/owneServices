using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.DocDataObjects.Testing
{
	[TestedType(typeof(DocDataObject))]
	sealed class DocDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		#region OnValueChanged

		public void TestOnValueChanged_DocDataObject_TriggeredWhenSettingValue()
		{
			var dummy = new DummyDocDataObject();

			var textValueChangedRun = 0;
			dummy.OnValueChanged(nameof(dummy.Text)).Do(() => textValueChangedRun++);

			dummy.Text = "text";
			AssertEquals("Text OnValueChanged run", 1, textValueChangedRun);
		}

		public void TestOnValueChanged_DynamicData_TriggeredWhenSettingValue()
		{
			var dummy = new DummyDocDataObject();

			var textValueChangedRun = 0;
			dummy.OnValueChanged(nameof(dummy.Text)).Do(() => textValueChangedRun++);

			var dynamicData = dummy.MakeDocDataDynamic();
			var text = dynamicData.GetDynamicProperty(nameof(dummy.Text));

			text.SetValue("text");
			AssertEquals("Text OnValueChanged run", 1, textValueChangedRun);
		}

		public void TestOnValueChanged_DynamicData_TriggeredWhenSettingFromOverride()
		{
			var dummy = new DummyDocDataObject();

			var textValueChangedRun = 0;
			dummy.OnValueChanged(nameof(dummy.Text)).Do(() => textValueChangedRun++);

			var dynamicData = dummy.MakeDocDataDynamic();

			var xml = XDocument.Parse(
				@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Property Name=""Code"">
    <Value>AAA</Value>
  </Property>
</Entity>");

			dynamicData.MergeDataFromXml(xml);

			var text = dynamicData.GetDynamicProperty(nameof(dummy.Text));

			text.SetValue("text");
			AssertEquals("Text OnValueChanged run", 1, textValueChangedRun);
		}

		#endregion

		#region TestAddValidationRule

		public void TestAddValidationRule()
		{
			var dummy = new DummyDocDataObject();

			dummy.AddValidationRule(nameof(dummy.Text),
				Core.NotificationType.MessageError,
				() => dummy.Text == "aaa",
				"aaa is not a valid value.");

			dummy.Text = "aaa";

			AssertHasMessageError(dummy.TextInfo, "aaa is not a valid value.");

			dummy.Text = "bbb";

			AssertNoMessageError(dummy.TextInfo, "aaa is not a valid value.");
		}

		#endregion

		#region TestValidate

		public void TestValidate_DocDataObject()
		{
			var dummy = new DummyDocDataObject
			{
				Text = "aaa"
			};

			AssertNoExceptionThrown("No exception thrown when valitating object with no validation rules",
				() => dummy.Validate(nameof(dummy.Text)));

			AssertNoExceptionThrown("No exception thrown when valitating non existing property with no validation rules",
				() => dummy.Validate("zzz"));

			AssertNoMessageError(dummy.TextInfo, "aaa is not a valid value.");

			dummy.AddValidationRule(nameof(dummy.Text),
				Core.NotificationType.MessageError,
				() => dummy.Text == "aaa",
				"aaa is not a valid value.");

			dummy.AddValidationRule(nameof(dummy.Text),
				Core.NotificationType.DeliveryError,
				() => dummy.Text == "bbb",
				"Test Delivery Error.");

			dummy.Validate(nameof(dummy.Text));

			AssertHasMessageError(dummy.TextInfo, "aaa is not a valid value.");
			Assert(dummy.TextInfo.Notifications.All(n => n.Type.EnumValueName != nameof(Core.NotificationType.DeliveryError)));

			dummy.Text = "bbb";

			Assert(dummy.TextInfo.Notifications.Any(n => n.Message == "Test Delivery Error." && n.Type.EnumValueName == nameof(Core.NotificationType.DeliveryError)));
		}

		public void TestValidate_DocDataObject_ValidateRelated()
		{
			var dummy = new DummyDocDataObject("123");

			dummy.CodeDescriptionInfo.AddError(() =>
					!string.IsNullOrEmpty(dummy.Code)
					&& string.IsNullOrWhiteSpace(dummy.CodeDescription),
				"CodeDescription is required when Code is set");

			dummy.OnValueChanged(nameof(dummy.Code)).Validate(nameof(dummy.CodeDescription));

			dummy.ValidateAllIncludingChildren();

			AssertNoErrors("CodeDescription has no Errors", dummy.CodeDescriptionInfo);

			dummy.Code = "AAA";

			AssertHasError(dummy.CodeDescriptionInfo, "CodeDescription is required when Code is set");
		}

		public void TestValidate_DynamicData_DefaultValueIsInvalid()
		{
			var dummy = new DummyDocDataObject("123");
			dummy.Code = "AAA";
			dummy.CodeInfo.AddError(() => dummy.Code == "AAA", "Code has invalid value");

			dummy.ValidateAllIncludingChildren();

			var dynamicData = dummy.MakeDocDataDynamic();
			var dummyNotifications = GetNotifications(dynamicData);

			AssertContainsExactElementsInAnyOrder("DocDataObject validation messages",
				Array.Empty<string>(),
				dummyNotifications);

			var code = dynamicData.GetDynamicProperty(nameof(dummy.Code));
			var codeNotifications = GetNotifications(code);

			AssertContainsExactElementsInAnyOrder("Code validation messages",
				new[]
				{
					"Error|Code has invalid value"
				},
				codeNotifications);
		}

		public void TestValidate_DynamicData_AfterSettingProperty()
		{
			var dummy = new DummyDocDataObject("123");
			dummy.CodeInfo.AddError(() => dummy.Code == "AAA", "Code has invalid value");

			dummy.ValidateAllIncludingChildren();

			var dynamicData = dummy.MakeDocDataDynamic();
			var dummyNotifications = GetNotifications(dynamicData);

			AssertContainsExactElementsInAnyOrder("validation messages",
				Array.Empty<string>(),
				dummyNotifications);

			var code = dynamicData.GetDynamicProperty(nameof(dummy.Code));
			var codeNotifications = GetNotifications(code);

			AssertContainsExactElementsInAnyOrder("Code validation messages before setting invalid value",
				Array.Empty<string>(),
				codeNotifications);

			// this will set value and clear all validation notifications
			code.SetValue("AAA");

			AssertContainsExactElementsInAnyOrder("Code validation messages before setting invalid value",
				new[]
				{
					"Error|Code has invalid value"
				},
				codeNotifications);
		}

		public void TestValidate_DynamicData_ValidateRelated()
		{
			var dummy = new DummyDocDataObject("123");

			dummy.CodeDescriptionInfo.AddError(() =>
					!string.IsNullOrEmpty(dummy.Code)
					&& string.IsNullOrWhiteSpace(dummy.CodeDescription),
				"CodeDescription is required when Code is set");

			dummy.OnValueChanged(nameof(dummy.Code)).Validate(nameof(dummy.CodeDescription));

			dummy.ValidateAllIncludingChildren();

			var dynamicData = dummy.MakeDocDataDynamic();
			var dummyNotifications = GetNotifications(dynamicData);

			AssertContainsExactElementsInAnyOrder("validation messages",
				Array.Empty<string>(),
				dummyNotifications);

			var code = dynamicData.GetDynamicProperty(nameof(dummy.Code));
			var codeNotifications = GetNotifications(code);

			AssertContainsExactElementsInAnyOrder("Code validation messages before setting value",
				Array.Empty<string>(),
				codeNotifications);

			var codeDescription = dynamicData.GetDynamicProperty(nameof(dummy.CodeDescription));
			var codeDescriptionNotifications = GetNotifications(codeDescription);

			AssertContainsExactElementsInAnyOrder("CodeDescription validation messages before setting Code value",
				Array.Empty<string>(),
				codeDescriptionNotifications);

			// this will set value on Code and clear all notifications
			code.SetValue("AAA");

			// CodeDescription notifications aren't explicitly cleared hence we need to clear them manually
			((DynamicData)codeDescription).ClearNotifications();

			AssertContainsExactElementsInAnyOrder("Code validation messages before setting invalid value",
				Array.Empty<string>(),
				codeNotifications);

			AssertContainsExactElementsInAnyOrder("CodeDescription validation messages before setting invalid value",
				new[]
				{
					"Error|CodeDescription is required when Code is set"
				},
				codeDescriptionNotifications);
		}

		public void TestValidate_DynamicData_ValidateMultipleRelated()
		{
			var dummy = new DummyDocDataObject("123");

			dummy.CodeDescriptionInfo.AddError(() =>
					!string.IsNullOrEmpty(dummy.Code)
					&& string.IsNullOrWhiteSpace(dummy.CodeDescription),
				"CodeDescription is required when Code is set");

			dummy.TextInfo.AddError(() =>
					!string.IsNullOrEmpty(dummy.Code)
					&& string.IsNullOrWhiteSpace(dummy.Text),
				"Text is required when Code is set");

			dummy.OnValueChanged(nameof(dummy.Code))
				.Validate(nameof(dummy.CodeDescription), nameof(dummy.Text));

			dummy.ValidateAllIncludingChildren();

			var dynamicData = dummy.MakeDocDataDynamic();
			var dummyNotifications = GetNotifications(dynamicData);

			AssertContainsExactElementsInAnyOrder("validation messages",
				Array.Empty<string>(),
				dummyNotifications);

			var code = dynamicData.GetDynamicProperty(nameof(dummy.Code));
			var codeNotifications = GetNotifications(code);

			AssertContainsExactElementsInAnyOrder("Code validation messages before setting value",
				Array.Empty<string>(),
				codeNotifications);

			var codeDescription = dynamicData.GetDynamicProperty(nameof(dummy.CodeDescription));
			var codeDescriptionNotifications = GetNotifications(codeDescription);

			AssertContainsExactElementsInAnyOrder("CodeDescription validation messages before setting Code value",
				Array.Empty<string>(),
				codeDescriptionNotifications);

			var text = dynamicData.GetDynamicProperty(nameof(dummy.Text));
			var textNotifications = GetNotifications(text);

			AssertContainsExactElementsInAnyOrder("Text validation messages before setting Code value",
				Array.Empty<string>(),
				textNotifications);

			// this will set value on Code and clear all notifications
			code.SetValue("AAA");

			// CodeDescription notifications aren't explicitly cleared hence we need to clear them manually
			((DynamicData)codeDescription).ClearNotifications();
			((DynamicData)text).ClearNotifications();

			AssertContainsExactElementsInAnyOrder("Code validation messages before setting invalid value",
				Array.Empty<string>(),
				codeNotifications);

			AssertContainsExactElementsInAnyOrder("CodeDescription validation messages before setting invalid value",
				new[]
				{
					"Error|CodeDescription is required when Code is set"
				},
				codeDescriptionNotifications);

			AssertContainsExactElementsInAnyOrder("Text validation messages before setting invalid value",
				new[]
				{
					"Error|Text is required when Code is set"
				},
				textNotifications);
		}

		public void TestValidate_DynamicData_AfterMergingFromOverrides()
		{
			var dummy = new DummyDocDataObject("123");
			dummy.CodeInfo.AddError(() => dummy.Code == "AAA", "Code has invalid value");

			dummy.ValidateAllIncludingChildren();

			var dynamicData = dummy.MakeDocDataDynamic();

			var xml = XDocument.Parse(
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Property Name=""Code"">
    <Value>AAA</Value>
  </Property>
</Entity>");

			dynamicData.MergeDataFromXml(xml);

			var dummyNotifications = GetNotifications(dynamicData);

			AssertContainsExactElementsInAnyOrder("DocDataObject validation messages",
				Array.Empty<string>(),
				dummyNotifications);

			var code = dynamicData.GetDynamicProperty(nameof(dummy.Code));
			AssertEquals("prerequisite: Code has been set from override", "AAA", code.Value);

			var codeNotifications = GetNotifications(code);

			AssertContainsExactElementsInAnyOrder("validation messages",
				new[]
				{
					"Error|Code has invalid value"
				},
				codeNotifications);
		}

		IEnumerable<string> GetNotifications(IDynamicData dynamicData)
		{
			return dynamicData
				.GetNotifications()
				.Select(n => $"{n.Type}|{n.Message}");
		}

		#endregion

		#region TestValidateWrappedProperty

		public void TestValidateWrappedProperty()
		{
			var dummy = new DummyDocDataObject
			{
				Text = "",
			};

			dummy.Wrapped = new DummyWrappedDocDataObject(dummy);
			dummy.TextInfo.AddMessageErrorIfEmpty();
			dummy.ValidateAllIncludingChildren();

			AssertHasMessageError(dummy.Wrapped.WrappedTextInfo, "Value is required.");
		}

		#endregion

		#region TestValidateAll

		public void TestValidateAll()
		{
			var dummy = new DummyDocDataObject
			{
				Text = "aaa"
			};

			AssertNoExceptionThrown("No exception thrown when valitating object with no validation rules",
				() => dummy.ValidateAll());

			AssertNoMessageError(dummy.TextInfo, "aaa is not a valid value.");

			dummy.AddValidationRule(nameof(dummy.Text),
				Core.NotificationType.MessageError,
				() => dummy.Text == "aaa",
				"aaa is not a valid value.");

			dummy.ValidateAll();

			AssertHasMessageError(dummy.TextInfo, "aaa is not a valid value.");
		}

		#endregion

		#region TestValidateAllIncludingChildren

		public void TestValidateAllIncludingChildren()
		{
			var dummy = new DummyDocDataObject
			{
				Text = "aaa"
			};

			AssertNoExceptionThrown("No exception thrown when valitating object with null children and no validation rules",
				() => dummy.ValidateAllIncludingChildren());

			dummy.Child = new DummyDocDataObject
			{
				Text = "aaa",
				Child = new DummyDocDataObject
				{
					Text = "aaa"
				}
			};

			dummy.Collection = new List<DummyDocDataObject>
			{
				new DummyDocDataObject
				{
					Text = "aaa"
				}
			};

			AddTextValidationRule(dummy);
			AddTextValidationRule(dummy.Child);
			AddTextValidationRule(dummy.Collection.Single());
			AddTextValidationRule(dummy.Child.Child);

			dummy.ValidateAllIncludingChildren();

			AssertHasMessageError(dummy.TextInfo, "aaa is not a valid value.");
			AssertHasMessageError(dummy.Child.TextInfo, "aaa is not a valid value.");
			AssertHasMessageError(dummy.Child.Child.TextInfo, "aaa is not a valid value.");
			AssertHasMessageError(dummy.Collection.Single().TextInfo, "aaa is not a valid value.");
		}

		void AddTextValidationRule(DummyDocDataObject dummy)
		{
			dummy.AddValidationRule(nameof(dummy.Text),
				Core.NotificationType.MessageError,
				() => dummy.Text == "aaa",
				"aaa is not a valid value.");
		}

		#endregion

		#region TestSettingInvalidZDateDoesNotThrowAnException

		public void TestSettingInvalidZDateDoesNotThrowAnException()
		{
			var dummy = new DummyDocDataObject
			{
				DateTime = new ZDateTime(2021, 1, 26)
			};

			var dynamicData = dummy.MakeDocDataDynamic();

			var date = dynamicData.GetDynamicProperty(nameof(dummy.DateTime));
			date.SetValue(ZDateTime.Invalid.ToString());

			AssertEquals("invalid date has been set", ZDateTime.Invalid, date.Value);
		}

		#endregion

		#region TestMergeOverrides

		public void TestMergeOverride()
		{
			var dummy = new DummyDocDataObject
			{
				Code = "AAA",
				Text = "frozen ducks"
			};

			var dynamicData = dummy.MakeDocDataDynamic();

			var xml = XDocument.Parse(
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Property Name=""Code"">
    <Value>OVR</Value>
  </Property>
  <Property Name=""Text"">
    <Value>overridden text</Value>
  </Property>
</Entity>");

			dynamicData.MergeDataFromXml(xml);

			var code = dynamicData.GetDynamicProperty(nameof(dummy.Code));
			var text = dynamicData.GetDynamicProperty(nameof(dummy.Text));

			AssertEquals("Code override was applied", "OVR", code.Value);
			AssertEquals("Text override was applied", "overridden text", text.Value);

			AssertEquals("Code override was applied to the DocDataObject", "OVR", dummy.Code);
			AssertEquals("Text override was applied to the DocDataObject", "overridden text", dummy.Text);
		}

		public void TestApplyOverride()
		{
			var dummy = new DummyDocDataObject
			{
				Code = "AAA",
				Text = "frozen ducks"
			};

			var dynamicData = dummy.MakeDocDataDynamic();

			var xml = XDocument.Parse(
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Property Name=""Code"">
    <Value>OVR</Value>
  </Property>
  <Property Name=""Text"">
    <Value>overridden text</Value>
  </Property>
</Entity>");

			dynamicData.ApplyDataFromXml(xml);

			AssertEquals("Code override was applied eagerly to the DocDataObject", "OVR", dummy.Code);
			AssertEquals("Text override was applied eagerly to the DocDataObject", "overridden text", dummy.Text);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DummyDocDataObject
			{
				Collection = Array.Empty<DummyDocDataObject>()
			};
		}

		#endregion
	}
}
