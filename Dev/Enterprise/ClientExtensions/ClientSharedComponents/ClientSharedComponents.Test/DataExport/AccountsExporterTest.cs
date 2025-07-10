using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Testing
{
	public abstract class AccountsExporterTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2008, 2, 8, 1, 2, 3)]
		public void TestFilename()
		{
			Assert("Filename not empty", !Exporter.FileName.IsEmpty);
			AssertEquals("Correct filename format", FileNamePrefix + "_20080208010203_0000.csv", Exporter.FileName);
		}

		protected NotificationBuffer Notifications
		{
			get { return notifications ?? (notifications = new NotificationBuffer()); }
		}
		NotificationBuffer notifications;

		protected abstract AccountsExporter Exporter { get; }
		protected abstract string FileNamePrefix { get; }
	}
}
