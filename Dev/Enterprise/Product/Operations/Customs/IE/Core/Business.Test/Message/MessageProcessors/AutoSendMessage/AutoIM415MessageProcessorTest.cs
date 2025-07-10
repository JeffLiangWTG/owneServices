using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(AutoIM415MessageProcessor))]
	sealed class AutoIM415MessageProcessorTest : AutoMessageProcessorTest
	{
		protected override ZString ExpectedMessageDescription => AISOutgoingMessageTypeListForDisplay.Descriptions.CustomsDeclaration;

		protected override string ExpectedMessageType => AISOutgoingMessageTypeList.Codes.CustomsDeclaration;

		protected override IProcessor CreateProcessor(BaseJobDeclaration declaration) => new AutoIM415MessageProcessor(declaration);

		protected override void PrepareDeclaration(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
		}
	}
}
