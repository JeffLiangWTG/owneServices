using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.IT.Business.Testing;

[TestsSubclassesOf(typeof(OneItemPasswordCollection<>))]
abstract class OneItemPasswordCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestMaxCountValidation()
	{
		var collection = (ISupportMaxCountValidation)GetCollectionToTest();
		CombineAssertions(() =>
		{
			var validator = collection.MaxCountValidator;
			var notification = validator.Notification;
			AssertEquals("MaxCount", 1, validator.MaxCount);
			AssertEquals("WarnAtHalfway", false, validator.WarnAtHalfway);
			AssertEquals("Notification Type", NotificationType.Error, notification.Type);
			AssertEquals("Notification Message", ExpectedMaxCountValidationMessage, notification.Message);
		});
	}

	protected abstract string ExpectedMaxCountValidationMessage { get; }

	public void TestErrorReportedIfMoreThanOneElementIsAdded()
	{
		var collection = GetCollectionToTest();
		collection.AddNew();
		AssertEquals("ErrorReporter.LastMessageReported", "", ErrorReporter.LastMessageReported);

		collection.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("ErrorReporter.LastKeyReported", $"{TestedTypeHelper.GetTestedType(GetType()).FullName} error", ErrorReporter.LastKeyReported);
			AssertEquals("ErrorReporter.LastMessageReported", "More than one element has been added to the collection. Only one is allowed", ErrorReporter.LastMessageReported);
		});
		ErrorReporter.Clear();
	}

	public override void TestAdd()
	{
		base.TestAdd();
		ErrorReporter.Clear();
	}

	public override void TestAddNew()
	{
		base.TestAddNew();
		ErrorReporter.Clear();
	}

	public override void TestDelete()
	{
		base.TestDelete();
		ErrorReporter.Clear();
	}

	public override void TestSuspendCountChanged()
	{
		base.TestSuspendCountChanged();
		ErrorReporter.Clear();
	}
}
