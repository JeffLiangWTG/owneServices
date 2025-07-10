using NUnit.Framework;

namespace Enterprise.DataTransfer.IO.Testing
{
	public abstract class FileSystemInformationTest : TestCase
	{
		public abstract void TestFullName();
		public abstract void TestName();
		public abstract void TestExists();
		public abstract void TestDelete();

		public abstract string FileSystemObjectName { get; }
	}
}
