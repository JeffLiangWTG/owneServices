using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	public class DeclarationDmExtPreviousDocumentWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationDmExtPreviousDocument>
	{
		public void TestNewOrNull()
		{
			AssertNull(DeclarationDmExtPreviousDocumentWrapper.NewOrNull(null));
			AssertNotNull(Provider);
		}

		public void TestID()
		{
			AssertEquals("ReferenceNumber", Provider.ID.Value);
		}

		public void TestTypeCode()
		{
			AssertEquals("CODE", Provider.TypeCode.Value);
		}

		protected override IDeclarationDmExtPreviousDocument GetProvider()
		{
			var previousDocument = Factory.New<CusSupportingInfo>();
			previousDocument.CSI_Code = "CODE";
			previousDocument.CSI_ReferenceNumber = "ReferenceNumber";
			return DeclarationDmExtPreviousDocumentWrapper.NewOrNull(previousDocument);
		}
	}
}
