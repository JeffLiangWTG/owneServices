using Enterprise.Customs.CN.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.DataTransfer.Universal.Testing
{
	class UniversalCustomsDataObjectProviderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestTableSpecificCusSupportingInfoTypeList()
		{
			var provider = new UniversalCustomsDataObjectProvider();

			var supportingInfoJI = provider.TableSpecificCusSupportingInfoTypeList("JI");
			AssertEquals(2, supportingInfoJI.Count);
			Assert(supportingInfoJI.ContainsCode(Business.Constants.CusSupportingInfoTypes.CusSupportingDocument));
			Assert(supportingInfoJI.ContainsCode(Business.Constants.CusSupportingInfoTypes.CIQProductQualification));
		}

		public void TestTableSpecificCusAddInfoTypeList()
		{
			var provider = new UniversalCustomsDataObjectProvider();

			var addinfoTypeCEI = provider.TableSpecificCusAddInfoTypeList("CEI");
			AssertEquals(1, addinfoTypeCEI.Count);
			Assert(addinfoTypeCEI.ContainsCode("RQD"));
		}

		public void TestTableSpecificCusCodeDataTypeList()
		{
			var provider = new UniversalCustomsDataObjectProvider();

			var list = provider.TableSpecificCusCodeDataTypeList("CI");
			AssertEquals(1, list.Count);
			Assert(list.ContainsCode("ADI"));

			list = provider.TableSpecificCusCodeDataTypeList("JI");
			AssertEquals(2, list.Count);
			Assert(list.ContainsCode("CAT"));
			Assert(list.ContainsCode("CIQ"));

			list = provider.TableSpecificCusCodeDataTypeList("JE");
			AssertEquals(2, list.Count);
			Assert(list.ContainsCode("COF"));
			Assert(list.ContainsCode("MGR"));

			list = provider.TableSpecificCusCodeDataTypeList("CEI");
			AssertEquals(5, list.Count);
			Assert(list.ContainsCode("EPQ"));
			Assert(list.ContainsCode("SBI"));
			Assert(list.ContainsCode("PKG"));
			Assert(list.ContainsCode("OPM"));
			Assert(list.ContainsCode("ATH"));
		}

		public void TestTableSpecificCusCodeDataCodeList()
		{
			var provider = new UniversalCustomsDataObjectProvider();

			var list = provider.TableSpecificCusCodeDataCodeList("JI");
			AssertEquals(1, list.Count);
			Assert(list.ContainsCode("BN"));
		}

		public void TestGetNewJobDeclarationDataObjectReader()
		{
			var provider = new UniversalCustomsDataObjectProvider();
			var reader = provider.GetNewJobDeclarationDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new TestErrorLogger(), Factory, null);
			Assert(reader is CNJobDeclarationDataObjectReader);
		}

		public void TestGetNewDeclarationDataObjectWriter()
		{
			AssertType(typeof(CNJobDeclarationDataObjectWriter), new UniversalCustomsDataObjectProvider().GetNewDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<JobDeclaration>()))));
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
