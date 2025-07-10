using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CusGoodsCatalog))]
	class CusGoodsCatalogTest : BaseCusGoodsCatalogTest
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return Factory.NewWithValidTestData<CusGoodsCatalog>();
		}

		protected override ZString DefaultCustomsStatus => CustomsPostedStatusList.Codes.Active;

		public void TestIMessageAttachee()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			AssertEquals(GlbCompany.CurrentCompany.FirstActiveBranch.PK, ((IMessageAttachee)goodsCatalog).BranchPK);

			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();
			goodsCatalog.CGC_GC_Company = company.PK;
			AssertEquals(branch.PK, ((IMessageAttachee)goodsCatalog).BranchPK);
		}

		public void TestCreateForeignOperators()
		{
			var catalog = Factory.New<CusGoodsCatalog>();
			AssertType<ForeignOperatorCollection>(catalog.ForeignOperators);
		}

		public void TestComplementaryDescription()
		{
			var cusGoodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var goodsCatalogPK = cusGoodsCatalog.PK;
			AssertEquals("MaxLength of the ComplementaryDescription should be", 3700, cusGoodsCatalog.ComplementaryDescriptionInfo.MaxLength);

			cusGoodsCatalog.ComplementaryDescription = "Complementary Description";
			var note = FindNote(goodsCatalogPK, PredefinedNoteTypes.Instance.BRComplementaryDescription);
			AssertEquals("ComplementaryDescription should be", "Complementary Description", note.ST_NoteText);

			cusGoodsCatalog.ComplementaryDescription = ZString.Empty;
			Factory.Save();
			AssertEquals("ComplementaryDescription Note should be deleted", true, note.IsDeleted);

			cusGoodsCatalog.ComplementaryDescription = "Complementary Description";
			cusGoodsCatalog.Delete();
			Factory.Save();
			AssertEquals("ComplementaryDescription Note should be deleted", true, note.IsDeleted);

			StmNote FindNote(ZGuid parentID, PredefinedNoteType predefinedNoteType)
			{
				var query = new ZQuery(StmNoteSchema.ST_Description, predefinedNoteType.MultilingualDescription.GetUnresolvedString());
				query.AddToFilter(StmNoteSchema.ST_ParentID, parentID);
				return Factory.LoadTop1<StmNote>(query);
			}
		}

		public void TestLocalPartNumbers()
		{
			var cusGoodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();

			AssertEquals(0, cusGoodsCatalog.LocalPartNumbers.Count);

			var localPartNumber1 = cusGoodsCatalog.LocalPartNumbers.AddNew();
			localPartNumber1.CGI_Reference = "1";

			var localPartNumber2 = cusGoodsCatalog.LocalPartNumbers.AddNew();
			localPartNumber2.CGI_Reference = "2";

			var localPartNumber3 = cusGoodsCatalog.LocalPartNumbers.AddNew();
			localPartNumber3.CGI_Reference = "3";

			AssertEquals("Count should be", 3, cusGoodsCatalog.LocalPartNumbers.Count);

			AssertEquals("LocalPartNumbers[0].CGI_Reference should be", "1", cusGoodsCatalog.LocalPartNumbers[0].CGI_Reference);
			AssertEquals("LocalPartNumbers[1].CGI_Reference should be", "2", cusGoodsCatalog.LocalPartNumbers[1].CGI_Reference);
			AssertEquals("LocalPartNumbers[2].CGI_Reference should be", "3", cusGoodsCatalog.LocalPartNumbers[2].CGI_Reference);
		}

		public void TestDeleteLocalPartNumbersWhenDeletingBusinessObject()
		{
			var cusGoodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();

			var localPartNumber1 = cusGoodsCatalog.LocalPartNumbers.AddNew();
			localPartNumber1.CGI_Reference = "1";

			var localPartNumber2 = cusGoodsCatalog.LocalPartNumbers.AddNew();
			localPartNumber2.CGI_Reference = "2";

			var localPartNumber3 = cusGoodsCatalog.LocalPartNumbers.AddNew();
			localPartNumber3.CGI_Reference = "3";

			AssertEquals("Count should be", 3, cusGoodsCatalog.LocalPartNumbers.Count);

			cusGoodsCatalog.Delete();

			CombineAssertions(() =>
			{
				Assert("LocalPartNumbers 1", localPartNumber1.IsDeleted);
				Assert("LocalPartNumbers 2", localPartNumber2.IsDeleted);
				Assert("LocalPartNumbers 3", localPartNumber3.IsDeleted);
			});
		}

		public void TestCloneInternal()
		{
			ReferenceTestDataHelper.CreateNCMRefCusProfileQuestions(Factory);

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var catalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
				catalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
				catalog.CGC_Tariff = "87654321";
				catalog.ComplementaryDescription = "Complementary Description";
				var partNumber = catalog.LocalPartNumbers.AddNew();
				partNumber.CGI_Reference = "Test123";
				var foreignOperator = catalog.ForeignOperators.AddNew();
				foreignOperator.CountryCode = Core.Constants.CountryCodes.Australia;
				catalog.CGC_CustomsStatus = CustomsPostedStatusList.Codes.Accepted;

				AssertEquals("ForeignOperators", 1, catalog.ForeignOperators.Count);
				var attList = catalog.Attributes.GetFirstElementHaving("ATT_2557");
				attList.Content = "2";
				var attNumberReal = catalog.Attributes.GetFirstElementHaving("ATT_3892");
				attNumberReal.Content = "45678";
				var attString = catalog.Attributes.GetFirstElementHaving("ATT_4807");
				attString.Content = "123\r\n456";
				AssertEquals("Attributes", 3, catalog.Attributes.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "2", "45678", "123" }, catalog.Attributes.Select(s => s.CY_Data));

				var clonedusGoodsCatalog = catalog.Clone() as CusGoodsCatalog;

				CombineAssertions(() =>
				{
					AssertEquals("Complementary Description", clonedusGoodsCatalog.ComplementaryDescription);
					AssertEquals("Attributes count", 3, clonedusGoodsCatalog.Attributes.Count);
					AssertContainsExactElementsInAnyOrder("Attributes Content", new[] { "2", "45678", "123" }, clonedusGoodsCatalog.Attributes.Select(s => s.CY_Data));
					AssertContainsExactElementsInAnyOrder("Attributes CY_Code", new[] { "ATT_2557", "ATT_3892", "ATT_4807" }, clonedusGoodsCatalog.Attributes.Select(s => s.CY_Code));
					AssertEquals("ForeignOperators", 1, clonedusGoodsCatalog.ForeignOperators.Count);
					AssertEquals("ForeignOperators.CGI_CustomsStatus", CustomsPostedStatusList.Codes.Active, clonedusGoodsCatalog.ForeignOperators[0].CGI_CustomsStatus);
					AssertEquals("ForeignOperators.CGI_MessageStatus", ZString.Empty, clonedusGoodsCatalog.ForeignOperators[0].CGI_MessageStatus);
					AssertEquals("ForeignOperators", 1, clonedusGoodsCatalog.ForeignOperators.Count);
					AssertEquals("Should not copy this value", 0, clonedusGoodsCatalog.LocalPartNumbers.Count);
					AssertEquals("CGC_CustomsStatus shoulb be ACT", CustomsPostedStatusList.Codes.Active, clonedusGoodsCatalog.CGC_CustomsStatus);
				});
			}
		}

		public void TestTypeReadOnly()
		{
			var cusGoodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			Assert("CGC_Type should NOT be ReadOnly", !cusGoodsCatalog.CGC_TypeInfo.ReadOnly);

			cusGoodsCatalog.CGC_AuthorityIdentifier = "1";
			Assert("CGC_Type should be ReadOnly", cusGoodsCatalog.CGC_TypeInfo.ReadOnly);
		}

		public void TestConsigneeReadOnly()
		{
			var cusGoodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			Assert("CGC_OH_Owner should NOT be ReadOnly", !cusGoodsCatalog.CGC_OH_OwnerInfo.ReadOnly);

			cusGoodsCatalog.CGC_AuthorityIdentifier = "1";
			Assert("CGC_OH_Owner should be ReadOnly", cusGoodsCatalog.CGC_OH_OwnerInfo.ReadOnly);
		}

		public void TestTariffReadOnly()
		{
			var cusGoodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			Assert("CGC_Tariff should NOT be ReadOnly", !cusGoodsCatalog.CGC_TariffInfo.ReadOnly);

			cusGoodsCatalog.CGC_AuthorityIdentifier = "1";
			Assert("CGC_Tariff should be ReadOnly", cusGoodsCatalog.CGC_TariffInfo.ReadOnly);
		}

		public void TestMessages()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			var message = Factory.New<BREDIMessage>();
			message.EM_LinkTable = goodsCatalog.TableName;
			message.EM_LinkUniqueID = goodsCatalog.PK;

			Assert(goodsCatalog.Messages.IsManagedForDataRefresh);
			AssertContainsExactElementsInAnyOrder(new[] { message }, goodsCatalog.Messages);
		}

		public void TestCGC_CatalogCode()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			AssertEquals(true, goodsCatalog.CGC_CatalogCodeInfo.ReadOnly);
		}

		[TestDate(2024, 4, 4)]
		public void TestCGC_CatalogCode_IsGeneratedByNumberFountain()
		{
			var registry = new BRCustomsDataRegistry();
			var billItem = new UniqueNumberCustomisation();
			billItem.Elements.Cast<BillOfLadingNumberCustomisationElement>().ForEach(element => element.Include = true);
			using (registry.CatalogCodeCustomization.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, billItem))
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();

				CombineAssertions(() =>
				{
					var goodsCatalog = Factory.New<CusGoodsCatalog>();
					goodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
					goodsCatalog.CGC_Description = "TestDesc";
					goodsCatalog.CGC_OH_Owner = ZGuid.Invalid;
					AssertExceptionThrown<ZSaveException>(Factory.Save);
					AssertEquals(ZString.Empty, goodsCatalog.CGC_CatalogCode);

					goodsCatalog.CGC_OH_Owner = orgHeader.PK;
					Factory.Save();
					AssertEquals("BNEEDIEDI04DDAT4XQ2I00000002", goodsCatalog.CGC_CatalogCode);

					var goodsCatalog1 = Factory.New<CusGoodsCatalog>();
					goodsCatalog1.CGC_Type = GoodsCatalogTypeList.Codes.Import;
					goodsCatalog1.CGC_Description = "TestDesc";
					goodsCatalog1.CGC_OH_Owner = orgHeader.PK;
					Factory.Save();
					AssertEquals("BNEEDIEDI04DDAT4XQ2I00000003", goodsCatalog1.CGC_CatalogCode);

					var goodsCatalog2 = Factory.New<CusGoodsCatalog>();
					goodsCatalog2.CGC_Type = GoodsCatalogTypeList.Codes.Export;
					goodsCatalog2.CGC_Description = "TestDesc";
					goodsCatalog2.CGC_OH_Owner = orgHeader.PK;
					Factory.Save();
					AssertEquals("BNEEDIEDI04DDAT4XQ2E00000004", goodsCatalog2.CGC_CatalogCode);

					goodsCatalog2.CGC_OH_Owner = ZGuid.Invalid;
					AssertExceptionThrown<ZSaveException>(Factory.Save);
					AssertEquals("BNEEDIEDI04DDAT4XQ2E00000004", goodsCatalog2.CGC_CatalogCode);
				});
			}
		}

		public void TestLocalPartNumbersConcatenated()
		{
			var cusGoodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var partNumber1 = cusGoodsCatalog.LocalPartNumbers.AddNew();
			partNumber1.CGI_Reference = "Number 1";

			var partNumber2 = cusGoodsCatalog.LocalPartNumbers.AddNew();
			partNumber2.CGI_Reference = "Number 2";

			var partNumber3 = cusGoodsCatalog.LocalPartNumbers.AddNew();
			partNumber3.CGI_Reference = "Number 3";

			AssertEquals("LocalPartNumbersConcatenated should be", "Number 1;Number 2;Number 3", cusGoodsCatalog.LocalPartNumbersConcatenated);
		}

		public void TestIsImportExport()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			goodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			Assert(goodsCatalog.IsImport);
			Assert(!goodsCatalog.IsExport);

			goodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Export;
			Assert(!goodsCatalog.IsImport);
			Assert(goodsCatalog.IsExport);
		}

		public void TestSupportsWorkflow()
		{
			Assert(Factory.New<CusGoodsCatalog>().SupportsWorkflow);
		}

		public void TestAddCustomsUpdateLog()
		{
			var date = ZDateTimeOffset.Now;
			var catalog = Factory.New<CusGoodsCatalog>();

			AssertCustomsUpdateLog(catalog.AddCustomsUpdateLog(date, description: "Produtos desativados no catálogo da empresa 99256934000156"), "|DES=Produtos desativados no catálogo da empresa 99256934000156");
			AssertCustomsUpdateLog(catalog.AddCustomsUpdateLog(date, product: "123456"), "|PRD=123456");
			AssertCustomsUpdateLog(catalog.AddCustomsUpdateLog(date, reason: "TEST_MESSAGE"), "|RES=TEST_MESSAGE");
			AssertCustomsUpdateLog(catalog.AddCustomsUpdateLog(date, description: "TITLE", product: "789", reason: "TEST_MESSAGE"), "|DES=TITLE|PRD=789|RES=TEST_MESSAGE");
			AssertCustomsUpdateLog(catalog.AddCustomsUpdateLog(date, "TITLE", "123456", "TEST_MESSAGE"), "|DES=TITLE|PRD=123456|RES=TEST_MESSAGE");

			void AssertCustomsUpdateLog(StmALog log, ZString expectedEventReference) => CombineAssertions(() =>
			{
				AssertEquals("SL_Parent", catalog.PK, log.SL_Parent);
				AssertEquals("SL_SE_NKEvent", Events.CustomsUpdate.Code, log.SL_SE_NKEvent);
				AssertEquals("SL_Reference", expectedEventReference, log.SL_Reference);
				AssertEquals("SL_EventTime", date.ToDateTime(), log.SL_EventTime.ToDateTime());
			});
		}

		public void TestCGC_MessageStatusReadOnly()
		{
			var cusGoodsCatalog = Factory.New<CusGoodsCatalog>();
			Assert("CGC_MessageStatus should be ReadOnly", cusGoodsCatalog.CGC_MessageStatusInfo.ReadOnly);
		}

		public void TestAttributes()
		{
			ReferenceTestDataHelper.CreateNCMRefCusProfileQuestions(Factory);

			var catalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog.CGC_Type = GoodsCatalogTypeList.Codes.Export;
			catalog.CGC_Tariff = "87654321";

			AssertEquals("Attributes Count", 4, catalog.Attributes.Count);
			Factory.Save();

			var newGoodsCatalog = new BusinessObjectFactory().Load<CusGoodsCatalog>(catalog.PK);
			AssertEquals("Attributes Count", 4, newGoodsCatalog.Attributes.Count);
		}

		public void TestDeleteAttributesWhenDeletingBusinessObject()
		{
			var cusGoodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();

			var attribute = cusGoodsCatalog.Attributes.AddNew();
			attribute.CY_Code = "1";
			var attribute2 = cusGoodsCatalog.Attributes.AddNew();
			attribute2.CY_Code = "1";
			var attribute3 = cusGoodsCatalog.Attributes.AddNew();
			attribute3.CY_Code = "1";

			AssertEquals("Count should be", 3, cusGoodsCatalog.Attributes.Count);

			cusGoodsCatalog.Delete();

			CombineAssertions(() =>
			{
				Assert("attribute 1", attribute.IsDeleted);
				Assert("attribute 2", attribute2.IsDeleted);
				Assert("attribute 3", attribute3.IsDeleted);
			});
		}

		public override void TestFormatTariffForSaving()
		{
			var cusGoodsCatalog = Factory.New<CusGoodsCatalog>();
			cusGoodsCatalog.CGC_Tariff = "awd/-1234567800-1234567800-1234567800-123456./dawd";
			AssertEquals("CGC_Tariff", "1234567800", cusGoodsCatalog.CGC_Tariff);
		}

		public void TestHasAuthorityIdentifierValue()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			Assert("HasAuthorityIdentifierValue should be FALSE", !goodsCatalog.HasAuthorityIdentifier);

			goodsCatalog.CGC_AuthorityIdentifier = "1";
			Assert("HasAuthorityIdentifierValue should be TRUE", goodsCatalog.HasAuthorityIdentifier);
		}

		public void TestMessageStatusProperties()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.NotSent;
			CombineAssertions($"When message is {nameof(BRMessageStatusList.Codes.NotSent)}", () =>
			{
				Assert("IsMessageSent should be FALSE", !goodsCatalog.IsMessageSent);
				Assert("IsMessageRejected should be FALSE", !goodsCatalog.IsMessageRejected);
				Assert("IsMessageAwaitingResponse should be FALSE", !goodsCatalog.IsMessageAwaitingResponse);
				Assert("IsMessageAccepted should be FALSE", !goodsCatalog.IsMessageAccepted);
			});

			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			CombineAssertions($"When message is {nameof(BRMessageStatusList.Codes.AwaitingResponse)}", () =>
			{
				Assert("IsMessageSent should be TRUE", goodsCatalog.IsMessageSent);
				Assert("IsMessageRejected should be FALSE", !goodsCatalog.IsMessageRejected);
				Assert("IsMessageAwaitingResponse should be TRUE", goodsCatalog.IsMessageAwaitingResponse);
				Assert("IsMessageAccepted should be FALSE", !goodsCatalog.IsMessageAccepted);
			});

			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.Rejected;
			CombineAssertions($"When message is {nameof(BRMessageStatusList.Codes.Rejected)}", () =>
			{
				Assert("IsMessageSent should be TRUE", goodsCatalog.IsMessageSent);
				Assert("IsMessageRejected should be TRUE", goodsCatalog.IsMessageRejected);
				Assert("IsMessageAwaitingResponse should be FALSE", !goodsCatalog.IsMessageAwaitingResponse);
				Assert("IsMessageAccepted should be FALSE", !goodsCatalog.IsMessageAccepted);
			});

			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.Accepted;
			CombineAssertions($"When message is {nameof(BRMessageStatusList.Codes.Accepted)}", () =>
			{
				Assert("IsMessageSent should be TRUE", goodsCatalog.IsMessageSent);
				Assert("IsMessageRejected should be FALSE", !goodsCatalog.IsMessageRejected);
				Assert("IsMessageAwaitingResponse should be FALSE", !goodsCatalog.IsMessageAwaitingResponse);
				Assert("IsMessageAccepted should be TRUE", goodsCatalog.IsMessageAccepted);
			});
		}

		public void TestResetStatuses()
		{
			CombineAssertions(() =>
			{
				AssertResetStatus(nameof(CusGoodsCatalog.CGC_CustomsStatus), CustomsPostedStatusList.Codes.Active, CustomsPostedStatusList.Codes.Accepted);
				AssertResetStatus(nameof(CusGoodsCatalog.CGC_MessageStatus), ZString.Empty, BRMessageStatusList.Codes.Accepted);
			});

			void AssertResetStatus(string propertyName, string resetStatus, string otherStatus)
			{
				var cusGoodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
				cusGoodsCatalog[propertyName] = otherStatus;
				cusGoodsCatalog.ResetStatuses();
				AssertEquals($"{propertyName} should NOT be reset when not in database", otherStatus, cusGoodsCatalog[propertyName]);
				Factory.Save();

				cusGoodsCatalog.SuspendResetStatusesUntilSaved();
				cusGoodsCatalog.ResetStatuses();
				Assert("ResetStatuses Suspended", cusGoodsCatalog.IsResetStatusesSuspended);
				AssertEquals($"{propertyName} should NOT be reset when when suspended", otherStatus, cusGoodsCatalog[propertyName]);

				Factory.Save();
				Assert("ResetStatuses NOT Suspended", !cusGoodsCatalog.IsResetStatusesSuspended);

				cusGoodsCatalog.ResetStatuses();
				AssertEquals($"{propertyName} should be set to reset when not suspended", resetStatus, cusGoodsCatalog[propertyName]);
				Factory.Save();

				cusGoodsCatalog[propertyName] = otherStatus;
				cusGoodsCatalog.ResetStatuses();
				AssertEquals($"{propertyName} should NOT be reset when HasChanges", otherStatus, cusGoodsCatalog[propertyName]);
			}
		}

		public void TestUpdateCustomsStatusOnSaving()
		{
			var brazil = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Brazil);
			var importer = new MasterFilesTestHelper(Factory).CreateOrganisation("TEST ORG 2", "CATOR", "STREET 2", "CITY", "0987654321");
			importer.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "33.775.353/0001-12", brazil);
			importer.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "33775353", brazil);
			Factory.Save();

			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog.CGC_AuthorityIdentifier = "1";
			goodsCatalog.CGC_OH_Owner = importer.PK;
			goodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			Factory.Save();

			goodsCatalog.CGC_CustomsStatus = CustomsPostedStatusList.Codes.Accepted;
			Factory.Save();
			AssertEquals("Should not update CGC_CustomsStatus to UPD when CGC_CustomsStatus changed", CustomsPostedStatusList.Codes.Accepted, goodsCatalog.CGC_CustomsStatus);

			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			Factory.Save();
			AssertEquals("Should not update CGC_CustomsStatus to UPD when changes not affect message", CustomsPostedStatusList.Codes.Accepted, goodsCatalog.CGC_CustomsStatus);

			goodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Export;
			Factory.Save();
			AssertEquals("Should update CGC_CustomsStatus to UPD when any changes affect message", CustomsPostedStatusList.Codes.UpdatePending, goodsCatalog.CGC_CustomsStatus);

			goodsCatalog.CGC_CustomsStatus = CustomsPostedStatusList.Codes.Accepted;
			Factory.Save();

			goodsCatalog.SuspendUpdateCustomStatusOnSavingUntilSaved();
			goodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			Assert("UpdateCustomStatusOnSaving Suspended", goodsCatalog.IsUpdateCustomStatusOnSavingSuspended);
			Factory.Save();
			Assert("UpdateCustomStatusOnSaving not Suspended", !goodsCatalog.IsUpdateCustomStatusOnSavingSuspended);
			AssertEquals("Should not update CGC_CustomsStatus when suspended", CustomsPostedStatusList.Codes.Accepted, goodsCatalog.CGC_CustomsStatus);
		}

		public void TestSetDefaultCGC_CustomsStatus()
		{
			var catalog = Factory.New<CusGoodsCatalog>();
			AssertEquals(CustomsPostedStatusList.Codes.Active, catalog.CGC_CustomsStatus);
		}

		public void TestIMessageManageableBizObj()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			var messageSendingObject = new GoodsCatalogMessageSendingObject(goodsCatalog);

			IMessageManageableBizObj bizObj = goodsCatalog;
			AssertType<GoodsCatalogMultiMessageManager>(bizObj.GetMessageManagerForAmendmentDetection());
			AssertEquals(ContinueWithDetection.Yes, bizObj.ProcessBeforeDetectingAmendmentAndContinue());
		}

		public void TestCanDelete()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();

			goodsCatalog.CGC_AuthorityIdentifier = ZString.Empty;
			AssertEquals("Can be deleted if Empty", true, goodsCatalog.CanDelete);
			AssertNotEquals("Should not have this Reason ", "This Catalog cannot be deleted due to the following reason(s): It has an Authority Identifier.", goodsCatalog.ReasonForNotAbleToDelete);

			goodsCatalog.CGC_AuthorityIdentifier = "1234";
			AssertEquals("Cannot be deleted if Empty", false, goodsCatalog.CanDelete);
			AssertEquals("Should have the reason", "This Catalog cannot be deleted due to the following reason(s): It has an Authority Identifier.", goodsCatalog.ReasonForNotAbleToDelete);
		}

		public void TestIBackDoorSavingSupportableBizObj()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			var messageSendingObject = new GoodsCatalogMessageSendingObject(goodsCatalog);

			IBackDoorSavingSupportableBizObj bizObj = goodsCatalog;
			AssertType<AmendmentWithdrawalReason>(bizObj.GetAmendmentWithdrawalReason());
			Assert(bizObj.SupportBackDoorForSavingWhenAmendmentDetected);
		}

		public override void TestGetProductionInfoType()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			AssertEquals(typeof(LocalPartNumber), goodsCatalog.GetProductionInfoType(CusGoodsCatalogProductionInfoTypeList.Codes.LPN));
			AssertEquals(typeof(ForeignOperator), goodsCatalog.GetProductionInfoType(CusGoodsCatalogProductionInfoTypeList.Codes.FOR));
		}

		public void TestGetAttributes()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			CombineAssertions(() =>
			{
				AssertSame("When Type is ATT, should return Attributes", goodsCatalog.Attributes, goodsCatalog.GetAttributes(CusCodeDataTypeList.Codes.Attribute));
				AssertNull("When Type is not ATT, should return null", goodsCatalog.GetAttributes(CusCodeDataTypeList.Codes.NVE));
			});
		}
	}

	[TestedType(typeof(CusGoodsCatalog.Loader))]
	class CusGoosCatalogLoaderTest : LoaderTestCase
	{
		public void TestGetGoodsCatalogByAuthorityIdentifierAndOwner()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			var owner2 = Factory.NewWithValidTestData<OrgHeader>();
			var owner3 = Factory.NewWithValidTestData<OrgHeader>();
			var owner4 = Factory.NewWithValidTestData<OrgHeader>();

			var goodsCatalog1Old = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog1Old.CGC_AuthorityIdentifier = "1";
			goodsCatalog1Old.CGC_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			goodsCatalog1Old.CGC_OH_Owner = owner.PK;

			var goodsCatalog1New = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog1New.CGC_AuthorityIdentifier = "1";
			goodsCatalog1New.CGC_SystemCreateTimeUtc = ZDateTime.UtcNow;
			goodsCatalog1New.CGC_OH_Owner = owner.PK;

			var goodsCatalog1New2 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog1New2.CGC_AuthorityIdentifier = "1";
			goodsCatalog1New2.CGC_SystemCreateTimeUtc = ZDateTime.UtcNow;
			goodsCatalog1New2.CGC_OH_Owner = owner2.PK;

			var goodsCatalog2 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog2.CGC_AuthorityIdentifier = "2";
			goodsCatalog2.CGC_OH_Owner = owner3.PK;

			Factory.Save();
			var loader = GetNewLoaderToTest() as CusGoodsCatalog.Loader;
			CombineAssertions(() =>
			{
				AssertEquals("Loader should be equal to GoodsCatalog1New", goodsCatalog1New, loader.GetGoodsCatalogByAuthorityIdentifierAndOwner("1", owner.PK));
				AssertEquals("Loader should be equal to GoodsCatalog2New", goodsCatalog1New2, loader.GetGoodsCatalogByAuthorityIdentifierAndOwner("1", owner2.PK));
				AssertEquals("Loader should be equal to GoodsCatalog2", goodsCatalog2, loader.GetGoodsCatalogByAuthorityIdentifierAndOwner("2", owner3.PK));
				AssertNull("Loader should be NULL when Empty for owner4", loader.GetGoodsCatalogByAuthorityIdentifierAndOwner("2", owner4.PK));
				AssertNull("Loader should be NULL when Empty", loader.GetGoodsCatalogByAuthorityIdentifierAndOwner(string.Empty,ZGuid.Empty));
				AssertNull("Loader should be NULL when W", loader.GetGoodsCatalogByAuthorityIdentifierAndOwner("W", ZGuid.Empty));
			});
		}

		public void TestGetGoodsCatalogByOwnerRootCnpjAndCatalogCode()
		{
			var brazil = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Brazil);
			var importer1 = new MasterFilesTestHelper(Factory).CreateOrganisation("TEST ORG", "CATOR", "STREET 1", "CITY", "0987654321");
			importer1.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "22.955.307/0001-89", brazil);
			importer1.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "22955307", brazil);

			var importer2 = new MasterFilesTestHelper(Factory).CreateOrganisation("TEST ORG 2", "CATOR", "STREET 2", "CITY", "0987654321");
			importer2.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "33.775.353/0001-12", brazil);
			importer2.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "33775353", brazil);
			Factory.Save();

			var goodsCatalog1Old = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog1Old.CGC_AuthorityIdentifier = "1";
			goodsCatalog1Old.CGC_OH_Owner = importer1.PK;
			goodsCatalog1Old.CGC_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			var goodsCatalog1New = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog1New.CGC_AuthorityIdentifier = "1";
			goodsCatalog1New.CGC_OH_Owner = importer1.PK;
			goodsCatalog1New.CGC_SystemCreateTimeUtc = ZDateTime.UtcNow;

			var goodsCatalog2 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog2.CGC_AuthorityIdentifier = "2";
			goodsCatalog2.CGC_OH_Owner = importer1.PK;

			var goodsCatalog3 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog3.CGC_AuthorityIdentifier = "2";
			goodsCatalog3.CGC_OH_Owner = importer2.PK;
			Factory.Save();

			var loader = GetNewLoaderToTest() as CusGoodsCatalog.Loader;
			CombineAssertions(() =>
			{
				AssertEquals("Loader should be equal to GoodsCatalog1New", goodsCatalog1New, loader.GetGoodsCatalogByAuthorityIdentifierAndOwner("1", "22955307"));
				AssertEquals("Loader should be equal to GoodsCatalog2", goodsCatalog2, loader.GetGoodsCatalogByAuthorityIdentifierAndOwner("2", "22955307"));
				AssertEquals("Loader should be equal to GoodsCatalog3", goodsCatalog3, loader.GetGoodsCatalogByAuthorityIdentifierAndOwner("2", "33775353"));
				AssertNull("Loader should be NULL when Code = 3", loader.GetGoodsCatalogByAuthorityIdentifierAndOwner("3", "33775353"));
				AssertNull("Loader should be NULL when Empty", loader.GetGoodsCatalogByAuthorityIdentifierAndOwner(string.Empty, string.Empty));
				AssertNull("Loader should be NULL when RootCNPJ = 12345678", loader.GetGoodsCatalogByAuthorityIdentifierAndOwner("2", "12345678"));
			});
		}

		public void TestIsInAStatusAmendmentSendable()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();

			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.NotSent;
			AssertEquals("IsInAStatusAmendmentSendable should be false", false, goodsCatalog.IsInAStatusAmendmentSendable);

			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			AssertEquals("IsInAStatusAmendmentSendable should be true", true, goodsCatalog.IsInAStatusAmendmentSendable);

			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.Rejected;
			AssertEquals("IsInAStatusAmendmentSendable should be true", false, goodsCatalog.IsInAStatusAmendmentSendable);

			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.Accepted;
			goodsCatalog.CGC_AuthorityIdentifier = "1";
			AssertEquals("IsInAStatusAmendmentSendable should be false", true, goodsCatalog.IsInAStatusAmendmentSendable);
		}

		public void TestCGC_OH_Owner()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			var foreignOperator = goodsCatalog.ForeignOperators.AddNew();
			foreignOperator.MarkLightValidationAsValidForTesting();

			Assert(foreignOperator.LightValidationIsValid);

			var owner = Factory.New<OrgHeader>();
			goodsCatalog.CGC_OH_Owner = owner.PK;

			Assert(!foreignOperator.LightValidationIsValid);
		}

		public void TestGetGoodsCatalogsByLocalPartNumber()
		{
			var brazil = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Brazil);
			var importer1 = new MasterFilesTestHelper(Factory).CreateOrganisation("TEST ORG", "CATOR", "STREET 1", "CITY", "0987654321");
			var importer2 = new MasterFilesTestHelper(Factory).CreateOrganisation("TEST ORG 2", "CATOR", "STREET 2", "CITY", "0987654321");

			var goodsCatalog1 = CreateGoodsCatalog(GoodsCatalogTypeList.Codes.Import, importer1, "TEST1");
			var goodsCatalog2 = CreateGoodsCatalog(GoodsCatalogTypeList.Codes.Import, importer2, "TEST1");
			var goodsCatalog3 = CreateGoodsCatalog(GoodsCatalogTypeList.Codes.Export, importer1, "TEST1");
			var goodsCatalog4 = CreateGoodsCatalog(GoodsCatalogTypeList.Codes.Import, importer2, "TEST2");
			var goodsCatalog5 = CreateGoodsCatalog(GoodsCatalogTypeList.Codes.Export, importer2, "TEST1");
			Factory.Save();

			var loader = GetNewLoaderToTest() as CusGoodsCatalog.Loader;

			CombineAssertions(() =>
			{
				AssertNull("Loader should return any CusGoodsCatalog when Local Part Number is empty", loader.GetUniqueGoodsCatalogsByLocalPartNumber(string.Empty, importer1.PK, GoodsCatalogTypeList.Codes.Import));
				AssertNull("Loader should return any CusGoodsCatalog when Importer is empty", loader.GetUniqueGoodsCatalogsByLocalPartNumber("TEST1", ZGuid.Empty, GoodsCatalogTypeList.Codes.Import));

				AssertContainsExactElementsInAnyOrder("Should return only goodsCatalog1", new[] { goodsCatalog1 }, loader.GetUniqueGoodsCatalogsByLocalPartNumber("TEST1", importer1.PK, GoodsCatalogTypeList.Codes.Import));
				AssertCollectionNotContains("Should return only goodsCatalog1", goodsCatalog2, loader.GetUniqueGoodsCatalogsByLocalPartNumber("TEST1", importer1.PK, GoodsCatalogTypeList.Codes.Import));
				AssertCollectionNotContains("Should return only goodsCatalog1, goodsCatalog3 is Export", goodsCatalog3, loader.GetUniqueGoodsCatalogsByLocalPartNumber("TEST1", importer1.PK, GoodsCatalogTypeList.Codes.Import));

				AssertContainsExactElementsInAnyOrder("Should return only goodsCatalog2", new[] { goodsCatalog2 }, loader.GetUniqueGoodsCatalogsByLocalPartNumber("TEST1", importer2.PK, GoodsCatalogTypeList.Codes.Import));
				AssertCollectionNotContains("Should return only goodsCatalog2", goodsCatalog1, loader.GetUniqueGoodsCatalogsByLocalPartNumber("TEST1", importer2.PK, GoodsCatalogTypeList.Codes.Import));
				AssertCollectionNotContains("Should return only goodsCatalog2, goodsCatalog5 is Export", goodsCatalog5, loader.GetUniqueGoodsCatalogsByLocalPartNumber("TEST1", importer1.PK, GoodsCatalogTypeList.Codes.Import));

				AssertContainsExactElementsInAnyOrder("Should return only goodsCatalog4", new[] { goodsCatalog4 }, loader.GetUniqueGoodsCatalogsByLocalPartNumber("TEST2", importer2.PK, GoodsCatalogTypeList.Codes.Import));
				AssertCollectionNotContains("Should return only goodsCatalog4", goodsCatalog2, loader.GetUniqueGoodsCatalogsByLocalPartNumber("TEST2", importer2.PK, GoodsCatalogTypeList.Codes.Import));
				AssertCollectionNotContains("Should return only goodsCatalog4, goodsCatalog5 is Export", goodsCatalog5, loader.GetUniqueGoodsCatalogsByLocalPartNumber("TEST2", importer1.PK, GoodsCatalogTypeList.Codes.Import));
			});

			CusGoodsCatalog CreateGoodsCatalog(string type, OrgHeader importer, string localNumber)
			{
				var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
				goodsCatalog.CGC_Type = type;
				goodsCatalog.CGC_OH_Owner = importer.PK;
				var localPartNumber = goodsCatalog.LocalPartNumbers.AddNew();
				localPartNumber.CGI_Reference = localNumber;
				return goodsCatalog;
			}
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new CusGoodsCatalog.Loader(Factory);
	}
}
