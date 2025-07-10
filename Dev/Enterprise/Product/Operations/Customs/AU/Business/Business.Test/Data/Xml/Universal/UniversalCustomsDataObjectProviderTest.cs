using System.Linq;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class UniversalCustomsDataObjectProviderTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetNewUniversalDataObjectReaderHelper()
		{
			AssertType<UniversalDataObjectReaderHelper>(new UniversalCustomsDataObjectProvider().GetNewUniversalDataObjectReaderHelper(new UniversalObjectFactory(new CargoWise.EntityFramework.BusinessObjectFactory()), Core.Constants.CountryCodes.Australia, string.Empty));
		}

		public void TestGetNewDeclarationDataObjectWriter()
		{
			AssertType<DeclarationDataObjectWriter>(new UniversalCustomsDataObjectProvider().GetNewDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<JobDeclaration>()))));
		}

		public void TestGetNewJobDeclarationDataObjectReader()
		{
			AssertType<JobDeclarationDataObjectReader>(new UniversalCustomsDataObjectProvider().GetNewJobDeclarationDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new TestErrorLogger(), Factory, null));
		}

		public void TestGetNewStandaloneCommercialInvoiceDataObjectReader()
		{
			AssertType<StandaloneCommercialInvoiceDataObjectReader>(new UniversalCustomsDataObjectProvider().GetNewStandaloneCommercialInvoiceDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new CommercialInvoiceHeader(), new TestErrorLogger(), Factory));
		}

		public void TestGetNewAirManifestDataObjectReader()
		{
			AssertType<CusMAWBDataObjectReader>(new UniversalCustomsDataObjectProvider().GetNewAirManifestDataObjectReaders(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), null, new TestErrorLogger(), Factory, false).FirstOrDefault());
		}

		public void TestStandaloneCommercialInvoiceDataObjectWriter()
		{
			AssertType<StandaloneCommercialInvoiceDataObjectWriter>(new UniversalCustomsDataObjectProvider().GetNewStandaloneCommercialInvoiceDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<JobComInvoiceHeader>()))));
		}

		public void TestGetNewAirManifestDataObjectWriter()
		{
			AssertType<CusMAWBDataObjectWriter>(new UniversalCustomsDataObjectProvider().GetNewAirManifestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<CusMAWB>()))));
		}

		public void TestGetNewAirManifestLineDataObjectWriter()
		{
			AssertType<CusHAWBDataObjectWriter>(new UniversalCustomsDataObjectProvider().GetNewAirManifestLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<CusHAWB>())), null));
		}

		public void TestSeaCargoReaderWriterTypes()
		{
			var provider = DataTransfer.Universal.Extensions.GetUniversalCustomsDataObjectProvider(Factory.BOFactory, "AU");
			AssertType<UniversalCustomsDataObjectProvider>(provider);
			var readers = provider.GetNewCusSCAOceanBillDataObjectReaders(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), null, new TestErrorLogger(), Factory);
			AssertType<CMRCusSCAOceanBillDataObjectReader>(readers.Single());
			AssertType<CMRCusSCAOceanBillDataObjectWriter>(provider.GetNewCusSCAOceanBillDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.HSA, Factory.New<CusSCAOceanBill>()))));
		}

		public void TestCusAddInfoTypeListForInvoiceLine()
		{
			var list = new UniversalCustomsDataObjectProvider().TableSpecificCusAddInfoTypeList(JobComInvoiceLineSchema.Constants.Prefix);
			AssertEquals(1, list.Count);
			AssertEquals(CusAddInfoTypeAttribute.Codes.AURFPNumber, "RFP Numbers", list.GetDescriptionFromCode(CusAddInfoTypeAttribute.Codes.AURFPNumber));
		}

		public void TestCusCodeDataListForInvoiceLine()
		{
			var list = new UniversalCustomsDataObjectProvider().TableSpecificCusCodeDataTypeList(JobComInvoiceLineSchema.Constants.Prefix);
			AssertEquals(1, list.Count);
			AssertEquals(CusCodeDataTypeList.Codes.ICSPermit, CusCodeDataTypeList.Descriptions.ICSPermit, list.GetDescriptionFromCode(CusCodeDataTypeList.Codes.ICSPermit));

			list = new UniversalCustomsDataObjectProvider().TableSpecificCusCodeDataCodeList(JobComInvoiceLineSchema.Constants.Prefix);
			AssertEquals(1, list.Count);
			AssertEquals(CusCodeDataTypeList.Codes.ICSPermit, CusCodeDataTypeList.Descriptions.ICSPermit, list.GetDescriptionFromCode(CusCodeDataTypeList.Codes.ICSPermit));
		}

		public void TestTableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList()
		{
			AssertNull(new UniversalCustomsDataObjectProvider().TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(JobDeclarationSchema.Constants.Prefix, ""));
		}

		public void TestTableSpecificCusReferenceTypeList()
		{
			AssertNull(new UniversalCustomsDataObjectProvider().TableSpecificCusReferenceTypeList(CusEntryInstructionSchema.Constants.Prefix, string.Empty));
		}
	}
}
