using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(AutoIM515MessageProcessor))]
	sealed class AutoIM515MessageProcessorTest : AutoMessageProcessorTest
	{
		protected override ZString ExpectedMessageDescription => AESOutgoingMessageTypeListForDisplay.Descriptions.ExportOriginal;

		protected override string ExpectedMessageType => AESOutgoingMessageTypeList.Codes.ExportOriginal;

		protected override IProcessor CreateProcessor(BaseJobDeclaration declaration) => new AutoIM515MessageProcessor(declaration);

		protected override void PrepareDeclaration(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		}
	}
}
