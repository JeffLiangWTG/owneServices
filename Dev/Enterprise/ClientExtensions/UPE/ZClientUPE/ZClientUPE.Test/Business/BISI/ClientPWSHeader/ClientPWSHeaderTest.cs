using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(ClientPWSHeader))]
	public class ClientPWSHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDelete()
		{
			ClientPWSHeader header = Factory.NewWithValidTestData<ClientPWSHeader>();
			header.Charges.AddNew();
			header.Charges.AddNew();
			header.Charges.AddNew();
			header = Factory.NewWithValidTestData<ClientPWSHeader>();
			header.Charges.AddNew();
			header.Charges.AddNew();
			Factory.Save();
			ClientPWSHeader[] headers = Factory.Load<ClientPWSHeader>(new ZQuery());
			Assert(headers.Length == 2);
			ClientPWSCharge[] charges = Factory.Load<ClientPWSCharge>(new ZQuery());
			Assert(charges.Length == 5);
			header.Delete();
			Factory.Save();
			headers = Factory.Load<ClientPWSHeader>(new ZQuery());
			Assert(headers.Length == 1);
			charges = Factory.Load<ClientPWSCharge>(new ZQuery());
			Assert(charges.Length == 3);
		}

		public void TestCharges()
		{
			ClientPWSHeader header = Factory.New<ClientPWSHeader>();
			AssertNotNull(header.Charges);
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
