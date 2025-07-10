using System.Data;
using CargoWise.Common;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DatabaseUpgradeInProgressExceptionTest : TestCase
	{
		public void TestConstructor_DoesNotUseResourceStrings()
		{
			// Arrange
			var mockResStrings = new Moq.Mock<IResourceStrings>();
			using (new DisposableAction(
				() => Res.SetResourceStringsGetter(() => mockResStrings.Object),
				() => Res.SetResourceStringsGetter(null)))
			{
				// Act
				new DatabaseUpgradeInProgressException();

				// Assert
				AssertEquals(
					"Should not use resource strings since that can hit the database",
					0,
					mockResStrings.Invocations.Count);
			}
		}

		public void TestConstructor_DoesNotHitDb()
		{
			// Arrange
			Db.Connection.CloseConnection();

			// Act
			new DatabaseUpgradeInProgressException();

			// Assert
			AssertEquals(ConnectionState.Closed, Db.Connection.State);
		}
	}
}
