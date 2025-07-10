using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.DataTransfer.Universal.Testing
{
	class UniversalCustomsDataObjectProviderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestGetNewJobDeclarationDataObjectReader()
		{
			AssertType<JobDeclarationDataObjectReader>(new UniversalCustomsDataObjectProvider().GetNewJobDeclarationDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new TestErrorLogger(), Factory, null));
		}

		public void TestGetNewUniversalDataObjectReaderHelper()
		{
			AssertType<UniversalDataObjectReaderHelper>(new UniversalCustomsDataObjectProvider().GetNewUniversalDataObjectReaderHelper(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
		}

		public void TestGetNewDeclarationDataObjectWriter()
		{
			AssertType<DeclarationDataObjectWriter>(new UniversalCustomsDataObjectProvider().GetNewDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, Factory.New<JobDeclaration>()))));
		}

		public void TestTableSpecificCusReferenceTypeList()
		{
			AssertNull(new UniversalCustomsDataObjectProvider().TableSpecificCusReferenceTypeList(CusEntryInstructionSchema.Constants.Prefix, string.Empty));
		}
	}
}
