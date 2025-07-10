using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaBillFetchStrategyTest : ManifestBase.Testing.AsycudaBillFetchStrategyTest
	{
		protected override Dictionary<string, int> FetchForDeleteExecuteActionExpectedHitCounts
		{
			get
			{
				var result = base.FetchForDeleteExecuteActionExpectedHitCounts;
				result[ProcessJobTriggerLinkSchema.Constants.TableName] = 2;
				result[StmDocDataOverrideSchema.Constants.TableName] = 26;
				result[StmNoteSchema.Constants.TableName] = 7;
				result[StmUniversalCopySchema.Constants.TableName] = 17;
				result[BMNCNShapeSchema.Constants.TableName] = 2;
				result[ProcessHeaderSchema.Constants.TableName] = 3;
				result[ProcessHeaderLinkSchema.Constants.TableName] = 3;
				result[ProcessTaskIterationLinkSchema.Constants.TableName] = 3;
				result[ProcessTasksSchema.Constants.TableName] = 3;
				result[TagLinkSchema.Constants.TableName] = 3;
				result.Add(AsycudaBillScreeningSchema.Constants.TableName, 1);
				result.Add(AsycudaTaxSchema.Constants.TableName, 1);
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForDeleteUnconsumedExpectedHitCounts
		{
			get
			{
				var result = base.FetchForDeleteUnconsumedExpectedHitCounts;
				result.Add(EDIMessageAttachSchema.Constants.TableName, 1);
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForLoadChildEditableObjectsExecuteActionExpectedHitCounts
		{
			get
			{
				var result = base.FetchForLoadChildEditableObjectsExecuteActionExpectedHitCounts;
				result.Add(CusEntryNumSchema.Constants.TableName, 1);
				result.Add(ProcessHeaderLinkSchema.Constants.TableName, 4);
				result.Add(ProcessTaskIterationLinkSchema.Constants.TableName, 2);
				result.Add(TagLinkSchema.Constants.TableName, 2);
				result[AsycudaPackedItemSchema.Constants.TableName] = 3;
				result.Add(AsycudaBillScreeningSchema.Constants.TableName, 1);
				result.Add(AsycudaTaxSchema.Constants.TableName, 1);
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForLoadChildEditableObjectsUnconsumedExpectedHitCounts
		{
			get
			{
				var result = base.FetchForLoadChildEditableObjectsUnconsumedExpectedHitCounts;
				result.Add(GenAddOnColumnSchema.Constants.TableName, 1);
				result.Add(ProcessTasksSchema.Constants.TableName, 1);
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForValidateExecuteActionExpectedHitCounts
		{
			get
			{
				var result = base.FetchForValidateExecuteActionExpectedHitCounts;
				result.Add(ZZRefCusCodeListCombinedSchema.Constants.TableName, 17);
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForValidateUnconsumedExpectedHitCounts
		{
			get
			{
				var result = base.FetchForValidateUnconsumedExpectedHitCounts;
				result.Add(GenCustomAddOnValueSchema.Constants.TableName, 1);
				result.Add(GenCustomAddOnRuleAckSchema.Constants.TableName, 1);
				result.Add(OrgAddressCapabilitySchema.Constants.TableName, 1);
				result.Add(OrgCompanyDataSchema.Constants.TableName, 1);
				result.Add(UNDGDataItemSchema.Constants.TableName, 1);
				result.Add(AsycudaBillScreeningSchema.Constants.TableName, 1);
				result.Add(AsycudaTaxSchema.Constants.TableName, 1);
				return result;
			}
		}

		protected override ZString Message => "ASYCUDA Bill (AsycudaBill)";

		protected override string ApplicationCode => ApplicationCodeTypeList.Codes.Consolidator;

		protected override Type AsycudaManifestHeaderTypeForTest => typeof(AsycudaManifestHeader);

		protected override void SetUp()
		{
			base.SetUp();
			var bill = (AsycudaBill)base.bill;
			bill.DutyAmount = 12.34m;
			bill.TaxAmount = 56.78m;
			bill.CustomsJobNumber = "CSJOB1";

			bill.ABL_RL_NKFinalDestination = "ZAJNB";
			bill.ABL_RL_NKOrigin = "DEFRA";

			bill.ABL_OA_Shipper = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			bill.ABL_OA_Consignee = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			bill.ABL_OA_NotifyParty = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			bill.ABL_OA_Forwarder = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			bill.ABL_RN_NKShipperCountry = Core.Constants.CountryCodes.Germany;
			bill.ABL_RN_NKConsigneeCountry = Core.Constants.CountryCodes.SouthAfrica;
			bill.ABL_RN_NKNotifyPartyCountry = Core.Constants.CountryCodes.UnitedKingdom;

			bill.ABL_CustomsValue = 1m;
			bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			bill.ABL_FreightValue = 1m;
			bill.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill.ABL_InsuranceValue = 1m;
			bill.ABL_RX_NKInsuranceValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			bill.ABL_TransportValue = 1m;
			bill.ABL_RX_NKTransportValueCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			bill.OtherChargesValue = 1m;
			bill.OtherChargesValueCurrency = Core.Constants.CurrencyCodes.Australia;
			bill.DiscountValue = 1m;
			bill.DiscountValueCurrency = Core.Constants.CurrencyCodes.HolySee;

			var billRegistrationNumber = bill.CustomsEntryNumbers.AddNew();
			billRegistrationNumber.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
			billRegistrationNumber.CE_EntryNum = "BILLREGO1";
			var billEntryNumber = bill.CustomsEntryNumbers.AddNew();
			billEntryNumber.CE_EntryType = "REG";
			billEntryNumber.CE_EntryNum = "BILLREGO2";

			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmTestHelper.EnableBMSInRegistry();
			bmTestHelper.CreateSystem(Factory, bill.WorkflowType);

			for (var i = 1; i < 3; i++)
			{
				var message = bill.Messages.AddNew();
				message.EM_MessageNum = "MSG0" + i;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				_ = bill.WorkflowItems.AddNew();
			}
			Factory.Save();
		}
	}
}
