using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	public class EDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			AssertNull(typeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			AssertNull(typeDecider.GetTypeForNew());
		}

		public void TestGetTypeForLoad()
		{
			var newFactory = new BusinessObjectFactory();
			var outboundMessage = Factory.New<EMCSOutboundEDIMessage>();
			Factory.Save();
			AssertEquals(typeof(EMCSOutboundEDIMessage), newFactory.Load<EDIMessage>(outboundMessage.PK).GetType());

			var inboundMessage = Factory.New<EMCSInboundEDIMessage>();
			Factory.Save();
			AssertEquals(typeof(EMCSInboundEDIMessage), newFactory.Load<EDIMessage>(inboundMessage.PK).GetType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			typeDecider = new EDIMessageTypeDecider();
		}
		EDIMessageTypeDecider typeDecider;
	}
}
