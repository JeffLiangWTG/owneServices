using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business.Testing
{
	class CusReconDeclarationFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForViewCore()
		{
			var factory = new BusinessObjectFactory();
			var orgHeader = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "KR1", "TestCompany");
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			factory.Save();

			var reconDeclaration = factory.NewWithValidTestData<CusReconDeclaration>();
			reconDeclaration.CusReconEntryLines.AddNew();
			var reconEntry = reconDeclaration.CusReconEntries[0];

			reconEntry.CRE_EntryType = "AA";
			reconEntry.CRE_CH_OriginalEntry = entry.PK;
			reconEntry.CRE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
			var reconEntryLine = reconDeclaration.CusReconEntryLines[0];
			reconEntryLine.CRL_OriginalEntryLineNumber = 1;

			var cusReconCustomsCharge = reconEntryLine.CusReconCharges.AddNew();
			cusReconCustomsCharge.CRC_Amount = 1;
			cusReconCustomsCharge.CRC_ChargeType = "AA";

			var message = reconDeclaration.Messages.AddNew();
			message.EM_MessageType = ElectronicDocumentTypeList.Codes._5UL;
			message.EM_SystemCreateTimeUtc = new ZDateTime(2024, 10, 30, 23, 0, 0);
			message.EM_LinkUniqueID = reconDeclaration.PK;
			message.EM_LinkTable = "CusReconDeclaration";

			var entryNum = factory.New<CusEntryNumber>();
			entryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNum.CE_EntryNum = "6N00221000001X";
			entryNum.CE_IssueDate = new ZDate(2024, 10, 30);
			entryNum.CE_ParentID = reconDeclaration.PK;
			entryNum.CE_ParentTable = CusReconDeclaration.Schema.TableName;

			factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var savedReconDeclaration = anotherFactory.Load<CusReconDeclaration>(reconDeclaration.PK);

			savedReconDeclaration.FetchStrategy.FetchForView(new[] { new TableColumn("CusReconDeclaration", "CRD_PK") });
			savedReconDeclaration.FetchStrategy.FetchForView(new[] { new TableColumn("CusReconDeclaration", "CRD_OA_DeclarantAddress") });
			savedReconDeclaration.FetchStrategy.FetchForView(new[] { new TableColumn("CusReconEntry", "CRE_CRD") });
			savedReconDeclaration.FetchStrategy.FetchForView(new[] { new TableColumn("CusEntryNum", "CE_ParentID") });
			savedReconDeclaration.FetchStrategy.FetchForView(new[] { new TableColumn("CusReconEntryLine", "CRL_CRE") });
			savedReconDeclaration.FetchStrategy.FetchForView(new[] { new TableColumn("CusReconCustomsCharge", "CRC_CRL_Line") });
			savedReconDeclaration.FetchStrategy.FetchForView(new[] { new TableColumn("EDIMessage", "EM_LinkUniqueID") });
			savedReconDeclaration.FetchStrategy.FetchForView(new[] { new TableColumn("OrgAddress", "OA_PK") });

			var expectedConditionDbHits = new Dictionary<string, int>()
			{
				{ CusReconDeclarationSchema.Constants.TableName, 1 },
				{ CusReconEntrySchema.Constants.TableName, 1 },
				{ CusReconEntryLineSchema.Constants.TableName, 1 },
				{ CusEntryNumSchema.Constants.TableName, 0 },
				{ CusReconCustomsChargeSchema.Constants.TableName, 0 },
				{ EDIMessageSchema.Constants.TableName, 0 },
				{ OrgAddressSchema.Constants.TableName, 0 },
			};

			AssertDbHits(expectedConditionDbHits, anotherFactory);
		}
	}
}
