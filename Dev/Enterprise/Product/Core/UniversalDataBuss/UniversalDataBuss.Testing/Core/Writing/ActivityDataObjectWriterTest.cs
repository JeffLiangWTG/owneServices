using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Testing.Core.Writing
{
	class ActivityDataObjectWriterTest : TestCaseWithUniversalObjectFactory
	{
		public void TestGetUserDefinedValues()
		{
			var template = CustomFieldProcessTaskTemplateTestHelper.GetTemplate(Factory.BOFactory);
			CustomFieldProcessTaskTemplateTestHelper.AddCustomField(template, "Squanch");
			var dummy = CustomFieldProcessTaskTemplateTestHelper.GetDummy(Factory.BOFactory);

			dummy.SetUserDefinedValue("Squanch", new ZString("Schwifty"));

			var writer = new ActivityDataObjectWriter_ForTest(new DataWritingManager(new ActionInfo(null, dummy)));
			var activity = writer.GetDataObject(dummy);

			var customFields = activity.CustomizedFieldCollection.Select(x => $"{x.DataType}: {x.Key} - {x.Value}");
			AssertContainsExactElementsInAnyOrder(new[] { "String: Squanch - Schwifty" }, customFields);
		}

		class ActivityDataObjectWriter_ForTest : ActivityDataObjectWriter<DummyWithCustomFields>
		{
			public ActivityDataObjectWriter_ForTest(IDataWritingManager writeManager)
				: base(writeManager)
			{
			}

			protected override DataContextType GetTopLevelDataContextType()
			{
				throw new NotImplementedException(); // Not needed for this test
			}

			protected override void PopulateDataObject(DummyWithCustomFields sourceBO, Activity dataObject)
			{
				// not needed for this test.
			}
		}
	}
}
