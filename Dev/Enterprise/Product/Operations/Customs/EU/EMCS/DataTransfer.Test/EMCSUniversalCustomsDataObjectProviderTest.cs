using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.EMCS.DataTransfer.Testing
{
	class EMCSUniversalCustomsDataObjectProviderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetNewDeclarationDataObjectWriter()
		{
			var emcs = Factory.New<EMCSJobDeclaration>();
			var provider = new EMCSUniversalCustomsDataObjectProvider();
			AssertType<EMCSDeclarationDataObjectWriter>(provider.GetNewDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(null, emcs))));
		}

		public void TestGetNewJobDeclarationDataObjectReader()
		{
			var provider = new EMCSUniversalCustomsDataObjectProvider();
			AssertType<EMCSDeclarationDataObjectReader>(provider.GetNewJobDeclarationDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new TestErrorLogger(), Factory, null));
		}

		public void TestCusCodeDataGetTypeListForJobDeclaration()
		{
			var list = new EMCSUniversalCustomsDataObjectProvider().TableSpecificCusCodeDataTypeList(JobDeclarationSchema.Constants.Prefix, "");
			AssertEquals(1, list.Count);
			AssertEquals(EU.Business.CusCodeDataTypeList.Codes.OfficeCode, EU.Business.CusCodeDataTypeList.Descriptions.OfficeCode, list.GetDescriptionFromCode(EU.Business.CusCodeDataTypeList.Codes.OfficeCode));
		}

		public void TestCusCodeDataGetTypeListForJobComInvoiceLine()
		{
			var list = new EMCSUniversalCustomsDataObjectProvider().TableSpecificCusCodeDataTypeList(JobComInvoiceLineSchema.Constants.Prefix, "");
			AssertEquals(1, list.Count);
			AssertEquals(Business.CusCodeDataTypeList.Codes.WineCode, Business.CusCodeDataTypeList.Descriptions.WineCode, list.GetDescriptionFromCode(Business.CusCodeDataTypeList.Codes.WineCode));
		}

		public void TestTableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList()
		{
			AssertNull(new EMCSUniversalCustomsDataObjectProvider().TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(JobDeclarationSchema.Constants.Prefix, ""));
		}
	}
}
