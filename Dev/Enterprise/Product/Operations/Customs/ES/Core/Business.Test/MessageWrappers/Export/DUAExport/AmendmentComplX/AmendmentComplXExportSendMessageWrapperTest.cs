using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(AmendmentComplXExportSendMessageWrapper))]
	class AmendmentComplXExportSendMessageWrapperTest : ExportSendMessageCommonWrapperAbstractTest<AmendmentComplXExportSendMessageWrapper>
	{
		public void TestCustomsProcedureCategory2()
		{
			AssertEquals("Expected filled CustomsProcedureCategory2", "X", wrapper.CustomsProcedureCategory2);
		}

		public void TestLinesType() => AssertType<AmendmentComplXExportLineWrapper>(wrapper.Lines.FirstOrDefault());

		protected override AmendmentComplXExportSendMessageWrapper GetProvider() => GetWrapper(entryHeader, Certificate);

		protected override AmendmentComplXExportSendMessageWrapper GetWrapper(CusEntryHeader entryheader, ICertificateProvider certificateData) => new AmendmentComplXExportSendMessageWrapper(entryheader, certificateData);

		protected override ZString ExpectedLocalReferenceNumber => "<<EXPORT_AMENDMENT_LOCAL_REF_NUMBER_PLACE_HOLDER>>";
	}
}
