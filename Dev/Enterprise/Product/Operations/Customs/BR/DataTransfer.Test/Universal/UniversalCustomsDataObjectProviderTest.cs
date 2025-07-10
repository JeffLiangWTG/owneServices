using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.DataTransfer.Universal.Testing
{
	public class UniversalCustomsDataObjectProviderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetNewJobDeclarationDataObjectReader()
		{
			var provider = new UniversalCustomsDataObjectProvider();
			var reader = provider.GetNewJobDeclarationDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new TestErrorLogger(), Factory, null);
			Assert(reader is BRJobDeclarationDataObjectReader);
		}

		public void TestGetNewDeclarationDataObjectWriter()
		{
			AssertType(typeof(BRJobDeclarationDataObjectWriter), new UniversalCustomsDataObjectProvider().GetNewDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<JobDeclaration>()))));
		}

		public void TestTableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList()
		{
			AssertNull(new UniversalCustomsDataObjectProvider().TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList("JE", ""));
		}

		public void TestTableSpecificCusCodeDataTypeList()
		{
			var provider = new UniversalCustomsDataObjectProvider();

			var list = provider.TableSpecificCusCodeDataTypeList(JobDeclarationSchema.Constants.Prefix);
			AssertEquals(3, list.Count);
			Assert(list.ContainsCode(CusCodeDataTypeList.Codes.CustomsOffice));
			Assert(list.ContainsCode(CusCodeDataTypeList.Codes.CustomsEnclosure));
			Assert(list.ContainsCode(CusCodeDataTypeList.Codes.WarehouseArea));

			list = provider.TableSpecificCusCodeDataTypeList(JobComInvoiceLineSchema.Constants.Prefix);
			AssertEquals(3, list.Count);
			Assert(list.ContainsCode(CusCodeDataTypeList.Codes.Attribute));
			Assert(list.ContainsCode(CusCodeDataTypeList.Codes.TariffDetach));
			Assert(list.ContainsCode(CusCodeDataTypeList.Codes.NVE));
		}

		public void TestTableSpecificCusReferenceTypeList()
		{
			AssertNull(new UniversalCustomsDataObjectProvider().TableSpecificCusReferenceTypeList(CusEntryInstructionSchema.Constants.Prefix, string.Empty));
		}

		public void TestTableSpecificCusSupportingInfoTypeList()
		{
			var provider = new UniversalCustomsDataObjectProvider();
			var list = provider.TableSpecificCusSupportingInfoTypeList(JobComInvoiceHeaderSchema.Constants.Prefix);
			AssertEquals(1, list.Count);
			Assert(list.ContainsCode(CusSupportingInfoTypeList.Codes.ExchangeHedge));

			list = provider.TableSpecificCusSupportingInfoTypeList(JobComInvoiceLineSchema.Constants.Prefix);
			AssertEquals(11, list.Count);
			Assert(list.ContainsCode(CusSupportingInfoTypeList.Codes.ElectronicLogisticInvoice));
			Assert(list.ContainsCode(CusSupportingInfoTypeList.Codes.ImportLicense));
			Assert(list.ContainsCode(CusSupportingInfoTypeList.Codes.MercosulForeignDeclaration));
			Assert(list.ContainsCode(CusSupportingInfoTypeList.Codes.TaxRegime));
			Assert(list.ContainsCode(CusSupportingInfoTypeList.Codes.LegalAct));
			Assert(list.ContainsCode(CusSupportingInfoTypeList.Codes.Drawback));
			Assert(list.ContainsCode(CusSupportingInfoTypeList.Codes.ConsentingProcess));
			Assert(list.ContainsCode(CusSupportingInfoTypeList.Codes.CertificateOfOrigin));
			Assert(list.ContainsCode(CusSupportingInfoTypeList.Codes.PreviousDocument));
			Assert(list.ContainsCode(CusSupportingInfoTypeList.Codes.Permit));
			Assert(list.ContainsCode(CusSupportingInfoTypeList.Codes.DuimpTaxRegime));
		}
	}
}
