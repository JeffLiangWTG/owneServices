using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	public class AsycudaManifestHeaderFetchStrategyTest : ManifestBase.Testing.AsycudaManifestHeaderFetchStrategyTest
	{
		public void TestFetchForEnableBillsLock()
		{
			FetchStrategyTestHelper.AssertFetchHints<AsycudaManifestHeader>(
				GetType(),
				Factory,
				TestFetchForEnableBillsLockTestCases,
				businessObject => businessObject.FetchStrategy.FetchForEnableBillsLock(),
				businessObject => businessObject.EnableBillsLock(true),
				tablesToCollectQueriesFor: GetTablesToCollectQueriesFor()
			);
		}

		public void TestFetchForUXMLExport()
		{
			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				FetchStrategyTestHelper.AssertFetchHints<AsycudaManifestHeader>(
					GetType(),
					Factory,
					TestFetchForUXMLExportTestCases,
					businessObject => { },
					businessObject => MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, businessObject),
					tablesToCollectQueriesFor: GetTablesToCollectQueriesFor());
			}
		}

		protected virtual Dictionary<string, int> FetchForUXMLExportStrategyExpectedHitCounts
		{
			get
			{
				return new Dictionary<string, int>()
				{
					{ CusEntryNumSchema.Constants.TableName, 1 }, //Loads from Header
					{ OrgAddressSchema.Constants.TableName, 6 }, //Loads from Header and Bill
					{ ProcessTasksSchema.Constants.TableName, 2 }, //Loads from Header and Bill
					{ RefUNLOCOSchema.Constants.TableName, 2 }, //Loads from Header and Bill
					{ UNDGSubstanceSchema.Constants.TableName, 4 },
					{ CusRefTradeGroupViewSchema.Constants.TableName,2 },
					{ OrgAddressCapabilitySchema.Constants.TableName, 3 },
					{ OrgCusCodeSchema.Constants.TableName, 3 },
					{ OrgHeaderSchema.Constants.TableName, 3 },
					{ OrgTranslatedAddressSchema.Constants.TableName, 3 },
				};
			}
		}

		protected override Dictionary<string, int> FetchForDeleteExecuteActionExpectedHitCounts
		{
			get
			{
				var result = base.FetchForDeleteExecuteActionExpectedHitCounts;
				result[ProcessTasksSchema.Constants.TableName] = 2;
				result.Add(ProcessTaskIterationLinkSchema.Constants.TableName, 4); //Workflow subcollections parent is ProcessTask
				result.Add(ProcessTaskIterationLinkPivotSchema.Constants.TableName, 2); //Workflow subcollections parent is ProcessTask
				result.Add(CusCodeDataSchema.Constants.TableName, 4);
				result.Add(CusEntryNumSchema.Constants.TableName, 1);
				result[StmDocDataOverrideSchema.Constants.TableName] = 68;
				result.Add(StmNoteSchema.Constants.TableName, 21); // ZLogsOrNotes line 173 always loads as BusinessObjectCollection for CusPerson, Header, Bill, etc to delete.
				result[ProcessHeaderSchema.Constants.TableName] = 2;
				result[StmUniversalCopySchema.Constants.TableName] = 45;
				result.Add(TagLinkSchema.Constants.TableName, 4); //Workflow subcollections parent is TagLinkRule
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForDeleteUnconsumedExpectedHitCounts
		{
			get
			{
				var result = base.FetchForDeleteUnconsumedExpectedHitCounts;
				result[AsycudaBillScreeningSchema.Constants.TableName] = 0;
				result[AsycudaTaxSchema.Constants.TableName] = 0;
				result.Add(EDIMessageAttachSchema.Constants.TableName, 1);
				result.Add(GenCustomAddOnRuleAckSchema.Constants.TableName, 1);
				result.Add(GenCustomAddOnValueSchema.Constants.TableName, 1);
				result.Add(OrgAddressCapabilitySchema.Constants.TableName, 1);
				result.Add(OrgCompanyDataSchema.Constants.TableName, 1);
				result.Add(ProcessJobTriggerLinkSchema.Constants.TableName, 1);
				result.Add(GenAddOnColumnSchema.Constants.TableName, 1);
				result.Add(JobConsolTransportSchema.Constants.TableName, 1);
				result.Add(StmALogSchema.Constants.TableName, 1);
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForLoadChildEditableObjectsExecuteActionExpectedHitCounts
		{
			get
			{
				var result = base.FetchForLoadChildEditableObjectsExecuteActionExpectedHitCounts;
				result.Add(CusEntryNumSchema.Constants.TableName, 1);
				result[AsycudaPackedItemSchema.Constants.TableName] = 9;
				result[ProcessHeaderLinkSchema.Constants.TableName] = 2;
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForLoadChildEditableObjectsUnconsumedExpectedHitCounts
		{
			get
			{
				var result = base.FetchForLoadChildEditableObjectsUnconsumedExpectedHitCounts;
				result[AsycudaBillScreeningSchema.Constants.TableName] = 0;
				result[AsycudaTaxSchema.Constants.TableName] = 0;
				result.Add(GenAddOnColumnSchema.Constants.TableName, 1);
				result.Add(GenCustomAddOnValueSchema.Constants.TableName, 1);
				result.Add(StmALogSchema.Constants.TableName, 1);
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForValidateFetchStrategyExpectedHitCounts
		{
			get
			{
				var result = base.FetchForValidateFetchStrategyExpectedHitCounts;
				result.Add(OrgAddressSchema.Constants.TableName, 2); //(2 SQL In Statements)One hit for the header OrgAddresses, second hit for all the Bills OrgAddress.
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForValidateExecuteActionExpectedHitCounts
		{
			get
			{
				var result = base.FetchForValidateExecuteActionExpectedHitCounts;
				result.Add(CusEntryNumSchema.Constants.TableName, 1);
				result.Add(RefCurrencySchema.Constants.TableName, 4); //Storing Currency in GenAddOn, move these to real fields as they are in base.
				result.Add(ZZRefCusCodeListCombinedSchema.Constants.TableName, 27);
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForValidateUnconsumedExpectedHitCounts
		{
			get
			{
				var result = base.FetchForValidateUnconsumedExpectedHitCounts;
				result[AsycudaPackedItemSchema.Constants.TableName] = 1;
				result[AsycudaPackPackedItemPivotSchema.Constants.TableName] = 1;
				result.Add(GenCustomAddOnValueSchema.Constants.TableName, 1);
				result.Add(OrgAddressCapabilitySchema.Constants.TableName, 1);
				result.Add(OrgCompanyDataSchema.Constants.TableName, 1);
				result.Add(UNDGDataItemSchema.Constants.TableName, 1);
				return result;
			}
		}

		protected override ZString Message => "ASYCUDA Header (AsycudaManifestHeader)";
		protected override string ApplicationCode => ApplicationCodeTypeList.Codes.Consolidator;
		protected override Type AsycudaManifestHeaderTypeForTest => typeof(AsycudaManifestHeader);
		protected virtual ZString HeaderCountry => Core.Constants.CountryCodes.Eritrea;

		protected override void SetUp()
		{
			base.SetUp();
			var header = (AsycudaManifestHeader)base.header;
			SetupHeaderProperties(header);
			SetupMasterBills(header);
			SetupBills(header);
			SetupPacks(header);
			SetupTaxes(header);
			SetupPersons(header);
			SetupEntryNumbers(header);
			SetupMessages(header);
			SetupWorkFlowItems(header);
			Factory.Save();
		}

		void TestFetchForEnableBillsLockTestCases(out IList<FetchStrategyTestHelper.TestCase<AsycudaManifestHeader>> testCases)
		{
			testCases = new List<FetchStrategyTestHelper.TestCase<AsycudaManifestHeader>>
			{
				new FetchStrategyTestHelper.TestCase<AsycudaManifestHeader>
				{
					Message = "ASYCUDA Header Enable Bills Lock",
					BusinessObject = (AsycudaManifestHeader)header,
					ThresholdForUnspecified = 1
				}
			};
		}

		void TestFetchForUXMLExportTestCases(out IList<FetchStrategyTestHelper.TestCase<AsycudaManifestHeader>> testCases)
		{
			testCases = new List<FetchStrategyTestHelper.TestCase<AsycudaManifestHeader>>
			{
				new FetchStrategyTestHelper.TestCase<AsycudaManifestHeader>
				{
					Message = "ASYCUDA Header Tree Table Fetch Strategy",
					BusinessObject = (AsycudaManifestHeader)header,
					ExecuteActionExpectedHitCounts = FetchForUXMLExportStrategyExpectedHitCounts,
					UnconsumedExpectedHitCounts = new Dictionary<string, int>()
					{
						{ AsycudaBillScreeningSchema.Constants.TableName, 1 },
						{ AsycudaPackPackedItemPivotSchema.Constants.TableName, 1 },
						{ AsycudaPackedItemSchema.Constants.TableName, 1 },
						{ AsycudaTaxSchema.Constants.TableName, 1 },
						{ GenCustomAddOnRuleAckSchema.Constants.TableName, 1 },
						{ GenCustomAddOnValueSchema.Constants.TableName, 1 },
						{ OrgCompanyDataSchema.Constants.TableName, 1 },
						{ GenAddOnColumnSchema.Constants.TableName, 1 }
					}
				}
			};
		}

		void SetupMasterBills(AsycudaManifestHeader header)
		{
			header.MasterBill.ABL_BillNumber = "MASTERBILL1";

			var secondMasterBill = Factory.New<AsycudaBill>();
			secondMasterBill.ABL_BolType = AsycudaBill.ChildBolCode;
			secondMasterBill.ABL_BillNumber = "MASTERBILL2";
			secondMasterBill.ABL_AMA = header.PK;
			secondMasterBill.ABL_ClusterKey = header.AMA_ClusterKey;
		}

		void SetupBills(AsycudaManifestHeader header)
		{
			PopulateBill(header.Bills[0], "ZAJNB", "DEFRA");
			PopulateBill(header.Bills[1], "ZABFV", "SGSIN");

			void PopulateBill(AsycudaBill bill, ZString finalDestination, ZString origin)
			{
				bill.DutyAmount = 12.34m;
				bill.TaxAmount = 56.78m;
				bill.CustomsJobNumber = "CSJOB1";

				bill.ABL_RL_NKFinalDestination = finalDestination;
				bill.ABL_RL_NKOrigin = origin;

				bill.ABL_OA_Shipper = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				bill.ABL_OA_Consignee = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				bill.ABL_OA_NotifyParty = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				bill.ABL_OA_Forwarder = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

				bill.ABL_RN_NKShipperCountry = Core.Constants.CountryCodes.Germany;
				bill.ABL_RN_NKConsigneeCountry = Core.Constants.CountryCodes.SouthAfrica;
				bill.ABL_RN_NKNotifyPartyCountry = Core.Constants.CountryCodes.UnitedKingdom;

				bill.ABL_CustomsValue = 1m;
				bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.Thailand;
				bill.ABL_FreightValue = 1m;
				bill.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.VietNam;
				bill.ABL_InsuranceValue = 1m;
				bill.ABL_RX_NKInsuranceValueCurrency = Core.Constants.CurrencyCodes.Israel;
				bill.ABL_TransportValue = 1m;
				bill.ABL_RX_NKTransportValueCurrency = Core.Constants.CurrencyCodes.Canada;
				bill.OtherChargesValue = 1m;
				bill.OtherChargesValueCurrency = Core.Constants.CurrencyCodes.AntiguaAndBarbuda;
				bill.DiscountValue = 1m;
				bill.DiscountValueCurrency = Core.Constants.CurrencyCodes.CzechRepublic;

				var billRegistrationNumber = bill.CustomsEntryNumbers.AddNew();
				billRegistrationNumber.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
				billRegistrationNumber.CE_EntryNum = "BILLREGO1";
				var billEntryNumber = bill.CustomsEntryNumbers.AddNew();
				billEntryNumber.CE_EntryType = "REG";
				billEntryNumber.CE_EntryNum = "BILLREGO2";

				for (var i = 1; i < 3; i++)
				{
					var message = bill.Messages.AddNew();
					message.EM_MessageNum = "MSG0" + i;
					message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					_ = bill.WorkflowItems.AddNew();
				}
			}
		}

		void SetupPacks(AsycudaManifestHeader header)
		{
			PopulatePack(header.Bills[0].Packs[0], Core.Constants.CurrencyCodes.Angola, "0041A", "BAG", Core.Constants.CountryCodes.Uzbekistan, "NMB");
			PopulatePack(header.Bills[0].Packs[1], Core.Constants.CurrencyCodes.Benin, "0041B", "BSK", Core.Constants.CountryCodes.Vatican, "LOT");
			PopulatePack(header.Bills[1].Packs[0], Core.Constants.CurrencyCodes.Comoros, "0041C", "KG", Core.Constants.CountryCodes.Yemen, "SAC");
			PopulatePack(header.Bills[1].Packs[1], Core.Constants.CurrencyCodes.GuineaBissau, "0041D", "REL", Core.Constants.CountryCodes.Zambia, "TRY");

			void PopulatePack(AsycudaPack pack, ZString linePriceCurrency, ZString undgSub, ZString packUQ, ZString goodsOrigin, ZString customsUQ)
			{
				pack.LinePrice = 2m;
				pack.LinePriceCurrency = linePriceCurrency;

				var unno = undgSub.SubstringSafe(0, UNDGSubstanceSchema.DG_UNNO.MaxLength);
				var variant = undgSub.SubstringSafe(UNDGSubstanceSchema.DG_UNNO.MaxLength, UNDGSubstanceSchema.DG_Variant.MaxLength);
				DGSubstanceTestHelper.CreateIfDoesntExist(unno, variant, "IMO");

				var undg = pack.UNDGs.AddNew();
				var subs = UNDGSubstanceLoader.LoadSubstances(Factory, unno, variant, "IMO").FirstOrDefault();
				undg.LinkDefault(subs);
				pack.ConsignmentReference = 1;
				pack.MatchingReference = "2";
				pack.APA_PackQty = 23;
				pack.APA_PackUQ = packUQ;
				var packedItem = pack.PackedItemForTesting();
				packedItem.API_RN_NKGoodsOrigin = goodsOrigin;
				packedItem.API_CustomsQty = 34;
				packedItem.API_CustomsUQ = customsUQ;
			}
		}

		void SetupTaxes(AsycudaManifestHeader header)
		{
			PopulateTax(header.Bills[0].AsycudaTaxes[0], Core.Constants.CurrencyCodes.Angola, "BAG", Core.Constants.CountryCodes.Uzbekistan);
			PopulateTax(header.Bills[0].AsycudaTaxes[1], Core.Constants.CurrencyCodes.Benin, "BSK", Core.Constants.CountryCodes.Vatican);
			PopulateTax(header.Bills[1].AsycudaTaxes[0], Core.Constants.CurrencyCodes.Comoros, "KG", Core.Constants.CountryCodes.Yemen);
			PopulateTax(header.Bills[1].AsycudaTaxes[1], Core.Constants.CurrencyCodes.GuineaBissau, "REL", Core.Constants.CountryCodes.Zambia);
		}

		protected virtual void PopulateTax(AsycudaTax tax, ZString chargetype, ZString methodofcalculation, ZString methodofpayment)
		{
			tax.AET_Rate = 2m;
			tax.AET_ChargeType = chargetype;
			tax.AET_MethodOfCalculation = methodofcalculation;
			tax.AET_MethodOfPayment = methodofpayment;
		}

		void SetupHeaderProperties(AsycudaManifestHeader header)
		{
			header.ConsignmentReferenceTracker = 234;
			header.AMA_RN_NKCountry = HeaderCountry;
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			header.AMA_OA_ShippingAgent = new ZGuid("CAB45208-6744-4E9F-AF3C-86993F548B99");
			header.AMA_OA_DeconsolidateAddress = new ZGuid("0FF30DF8-5976-4A55-B1C0-8FE63DDE35B5");
			header.AMA_OA_DischargeTerminalAddress = new ZGuid("A615639A-ADAB-4348-B614-906A1CB9E57F");
			header.AMA_OA_Carrier = new ZGuid("0EA85FB9-EC2E-4713-8A96-85B6290E97BB");
			header.AMA_RN_NKConveyanceNationality = Core.Constants.CountryCodes.UnitedKingdom;
			header.AMA_E_DEP = ZDateTime.Today.AddDays(-1);
			header.AMA_E_ARV = ZDateTime.Today.AddDays(1);
			header.MasterBOL = "MASTERBOL123";
			header.AMA_RL_NKPortOfDischarge = "ERASA";
			header.AMA_RL_NKPortOfLoading = "USATL";
			header.AMA_RL_NKPortOfFirstArrival = "ERASM";
			header.AMA_VesselName = "Wrights' Flyer";
			header.AMA_Voyage = "BA123";

			for (var i = 1; i < 3; i++)
			{
				var note = header.Notes.AddNew();
				note.ST_Description = "TEST NOTE" + i;
				note.ST_NoteDataAsText = "CONTENTS OF NOTE" + i;
			}
		}

		void SetupEntryNumbers(AsycudaManifestHeader header)
		{
			var registrationNumber = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, header.AMA_RN_NKCountry);
			registrationNumber.CE_EntryNum = "TESTREG1";
		}

		void SetupPersons(AsycudaManifestHeader header)
		{
			for (var i = 1; i < 4; i++)
			{
				var person = header.Persons.AddNew();
				person.CPN_PER_Person = Factory.NewWithValidTestData<GlbPerson>().PK;
				var country = person.Countries.AddNew();
				country.CPC_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
				country.CPC_Type = "OCC";
				country.CPC_Value = "H";
			}
		}

		void SetupMessages(AsycudaManifestHeader header)
		{
			for (var i = 1; i < 4; i++)
			{
				var message = header.Messages.AddNew();
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message.EM_MessageNum = i.ToString();
				message.EM_LinkUniqueID = header.PK;
				message.EM_LinkTable = AsycudaManifestHeader.Schema.TableName;
			}
		}

		void SetupWorkFlowItems(AsycudaManifestHeader header)
		{
			for (var i = 1; i < 4; i++)
			{
				var milestone = header.WorkflowItems.Milestones.AddNew();
				milestone.SetMilestoneActualDateForTest(ZDateTime.Now.AddDays(i));
			}
		}
	}
}
