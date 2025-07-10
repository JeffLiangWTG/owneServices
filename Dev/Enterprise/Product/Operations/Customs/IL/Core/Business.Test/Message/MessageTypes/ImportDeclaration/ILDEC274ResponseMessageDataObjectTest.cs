using System;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP.DF_MSG10000_ImportDeclaration;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILDEC274ResponseMessageDataObjectTest : ILEDIMessageDataObjectTest<ILDEC274ResponseMessageDataObject, DfNg2754Msg10004ImportDeclarationResponse, ILDEC274ResponseMessage>
	{
		public void TestGetImportDeclarationResponseAdaptor()
		{
			var importDeclarationResponseAdaptor = messageDataObject.GetImportDeclarationResponseAdaptor();
			CombineAssertions("When Get Import Declaration Response Adaptor", () =>
			{
				AssertEquals("ExternalDeclarationID", "42430006402024", importDeclarationResponseAdaptor.ExternalDeclarationID);
				AssertEquals("CustomsStatusNameCode", "99", importDeclarationResponseAdaptor.CustomsStatusNameCode);
				AssertEquals("DeclarationVersionID", "0.3", importDeclarationResponseAdaptor.DeclarationVersionID);
				AssertEquals("CustomsDeclarationNumber", "24013304469780", importDeclarationResponseAdaptor.CustomsDeclarationNumber);
			});
		}

		protected override Type ExpectedPrettierType => typeof(ILDEC274ResponseMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("ImportDeclarationResponse_2754.xml"));
	}
}
