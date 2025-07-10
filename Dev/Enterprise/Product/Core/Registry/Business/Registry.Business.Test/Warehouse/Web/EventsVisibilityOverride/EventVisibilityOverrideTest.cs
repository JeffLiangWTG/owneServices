using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EventVisibilityOverride))]
	class EventVisibilityOverrideTest : RegistryBusinessObjectTest
	{
		public void TestDefaultValues()
		{
			var eventVisibilityOverride = new EventVisibilityOverride();
			Assert(eventVisibilityOverride.IncludeRelatedEvents);
		}

		public void TestSetCode_SetsDescription()
		{
			AssertNullOrEmpty("Prerequisite: Code", BizObj.Code);
			AssertNullOrEmpty("Prerequisite: Description", BizObj.Description);

			var code1 = "HVC";
			BizObj.Code = code1;
			BizObj.RunPreSaveValidation();
			AssertNoNotifications("Prerequesite: Validate to ensure the test code is in list", BizObj);

			AssertEquals(code1, BizObj.Code);
			AssertNotNullOrEmpty("Description", BizObj.Description);
			AssertNoNotifications(BizObj);

			var code2 = ZString.Empty;
			BizObj.Code = code2;
			AssertNullOrEmpty("Code", BizObj.Code);
			AssertNullOrEmpty("Description", BizObj.Description);
		}

		public void TestValidateCode_UnknownWorkflowCode()
		{
			BizObj.Code = "HVC";
			BizObj.RunPreSaveValidation();
			AssertNoNotifications(BizObj);

			BizObj.Code = "...";
			BizObj.RunPreSaveValidation();
			AssertHasWarning(BizObj.CodeInfo, "Unknown workflow code.");
		}

		public void TestDeserialize()
		{
			var xmlString = @"
<EventVisibilityOverride>
	<CodeMaxLength>3</CodeMaxLength>
	<Code>CLH</Code>
	<Description>Container Load List</Description>
	<ArrayOfEventVisibilityOverrideSetting>
		<EventVisibilityOverrideSetting>
			<CodeMaxLength>3</CodeMaxLength>
			<Code>AED</Code>
			<Description />
			<EventCode>AED</EventCode>
			<Description />
			<IsSystem>Y</IsSystem>
			<IsActive>Y</IsActive>
			<EventDetail>Y</EventDetail>
			<QuickView>N</QuickView>
			<EventDescriptionOverride>All Export Documents Received</EventDescriptionOverride>
			<DuplicateEventsHandlingMethod>ALL</DuplicateEventsHandlingMethod>
			<IncludeEstimates>N</IncludeEstimates>
		</EventVisibilityOverrideSetting>
		<EventVisibilityOverrideSetting>
			<CodeMaxLength>3</CodeMaxLength>
			<Code>AED</Code>
			<Description />
			<EventCode>AED</EventCode>
			<Description />
			<IsSystem>Y</IsSystem>
			<IsActive>Y</IsActive>
			<EventDetail>Y</EventDetail>
			<QuickView>N</QuickView>
			<EventDescriptionOverride>All Export Documents Received</EventDescriptionOverride>
			<DuplicateEventsHandlingMethod>ALL</DuplicateEventsHandlingMethod>
			<IncludeEstimates>N</IncludeEstimates>
		</EventVisibilityOverrideSetting>
	</ArrayOfEventVisibilityOverrideSetting>
	<IncludeRelatedEvents>Y</IncludeRelatedEvents>
</EventVisibilityOverride>";
			var dummyDataType = new DummyNonPersistentBusinessObjectRegistryDataType(ExpectedBusinessObjectType);
			var deserialisedBusinessObject = (EventVisibilityOverride)dummyDataType.Deserialise(Encoding.UTF8.GetBytes(xmlString));

			AssertEquals(3, deserialisedBusinessObject.CodeMaxLength);
			AssertEquals("CLH", deserialisedBusinessObject.Code);
			AssertEquals(2, deserialisedBusinessObject.EventVisibilityOverrideSettings.Count);
		}

		#region Test Cases

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			var clonedTypes = (EventVisibilityOverride)clone;
			CombineAssertions(() =>
			{
				AssertEquals(3, clonedTypes.CodeMaxLength);
				AssertEquals("AAS", clonedTypes.Code);
				AssertEquals(2, clonedTypes.EventVisibilityOverrideSettings.Count);
				AssertEquals("OR1", clonedTypes.EventVisibilityOverrideSettings[0].Code);
				Assert(clonedTypes.IncludeRelatedEvents);
			});
		}

		public override void TestMaxDescriptionLength()
		{
			AssertEquals("MaxDescriptionLength", 256, BizObj.DescriptionInfo.MaxLength);
		}

		#endregion

		#region Implementation
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetEventVisibilityOverrideWithSettingOverrides();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetEventVisibilityOverrideWithSettingOverrides();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new EventVisibilityOverride();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new EventVisibilityOverride BizObj
		{
			get { return (EventVisibilityOverride)base.BizObj; }
		}

		EventVisibilityOverride GetEventVisibilityOverrideWithSettingOverrides()
		{
			var collection = new EventVisibilityOverrideCollection();
			var eventOverride = collection.AddNew();
			eventOverride.Code = "AAS";
			eventOverride.IncludeRelatedEvents = true;
			eventOverride.EventVisibilityOverrideSettings.RemoveAll();

			var or1 = eventOverride.EventVisibilityOverrideSettings.AddNew();
			or1.EventCode = "OR1";

			var or2 = eventOverride.EventVisibilityOverrideSettings.AddNew();
			or2.EventCode = "OR2";

			return eventOverride;
		}

		#endregion
	}
}
