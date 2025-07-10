using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class InwardsCreationPreCMRTest : TestCaseWithFactory
	{
		[TestDate(2005, 10, 11)]
		public void TestInwardsCreation()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				var creator = new MergedDeclarationCreator(Factory);
				SetupVirtualWarehouse(Factory.New<IWhsWarehouse>(), creator.Buyer.MainAddress);
				creator.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				SetCMRMode(creator.Declaration);
				creator.Declaration.Importer.CompanyData.OB_IMUsedBondedWhs = true;

				creator.Declaration.JE_MergeBy = "NON";
				creator.InvoiceLine1.AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
				creator.Declaration.DoMerge();

				var part = AUOrgSupplierPart.New(Factory);
				part.OP_PartNum = "~~~";
				part.RelatedOrganisations.AddOrganisationIfNotExist(creator.Buyer.PK, OrgPartRelation.RelationshipTypes.Owner);

				var classification = Factory.New<Classification>();
				classification.CC_LookupCode = "TestLookup";
				classification.CC_ClassificationType = Classification.ClassificationType.IMP;
				classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				classification.CC_TariffNum = "2605.00.00 08";
				var pivot = Factory.New<CusClassPartPivot>();
				pivot.CI_CC = classification.PK;
				pivot.CI_OP = part.PK;
				Factory.Save(); // so part gets in DB

				creator.InvoiceLine1.JI_PartNo = "~~~";
				creator.InvoiceLine1.JI_InvoiceQuantity = 10;
				creator.InvoiceLine1.JI_InvoiceUQ = "KG";
				creator.Declaration.WarehouseDocAddress.E2_OA_Address = creator.Buyer.MainAddress.PK;
				creator.Declaration.DoMerge();
				Factory.Save(); // message sent

				var batchProcessorFactory = new BusinessObjectFactory();
				var entryOnBatchProcessor = batchProcessorFactory.Load<CusEntryHeader>(creator.Entry1.PK);
				SetEntryStatusToPaid(entryOnBatchProcessor); // pretend batch processor has processed customs clear message for pay
				entryOnBatchProcessor.EntryNumber = "~WhsTestEntry";
				batchProcessorFactory.Save();

				var filter = new ZQuery(WhsDocketSchema.WD_ExternalReference, "~WhsTestEntry-INW W00000001");
				filter.AddToFilter(WhsDocketSchema.WD_OH_Client, creator.Buyer.PK);
				filter.AddToFilter(WhsDocketSchema.WD_DocketType, "INW");
				var whsReceive = (BusinessObject)Factory.LoadTop1<IWhsReceive>(filter);

				AssertNotNull("Receive docket should be created", whsReceive);
				AssertEquals("Should be finalised", false, ((ZDateTimeOffset)whsReceive[WhsDocketSchema.WD_FinalisedDate.Name]).IsEmpty);
			}
		}

		[TestDate(2005, 10, 11)]
		public void TestInwardsCreationWithMissingDataException()
		{
			var staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, GlbStaff.CurrentUser.GS_Code);
			staff.GS_EmailAddress = "Dummy@dummy.com";

			IEnvironment env = EnvProxy.Instance;
			var groupPK = env.Registry.PostMasterGroup;
			var group = Factory.Load<GlbGroup>(groupPK);
			group.Staff.Add(staff);
			Factory.Save();

			MergedDeclarationCreator creator = new MergedDeclarationCreator(Factory);

			creator.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			SetCMRMode(creator.Declaration);
			creator.Declaration.Importer.CompanyData.OB_IMUsedBondedWhs = true;

			creator.Declaration.JE_MergeBy = "NON";
			creator.InvoiceLine1.AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			creator.Declaration.DoMerge();

			AUOrgSupplierPart part = AUOrgSupplierPart.New(Factory);
			part.OP_PartNum = "~~~";
			part.RelatedOrganisations.AddOrganisationIfNotExist(creator.Buyer.PK, OrgPartRelation.RelationshipTypes.Owner);

			var classification = Factory.New<Classification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = Classification.ClassificationType.IMP;
			classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			classification.CC_TariffNum = "2605.00.00 08";
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_CC = classification.PK;
			pivot.CI_OP = part.PK;
			Factory.Save(); // so part gets in DB

			creator.InvoiceLine1.JI_PartNo = "~~~";
			creator.InvoiceLine1.JI_PartAttrib1 = "SOME ATTRIBUTES";//This will cause a missing data exception

			creator.InvoiceLine1.JI_InvoiceQuantity = 10;
			creator.InvoiceLine1.JI_InvoiceUQ = "KG";
			creator.Declaration.WarehouseDocAddress.E2_OA_Address = creator.Buyer.MainAddress.PK;
			creator.Declaration.DoMerge();
			Factory.Save(); // message sent

			BusinessObjectFactory batchProcessorFactory = new BusinessObjectFactory();
			CusEntryHeader entryOnBatchProcessor = batchProcessorFactory.Load<CusEntryHeader>(creator.Entry1.PK);
			SetEntryStatusToPaid(entryOnBatchProcessor); // pretend batch processor has processed customs clear message for pay
			entryOnBatchProcessor.EntryNumber = "~WhsTestEntry";

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			batchProcessorFactory.Save();

			ZQuery filter = new ZQuery(WhsDocketSchema.WD_ExternalReference, "~WhsTestEntry-INW W00000001");
			filter.AddToFilter(WhsDocketSchema.WD_OH_Client, creator.Buyer.PK);
			filter.AddToFilter(WhsDocketSchema.WD_DocketType, "INW");
			BusinessObject whsReceive = (BusinessObject)Factory.LoadTop1<IWhsReceive>(filter);

			AssertNull("Receive docket should be created", whsReceive);
			AssertNotNull("Email should have been sent", Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("warehouse stock could not be updated.")));
		}

		protected virtual void SetEntryStatusToPaid(CusEntryHeader entry)
		{
			entry.CH_Status = EntryHeaderStatus.PayMessageCleared.Code;
		}

		protected virtual void SetCMRMode(JobDeclaration dec)
		{
			dec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
		}

		void SetupVirtualWarehouse(IWhsWarehouse warehouse, OrgAddress address)
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var locationType = Factory.LoadTop1<IWhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "RNO"));
			warehouse.WW_WLT_DefaultLocationType = locationType.PK;
			warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			helper.SetUpBondedWarehouse(warehouse.PK, address.PK);
		}
	}
}
