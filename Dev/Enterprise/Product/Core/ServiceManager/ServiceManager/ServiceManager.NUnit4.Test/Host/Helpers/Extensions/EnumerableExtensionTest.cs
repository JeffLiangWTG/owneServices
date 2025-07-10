using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Host.Testing
{
	class EnumerableExtensionTest
	{
		[Test]
		public void TestEnumerableExtensionIndexOfReturnsMinusOneIfNotFound()
		{
			// Arrange
			IEnumerable<int> enumerable = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
			// Act
			var index = enumerable.IndexOf(99);

			// Assert
			Assert.That(index, Is.EqualTo(-1));
		}

		[Test]
		public void TestEnumerableExtensionIndexOfReturnsExpectedIndex()
		{
			// Arrange
			IEnumerable<int> enumerable = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
			// Act
			var index = enumerable.IndexOf(9);

			// Assert
			Assert.That(index, Is.EqualTo(9 - 1));
		}
	}
}