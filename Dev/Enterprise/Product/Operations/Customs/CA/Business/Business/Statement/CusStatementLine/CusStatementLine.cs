using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	[DependentBusinessObject(typeof(CusStatementHeader), "StatementLines")]
	public class CusStatementLine : BaseCusStatementLine, Integration.Customs.CA.ICusStatementLine, IAdditionalBusinessObjectFetchStrategyProvider, ICusSupportingInfoTypeSupporter
	{
		public CusStatementLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusStatementLine.Schema
		{
			public const string EntryStatusDescription = "EntryStatusDescription";
			public const string B4_ChargeAmountDTY = "B4_ChargeAmountDTY";
			public const string B4_ChargeAmountSIM = "B4_ChargeAmountSIM";
			public const string B4_ChargeAmountGST = "B4_ChargeAmountGST";
			public const string B4_ChargeAmountGSTOrGSD = "B4_ChargeAmountGSTOrGSD";
			public const string B4_ChargeAmountEXS = "B4_ChargeAmountEXS";
			public const string B4_ChargeAmountOTH = "B4_ChargeAmountOTH";
			public const string B4_CARMDNChargeAmount_Duties = "B4_CARMDNChargeAmount_Duties";
			public const string B4_CARMDNChargeAmount_SIMA = "B4_CARMDNChargeAmount_SIMA";
			public const string B4_CARMDNChargeAmount_ExciseTax = "B4_CARMDNChargeAmount_ExciseTax";
			public const string B4_CARMDNChargeAmount_ExciseDuties = "B4_CARMDNChargeAmount_ExciseDuties";
			public const string B4_CARMDNChargeAmount_GSTAndHSTAndPST = "B4_CARMDNChargeAmount_GSTAndHSTAndPST";
			public const string B4_CARMDNChargeAmount_Interests = "B4_CARMDNChargeAmount_Interests";
			public const string B4_CARMDNChargeAmount_Others = "B4_CARMDNChargeAmount_Others";
			public const string B4_CARMDNChargeAmount_Penalties = "B4_CARMDNChargeAmount_Penalties";
			public const string B4_CARMDNChargeAmount_Payments = "B4_CARMDNChargeAmount_Payments";
			public const string ImporterCode = "ImporterCode";
			public const string ImporterName = "ImporterName";
		}

		[ChildEditable(true)]
		public CusStatementLineChargeCollection Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = new CusStatementLineChargeCollection(this);
					fCharges.Load();
					RegisterEditableChildObject(fCharges);
				}
				return fCharges;
			}
		}
		CusStatementLineChargeCollection fCharges;

		#region Property

		public ZString EntryStatusDescription => !B3_EntryStatus.IsEmpty
			? B3_EntryStatus.Length == 1
				? Factory.GetCachedValue<ARLLegacyTransactionStatusList>().GetDescriptionFromCode(B3_EntryStatus)
				: Factory.GetCachedValue<ARLTransactionStatusList>().GetDescriptionFromCode(B3_EntryStatus)
			: string.Empty;

		public ZPropertyInfo EntryStatusDescriptionInfo => GetZPropertyInfo(nameof(Schema.EntryStatusDescription));

		[ResourceStringData("CACusStatementLine|PaymentMethod", Caption = "Payment Method")]
		public ZString PaymentMethod
		{
			get
			{
				if (paymentMethodCached == null)
				{
					paymentMethodCached = new CachedProperty<ZString>(Factory, () =>
					{
						var result = ZString.Empty;
						var isIMP = false;
						if (!(StatementHeader?.B2_IsMonthlyStatement ?? true))
						{
							isIMP = Charges.Count > 0;
							foreach (CusStatementLineCharge charge in Charges)
							{
								if (charge.B4_ChargeType == EntryChargeTypeList.Codes.TotalGSTDirectAmount && charge.B4_PaymentParty == PaymentPartyCodeDescriptionList.Codes.Importer)
								{
									result = PaymentPartyCodeDescriptionList.Codes.GST;
									break;
								}

								isIMP &= charge.B4_PaymentParty == PaymentPartyCodeDescriptionList.Codes.Importer;
							}
						}
						return isIMP && result.IsEmpty ? (ZString)PaymentPartyCodeDescriptionList.Codes.Importer : result;
					});
				}
				return paymentMethodCached.Value;
			}
		}
		CachedProperty<ZString> paymentMethodCached;

		public override ZGuid B3_B2
		{
			get => base.B3_B2;
			set
			{
				var oldValue = B3_B2;
				var oldHeader = oldValue != value ? StatementHeader : null;
				base.B3_B2 = value;
				if (!IsCopying && oldValue != B3_B2)
				{
					RefreshLineGroupStatementLines(oldHeader);
					RefreshLineGroupStatementLines(StatementHeader);
				}
			}
		}

		public override ZString B3_ImporterCustomsID
		{
			get => base.B3_ImporterCustomsID;
			set
			{
				var oldValue = B3_ImporterCustomsID;
				base.B3_ImporterCustomsID = value;
				if (!IsCopying && oldValue != B3_ImporterCustomsID)
				{
					RefreshLineGroupStatementLines(StatementHeader);
				}
			}
		}

		public override ZString B3_BrokerReference
		{
			get => base.B3_BrokerReference;
			set
			{
				var oldValue = B3_BrokerReference;
				base.B3_BrokerReference = value;
				if (!IsCopying && oldValue != B3_BrokerReference)
				{
					declaration = null;
				}
			}
		}

		#endregion

		void RefreshLineGroupStatementLines(CusStatementHeader header)
		{
			header?.RefreshLineGroupStatementLines();
		}

		public override void Delete()
		{
			CusStatementLineGroup relatedLineGroup = null;
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				Charges.RemoveAndDeleteAll();
				relatedLineGroup = LineGroup;
			}

			base.Delete();

			if (relatedLineGroup != null)
			{
				relatedLineGroup.RefreshStatementLines();
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var entryType = B3_EntryType;
				return entryType.IsEmpty
					? Res.GetString("B8686B0A-5D8C-4715-936D-F106F5BE288F", "Statement Line - {0}", B3_EntryNum)
					: Res.GetString("0C4414D3-E85D-4B70-B828-11EC533CF223", "Statement Line - ({0}){1}", entryType, B3_EntryNum);
			}
		}

		#region DNHistory Columns
		public new CusStatementHeader StatementHeader
		{
			get { return (CusStatementHeader)base.StatementHeader; }
		}

		public ZDateTime B2_ProcessDate
		{
			get
			{
				return StatementHeader?.B2_ProcessDate ?? ZDateTime.Empty;
			}
		}

		public ZString B2_EntryFilerCode
		{
			get
			{
				return StatementHeader?.B2_EntryFilerCode ?? ZString.Empty;
			}
		}

		public ZString B2_Status
		{
			get
			{
				return StatementHeader?.B2_Status ?? ZString.Empty;
			}
		}

		public ZString B2_PaymentType
		{
			get
			{
				var result = ZString.Empty;
				if (StatementHeader is CusStatementHeader header)
				{
					return header.B2_StatementNumber.IsEmpty ? header.B2_PaymentType : GetB2_PaymentTypeForARL();
				}
				return result;
			}
		}

		ZString GetB2_PaymentTypeForARL()
		{
			var charges = Charges.Cast<CusStatementLineCharge>().Where(x => !x.B4_PaymentParty.IsEmpty).ToArray();

			var gstCharges = charges
			.Where(x => x.B4_ChargeType == EntryChargeTypeList.Codes.TotalGSTDirectAmount)
			.ToArray();

			if (gstCharges.Length > 0
				&& gstCharges.All(x => x.B4_PaymentParty == PaymentPartyCodeDescriptionList.Codes.Importer)
				&& charges.Except(gstCharges).All(x => x.B4_PaymentParty == PaymentPartyCodeDescriptionList.Codes.Broker))
			{
				return PaymentPartyCodeDescriptionList.Codes.GST;
			}

			if (charges.Select(x => x.B4_PaymentParty).Distinct().Count() == 1)
			{
				return charges.First().B4_PaymentParty;
			}

			return "BRK/IMP";
		}

		public ZString B2_AccountNo
		{
			get
			{
				return StatementHeader?.B2_AccountNo ?? ZString.Empty;
			}
		}

		public ZString B2_StatementType
		{
			get
			{
				var statementType = StatementHeader?.B2_StatementType ?? ZString.Empty;
				return statementType == CusStatementHeaderTypes.Codes.Unknown ? ZString.Empty : statementType;
			}
		}

		public ZString B2_RMNumber
		{
			get { return StatementHeader?.B2_RMNumber ?? ZString.Empty; }
		}

		public ZDateTime B2_DueDate
		{
			get { return StatementHeader?.B2_DueDate ?? ZDateTime.Empty; }
		}

		public ZDecimal B4_ChargeAmountDTY
		{
			get
			{
				return GetRoundedChargeAmounts(EntryChargeTypeList.Codes.TotalDutyAmount);
			}
		}

		public ZDecimal B4_ChargeAmountGST
		{
			get
			{
				return GetRoundedChargeAmounts(EntryChargeTypeList.Codes.TotalGSTAmount);
			}
		}

		public ZDecimal B4_ChargeAmountGSTOrGSD
		{
			get
			{
				var result = B4_ChargeAmountGST;
				return result.IsEmpty ? B4_ChargeAmountGSD : result;
			}
		}

		public ZDecimal B4_ChargeAmountGSD
		{
			get
			{
				return GetRoundedChargeAmounts(EntryChargeTypeList.Codes.TotalGSTDirectAmount);
			}
		}

		public ZDecimal B4_ChargeAmountSIM
		{
			get
			{
				return GetRoundedChargeAmounts(EntryChargeTypeList.Codes.TotalSIMAAmount);
			}
		}

		public ZDecimal B4_ChargeAmountEXS
		{
			get
			{
				return GetRoundedChargeAmounts(EntryChargeTypeList.Codes.TotalExciseTaxAmount);
			}
		}

		public ZDecimal B4_ChargeAmountKPM
		{
			get
			{
				return GetRoundedChargeAmounts(EntryChargeTypeList.Codes.K84LateFilingPenalty);
			}
		}

		public ZDecimal B4_ChargeAmountOTH
		{
			get
			{
				return GetRoundedChargeAmounts(EntryChargeTypeList.Codes.Others);
			}
		}

		public ZDecimal CustomsFeesTotal
		{
			get
			{
				return Utilities.Round(B3_CustomsFeesTotal, 2);
			}
		}

		public JobDeclaration Declaration
		{
			get
			{
				if (declaration == null && !B3_BrokerReference.IsEmpty)
				{
					var query = new ZDBOnlyQuery(typeof(JobDeclaration));
					query.AddToFilter(JobDeclarationSchema.JE_DeclarationReference, SQLComparisonOperator.StartsWith, B3_BrokerReference);

					var pks = StatementHeader?.Company?.Branches.Select(x => x.PK).ToArray() ?? Array.Empty<ZGuid>();
					query.AddToFilter(JobDeclarationSchema.JE_GB, pks);
					query.OrderBy = JobDeclaration.Schema.JE_DeclarationReference;
					query.IsNoResultQuery = pks.Length == 0;

					var declarations = Factory.Load<JobDeclaration>(query);
					declaration = declarations.Length == 1 ? declarations[0] : null;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		public ZString MessageType
		{
			get { return Declaration?.JE_MessageType ?? ZString.Empty; }
		}

		public ZString ARLMessageTypeCode
		{
			get { return StatementHeader?.ARLMessageTypeCode ?? ZString.Empty; }
		}

		#endregion

		public ZString ImporterCode
		{
			get
			{
				if (B2_StatementType == CusStatementHeaderTypes.Codes.Broker)
				{
					if (ARLMessageTypeCode == ARLMessageTypes.Codes.DailyNotice)
					{
						return Declaration?.Importer?.OH_Code ?? ZString.Empty;
					}
					else
					{
						return StatementHeader?.Importer?.OH_Code ?? ZString.Empty;
					}
				}
				return LineGroup?.B10_ImporterCode ?? ZString.Empty;
			}
		}

		public ZString ImporterName
		{
			get
			{
				if (B2_StatementType == CusStatementHeaderTypes.Codes.Broker)
				{
					if (ARLMessageTypeCode == ARLMessageTypes.Codes.DailyNotice)
					{
						return Declaration?.ImporterName ?? ZString.Empty;
					}
					else
					{
						return StatementHeader?.ImporterName ?? ZString.Empty;
					}
				}
				return LineGroup?.B10_ImporterName ?? ZString.Empty;
			}
		}

		CusStatementLineGroup LineGroup
		{
			get { return lineGroup ?? (lineGroup = StatementHeader?.LineGroupCollection?.Cast<CusStatementLineGroup>().FirstOrDefault(x => x.B10_ImporterCustomsID == B3_ImporterCustomsID)); }
		}
		CusStatementLineGroup lineGroup;

		public bool IsNormalLine => B3_Status == TransactionBatchConstants.TransactionCategoryCodes.Normal;

		public static ZBool IsDuplicatedStatementLine(CusStatementLine dnLine1, CusStatementLine dnLine2)
		{
			return dnLine1.PK != dnLine2.PK && dnLine1.B3_BrokerReference == dnLine2.B3_BrokerReference && dnLine1.B3_EntryNum == dnLine2.B3_EntryNum && dnLine1.B3_EntryType == dnLine2.B3_EntryType
				&& dnLine1.B2_ProcessDate == dnLine2.B2_ProcessDate && dnLine1.B3_Status == dnLine2.B3_Status && dnLine1.B3_ImporterCustomsID == dnLine2.B3_ImporterCustomsID;
		}

		#region CSA RSF Customs Assessment

		public CusStatementLineCharge CustomsAssessmentCharge
		{
			get
			{
				CusStatementLineCharge result = null;
				if (B3_EntryType == CSARSFAssessmentTypes.Codes.CustomsAssessment)
				{
					result = Charges.Cast<CusStatementLineCharge>().FirstOrDefault() ?? Charges.AddNew();
				}
				return result;
			}
		}

		#endregion

		#region CARM Daily Notice Charges

		public ZDecimal B4_CARMDNChargeAmount_Duties
		{
			get
			{
				return GetRoundedChargeAmounts(CARMDailyNoticeChargeTypeList.Codes.Duties);
			}
		}

		public ZDecimal B4_CARMDNChargeAmount_SIMA
		{
			get
			{
				return GetRoundedChargeAmounts(CARMDailyNoticeChargeTypeList.Codes.SIMA);
			}
		}

		public ZDecimal B4_CARMDNChargeAmount_ExciseTax
		{
			get
			{
				return GetRoundedChargeAmounts(CARMDailyNoticeChargeTypeList.Codes.ExciseTax);
			}
		}

		public ZDecimal B4_CARMDNChargeAmount_ExciseDuties
		{
			get
			{
				return GetRoundedChargeAmounts(CARMDailyNoticeChargeTypeList.Codes.ExciseDuties);
			}
		}

		public ZDecimal B4_CARMDNChargeAmount_GST
		{
			get
			{
				return GetRoundedChargeAmounts(CARMDailyNoticeChargeTypeList.Codes.GoodsAndServicesTax);
			}
		}

		public ZDecimal B4_CARMDNChargeAmount_HST
		{
			get
			{
				return GetRoundedChargeAmounts(CARMDailyNoticeChargeTypeList.Codes.HarmonizedSalesTax);
			}
		}

		public ZDecimal B4_CARMDNChargeAmount_PST
		{
			get
			{
				return GetRoundedChargeAmounts(CARMDailyNoticeChargeTypeList.Codes.ProvincialSalesTax);
			}
		}

		public ZDecimal B4_CARMDNChargeAmount_GSTAndHSTAndPST
		{
			get
			{
				return B4_CARMDNChargeAmount_GST + B4_CARMDNChargeAmount_HST + B4_CARMDNChargeAmount_PST;
			}
		}

		public ZDecimal B4_CARMDNChargeAmount_Interests
		{
			get
			{
				return GetRoundedChargeAmounts(CARMDailyNoticeChargeTypeList.Codes.Interest);
			}
		}

		public ZDecimal B4_CARMDNChargeAmount_Others
		{
			get
			{
				return GetRoundedChargeAmounts(CARMDailyNoticeChargeTypeList.Codes.Others);
			}
		}

		public ZDecimal B4_CARMDNChargeAmount_Penalties
		{
			get
			{
				return GetRoundedChargeAmounts(CARMDailyNoticeChargeTypeList.Codes.Penalties);
			}
		}

		public ZDecimal B4_CARMDNChargeAmount_Payments
		{
			get
			{
				return GetRoundedChargeAmounts(CARMDailyNoticeChargeTypeList.Codes.Payments);
			}
		}

		public ZDecimal B4_CARMSOAChargeAmount_Totals
		{
			get
			{
				return B4_CARMDNChargeAmount_Duties +
					B4_CARMDNChargeAmount_ExciseTax +
					B4_CARMDNChargeAmount_ExciseDuties +
					B4_CARMDNChargeAmount_SIMA +
					B4_CARMDNChargeAmount_GST +
					B4_CARMDNChargeAmount_HST +
					B4_CARMDNChargeAmount_PST +
					B4_CARMDNChargeAmount_Payments +
					B4_CARMDNChargeAmount_Others;
			}
		}

		#endregion

		ZDecimal GetRoundedChargeAmounts(ZString chargeType)
		{
			return Utilities.Round(Charges.GetAmountFor(chargeType), 2);
		}

		#region CARM Daily Notice Extention Properties

		public ZString CARMTransactionDescription
		{
			get { return PropertyGetter(x => x.CSI_Description); }
			set { CARMDailyNoticeExtension.CSI_Description = value; }
		}

		public ZString CARMCADVersion
		{
			get { return PropertyGetter(x => x.CSI_Code); }
			set { CARMDailyNoticeExtension.CSI_Code = value; }
		}

		public ZString CARMSubmittedBy
		{
			get { return PropertyGetter(x => x.CSI_ReferenceNumber); }
			set { CARMDailyNoticeExtension.CSI_ReferenceNumber = value; }
		}

		public ZString CARMStatus
		{
			get { return PropertyGetter(x => x.CSI_Status); }
			set { CARMDailyNoticeExtension.CSI_Status = value; }
		}

		public ZString CARMPort
		{
			get { return PropertyGetter(x => x.CSI_Procedure); }
			set { CARMDailyNoticeExtension.CSI_Procedure = value; }
		}

		public ZDecimal CARMTotal
		{
			get { return PropertyGetter(x => x.CSI_Value); }
			set { CARMDailyNoticeExtension.CSI_Value = value; }
		}

		T PropertyGetter<T>(Func<CARMDailyNoticeExtension, T> getter) where T : IZType
		{
			return CARMDailyNoticeExtensions.Any() ? getter(CARMDailyNoticeExtension) : default(T);
		}

		public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		public IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ Common.CA.CusSupportingInfoTypeList.Codes.CarmDailyNoticeExtension, typeof(CARMDailyNoticeExtension) }
			};
		}

		CARMDailyNoticeExtension CARMDailyNoticeExtension
		{
			get
			{
				var collection = CARMDailyNoticeExtensions;
				if (collection.Any())
				{
					return collection[0];
				}
				else
				{
					return collection.AddNew();
				}
			}
		}

		[ChildEditable]
		public CARMDailyNoticeExtensionCollection CARMDailyNoticeExtensions
		{
			get
			{
				if (carmDailyNoticeExsentions == null)
				{
					carmDailyNoticeExsentions = new CARMDailyNoticeExtensionCollection(this);
					carmDailyNoticeExsentions.Load();
					RegisterEditableChildObject(carmDailyNoticeExsentions);
				}

				return carmDailyNoticeExsentions;
			}
		}
		CARMDailyNoticeExtensionCollection carmDailyNoticeExsentions;

		#endregion
	}
}
