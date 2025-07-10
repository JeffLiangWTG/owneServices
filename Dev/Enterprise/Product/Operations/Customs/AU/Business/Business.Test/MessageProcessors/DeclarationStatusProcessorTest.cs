using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DeclarationStatusProcessorTest : TestCaseWithFactory
	{
		public void TestDrawbackLodgedStatus()
		{
			DrawbackDeclarationStatusProcessor.SetDeclarationStatus("Lodged");
			AssertEquals(CustomsEntryStatus.Lodged.Code, DrawbackDeclaration.JE_EntryStatus);
		}

		#region Implementation
		DeclarationStatusProcessor drawbackDeclarationStatusProcessor;
		DeclarationStatusProcessor DrawbackDeclarationStatusProcessor
		{
			get
			{
				if (drawbackDeclarationStatusProcessor == null)
				{
					drawbackDeclarationStatusProcessor = new DeclarationStatusProcessor(DrawbackDeclaration);
				}
				return drawbackDeclarationStatusProcessor;
			}
		}

		JobDeclaration drawbackDeclaration;
		JobDeclaration DrawbackDeclaration
		{
			get
			{
				if (drawbackDeclaration == null)
				{
					drawbackDeclaration = Factory.New<JobDeclaration>();
					drawbackDeclaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
					drawbackDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				}
				return drawbackDeclaration;
			}
		}
		#endregion

	}
}
