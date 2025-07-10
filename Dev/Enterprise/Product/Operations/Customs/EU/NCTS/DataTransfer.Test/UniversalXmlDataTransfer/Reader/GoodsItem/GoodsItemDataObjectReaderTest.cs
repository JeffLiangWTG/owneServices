using CargoWise.Types;
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
	class GoodsItemDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestGetExistingBusinessObject()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.MovementHeader.GoodsItems.AddNew();
			var reader = SetupReader(header, header.MovementHeader);
			AssertNull(reader.GetExistingBusinessObject());
		}

		public void TestGetNewBusinessObject_Departure()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.MovementHeader.GoodsItems.AddNew();
			var reader = SetupReader(header, header.MovementHeader);
			AssertType<NctsDepartureCargoDesc>(reader.GetNewBusinessObject());
		}

		public void TestGetNewBusinessObject_Arrival()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.ArrivalMovementHeader.GoodsItems.AddNew();
			var reader = SetupReader(header, header.ArrivalMovementHeader);
			AssertType<NctsArrivalAndUnloadingCargoDesc>(reader.GetNewBusinessObject());
		}

		GoodsItemDataObjectReaderForTest SetupReader(NctsHeader header, NctsCommonMovementHeader movementHeader)
		{
			var writeManager = new DataWritingManager(new ActionInfo(null, header));
			var writer = new NctsHeaderDataObjectWriter(writeManager);
			var shipmentData = writer.GetDataObject(header);
			var helper = new UniversalDataObjectReaderHelper(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, shipmentData.GetSourceCountryCode(), shipmentData.GetDataProviderForCodeMapping());
			return new GoodsItemDataObjectReaderForTest(shipmentData, new TestErrorLogger(), helper, header, movementHeader, shipmentData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0], 1);
		}

		class GoodsItemDataObjectReaderForTest : GoodsItemDataObjectReader<NctsCommonCargoDesc>
		{
			public GoodsItemDataObjectReaderForTest(Shipment moveHeaderDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, NctsHeader header, NctsCommonMovementHeader moveHeader, CommercialInvoiceLine commercialInvoiceLine, ZShort currentLineNumber)
				: base(moveHeaderDataObject, logger, helper, header, moveHeader, commercialInvoiceLine, currentLineNumber)
			{
			}

			public new NctsCommonCargoDesc GetNewBusinessObject() => base.GetNewBusinessObject();

			public new NctsCommonCargoDesc GetExistingBusinessObject() => base.GetExistingBusinessObject();
		}
	}
}
