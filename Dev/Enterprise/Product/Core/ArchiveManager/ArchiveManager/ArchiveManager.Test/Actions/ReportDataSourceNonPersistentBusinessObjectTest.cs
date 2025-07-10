using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.Actions
{
	[TestedType(typeof(ReportDataSourceNonPersistentBusinessObject))]
	sealed class PurgeRecordCountNonPersistentBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
			=> new ReportDataSourceNonPersistentBusinessObject();

		#endregion
	}

	[TestedType(typeof(ReportDataSourceNonPersistentBusinessObjectCollection))]
	sealed class PurgeRecordCountNonPersistentBusinessObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ReportDataSourceNonPersistentBusinessObjectCollection>
	{
		#region Implementation

		protected override ReportDataSourceNonPersistentBusinessObjectCollection GetCollectionToTest()
			=> new(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
			=> new ReportDataSourceNonPersistentBusinessObject(Factory);

		#endregion
	}

	sealed class PurgeRecordCountNonPersistentBusinessObjectValidationTest : BusinessObjectValidationTestCase
	{
	}
}
