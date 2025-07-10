using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Business.Testing
{
	sealed class LineMergerTest : Customs.Business.Testing.LineMergerTest
	{
		public void TestCreateEmptyEntryHeaderForEcrWhenMerging()
		{
			var declaration = GetJobDeclaration() as JobDeclaration;
			var messageSendingContext = new MessageSendingContext() { EnableMessageVisual = true, ProcedureCode = JPProcedureCodeList.Codes.ECR };
			declaration.SetCurrentMessageSendingContext(messageSendingContext);
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			var newLineMerger = GetNewLineMerger(declaration);
			newLineMerger.DoMerge();
			AssertEquals("2 empty entry headers should be create for 2 instruction during merging.", 2, declaration.CustomsEntryHeaders.Count);
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.CustomsEntryHeaders.AddNew();
			return declaration;
		}

		protected override Customs.Business.LineMerger GetNewLineMerger(BaseJobDeclaration declaration) => new LineMerger(declaration as JobDeclaration);

		protected override Type[] ExpectedEntryCreationStrategiesType => new[] { typeof(EntryCreationStrategy) };
	}
}
