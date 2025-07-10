using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Registry.Business.Testing
{
	[TestedType(typeof(ShippingAgentObjectWrapper))]
	public class ShippingAgentObjectWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShippingAgents()
		{
			var filter = wrappedShippingAgentObject.ShippingAgents.CompleteFilter;
			Assert(header.MatchesFilter(filter));
		}

		public void TestShippingAgentValidation()
		{
			wrappedShippingAgentObject.ValidateShippingAgentAddress();
			AssertHasErrorContaining(wrappedShippingAgentObject.ShippingAgentAddressInfo, MandatoryValidation.MustBeEntered);
			wrappedShippingAgentObject.ShippingAgentAddress_ZAddress.OrgPK = ZGuid.Invalid;
			AssertHasErrorContaining(wrappedShippingAgentObject.ShippingAgentAddressInfo, MandatoryValidation.MustBeEntered);
			wrappedShippingAgentObject.ShippingAgentAddress = ZGuid.Invalid;
			AssertHasErrorContaining(wrappedShippingAgentObject.ShippingAgentAddressInfo, "Enter a valid selection.");
			wrappedShippingAgentObject.ShippingAgentAddress_ZAddress.OrgPK = header.PK;
			wrappedShippingAgentObject.ShippingAgentAddress = header.MainAddress.PK;
			AssertNoErrors(wrappedShippingAgentObject.ShippingAgentAddressInfo);
		}

		public void TestDefaultAddressFromOrg()
		{
			var secondAddress = header.Addresses.AddNew();
			secondAddress.OA_Code = "ADDR1";
			secondAddress.OA_Address1 = "ADDRESS 1";
			Factory.Save();
			wrappedShippingAgentObject.ShippingAgentAddress_ZAddress.OrgPK = header.PK;
			AssertEquals(header.MainAddress.PK, wrappedShippingAgentObject.ShippingAgentAddress);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<OrgHeader>();
			header.OH_Code = "HEAD1";
			Factory.Save();
			wrappedShippingAgentObject = new ShippingAgentObjectWrapper(new ShippingAgentObject());
		}

		protected override BusinessObject GetNewBusinessObject() => wrappedShippingAgentObject;
		OrgHeader header;
		ShippingAgentObjectWrapper wrappedShippingAgentObject;
	}
}
