using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(BusinessObjectNotificationsViewerSupporter))]
	public sealed class BusinessObjectNotificationsViewerSupporterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var stringBuilder = new ZStringBuilder();
			var dummyBizObj1 = Factory.New<DummyBusinessObject>();
			dummyBizObj1.Z0_Code = "Code1";
			var dummyBizObj2 = Factory.New<DummyBusinessObject>();
			dummyBizObj2.Z0_Code = "Code2";

			using (dummyBizObj1.SuspendValidationTesting())
			using (dummyBizObj2.SuspendValidationTesting())
			{
				dummyBizObj1.Z0_CodeInfo.AddWarning("Test Warning");
			}

			var supporter = (BusinessObjectNotificationsViewerSupporter)GetNewBusinessObject();
			supporter.BusinessObjectCollection.Add(dummyBizObj1);
			supporter.BusinessObjectCollection.Add(dummyBizObj2);
			supporter.LoadingNotificationsProcessAction = (message, percent) =>
			{
				stringBuilder.AppendLine($"{message}|{percent}");
			};
			_ = supporter.NotificationsCollection;

			AssertEquals("BusinessObjectCollection.Count", 2, supporter.BusinessObjectCollection.Count);
			AssertEquals("HumanReadableColumnFieldNames.Count", 1, supporter.HumanReadableColumnFieldNames.Count);
			AssertEquals("HumanReadableColumnFieldNames[0]", DummyBusinessObject.Schema.Z0_Code, supporter.HumanReadableColumnFieldNames[0]);
			AssertEquals("HumanReadableColumnCaptionsAndWidth.Count", 1, supporter.HumanReadableColumnCaptionsAndWidth.Count);
			AssertEquals("HumanReadableColumnCaptionsAndWidth[0].Caption", "~Dummy Column~", supporter.HumanReadableColumnCaptionsAndWidth[0].Caption.Caption);
			AssertEquals("HumanReadableColumnCaptionsAndWidth[0].ColumnWidth", 100, supporter.HumanReadableColumnCaptionsAndWidth[0].ColumnWidth);
			AssertEquals("NotificationsCollection.Count", 1, supporter.NotificationsCollection.Count);
			AssertEquals("Loading notifications from CargoWise.EntityFramework.Testing.DummyBusinessObject|50\r\nLoading notifications from CargoWise.EntityFramework.Testing.DummyBusinessObject|100\r\nLoading notifications completed|100\r\n", stringBuilder.ToString());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var provider = new BusinessObjectCollectionNotificationsViewerProvider(Factory);
			return new BusinessObjectNotificationsViewerSupporter(provider);
		}
	}
}
