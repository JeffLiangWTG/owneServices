using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageJobHeader))]
	public class CusTempStorageJobHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIJobNumber()
		{
			var job = Factory.New<CusTempStorageJobHeader>();
			job.SJH_JobReference = "123";
			AssertEquals("123", ((IJobNumber)job).JobNumber);
		}

		public void TestIRelatedJobMembers()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			header.SJH_JobReference = "KD3232";
			IRelatedJob relatedJob = header;
			AssertEquals("BusinessObjectPK", header.PK, relatedJob.BusinessObjectPK);
			AssertEquals("ControllerID", ControllerIDs.Customs.TemporaryStorage, relatedJob.ControllerID);
			AssertEquals("JobDescription", "Temporary Storage Job Header", relatedJob.JobDescription);
			AssertEquals("JobNumber", "KD3232", relatedJob.JobNumber);
			AssertEquals("JobStatus", ZString.Empty, relatedJob.JobStatus);
		}

		public void TestUniversalDataContext()
		{
			var job = Factory.New<CusTempStorageJobHeader>();
			IDataContextManager manager = null;
			AssertNoExceptionThrown(() => { manager = job.GetUniversalDataContextManager(); });
			AssertNotNull("CusTempStorageJobHeader should have [UniversalDataContext(DataContextType.TemporaryStorage)] attribute", manager);
			AssertEquals(DataContextType.TemporaryStorage, manager.DataContextType);
			AssertEquals("", manager.DataContextKey);
		}

		public void TestSJH_GB()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			AssertEquals(Env.CurrentBranchPK, header.SJH_GB);
		}

		[TestDate(2017, 11, 01)]
		public void TestCustomsOfficeAndDescription()
		{
			AssertCustomsOfficeAndDescription("CustomsOffice");
		}

		[TestDate(2017, 11, 01)]
		public void TestCustomsOfficeOfEntryIntoEUAndDescription()
		{
			AssertCustomsOffice("CustomsOfficeOfEntryIntoEU");
		}

		public void TestCorrectSetup()
		{
			var storageJobHeader = Factory.New<Integration.Customs.EU.ICusTempStorageJobHeader>();
			AssertType(typeof(CusTempStorageJobHeader), storageJobHeader);
		}

		void AssertCustomsOffice(ZString fieldSubName)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ABC", "Office Description",
				ZDateTime.Today, ZDateTime.Today);

			Factory.Save();

			var jobHeader = (CusTempStorageJobHeader)GetNewBusinessObject(Factory);
			jobHeader.SJH_PresentationDate = ZDate.Today;
			jobHeader["SJH_" + fieldSubName] = "ABC";
			AssertNotNull("Valid Customs Office", jobHeader[fieldSubName]);

			jobHeader["SJH_" + fieldSubName] = "DEF";
			AssertNull("Invalid Customs Office", jobHeader[fieldSubName]);

			jobHeader.SJH_PresentationDate = ZDate.Empty;
			jobHeader["SJH_" + fieldSubName] = "ABC";
			AssertNotNull("Valid Customs Office", jobHeader[fieldSubName]);
		}

		void AssertCustomsOfficeAndDescription(ZString fieldSubName)
		{
			AssertCustomsOffice(fieldSubName);

			var jobHeader = (CusTempStorageJobHeader)GetNewBusinessObject(Factory);
			jobHeader.SJH_PresentationDate = ZDate.Today;
			jobHeader["SJH_" + fieldSubName] = "ABC";
			AssertEquals("Valid Customs Office", "Office Description", jobHeader["SJH_" + fieldSubName + "Description"]);

			jobHeader["SJH_" + fieldSubName] = "DEF";
			AssertEquals("Invalid Customs Office", ZString.Empty, jobHeader["SJH_" + fieldSubName + "Description"]);

			jobHeader.SJH_PresentationDate = ZDate.Empty;
			jobHeader["SJH_" + fieldSubName] = "ABC";
			AssertEquals("Valid Customs Office", "Office Description", jobHeader["SJH_" + fieldSubName + "Description"]);
		}

		public void TestDelete()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			storageHeader.SJH_JobReference = "TEST";
			storageHeader.SJH_OH_Customer = org.PK;
			var storageDec = storageHeader.CusTempStorageDecs.AddNew();
			storageDec.STH_DeclarationType = "CUSPRL";
			var processTask = storageHeader.WorkflowItems.AddNew();
			Factory.Save();
			storageHeader.Delete();
			Factory.Save();
			Assert(storageDec.IsDeleted);
			Assert(processTask.IsDeleted);
		}

		public void TestGetEDocsProviderSupporter()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			AssertType<EDocsProviderSupporter>(header.GetEDocsProviderSupporter());
		}

		public void TestDocumentSupporter()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			var supporter = header.DocumentSupporter;
			CombineAssertions(() =>
			{
				AssertType<CusTempStorageJobHeaderDocumentSupporter>("Type", supporter);
				AssertSame("Cached", supporter, header.DocumentSupporter);
			});
		}

		public void TestDocManagerInfo()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			var manager = header.DocManagerInfo;
			CombineAssertions(() =>
			{
				AssertType<CusTempStorageJobHeaderDocManagerInfo>("Type", manager);
				AssertSame("Cached", manager, header.DocManagerInfo);
			});
		}

		#region Guarantee
		public void TestGuaranteeDescription()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			AssertEquals(ZString.Empty, header.SJH_GuaranteeDescription);
			var guaranteeHeader = SetupGuaranteeHeader();
			header.SJH_CPH_Guarantee = guaranteeHeader.PK;
			AssertEquals("ORG001 COD ALT", header.SJH_GuaranteeDescription);
		}

		public void TestSJH_GuaranteeNumber()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			AssertEquals(ZString.Empty, header.SJH_GuaranteeNumber);
			var guaranteeHeader = SetupGuaranteeHeader();
			header.SJH_CPH_Guarantee = guaranteeHeader.PK;
			AssertEquals("Number", header.SJH_GuaranteeNumber);
		}

		public void TestGuaranteeHeader()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			var guaranteeHeader = SetupGuaranteeHeader();
			AssertNull(header.GuaranteeHeader);
			header.SJH_CPH_Guarantee = guaranteeHeader.PK;
			AssertNotNull(header.GuaranteeHeader);
		}

		CusGuaranteeHeader SetupGuaranteeHeader()
		{
			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Type = "COD";
			guaranteeHeader.CPH_SubType = "ALT";
			guaranteeHeader.CPH_Number = "Number";
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ORG001";
			guaranteeHeader.CPH_OH_PermitHolder = orgHeader.PK;
			return guaranteeHeader;
		}
		#endregion

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		protected BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var customer = factory.NewWithValidTestData<OrgHeader>();
			customer.OH_Code = "C1";
			var presenter = factory.NewWithValidTestData<OrgAddress>();
			var representative = factory.NewWithValidTestData<OrgAddress>();

			var fromJob = factory.New<CusTempStorageJobHeader>();
			fromJob.SJH_GB = GlbBranch.CurrentBranch.PK;
			fromJob.SJH_JobReference = "From1";
			fromJob.SJH_OH_Customer = customer.PK;
			fromJob.SJH_OA_Presenter = presenter.PK;
			fromJob.SJH_OA_Representative = representative.PK;

			return fromJob;
		}

		public void TestGetCusTempStorageJobHeaderProcessTaskCollection()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			AssertType("WorkflowItems's type should be ProcessTaskCollection<CusTempStorageJobHeaderProcessTask, CusTempStorageJobHeader>", typeof(ProcessTaskCollection<CusTempStorageJobHeaderProcessTask, CusTempStorageJobHeader>), ((IWorkflowProvider)header).WorkflowItems);
		}

		public void TestIsUCC6()
		{
			var header = Factory.New<CusTempStorageJobHeader>();

			ConfigurationTestHelper.ClearCusTempStorageJobHeaderConfiguration(header);

			string countryOrGrouping = header.CountryCode;
			var configurationTypeName = nameof(CusTempStorageJobHeaderConfiguration);
			var configurationMock = new Mock<CusTempStorageJobHeaderConfiguration>();
			configurationMock.Protected().Setup<ZBool>("IsUCC6Core", ItExpr.IsAny<BusinessObject>()).Returns(true);
			countryOrGrouping = countryOrGrouping ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			header.Factory.ClearCachedValue<CusTempStorageJobHeaderConfiguration>($"{configurationTypeName}_{countryOrGrouping}");

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject()).Returns(configurationMock.Object);

			var configuration = new KeyObjectHandleDictionaryObject
			{
				{ countryOrGrouping, objectHandleMock.Object }
			};

			using (ObjectFactory.Substitute(configurationTypeName, configuration))
			{
				AssertEquals(true, header.IsUcc6);
			}
		}
	}

	[TestedType(typeof(ProcessTaskCollection<CusTempStorageJobHeaderProcessTask, CusTempStorageJobHeader>))]
	class CusTempStorageJobHeaderProcessTaskCollectionTest : Enterprise.MasterFiles.Business.Testing.ProcessTaskCollectionTest<ProcessTaskCollection<CusTempStorageJobHeaderProcessTask, CusTempStorageJobHeader>>
	{
		public void TestAddNewProcessTask()
		{
			ProcessTaskCollection<CusTempStorageJobHeaderProcessTask, CusTempStorageJobHeader> collection = GetCollectionToTestCore();
			AssertEquals(typeof(CusTempStorageJobHeaderProcessTask), collection.AddNew().GetType());
		}

		protected override ProcessTaskCollection<CusTempStorageJobHeaderProcessTask, CusTempStorageJobHeader> GetCollectionToTestCore()
			=> (ProcessTaskCollection<CusTempStorageJobHeaderProcessTask, CusTempStorageJobHeader>)Factory.NewWithValidTestData<CusTempStorageJobHeader>().WorkflowItems;
	}

	#region CusTempStorageJobHeaderWorkflowProviderTest

	[TestedType(typeof(CusTempStorageJobHeader))]
	sealed class CusTempStorageJobHeaderWorkflowProviderTest : Enterprise.MasterFiles.Business.Testing.WorkflowProviderTest<CusTempStorageJobHeader, ProcessTaskCollection<CusTempStorageJobHeaderProcessTask, CusTempStorageJobHeader>>
	{
		protected override ZString ExpectedWorkflowType => new CusTempStorageJobHeaderWorkflowDescriptor().Code;
	}

	#endregion
}
