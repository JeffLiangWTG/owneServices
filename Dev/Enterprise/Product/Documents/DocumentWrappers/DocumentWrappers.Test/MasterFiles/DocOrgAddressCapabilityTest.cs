using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocOrgAddressCapability))]
	public class DocOrgAddressCapabilityTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocOrgAddressCapability.New(CapabilityWrapper(),Factory)
			};
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocOrgAddressCapability.New(CapabilityWrapper(), Factory);
		}

		public void TestAddressType()
		{
			AssertEquals(nameof(AddressType.OFC), DocAddressCapability.AddressType);
			AssertEquals(nameof(AddressType.PIC), PICDocAddressCapability.AddressType);
		}

		public void TestEnabled()
		{
			AssertEquals(ZBool.True, DocAddressCapability.Enabled);
			AssertEquals(ZBool.False, PICDocAddressCapability.Enabled);
		}

		public void TestIsMain()
		{
			AssertEquals(ZBool.True, DocAddressCapability.IsMain);
			AssertEquals(ZBool.False, PICDocAddressCapability.IsMain);
		}

		OrgHeader NewHeaderForTest()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "Test";
			OrgAddress address = header.MainAddress;
			address.OA_Address1 = "TestAddress1";
			return header;
		}

		OrgAddressCapabilityWrapper CapabilityWrapper()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "Test";
			OrgAddress address = header.MainAddress;
			address.OA_Address1 = "TestAddress";
			OrgAddressCapabilityWrapper capability = address.AddressCapability.GetAddressCapabilityOnCode(nameof(AddressType.OFC));
			return capability;
		}

		OrgAddressCapabilityWrapper AddressCapability;
		OrgAddressCapabilityWrapper PICAddressCapability;
		DocOrgAddressCapability DocAddressCapability;
		DocOrgAddressCapability PICDocAddressCapability;

		protected override void SetUp()
		{
			AddressCapability = NewHeaderForTest().MainAddress.AddressCapability.GetAddressCapabilityOnCode(nameof(AddressType.OFC));
			PICAddressCapability = NewHeaderForTest().MainAddress.AddressCapability.GetAddressCapabilityOnCode(nameof(AddressType.PIC));
			DocAddressCapability = DocOrgAddressCapability.New(AddressCapability, Factory);
			PICDocAddressCapability = DocOrgAddressCapability.New(PICAddressCapability, Factory);
			AssertNotNull(DocAddressCapability);
			AssertNotNull(PICDocAddressCapability);
			base.SetUp();
		}
	}
}
