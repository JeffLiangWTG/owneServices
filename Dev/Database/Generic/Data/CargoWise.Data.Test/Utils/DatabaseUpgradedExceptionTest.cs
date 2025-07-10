using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	public class DatabaseUpgradedExceptionTest : TestCase
	{
		public void TestConstructor_DoesNotUseResourceStrings()
		{
			// Arrange
			var mockResStrings = new Moq.Mock<IResourceStrings>();
			Res.SetResourceStringsGetter(() => mockResStrings.Object);
			try
			{
				// Act
				new DatabaseUpgradedException();

				// Assert
				AssertEquals("Should not use resource strings since that can hit the database", 0, mockResStrings.Invocations.Count);
			}
			finally
			{
				Res.SetResourceStringsGetter(null);
			}
		}
	}
}
