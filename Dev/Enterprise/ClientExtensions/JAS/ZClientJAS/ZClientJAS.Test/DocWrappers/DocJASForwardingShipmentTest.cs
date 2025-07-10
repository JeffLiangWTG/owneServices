using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.DocWrappers.Testing
{
	[TestedType(typeof(DocJASForwardingShipment))]
	public class DocJASForwardingShipmentTest : DocumentWrapperTestCase
	{
		public void TestBONDNumber()
		{
			var wrapper = DocJASForwardingShipment.New(Factory.NewWithValidTestData<ForwardingShipment>(), Factory);
			JASDataRegistry.Instance.BOLBondNum = "BOLBondNum1";
			AssertEquals("BOLBondNum1", wrapper.BONDNumber);
			JASDataRegistry.Instance.BOLBondNum = "BOLBondNum2";
			AssertEquals("BOLBondNum2", wrapper.BONDNumber);
		}

		#region Setup
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocJASForwardingShipment.New(Factory.New<ForwardingShipment>(), Factory) };
		}
		#endregion
	}
}
