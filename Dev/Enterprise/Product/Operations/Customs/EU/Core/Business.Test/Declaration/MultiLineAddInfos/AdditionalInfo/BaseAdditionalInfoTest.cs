using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	sealed class BaseAdditionalInfoTest : TestCaseWithFactory
	{
		public void TestKeyToDeterimeUniqueness()
		{
			additionalInfo.CSI_Code = "TYPE";
			additionalInfo.CSI_Description = "DESC";
			AssertEquals("TYPEDESC", additionalInfo.KeyToDeterimeUniqueness);
		}

		public void TestIsAnAdditionalReference()
		{
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			AssertEquals(true, additionalInfo.IsAnAdditionalReference);
		}

		public void TestIsAnAdditionalInformation()
		{
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			AssertEquals(true, additionalInfo.IsAnAdditionalInformation);
		}

		public void TestIsATransportDocument()
		{
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			AssertEquals(true, additionalInfo.IsATransportDocument);
		}

		protected override void SetUp()
		{
			base.SetUp();
			additionalInfo = Factory.New<ConcreteAdditionalInfoForTest>();
		}
		ConcreteAdditionalInfoForTest additionalInfo;
	}

	class ConcreteAdditionalInfoForTest : BaseAdditionalInfo
	{
		public ConcreteAdditionalInfoForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
