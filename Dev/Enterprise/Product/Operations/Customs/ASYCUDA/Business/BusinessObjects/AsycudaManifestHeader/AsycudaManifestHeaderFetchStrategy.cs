using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	[System.Runtime.InteropServices.Guid("58CC6F34-515C-482B-BC4B-EE5408B22F49")]
	public class AsycudaManifestHeaderFetchStrategy : ManifestBase.AsycudaManifestHeaderFetchStrategy
	{
		public AsycudaManifestHeaderFetchStrategy(AsycudaManifestHeader header)
			: base(header)
		{
			isInDatabase = header.IsInDatabase;
		}
		readonly ZBool isInDatabase;

		public void FetchForEnableBillsLock()
		{
			if (!GetBusinessObjectFetched(AsycudaFetchForEnableBillsLock))
			{
				SetBusinessObjectFetched(AsycudaFetchForEnableBillsLock);
				var clusterKey = BusinessObject.AMA_ClusterKey;
				Factory.AddFetchHint(AsycudaBillSchema.ABL_ClusterKey, clusterKey);
				Factory.AddFetchHint(typeof(CusEntryNumber), new ZQuery(CusEntryNumSchema.CE_ParentID, BusinessObject.Bills.Select(bill => bill.PK)));

				Factory.AddFetchHint(AsycudaPackSchema.APA_ClusterKey, clusterKey);
				Factory.AddFetchHint(AsycudaTaxSchema.AET_ClusterKey, clusterKey);
				if (!BusinessObject.IsNonePackedItemRelationship)
				{
					Factory.AddFetchHint(AsycudaPackedItemSchema.API_ClusterKey, clusterKey);
					var packedItemPKs = BusinessObject.Bills.Cast<AsycudaBill>()
						.SelectMany(bill => bill.Packs.Cast<AsycudaPack>()
							.SelectMany(pack => pack.PackedItems
							.Cast<ManifestBase.AsycudaPackPackedItemPivot>()
							.Select(x => x.APP_API_Item).ToArray()));
					Factory.AddFetchHint(typeof(CusEntryNumber), new ZQuery(CusEntryNumSchema.CE_ParentID, packedItemPKs));
				}
			}
		}

		protected new AsycudaManifestHeader BusinessObject => (AsycudaManifestHeader)base.BusinessObject;

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case AsycudaManifestHeader.Schema.AMA_NoOfBills:
					case AsycudaManifestHeader.Schema.AMA_CustomsDischargePort:
					case AsycudaManifestHeader.Schema.AMA_CustomsLoadPort:
						Factory.AddFetchHint(AsycudaBillSchema.ABL_AMA, BusinessObject.PK);
						break;
					case AsycudaManifestHeader.Schema.AMA_RL_NKPortOfDischarge:
					case AsycudaManifestHeader.Schema.AMA_RL_NKPortOfLoading:
						Factory.AddFetchHint(typeof(AsycudaBill), BusinessObject.GetMasterBillQuery());
						break;
					case AsycudaManifestHeader.Schema.RegistrationDate:
					case AsycudaManifestHeader.Schema.RegistrationNumber:
						Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
						break;
					case AsycudaManifestHeader.Schema.RegistrationStatus:
						Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
						Factory.AddFetchHint(typeof(ZZRefCusCodeListCombined), new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus));
						break;
				}
			}
		}

		protected override IEnumerable<BusinessObject> LoadChildren() => base.LoadChildren().Union(new BusinessObject[] { BusinessObject.MasterBill });

		protected override void AddCommonFetchHints()
		{
			base.AddCommonFetchHints();

			var billsQuery = DataHelper.GenerateClusterKeyQuery(BusinessObject.AMA_ClusterKey, BusinessObject.PK, AsycudaBillSchema.ABL_ClusterKey, AsycudaBillSchema.ABL_AMA, !isInDatabase);
			billsQuery.AddToFilter(new ZQuery(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, AsycudaBill.ChildBolCode));
			Factory.AddFetchHint(typeof(AsycudaBill), billsQuery);

			Factory.AddFetchHint(typeof(AsycudaBill), BusinessObject.GetMasterBillQuery());
		}

		protected override void AddFetchHintsForDeleteCore()
		{
			base.AddFetchHintsForDeleteCore();
			var businessObjectPK = BusinessObject.PK;
			Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, businessObjectPK);
			Factory.AddFetchHint(GenAddOnColumnSchema.XA_ParentID, businessObjectPK);
			Factory.AddFetchHint(ProcessHeaderSchema.FH_ParentId, businessObjectPK);
			Factory.AddFetchHint(ProcessJobTriggerLinkSchema.P9L_ParentId, businessObjectPK);
			Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, businessObjectPK);
		}

		protected override void AddFetchHintsForLoadChildEditableObjectsCore()
		{
			base.AddFetchHintsForLoadChildEditableObjectsCore();
			var businessObjectPK = BusinessObject.PK;
			Factory.AddFetchHint(AsycudaArrivalHeaderSchema.ATH_AMA_ManifestHeader, businessObjectPK);
			Factory.AddFetchHint(CusPersonSchema.CPN_ParentID, businessObjectPK);
			Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, businessObjectPK);
			Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, businessObjectPK);
			Factory.AddFetchHint(JobConsolTransportSchema.JW_ParentGUID, businessObjectPK);
			Factory.AddFetchHint(StmALogSchema.SL_Parent, businessObjectPK);
			if (ProcessJobHeaderProvider.BufferManagementEnabledForWorkflowProvider(BusinessObject, Factory))
			{
				Factory.AddFetchHint(ProcessHeaderSchema.FH_ParentId, businessObjectPK);
			}
		}

		protected override void AddFetchHintsForValidateCore()
		{
			base.AddFetchHintsForValidateCore();
			Factory.AddFetchHint(GenAddOnColumnSchema.XA_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(OrgAddressSchema.PK, BusinessObject.AMA_OA_Carrier);
			Factory.AddFetchHint(OrgAddressSchema.PK, BusinessObject.AMA_OA_ShippingAgent);
			Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, BusinessObject.AMA_RL_NKPortOfFirstArrival);
			Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, BusinessObject.AMA_RL_NKPortOfLoading);
			Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, BusinessObject.AMA_RL_NKPortOfDischarge);
		}

		protected override IEnumerable<BusinessObject> LoadChildrenForValidate() => base.LoadChildrenForValidate().Union(BusinessObject.Persons).Union(new BusinessObject[] { BusinessObject.ShippingAgent, BusinessObject.Carrier });

		protected override void AddFetchHintsForTreeTableStrategyCore(IExternalFetchHintSupporter externalFetchHintSupporter)
		{
			base.AddFetchHintsForTreeTableStrategyCore(externalFetchHintSupporter);
			AddCommonFetchHints();
			Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(OrgAddressSchema.PK, BusinessObject.AMA_OA_Carrier);
			Factory.AddFetchHint(OrgAddressSchema.PK, BusinessObject.AMA_OA_ShippingAgent);
			Factory.AddFetchHint(OrgAddressSchema.PK, BusinessObject.AMA_OA_DeconsolidateAddress);
			Factory.AddFetchHint(OrgAddressSchema.PK, BusinessObject.AMA_OA_DischargeTerminalAddress);
			Factory.AddFetchHint(RefCurrencySchema.RX_Code, BusinessObject.Country?.RN_RX_NKLocalCurrency);
			Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, BusinessObject.AMA_RL_NKPortOfFirstArrival);
			Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, BusinessObject.AMA_RL_NKPortOfLoading);
			Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, BusinessObject.AMA_RL_NKPortOfDischarge);

			externalFetchHintSupporter.AddTableFetchHintCreator(AsycudaBillSchema.Instance, GetAsycudaBillRelatedFetchHints, true);
			externalFetchHintSupporter.AddTableFetchHintCreator(AsycudaPackSchema.Instance, GetAsycudaPackRelatedFetchHints, true);
			externalFetchHintSupporter.AddTableFetchHintCreator(AsycudaTaxSchema.Instance, GetAsycudaTaxRelatedFetchHints, true);
			externalFetchHintSupporter.AddTableFetchHintCreator(AsycudaPackedItemSchema.Instance, GetAsycudaPackedItemRelatedFetchHints, true);
			externalFetchHintSupporter.AddTableFetchHintCreator(OrgAddressSchema.Instance, GetOrgAddressRelatedFetchHints, true);
			externalFetchHintSupporter.AddTableFetchHintCreator(GenAddOnColumnSchema.Instance, GetAsycudaGenAddOnColumnRelatedFetchHints, true);
		}

		IEnumerable<IFetchHint> GetAsycudaBillRelatedFetchHints(IColumnIndexer asycudaBillRow)
		{
			var billPk = asycudaBillRow.GetValue(AsycudaBillSchema.PK);
			yield return new FetchHint(CusEntryNumSchema.CE_ParentID, billPk);
			yield return new FetchHint(GenAddOnColumnSchema.XA_ParentID, billPk);
			yield return new FetchHint(OrgAddressSchema.PK, asycudaBillRow.GetValue(AsycudaBillSchema.ABL_OA_NotifyParty));
			yield return new FetchHint(OrgAddressSchema.PK, asycudaBillRow.GetValue(AsycudaBillSchema.ABL_OA_Shipper));
			yield return new FetchHint(OrgAddressSchema.PK, asycudaBillRow.GetValue(AsycudaBillSchema.ABL_OA_Consignee));
			yield return new FetchHint(ProcessTasksSchema.P9_ParentID, billPk);
			yield return new FetchHint(RefCurrencySchema.RX_Code, asycudaBillRow.GetValue(AsycudaBillSchema.ABL_RX_NKCustomsValueCurrency));
			yield return new FetchHint(RefCurrencySchema.RX_Code, asycudaBillRow.GetValue(AsycudaBillSchema.ABL_RX_NKFreightValueCurrency));
			yield return new FetchHint(RefCurrencySchema.RX_Code, asycudaBillRow.GetValue(AsycudaBillSchema.ABL_RX_NKInsuranceValueCurrency));
			yield return new FetchHint(RefCurrencySchema.RX_Code, asycudaBillRow.GetValue(AsycudaBillSchema.ABL_RX_NKTransportValueCurrency));
			yield return new FetchHint(RefUNLOCOSchema.RL_Code, asycudaBillRow.GetValue(AsycudaBillSchema.ABL_RL_NKOrigin));
			yield return new FetchHint(RefUNLOCOSchema.RL_Code, asycudaBillRow.GetValue(AsycudaBillSchema.ABL_RL_NKFinalDestination));
		}

		IEnumerable<IFetchHint> GetAsycudaPackRelatedFetchHints(IColumnIndexer asycudaPackRow)
		{
			yield return new ZQueryFetchHint(RefPacksSchema.Instance, CusRefPacksHelper.GetRefPacksQuery(BusinessObject.AMA_RN_NKCountry, RPTypeList.Codes.GlobalManifestLine, asycudaPackRow.GetValue(AsycudaPackSchema.APA_PackUQ)));
		}

		IEnumerable<IFetchHint> GetAsycudaTaxRelatedFetchHints(IColumnIndexer asycudaTaxrow)
		{
			yield return new FetchHint(CusEntryNumSchema.CE_ParentID, asycudaTaxrow.GetValue(AsycudaTaxSchema.PK));
		}

		IEnumerable<IFetchHint> GetAsycudaPackedItemRelatedFetchHints(IColumnIndexer asycudaPackedItemRow)
		{
			yield return new FetchHint(CusEntryNumSchema.CE_ParentID, asycudaPackedItemRow.GetValue(AsycudaPackedItemSchema.PK));
		}

		IEnumerable<IFetchHint> GetOrgAddressRelatedFetchHints(IColumnIndexer orgAddressRow)
		{
			var orgAddressPk = orgAddressRow.GetValue(OrgAddressSchema.PK);
			var orgHeaderPk = orgAddressRow.GetValue(OrgAddressSchema.OA_OH);
			yield return new FetchHint(OrgHeaderSchema.PK, orgHeaderPk);
			yield return new ZQueryFetchHint(OrgAddressSchema.Instance, new ZQuery(OrgAddressSchema.OA_OH, orgHeaderPk));
			yield return new ZQueryFetchHint(OrgCusCodeSchema.Instance, new ZQuery(OrgCusCodeSchema.OK_OH, orgHeaderPk));
			yield return new ZQueryFetchHint(OrgAddressCapabilitySchema.Instance, new ZQuery(OrgAddressCapabilitySchema.PZ_OA, orgAddressPk));
			yield return new ZQueryFetchHint(OrgTranslatedAddressSchema.Instance, new ZQuery(OrgTranslatedAddressSchema.OTA_OA, orgAddressPk));
			yield return new FetchHint(RefUNLOCOSchema.RL_Code, orgAddressRow.GetValue(OrgAddressSchema.OA_RL_NKRelatedPortCode));
		}

		IEnumerable<IFetchHint> GetAsycudaGenAddOnColumnRelatedFetchHints(IColumnIndexer genAddOnColumnRow)
		{
			var name = genAddOnColumnRow.GetValue(GenAddOnColumnSchema.XA_Name);
			var table = genAddOnColumnRow.GetValue(GenAddOnColumnSchema.XA_ParentTableCode);
			var data = genAddOnColumnRow.GetValue(GenAddOnColumnSchema.XA_Data);
			if (table == AsycudaBillSchema.Constants.Prefix)
			{
				if (name == AsycudaBill.Schema.DiscountValueCurrency || name == AsycudaBill.Schema.OtherChargesValueCurrency)
				{
					yield return new FetchHint(RefCurrencySchema.RX_Code, data);
				}
			}
			else if (table == AsycudaPackSchema.Constants.Prefix)
			{
				if (name == AsycudaPack.Schema.LinePriceCurrency)
				{
					yield return new FetchHint(RefCurrencySchema.RX_Code, data);
				}
			}
		}

		const string AsycudaFetchForEnableBillsLock = "AsycudaFetchForEnableBillsLock";
	}
}
