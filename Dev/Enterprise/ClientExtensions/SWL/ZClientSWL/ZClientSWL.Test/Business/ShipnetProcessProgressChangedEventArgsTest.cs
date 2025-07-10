using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.SWL.Business.Testing
{
	public class ShipnetProcessProgressChangedEventArgsTest : ShipnetTestCase
	{
		public void TestConstructor()
		{
			StmALog log = Factory.New<StmALog>();
			ShipnetProcessProgressChangedEventArgs e = new ShipnetProcessProgressChangedEventArgs(2, 10, log);
			AssertEquals("Count", 2, e.Count);
			AssertEquals("Total", 10, e.Total);
			AssertEquals("Log", log, e.Log);
		}
	}
}
