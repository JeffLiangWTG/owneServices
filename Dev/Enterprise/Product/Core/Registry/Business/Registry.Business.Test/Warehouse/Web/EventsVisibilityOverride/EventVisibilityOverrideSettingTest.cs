using System.Collections.Generic;
using System.Text;
using Enterprise.Registry.Business.Web;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EventVisibilityOverrideSetting))]
	sealed class EventVisibilityOverrideSettingTest : RegistryBusinessObjectTest
	{
		public void TestDefaultValues()
		{
			var overrideSetting = new EventVisibilityOverrideSetting();
			CombineAssertions(() =>
			{
				Assert(overrideSetting.IsActive);
				Assert(overrideSetting.EventDetail);
				Assert(!overrideSetting.QuickView);
				Assert(!overrideSetting.IncludeEstimates);
				AssertEquals(DuplicateEventHandlingMethodList.Codes.ShowAll, overrideSetting.DuplicateEventsHandlingMethod);
			});
		}

		public void TestDefaultValuesOfSystem()
		{
			var expectedCodes = new HashSet<string> { "ARV", "DEP", "COF", "CAV", "RTC", "SRC", "GOU", "GIN", "FLO", "FUL", "PCF", "DCF", "UPC", "PKC", "CLR", "RLS", "SHL", "DHR" };

			var collection = new EventVisibilityOverrideCollection();
			var eventOverride1 = collection.AddNew();
			eventOverride1.Code = "SHP";
			var overrideSettings1 = eventOverride1.EventVisibilityOverrideSettings;

			foreach (EventVisibilityOverrideSetting setting in overrideSettings1)
			{
				Assert(expectedCodes.Contains(setting.EventCode) == setting.IsSystem);
			}

			var eventOverride2 = collection.AddNew();
			eventOverride2.Code = "HVO";
			var overrideSettings2 = eventOverride1.EventVisibilityOverrideSettings;

			foreach (EventVisibilityOverrideSetting setting in overrideSettings2)
			{
				Assert(expectedCodes.Contains(setting.EventCode) == setting.IsSystem);
			}
		}

		public void TestColumnsEditableOrReadOnlyWhenSystemIsTrue()
		{
			var collection = new EventVisibilityOverrideCollection();
			var eventOverride = collection.AddNew();
			eventOverride.Code = "SHP";
			eventOverride.IncludeRelatedEvents = true;

			var eventVisibilityOverrideSetting = eventOverride.EventVisibilityOverrideSettings.AddNew();
			eventVisibilityOverrideSetting.EventCode = "ARV";
			Assert(eventVisibilityOverrideSetting.IsActiveInfo.ReadOnly);
			Assert(eventVisibilityOverrideSetting.EventDetailInfo.ReadOnly);
			Assert(!eventVisibilityOverrideSetting.IncludeEstimatesInfo.ReadOnly);
			Assert(!eventVisibilityOverrideSetting.QuickViewInfo.ReadOnly);
			Assert(!eventVisibilityOverrideSetting.EventDescriptionOverrideInfo.ReadOnly);
			Assert(!eventVisibilityOverrideSetting.DuplicateEventsHandlingMethodInfo.ReadOnly);
		}

		public void TestColumnsEditableOrReadOnlyWhenSystemIsFalse()
		{
			var collection = new EventVisibilityOverrideCollection();
			var eventOverride = collection.AddNew();
			eventOverride.Code = "SHP";
			eventOverride.IncludeRelatedEvents = true;

			var eventVisibilityOverrideSetting = eventOverride.EventVisibilityOverrideSettings.AddNew();
			eventVisibilityOverrideSetting.EventCode = "AED";
			Assert(!eventVisibilityOverrideSetting.IsActiveInfo.ReadOnly);
			Assert(!eventVisibilityOverrideSetting.EventDetailInfo.ReadOnly);
			Assert(!eventVisibilityOverrideSetting.IncludeEstimatesInfo.ReadOnly);
			Assert(!eventVisibilityOverrideSetting.QuickViewInfo.ReadOnly);
			Assert(!eventVisibilityOverrideSetting.EventDescriptionOverrideInfo.ReadOnly);
			Assert(!eventVisibilityOverrideSetting.DuplicateEventsHandlingMethodInfo.ReadOnly);
		}

		public void TestEventDescriptionOverrideReadOnly()
		{
			BizObj.IsActive = true;
			Assert("When IsActive is true, EventDescriptionOverride should not be read only", !BizObj.EventDescriptionOverrideInfo.ReadOnly);

			BizObj.IsActive = false;
			Assert("When IsActive is false, EventDescriptionOverride should be read only", BizObj.EventDescriptionOverrideInfo.ReadOnly);
		}

		public void TestIsSystemIsTrueWhenWorkflowCodeIsSHPAndEventCodeIsARV()
		{
			var eventOverride = new EventVisibilityOverride("SHP");
			var collection = new EventVisibilityOverrideSettingCollection(eventOverride);
			var eventOverrideSetting = collection.AddNew();
			eventOverrideSetting.EventCode = "ARV";

			Assert(eventOverrideSetting.IsSystem);
		}

		public void TestIsSystemIsFalseWhenWorkflowCodeIsNotSHP()
		{
			var eventOverride = new EventVisibilityOverride("HVH");
			var collection = new EventVisibilityOverrideSettingCollection(eventOverride);
			var eventOverrideSetting = collection.AddNew();

			AssertEquals(false, eventOverrideSetting.IsSystem);
		}

		public void TestIsSystemIsTrueWhenWorkflowCodeIsSHPAndEventCodeIsAED()
		{
			var eventOverride = new EventVisibilityOverride("SHP");
			var collection = new EventVisibilityOverrideSettingCollection(eventOverride);
			var eventOverrideSetting = collection.AddNew();
			eventOverrideSetting.EventCode = "AED";

			AssertEquals(false, eventOverrideSetting.IsSystem);
		}

		public void TestReadMoreElement()
		{
			var xmlString = @"
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
</EventVisibilityOverrideSetting>";
			var dummyDataType = new DummyNonPersistentBusinessObjectRegistryDataType(ExpectedBusinessObjectType);
			var deserialisedBusinessObject = (EventVisibilityOverrideSetting)dummyDataType.Deserialise(Encoding.UTF8.GetBytes(xmlString));

			AssertEquals(3, deserialisedBusinessObject.CodeMaxLength);
			AssertEquals("AED", deserialisedBusinessObject.Code);
			AssertEquals("", deserialisedBusinessObject.Description);
			AssertEquals("AED", deserialisedBusinessObject.EventCode);
			AssertEquals(true, deserialisedBusinessObject.IsActive);
			AssertEquals(true, deserialisedBusinessObject.EventDetail);
			AssertEquals(false, deserialisedBusinessObject.QuickView);
			AssertEquals("All Export Documents Received", deserialisedBusinessObject.EventDescriptionOverride);
			AssertEquals("ALL", deserialisedBusinessObject.DuplicateEventsHandlingMethod);
			AssertEquals(false, deserialisedBusinessObject.IncludeEstimates);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var businessObject = (EventVisibilityOverrideSetting)base.GetBusinessObjectToClone();
			businessObject.EventCode = "AAS";
			return businessObject;
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			var clonedTypes = (EventVisibilityOverrideSetting)clone;
			CombineAssertions(() =>
			{
				AssertEquals(5, clonedTypes.CodeMaxLength);
				AssertEquals("AAS", clonedTypes.EventCode);
			});
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		new EventVisibilityOverrideSetting BizObj => (EventVisibilityOverrideSetting)base.BizObj;

		#endregion
	}
}
