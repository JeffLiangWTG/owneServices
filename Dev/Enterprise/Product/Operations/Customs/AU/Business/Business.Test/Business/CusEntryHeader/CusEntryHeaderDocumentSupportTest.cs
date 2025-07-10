using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderDocumentSupporter))]
	public sealed class CusEntryHeaderDocumentSupportTest : Customs.Business.Testing.CusEntryHeaderDocumentSupportTest
	{
		public void TestGetBODocDataProvidersNotFoundMessageOnEntryHeader()
		{
			AssertEquals("Entry Header cannot be found. Please selecting Brokerage > Answer Declaration Questions to Merge the Declaration.", entryHeader.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.EFTPaymentAdvice), null));
			AssertEquals("Entry Header cannot be found. Please selecting Brokerage > Answer Declaration Questions to Merge the Declaration.", entryHeader.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.ATD), null));
		}

		public override void TestSupportedDataContexts()
		{
			DocumentSupporter documentSupporter = entryHeader.DocumentSupporter;
			AssertEquals("DataContext.ATD is Supported", true, documentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.ATD)));
			AssertEquals("DataContext.EFTPaymentAdvice is Supported", true, documentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.EFTPaymentAdvice)));
			// And one from Base
			AssertEquals("DataContext.CusEntryHeader is Supported", true, documentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.CusEntryHeader)));
		}

		public void TestATDGetDocBusinessObjectsAndDataStateBeforeRun()
		{
			DocumentWrapper[] result = entryHeader.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ATD, null);
			AssertEquals("No ATD on CusEntryHeaders - no wrappers created", 0, result.Length);

			StmMenuItem aTDMenu = Factory.New<StmMenuItem>();
			aTDMenu.SU_MenuName = "Authority To Deal";
			AssertEquals("GetDataState should be false", ZBool.False, declaration.DocumentSupporter.GetDataStateBeforeRun(aTDMenu).IsValid);
			EDIMessage aTDMessage = entryHeader.Messages.AddNew(typeof(CMRATDMessage));

			aTDMessage.EM_MessageText = CusEntryHeaderTest.ATDMessageText;

			result = entryHeader.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ATD, null);
			AssertEquals("GetDataState should be true", ZBool.True, entryHeader.DocumentSupporter.GetDataStateBeforeRun(aTDMenu).IsValid);
			AssertEquals("1 ATD on CusEntryHeaders - 1 wrapper should be created", 1, result.Length);
			AssertEquals("Wrapper type DocCusEntryHeader", "Enterprise.DocumentWrappers.Customs.AU.DocCusEntryHeader", result[0].GetType().ToString());
		}

		public void TestEFTPaymentAdviceWrapperWithPAYREC()
		{
			declaration.JE_DeclarationReference = "S12345";
			entryHeader.CH_BGMReference = "S12345/1";

			entryHeader.Messages.Add(CreateCMRPAYRECMessage(Factory));
			AssertEquals("1 PAYREC wrapper", 1, entryHeader.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.EFTPaymentAdvice, null).Length);
		}

		public void TestEFTPaymentAdviceWrapperWithPAYRECAndREFACC()
		{
			declaration.JE_DeclarationReference = "S12345";
			entryHeader.CH_BGMReference = "S12345/1";

			entryHeader.Messages.Add(CreateCMRPAYRECMessage(Factory));
			entryHeader.Messages.Add(CreateCMRREFACCMessage(Factory));

			AssertEquals("2 wrappers", 2, entryHeader.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.EFTPaymentAdvice, null).Length);
		}

		protected override void SetUp()
		{
			declaration = JobDeclaration.New(Factory);
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			base.SetUp();
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_DeclarationReference = "B00148999";
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			EDIMessage aTDMessage = entryHeader.Messages.AddNew(typeof(CMRATDMessage));
			aTDMessage.EM_MessageText = CusEntryHeaderTest.ATDMessageText;
			entryHeader.Messages.Add(CreateCMRPAYRECMessage(Factory));
			return entryHeader;
		}

		public static CMRPAYRECMessage CreateCMRPAYRECMessage(BusinessObjectFactory factory)
		{
			CMRPAYRECMessage message = factory.New<CMRPAYRECMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.PAYREC;
			return message;
		}

		public static CMRREFACCMessage CreateCMRREFACCMessage(BusinessObjectFactory factory)
		{
			CMRREFACCMessage message2 = factory.New<CMRREFACCMessage>();
			message2.EM_MessageText = CMRImportDeclarationTestData.REFACC;
			return message2;
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
	}
}
