using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase4.Testing
{
	class NctsMoveHeaderDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestGetNewBusinessObject_Departure()
		{
			var reader = SetupReader(NctsMovementType.Codes.Departure);
			AssertType<NctsDepartureMovementHeader>(reader.GetNewBusinessObject());
		}

		public void TestGetNewBusinessObject_Arrival()
		{
			var reader = SetupReader(NctsMovementType.Codes.Arrival);
			AssertType<NctsArrivalMovementHeader>(reader.GetNewBusinessObject());
		}

		public void TestGetExistingBusinessObject_Departure()
		{
			var reader = SetupReader(NctsMovementType.Codes.Departure);
			AssertType<NctsDepartureMovementHeader>(reader.GetExistingBusinessObject());
		}

		public void TestGetExistingBusinessObject_Arrival()
		{
			var reader = SetupReader(NctsMovementType.Codes.Arrival);
			AssertType<NctsArrivalMovementHeader>(reader.GetExistingBusinessObject());
		}

		NctsMoveHeaderDataObjectReaderForTest SetupReader(string headerType)
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.BH_HeaderType = headerType;
			var writeManager = new DataWritingManager(new ActionInfo(null, header));
			var writer = new NctsHeaderDataObjectWriter(writeManager);
			var shipmentData = writer.GetDataObject(header);
			var helper = new UniversalDataObjectReaderHelper(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, shipmentData.GetSourceCountryCode(), shipmentData.GetDataProviderForCodeMapping());
			return new NctsMoveHeaderDataObjectReaderForTest(shipmentData.CommercialInfo, shipmentData, new TestErrorLogger(), helper, header);
		}

		class NctsMoveHeaderDataObjectReaderForTest : NctsMoveHeaderDataObjectReader
		{
			public NctsMoveHeaderDataObjectReaderForTest(CommercialInfo commercialInfoDataObject, Shipment shipmentDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, NctsHeader header)
				: base(commercialInfoDataObject, shipmentDataObject, logger, helper, header)
			{
			}

			public new NctsCommonMovementHeader GetNewBusinessObject() => base.GetNewBusinessObject();

			public new NctsCommonMovementHeader GetExistingBusinessObject() => base.GetExistingBusinessObject();
		}
	}
}
