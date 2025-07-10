using WTG.TestHelpers.SpecTesting;

namespace Enterprise.SqlSecurity.SpecTesting.Regenerate
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			SpecFileWriter.WriteSpecFiles(new ServerLevelInfoSpecBuilder());
			SpecFileWriter.WriteSpecFiles(new DatabaseLevelInfoSpecBuilder());
		}
	}
}
