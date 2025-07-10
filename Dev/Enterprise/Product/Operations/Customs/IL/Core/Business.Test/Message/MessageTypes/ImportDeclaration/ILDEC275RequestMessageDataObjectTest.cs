using System;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP.DF_MSG10000_ImportDeclaration;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILDEC275RequestMessageDataObjectTest : ILEDIMessageDataObjectTest<ILDEC275RequestMessageDataObject, DfMsg10000ImportDeclaration, ILDEC274ResponseMessage>
	{
		protected override Type ExpectedPrettierType => null;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("ImportDeclarationRequest_275.xml"));
	}
}
