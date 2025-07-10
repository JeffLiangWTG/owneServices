using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ZAddressListTest : TestCase
	{
		public void TestECAAddressOrFallback()
		{
			AssertEquals("Should find ZGuid.Empty", ZGuid.Empty, List.ECAAddressOrFallback);

			AddAddress(First_PK, "1ST");
			AssertEquals("Should find first item in list)", First_PK, List.ECAAddressOrFallback);

			AddAddress(OFC_PK, "OFC");
			AssertEquals("Should find OFC address", OFC_PK, List.ECAAddressOrFallback);

			AddAddress(ECA_PK, nameof(AddressType.ECA));
			AssertEquals("Should find ECA address", ECA_PK, List.ECAAddressOrFallback);

			AddAddress(ZGuid.NewZGuid(), nameof(AddressType.ECA), false);
			AssertEquals("Should find ECA address", ECA_PK, List.ECAAddressOrFallback);

			AddAddress(DefECA_PK, nameof(AddressType.ECA), true);
			AssertEquals("Should find default ECA address", DefECA_PK, List.ECAAddressOrFallback);
		}

		public void TestPICAddressOrFallback()
		{
			AssertEquals("Should find ZGuid.Empty", ZGuid.Empty, List.PICAddressOrFallback);

			AddAddress(First_PK, "1ST");
			AssertEquals("Should find first item in list)", First_PK, List.PICAddressOrFallback);

			AddAddress(OFC_PK, "OFC");
			AssertEquals("Should find OFC address", OFC_PK, List.PICAddressOrFallback);

			AddAddress(PAD_PK, "PAD");
			AssertEquals("Should find PAD address", PAD_PK, List.PICAddressOrFallback);

			AddAddress(PIC_PK, nameof(AddressType.PIC));
			AssertEquals("Should find PIC address", PIC_PK, List.PICAddressOrFallback);

			AddAddress(ZGuid.NewZGuid(), nameof(AddressType.PIC), false);
			AssertEquals("Should find PIC address", PIC_PK, List.PICAddressOrFallback);

			AddAddress(DefPIC_PK, nameof(AddressType.PIC), true);
			AssertEquals("Should find default PIC address", DefPIC_PK, List.PICAddressOrFallback);
		}

		public void TestPICAddressOrFallbackWithDefaultPAD()
		{
			AddAddress(First_PK, "1ST");
			AddAddress(OFC_PK, "OFC");
			AddAddress(PAD_PK, "PAD");
			AssertEquals("Should find PAD address", PAD_PK, List.PICAddressOrFallback);

			AddAddress(PIC_PK, nameof(AddressType.PIC));
			AssertEquals("Should find PIC address", PIC_PK, List.PICAddressOrFallback);

			AddAddress(ZGuid.NewZGuid(), "PAD", false);
			AssertEquals("Should find PIC address", PIC_PK, List.PICAddressOrFallback);

			AddAddress(DefPIC_PK, "PAD", true);
			AssertEquals("Should find default PAD address", DefPIC_PK, List.PICAddressOrFallback);
		}

		public void TestDLVAddressOrFallback()
		{
			AssertEquals("Should find ZGuid.Empty", ZGuid.Empty, List.DLVAddressOrFallback);

			AddAddress(First_PK, "1ST");
			AssertEquals("Should find first item in list)", First_PK, List.DLVAddressOrFallback);

			AddAddress(OFC_PK, "OFC");
			AssertEquals("Should find OFC address", OFC_PK, List.DLVAddressOrFallback);

			AddAddress(PAD_PK, "PAD");
			AssertEquals("Should find PAD address", PAD_PK, List.DLVAddressOrFallback);

			AddAddress(DLV_PK, nameof(AddressType.DLV));
			AssertEquals("Should find DLV address", DLV_PK, List.DLVAddressOrFallback);

			AddAddress(ZGuid.NewZGuid(), nameof(AddressType.DLV), false);
			AssertEquals("Should find DLV address", DLV_PK, List.DLVAddressOrFallback);

			AddAddress(DefDLV_PK, nameof(AddressType.DLV), true);
			AssertEquals("Should find default DLV address", DefDLV_PK, List.DLVAddressOrFallback);
		}

		public void TestDLVAddressOrFallbackWithDefaultPAD()
		{
			AddAddress(First_PK, "1ST");
			AddAddress(OFC_PK, "OFC");
			AddAddress(PAD_PK, "PAD");
			AssertEquals("Should find PAD address", PAD_PK, List.DLVAddressOrFallback);

			AddAddress(DLV_PK, nameof(AddressType.DLV));
			AssertEquals("Should find DLV address", DLV_PK, List.DLVAddressOrFallback);

			AddAddress(ZGuid.NewZGuid(), "PAD", false);
			AssertEquals("Should find DLV address", DLV_PK, List.DLVAddressOrFallback);

			AddAddress(DefDLV_PK, "PAD", true);
			AssertEquals("Should find default PAD address", DefDLV_PK, List.DLVAddressOrFallback);
		}

		public void TestARMAddressOrFallback()
		{
			AssertEquals("Should find ZGuid.Empty", ZGuid.Empty, List.ARMAddressOrFallback);

			AddAddress(First_PK, "1ST");
			AssertEquals("Should find first item in list)", First_PK, List.ARMAddressOrFallback);

			AddAddress(OFC_PK, "OFC");
			AssertEquals("Should find OFC address", OFC_PK, List.ARMAddressOrFallback);

			AddAddress(ARM_PK, nameof(AddressType.ARM));
			AssertEquals("Should find ARM address", ARM_PK, List.ARMAddressOrFallback);

			AddAddress(ZGuid.NewZGuid(), nameof(AddressType.ARM), false);
			AssertEquals("Should find ARM address", ARM_PK, List.ARMAddressOrFallback);

			AddAddress(DefARM_PK, nameof(AddressType.ARM), true);
			AssertEquals("Should find default ARM address", DefARM_PK, List.ARMAddressOrFallback);
		}

		public void TestCSTAddressOrFallback()
		{
			AssertEquals("Should find ZGuid.Empty", ZGuid.Empty, List.CSTAddressOrFallback);

			AddAddress(First_PK, "1ST");
			AssertEquals("Should find first item in list)", First_PK, List.CSTAddressOrFallback);

			AddAddress(OFC_PK, "OFC");
			AssertEquals("Should find OFC address", OFC_PK, List.CSTAddressOrFallback);

			AddAddress(CST_PK, nameof(AddressType.CST));
			AssertEquals("Should find CST address", CST_PK, List.CSTAddressOrFallback);

			AddAddress(ZGuid.NewZGuid(), nameof(AddressType.CST), false);
			AssertEquals("Should find CST address", CST_PK, List.CSTAddressOrFallback);

			AddAddress(DefCST_PK, nameof(AddressType.CST), true);
			AssertEquals("Should find default CST address", DefCST_PK, List.CSTAddressOrFallback);
		}

		public void TestAPMAddressOrFallback()
		{
			AssertEquals("Should find ZGuid.Empty", ZGuid.Empty, List.APMAddressOrFallback);

			AddAddress(First_PK, "1ST");
			AssertEquals("Should find first item in list)", First_PK, List.APMAddressOrFallback);

			AddAddress(OFC_PK, "OFC");
			AssertEquals("Should find OFC address", OFC_PK, List.APMAddressOrFallback);

			AddAddress(APM_PK, nameof(AddressType.APM));
			AssertEquals("Should find APM address", APM_PK, List.APMAddressOrFallback);

			AddAddress(ZGuid.NewZGuid(), nameof(AddressType.APM), false);
			AssertEquals("Should find APM address", APM_PK, List.APMAddressOrFallback);

			AddAddress(DefAPM_PK, nameof(AddressType.APM), true);
			AssertEquals("Should find default APM address", DefAPM_PK, List.APMAddressOrFallback);
		}

		public void TestSQMAddressOrFallback()
		{
			AssertEquals("Should find ZGuid.Empty", ZGuid.Empty, List.SQMAddressOrFallback);

			AddAddress(First_PK, "1ST");
			AssertEquals("Should find first item in list)", First_PK, List.SQMAddressOrFallback);

			AddAddress(OFC_PK, "OFC");
			AssertEquals("Should find OFC address", OFC_PK, List.SQMAddressOrFallback);

			AddAddress(SQM_PK, nameof(AddressType.SQM));
			AssertEquals("Should find SQM address", SQM_PK, List.SQMAddressOrFallback);

			AddAddress(ZGuid.NewZGuid(), nameof(AddressType.SQM), false);
			AssertEquals("Should find SQM address", SQM_PK, List.SQMAddressOrFallback);

			AddAddress(DefSQM_PK, nameof(AddressType.SQM), true);
			AssertEquals("Should find default SQM address", DefSQM_PK, List.SQMAddressOrFallback);
		}

		public void TestOFCAddressOrFallback()
		{
			AssertEquals("Should find ZGuid.Empty", ZGuid.Empty, List.OFCAddressOrFallback);

			AddAddress(First_PK, "1ST");
			AssertEquals("Should find first item in list)", First_PK, List.OFCAddressOrFallback);

			AddAddress(OFC_PK, "OFC");
			AssertEquals("Should find OFC address", OFC_PK, List.OFCAddressOrFallback);

			AddAddress(ZGuid.NewZGuid(), "OFC", false);
			AssertEquals("Should find OFC address", OFC_PK, List.OFCAddressOrFallback);

			AddAddress(DefOFC_PK, "OFC", true);
			AssertEquals("Should find default OFC address", DefOFC_PK, List.OFCAddressOrFallback);
		}

		public void TestPADAddressOrFallback()
		{
			AssertEquals("Should find ZGuid.Empty", ZGuid.Empty, List.PADAddressOrFallback);

			AddAddress(First_PK, "1ST");
			AssertEquals("Should find first item in list)", First_PK, List.PADAddressOrFallback);

			AddAddress(PAD_PK, "PAD");
			AssertEquals("Should find PAD address", PAD_PK, List.PADAddressOrFallback);

			AddAddress(ZGuid.NewZGuid(), "PAD", false);
			AssertEquals("Should find PAD address", PAD_PK, List.PADAddressOrFallback);

			AddAddress(DefPAD_PK, "PAD", true);
			AssertEquals("Should find default PAD address", DefPAD_PK, List.PADAddressOrFallback);
		}

		protected override void SetUp()
		{
			base.SetUp();

			List = new ZAddressList();
			First_PK = ZGuid.NewZGuid();
			PAD_PK = ZGuid.NewZGuid();
			DLV_PK = ZGuid.NewZGuid();
			PIC_PK = ZGuid.NewZGuid();
			OFC_PK = ZGuid.NewZGuid();
			ARM_PK = ZGuid.NewZGuid();
			APM_PK = ZGuid.NewZGuid();
			SQM_PK = ZGuid.NewZGuid();
			CST_PK = ZGuid.NewZGuid();
			ECA_PK = ZGuid.NewZGuid();

			DefPAD_PK = ZGuid.NewZGuid();
			DefDLV_PK = ZGuid.NewZGuid();
			DefPIC_PK = ZGuid.NewZGuid();
			DefOFC_PK = ZGuid.NewZGuid();
			DefARM_PK = ZGuid.NewZGuid();
			DefAPM_PK = ZGuid.NewZGuid();
			DefSQM_PK = ZGuid.NewZGuid();
			DefCST_PK = ZGuid.NewZGuid();
			DefECA_PK = ZGuid.NewZGuid();
		}

		void AddAddress(ZGuid pK, string addressType)
		{
			AddAddress(pK, addressType, false);
		}

		void AddAddress(ZGuid pK, string addressType, bool isDefault)
		{
			AddressCapabilityItem capability = new AddressCapabilityItem() { Capability = addressType, IsDefault = isDefault };
			List.AddAddress(pK, "", "", capability);
		}

		ZAddressList List;
		ZGuid First_PK;
		ZGuid PAD_PK;
		ZGuid DLV_PK;
		ZGuid PIC_PK;
		ZGuid OFC_PK;
		ZGuid ARM_PK;
		ZGuid APM_PK;
		ZGuid SQM_PK;
		ZGuid CST_PK;
		ZGuid ECA_PK;

		ZGuid DefPAD_PK;
		ZGuid DefDLV_PK;
		ZGuid DefPIC_PK;
		ZGuid DefOFC_PK;
		ZGuid DefARM_PK;
		ZGuid DefAPM_PK;
		ZGuid DefSQM_PK;
		ZGuid DefCST_PK;
		ZGuid DefECA_PK;
	}
}
