using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	abstract class TempFileTestCase : TestCaseWithFactory
	{
		protected virtual string DefaultPath
		{
			get { return UnitTestingConstants.TestFilesDir; }
		}

		protected virtual string PrepFile(string filename)
		{
			FileInfo pristine = new FileInfo(Path.Combine(DefaultPath, filename));
			FileInfo working = pristine.CopyTo(Env.GetTempFileName(), true);
			working.Attributes &= ~(FileAttributes.ReadOnly);

			return working.FullName;
		}
	}
}
