using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class FilenameEventArgsTest : TransactionedTestCase
	{
		public void TestFilenameEventArgs()
		{
			using (TempFile file = TempFile.New())
			{
				FilenameEventArgs args = new FilenameEventArgs();
				AssertEquals("Filename empty by default", ZString.Empty, args.UnmappedFilename);

				args.UnmappedFilename = file.Filename;
				AssertEquals("Filename saved is same as set", file.Filename, args.UnmappedFilename);
			}
		}
	}
}
