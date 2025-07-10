using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.DIF.Business.Testing;
using Enterprise.Customs.Common.CA.DIF;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.DIF.Business.UniversalDataTransfer.Testing
{
	sealed class DIFDocumentEventParentFinderTest : TestCaseWithFactory
	{
		public void TestFindParentBO()
		{
			var jobDeclaration = new TestHelper(Factory).GetJobDeclaration();
			var requiredDocument = ((IDISHost)jobDeclaration).RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			addInfo.EX_Status = StatusList.Codes.AwaitingOriginal;
			addInfo.EX_ReferenceNumber = "REF001";
			addInfo.EX_GC_Company = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CADIFDocument, "REF001");
			var eventXml = new UniversalEvent()
			{
				DataContext = dataContext,
				EventType = "DDV",
				EventParameters = new EventParameters()
				{
					ReferenceNumber = "10207000013799",
					RequestNumber = "REF001",
				},
			};

			var finder = new DIFDocumentEventParentFinder(Factory, new DIFDocumentDataContextManager(), new XmlSessionTracker(new ServiceTaskLogForTesting()));

			var parentBOs = finder.GetLogParentsForEvent(eventXml);
			AssertNotNull(parentBOs);
			AssertEquals(1, parentBOs.Length);
			AssertEquals(addInfo, parentBOs[0]);
		}
	}
}
