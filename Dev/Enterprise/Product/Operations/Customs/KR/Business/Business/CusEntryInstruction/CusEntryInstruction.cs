using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.KR.Business
{
	public class CusEntryInstruction : AutoKRCusEntryInstruction,
		Integration.Customs.KR.ICusEntryInstruction,
		ICusCodeDataTypeSupporter,
		ICusSupportingInfoTypeSupporter
	{
		public CusEntryInstruction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool SupportsCloneCore() => true;
		public new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;

		public ZString TariffRateClassificationShortName
		{
			get
			{
				ZString result = ZString.Empty;
				if (!CEI_AgreedDutyRatePreferenceCode.IsEmpty)
				{
					result = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.RefCusMap.KRPreferenceCode, CEI_AgreedDutyRatePreferenceCode, ZDateTime.Now);
				}
				return result;
			}
		}

		[ResourceStringData("06BCAFA6-81F1-43E4-8C6D-4DEBA49D9071", Caption = "Use Type")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.BondedFactoryUseCodeList))]
		public override ZString CEI_BondedFactoryUseCode { get => base.CEI_BondedFactoryUseCode; set => base.CEI_BondedFactoryUseCode = value; }

		[ResourceStringData("DB6563F5-2258-40C9-93C0-2E76E1BB28F3", Caption = "Use Date")]
		public override ZDateTime CEI_BondedFactoryArrivalDate
		{
			get => base.CEI_BondedFactoryArrivalDate;
			set => base.CEI_BondedFactoryArrivalDate = value;
		}

		[ChildEditable(true)]
		public OnlineOrderCollection OnlineOrders
		{
			get
			{
				if (onlineOrders == null)
				{
					onlineOrders = new OnlineOrderCollection(this);
					onlineOrders.Load();
					RegisterEditableChildObject(onlineOrders);
				}
				return onlineOrders;
			}
		}
		OnlineOrderCollection onlineOrders;
		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ CusCodeDataTypeList.Codes.OnlineOrder, typeof(OnlineOrder) }
			};
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
			yield return new Customs.Business.FetchStrategies.CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		[ResourceStringData("5472E088-B1B4-4B41-A111-9974C9F53FF9", Caption = "Agreed Rate")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.AgreedRateList))]
		public override ZString CEI_AgreedRateApp { get => base.CEI_AgreedRateApp; set => base.CEI_AgreedRateApp = value; }

		[ResourceStringData("2B7ACFC6-B62B-4BB6-8C24-00F3E5B93361", Caption = "Total Packages")]
		public override ZInt CEI_PackQty { get => base.CEI_PackQty; set => base.CEI_PackQty = value; }

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.FTARelationArticleCodeList))]
		public override ZString CEI_FTARelationArticleCode { get => base.CEI_FTARelationArticleCode; set => base.CEI_FTARelationArticleCode = value; }

		[ResourceStringData("4EB7810C-DDBA-4C6D-A2B0-3867665F99EE", Caption = "Customs Disbursement Bill #")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.StatementNumber5WNList))]
		public override ZString CEI_StatementNumber5WN { get => base.CEI_StatementNumber5WN; set => base.CEI_StatementNumber5WN = value; }

		[ResourceStringData("Enterprise.Customs.Business.CusEntryInstruction|CEI_Style", Caption = "ID")]
		public override ZString CEI_Style { get => base.CEI_Style; set => base.CEI_Style = value; }

		[ResourceStringData("56912AE5-740A-451C-83C8-FBBCB3B68480", Caption = "Accepted Date")]
		public ZDateTime AcceptedDate => EntryHeader?.AcceptedDate ?? ZDateTime.Empty;

		[ChildEditable(true)]
		public AmendmentSessionalDataCollection AmendmentSessionalDataCollection
		{
			get
			{
				if (amendmentSessionalDataCollection == null)
				{
					amendmentSessionalDataCollection = new AmendmentSessionalDataCollection(this);
					amendmentSessionalDataCollection.Load();
					RegisterEditableChildObject(amendmentSessionalDataCollection);
				}
				return amendmentSessionalDataCollection;
			}
		}
		AmendmentSessionalDataCollection amendmentSessionalDataCollection;

		[ChildEditable(true)]
		public RefundSessionalDataCollection RefundSessionalDataCollection
		{
			get
			{
				if (refundSessionalDataCollection == null)
				{
					refundSessionalDataCollection = new RefundSessionalDataCollection(this);
					refundSessionalDataCollection.Load();
					RegisterEditableChildObject(refundSessionalDataCollection);
				}
				return refundSessionalDataCollection;
			}
		}
		RefundSessionalDataCollection refundSessionalDataCollection;

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type> {
				{ CusSupportingInfoTypeList.Codes._5FE, typeof(AmendmentSessionalData) },
				{ CusSupportingInfoTypeList.Codes._5UA, typeof(PenaltyExemptionSessionalData) },
				{ CusSupportingInfoTypeList.Codes._5UL, typeof(RefundSessionalData) }
			};
			return result;
		}

		protected override Customs.Business.CusEntryInstructionValidation GetNewValidation()
		{
			return new CusEntryInstructionValidation(this);
		}
		public new CusEntryInstructionLookups Lookups => (CusEntryInstructionLookups)base.Lookups;
		protected override Customs.Business.CusEntryInstructionLookups GetNewLookups() => new CusEntryInstructionLookups(this);
	}
}
