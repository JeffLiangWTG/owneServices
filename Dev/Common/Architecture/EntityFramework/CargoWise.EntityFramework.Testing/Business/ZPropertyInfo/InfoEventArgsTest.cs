namespace CargoWise.EntityFramework.Testing
{
	sealed class InfoEventArgsTest : TestCaseWithDummy
	{
		public void TestConstructor()
		{
			ZPropertyInfoString info = new ZPropertyInfoString(Dummy, "Trevor");
			InfoEventArgs e = new InfoEventArgs(info);
			AssertEquals(info, e.Info);
		}
	}
}
