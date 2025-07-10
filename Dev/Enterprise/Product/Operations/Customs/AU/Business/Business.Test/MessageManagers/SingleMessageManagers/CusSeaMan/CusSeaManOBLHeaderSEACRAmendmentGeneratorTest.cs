using System;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSeaManOBLHeaderSEACRAmendmentGeneratorTest : CMRAmendmentGeneratorAbstractTest
	{
		public void TestUniqueIdentifierInfos()
		{
			var header = Bizo as CusSeaManOBLHeader;
			var result = ((CusSeaManOBLHeaderSEACRAmendmentGeneratorForTest)Generator).UniqueIdentifierInfos;
			AssertEquals("Length", 3, result.Length);
			AssertEquals("Result[0]", header.TransportHeader.BT_VesselNameInfo, result[0]);
			AssertEquals("Result[1]", header.TransportHeader.BT_VoyageNumInfo, result[1]);
			AssertEquals("Result[2]", header.BO_OceanBillInfo, result[2]);
		}

		protected override ICMRAmendmentGeneratorForTest GetGenerator(BusinessObject bizo) => new CusSeaManOBLHeaderSEACRAmendmentGeneratorForTest(bizo as CusSeaManOBLHeader);

		protected override Type ExpectedMessageType => typeof(CMRSEACRMessage);

		protected override BusinessObject GetSavedBizo()
		{
			var transportHeader = Factory.New<CusSeaManTranHead>();
			var result = transportHeader.OceanBills.AddNew();
			Factory.Save();
			return result;
		}

		sealed class CusSeaManOBLHeaderSEACRAmendmentGeneratorForTest : CusSeaManOBLHeaderSEACRAmendmentGenerator, ICMRAmendmentGeneratorForTest
		{
			public CusSeaManOBLHeaderSEACRAmendmentGeneratorForTest(CusSeaManOBLHeader transportHeader) : base(transportHeader)
			{
			}

			public new ZPropertyInfo[] UniqueIdentifierInfos => base.UniqueIdentifierInfos;
			public new EDIMessage GenerateOriginalMessageCore() => base.GenerateOriginalMessageCore();
			public new EDIMessage GenerateAmendmentMessageCore() => base.GenerateAmendmentMessageCore();
			public new EDIMessage GenerateWithdrawalMessageCore(BusinessObject bizo, BusinessObject bizoInNewFactory) => base.GenerateWithdrawalMessageCore(bizo, bizoInNewFactory);
		}
	}
}
