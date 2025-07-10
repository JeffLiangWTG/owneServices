using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.ExportMessageConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(AmendmentDUAExportSendMessageWrapper))]
	public class AmendmentDUAExportSendMessageWrapperTest : ExportSendMessageCommonWrapperAbstractTest<AmendmentDUAExportSendMessageWrapper>
	{
		public void TestMessageType()
		{
			AssertEquals("MessageType is 33", ExportDeclarationWrapperMessageTypeCodeList.ExportAmendmentDeclaration, wrapper.MessageType);
		}

		public void TestCustomsProcedureCategory5()
		{
			entryHeader.MovementReferenceNumberSetter(MovementReferenceNumber, ZDateTime.Today);
			AssertEquals("Expected filled CustomsProcedureCategory5", MovementReferenceNumber, wrapper.CustomsProcedureCategory5);
		}

		protected override AmendmentDUAExportSendMessageWrapper GetProvider() => GetWrapper(entryHeader, Certificate);

		protected override AmendmentDUAExportSendMessageWrapper GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new AmendmentDUAExportSendMessageWrapper(cusEntryHeader, certificateData);

		protected override ZString ExpectedLocalReferenceNumber => "<<EXPORT_AMENDMENT_LOCAL_REF_NUMBER_PLACE_HOLDER>>";
	}
}
