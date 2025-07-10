using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(TallyOutturnHeader))]
	public class TallyOutturnHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestOutturns()
		{
			TallyOutturnHeader header = Factory.New<TallyOutturnHeader>();
			AssertNotNull(header.Outturns);
			AssertEquals(typeof(TallyOutturnHeaderOutturnCollection), header.Outturns.GetType());
			AssertEquals(true, header.IsRegisteredEditableChildObject(header.Outturns));
			TallyOutturn outturn = header.Outturns.AddNew();
			AssertEquals(header, outturn.Header);
		}
	}
}
