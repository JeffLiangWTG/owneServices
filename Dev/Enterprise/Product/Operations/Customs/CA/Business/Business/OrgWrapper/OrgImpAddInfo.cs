using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class OrgImpAddInfo : AutoCAOrgImpAddInfo, Integration.Customs.CA.IOrgImpAddInfo
	{
		public OrgImpAddInfo(ZPropertyInfoString parentPropertyInfo)
			: base(parentPropertyInfo.BizObj.Factory)
		{
			this.ParentPropertyInfo = parentPropertyInfo;
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				Deserialise();
			}
		}

		[ChildEditable(true)]
		public TradeChainPartnerCollection TradeChainPartners
		{
			get
			{
				if (tradeChainPartners == null)
				{
					tradeChainPartners = new TradeChainPartnerCollection(OrgHeader);
					tradeChainPartners.Load();
					RegisterEditableChildObject(tradeChainPartners);
				}
				return tradeChainPartners;
			}
		}
		TradeChainPartnerCollection tradeChainPartners;

		[ChildEditable(true)]
		public TradeChainPartnerSendingObjectCollection TradeChainPartnersToBeUpdated
		{
			get
			{
				return new TradeChainPartnerSendingObjectCollection(this);
			}
		}

		public bool IsTCPMessageAvailable => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.TCPMessage, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today);

		public OrgHeader OrgHeader => Parent?.OrgHeader;

		public OrgCountryData Parent
		{
			get
			{
				return (OrgCountryData)ParentPropertyInfo.BizObj;
			}
		}

		public static OrgImpAddInfo Get(OrgHeader organisation)
		{
			if (organisation == null)
			{
				return null;
			}
			else
			{
				var countryData = organisation.GetCountryData(Core.Constants.CountryCodes.Canada);
				organisation.RegisterEditableChildObject(countryData);
				return (OrgImpAddInfo)countryData.ImpAddInfo;
			}
		}

		public bool IsAscPasswordNotSpecified
		{
			get { return !ZO_AccountSecurityNumber.IsEmpty && ZO_AccountSecirityPassword.IsEmpty; }
		}

		public static string AscPasswordNotSpecifiedErrorText
		{
			get
			{
				return Res.GetString("913507e6-0be4-4270-8bd1-fbb568d6e6bf", "This Importer has an Account Security Code specified but the associated password is blank, the Importer's Account Security Number cannot be used unless there is an associated password.");
			}
		}

		public static string CFIAPaymentMethodNotSpecifiedErrorText
		{
			get
			{
				return Res.GetString("b3f1019b-36eb-4f4f-b982-d26bd6b5d35a", "This Importer does not specify the CFIA Payment Method, please see Organization -> Details -> Config tab for Canadian specific organization data.");
			}
		}

		public static string CFIAAccountNumberNotSpecifiedOnImporterErrorText
		{
			get
			{
				return Res.GetString("540cb26d-6dc2-4367-8f97-2eca05448f48", "This Importer does not specify the CFIA Account Number, please see Organization -> Details -> Config -> Registration Numbers / Codes.");
			}
		}

		public static string CFIAAccountNumberNotSpecifiedOnOrgProxyErrorText
		{
			get
			{
				return Res.GetString("2552f7a9-6df9-42d6-b4c8-25f4e5d106f0", "The branch/company organization proxy does not specify the CFIA Account Number. If an inspection is performed by CFIA, a fee may be charged. Failure to provide a CFIA with an account number will result in a rejection. Please see Organization -> Details -> Config -> Registration Numbers / Code.");
			}
		}

		public bool HasAccountSecurityNumber
		{
			get { return !ZO_AccountSecurityNumber.IsEmpty && !ZO_AccountSecirityPassword.IsEmpty; }
		}

		[ReadOnlyMember(nameof(NotGSTDirect))]
		public override ZBool ZO_IsGSTDirectAutoRated
		{
			get { return base.ZO_IsGSTDirectAutoRated; }
			set { base.ZO_IsGSTDirectAutoRated = value; }
		}

		public bool NotGSTDirect
		{
			get { return !ZO_IsGSTDirectPayment; }
		}

		[ReadOnlyMember(nameof(ZO_IsImporterDirectPayment))]
		public override ZBool ZO_IsGSTDirectPayment
		{
			get { return base.ZO_IsGSTDirectPayment; }
			set
			{
				base.ZO_IsGSTDirectPayment = value;
				if (!value)
				{
					ZO_IsGSTDirectAutoRated = false;
				}
			}
		}

		bool IsHighImporterAutoDutyDirectAmts_ReadOnly => !ZO_IsImporterDirectPayment;
		bool IsLVSImporterAutoDutyDirectAmts_ReadOnly => !ZO_IsLVSImporterDirectPayment;

		[ReadOnlyMember(nameof(IsHighImporterAutoDutyDirectAmts_ReadOnly))]
		[ResourceStringData("Organization|B3|ZO_IsHighImporterAutoDutyDirectAmts", Caption = "Auto Rate Duty && GST direct amounts", FullDescription = "Auto Rate Duty && GST direct amounts of High Value Shipments")]
		public override ZBool ZO_IsHighImporterAutoDutyDirectAmts { get => base.ZO_IsHighImporterAutoDutyDirectAmts; set => base.ZO_IsHighImporterAutoDutyDirectAmts = value; }

		[ResourceStringData("Organization|B3|ZO_IsImporterDirectPayment", Caption = "Importer Direct for High Value Shipments", FullDescription = "Importer Direct (Pay Duty and GST from Importer Posted Security using Broker Account Security Number)")]
		public override ZBool ZO_IsImporterDirectPayment
		{
			get { return base.ZO_IsImporterDirectPayment; }
			set
			{
				base.ZO_IsImporterDirectPayment = value;
				if (value)
				{
					ZO_IsGSTDirectPayment = false;
				}
				else
				{
					ZO_IsHighImporterAutoDutyDirectAmts = false;
				}
			}
		}

		public override ZBool ZO_IsLVSImporterDirectPayment
		{
			get => base.ZO_IsLVSImporterDirectPayment;
			set
			{
				base.ZO_IsLVSImporterDirectPayment = value;
				if (!value)
				{
					ZO_IsLVSImporterAutoDutyDirectAmts = false;
				}
			}
		}

		[ReadOnlyMember(nameof(IsLVSImporterAutoDutyDirectAmts_ReadOnly))]
		[ResourceStringData("Organization|B3|ZO_IsLVSImporterAutoDutyDirectAmts", Caption = "Auto Rate Duty && GST direct amounts", FullDescription = "Auto Rate Duty && GST direct amounts of Lower Value Shipments")]
		public override ZBool ZO_IsLVSImporterAutoDutyDirectAmts { get => base.ZO_IsLVSImporterAutoDutyDirectAmts; set => base.ZO_IsLVSImporterAutoDutyDirectAmts = value; }

		[List(nameof(Lookups) + "." + nameof(CAOrgImpAddInfoLookups.DelayIntervalTypeCodesListForHVS))]
		[BusinessObjectTestExclude]
		public override ZString ZO_HVSDelayIntervalTypeAutoSend
		{
			get { return base.ZO_HVSDelayIntervalTypeAutoSend.IsEmpty ? new ZString(DelayIntervalTypeCodes.Codes.Default) : base.ZO_HVSDelayIntervalTypeAutoSend; }
		}

		[ReadOnlyMember(nameof(ZO_HVSDelayIntervalAutoSend_ReadOnly))]
		public override ZInt ZO_HVSDelayIntervalAutoSend
		{
			get { return base.ZO_HVSDelayIntervalAutoSend; }
			set { base.ZO_HVSDelayIntervalAutoSend = value; }
		}

		ZBool ZO_HVSDelayIntervalAutoSend_ReadOnly
		{
			get
			{
				return ZO_HVSDelayIntervalTypeAutoSend == DelayIntervalTypeCodes.Codes.None
					|| ZO_HVSDelayIntervalTypeAutoSend == DelayIntervalTypeCodes.Codes.Default;
			}
		}

		[List(nameof(Lookups) + "." + nameof(CAOrgImpAddInfoLookups.DelayIntervalTypeCodesListForCON))]
		[BusinessObjectTestExclude]
		public override ZString ZO_CONDelayIntervalTypeAutoSend
		{
			get { return base.ZO_CONDelayIntervalTypeAutoSend.IsEmpty ? new ZString(DelayIntervalTypeCodes.Codes.Default) : base.ZO_CONDelayIntervalTypeAutoSend; }
		}

		[ReadOnlyMember(nameof(ZO_CONDelayIntervalAutoSend_ReadOnly))]
		public override ZInt ZO_CONDelayIntervalAutoSend
		{
			get { return base.ZO_CONDelayIntervalAutoSend; }
			set { base.ZO_CONDelayIntervalAutoSend = value; }
		}

		ZBool ZO_CONDelayIntervalAutoSend_ReadOnly
		{
			get
			{
				return ZO_CONDelayIntervalTypeAutoSend == DelayIntervalTypeCodes.Codes.None
					|| ZO_CONDelayIntervalTypeAutoSend == DelayIntervalTypeCodes.Codes.Default;
			}
		}

		[List(nameof(Lookups) + "." + nameof(CAOrgImpAddInfoLookups.DelayIntervalTypeCodesListForHVS))]
		[BusinessObjectTestExclude]
		public override ZString ZO_HVSDelayIntervalTypeFailSafe
		{
			get { return base.ZO_HVSDelayIntervalTypeFailSafe.IsEmpty ? new ZString(DelayIntervalTypeCodes.Codes.Default) : base.ZO_HVSDelayIntervalTypeFailSafe; }
		}

		[ReadOnlyMember(nameof(ZO_HVSDelayIntervalFailSafe_ReadOnly))]
		public override ZInt ZO_HVSDelayIntervalFailSafe
		{
			get { return base.ZO_HVSDelayIntervalFailSafe; }
			set { base.ZO_HVSDelayIntervalFailSafe = value; }
		}

		ZBool ZO_HVSDelayIntervalFailSafe_ReadOnly
		{
			get
			{
				return ZO_HVSDelayIntervalTypeFailSafe == DelayIntervalTypeCodes.Codes.None
					|| ZO_HVSDelayIntervalTypeFailSafe == DelayIntervalTypeCodes.Codes.Default;
			}
		}

		[List(nameof(Lookups) + "." + nameof(CAOrgImpAddInfoLookups.DelayIntervalTypeCodesListForCON))]
		[BusinessObjectTestExclude]
		public override ZString ZO_CONDelayIntervalTypeFailSafe
		{
			get { return base.ZO_CONDelayIntervalTypeFailSafe.IsEmpty ? new ZString(DelayIntervalTypeCodes.Codes.Default) : base.ZO_CONDelayIntervalTypeFailSafe; }
		}

		[ReadOnlyMember(nameof(ZO_CONDelayIntervalFailSafe_ReadOnly))]
		public override ZInt ZO_CONDelayIntervalFailSafe
		{
			get { return base.ZO_CONDelayIntervalFailSafe; }
			set { base.ZO_CONDelayIntervalFailSafe = value; }
		}

		ZBool ZO_CONDelayIntervalFailSafe_ReadOnly
		{
			get
			{
				return ZO_CONDelayIntervalTypeFailSafe == DelayIntervalTypeCodes.Codes.None
					|| ZO_CONDelayIntervalTypeFailSafe == DelayIntervalTypeCodes.Codes.Default;
			}
		}

		#region ZO_CFIAFeePaymentMethod

		[List(nameof(Lookups) + "." + nameof(CAOrgImpAddInfoLookups.CFIAPaymentMethods))]
		[BusinessObjectTestExclude]
		public override ZString ZO_CFIAFeePaymentMethod
		{
			get { return base.ZO_CFIAFeePaymentMethod.IsEmpty ? new ZString(CFIAPaymentMethods.Codes.RegistryDefault) : base.ZO_CFIAFeePaymentMethod; }
		}

		[MaxLength(AutoCAOrgImpAddInfo.Schema.ZO_CFIAFeePaymentMethodMaxLength)]
		public ZString ZO_EffectiveCFIAFeePaymentMethod
		{
			get { return ZO_CFIAFeePaymentMethod == CFIAPaymentMethods.Codes.RegistryDefault ? CACustomsDataRegistry.Instance.DefaultCFIAFeePaymentMethod.Value : ZO_CFIAFeePaymentMethod.ToString(); }
		}

		#endregion

		#region ZO_LVSInvoiceDetailCode

		[List(nameof(Lookups) + "." + nameof(CAOrgImpAddInfoLookups.LVSInvoiceDetailCodes))]
		[BusinessObjectTestExclude]
		public override ZString ZO_LVSInvoiceDetailCode
		{
			get { return base.ZO_LVSInvoiceDetailCode.IsEmpty ? new ZString(LVSInvoiceDetailCodes.Codes.RegistryDefault) : base.ZO_LVSInvoiceDetailCode; }
		}

		[MaxLength(AutoCAOrgImpAddInfo.Schema.ZO_LVSInvoiceDetailCodeMaxLength)]
		public ZString ZO_EffectiveLVSInvoiceDetailCode
		{
			get { return ZO_LVSInvoiceDetailCode == LVSInvoiceDetailCodes.Codes.RegistryDefault ? CACustomsDataRegistry.Instance.DefaultLVSInvoiceDetailsCode.Value : ZO_LVSInvoiceDetailCode.ToString(); }
		}

		#endregion

		#region ReadOnly member for ZO_IsConsolidateBy properties

		bool ZO_IsConsolidateBy_ReadOnly
		{
			get
			{
				if (!isCADMessageEnabled.HasValue)
				{
					isCADMessageEnabled = UniversalReferenceConstants.IsCarmR2;
				}
				return isCADMessageEnabled.Value;
			}
		}
		ZBool? isCADMessageEnabled;

		#endregion

		#region ZO_IsLVSConsolidated

		[ReadOnlyMember(nameof(ZO_IsConsolidateBy_ReadOnly))]
		public override ZBool ZO_IsLVSConsolidated
		{
			get { return ZO_IsConsolidateBy_ReadOnly ? ZBool.True : base.ZO_IsLVSConsolidated; }
			set { base.ZO_IsLVSConsolidated = value; }
		}

		#endregion

		#region ZO_IsConsolidateByBranch

		[ReadOnlyMember(nameof(ZO_IsConsolidateBy_ReadOnly))]
		public override ZBool ZO_IsConsolidateByBranch
		{
			get => ZO_IsConsolidateBy_ReadOnly ? ZBool.False : base.ZO_IsConsolidateByBranch;
			set => base.ZO_IsConsolidateByBranch = value;
		}

		#endregion

		#region ZO_IsConsolidateByBroker

		[ReadOnlyMember(nameof(ZO_IsConsolidateBy_ReadOnly))]
		public override ZBool ZO_IsConsolidateByBroker
		{
			get => ZO_IsConsolidateBy_ReadOnly ? ZBool.False : base.ZO_IsConsolidateByBroker;
			set => base.ZO_IsConsolidateByBroker = value;
		}

		#endregion

		#region ZO_IsConsolidateByProvinceofClearance

		[ReadOnlyMember(nameof(ZO_IsConsolidateBy_ReadOnly))]
		public override ZBool ZO_IsConsolidateByProvinceofClearance
		{
			get => ZO_IsConsolidateBy_ReadOnly ? ZBool.False : base.ZO_IsConsolidateByProvinceofClearance;
			set => base.ZO_IsConsolidateByProvinceofClearance = value;
		}

		#endregion

		#region ZO_DeferredNormalB3SendAction

		[List(nameof(Lookups) + "." + nameof(CAOrgImpAddInfoLookups.DeferedB3SendActionList))]
		[BusinessObjectTestExclude]
		public override ZString ZO_DeferredNormalB3SendAction
		{
			get { return base.ZO_DeferredNormalB3SendAction.IsEmpty ? new ZString(DeferredB3SendActionListOverride.Codes.RegistryDefault) : base.ZO_DeferredNormalB3SendAction; }
		}

		#endregion

		#region ZO_DeferredLowValueB3SendAction

		[List(nameof(Lookups) + "." + nameof(CAOrgImpAddInfoLookups.DeferedB3SendActionList))]
		[BusinessObjectTestExclude]
		public override ZString ZO_DeferredLowValueB3SendAction
		{
			get { return base.ZO_DeferredLowValueB3SendAction.IsEmpty ? new ZString(DeferredB3SendActionListOverride.Codes.RegistryDefault) : base.ZO_DeferredLowValueB3SendAction; }
		}

		#endregion

		#region ZO_ACROSSHighValueProductAuditAction

		[List(nameof(Lookups) + "." + nameof(CAOrgImpAddInfoLookups.ProductAuditActions))]
		[BusinessObjectTestExclude]
		public override ZString ZO_ACROSSHighValueProductAuditAction
		{
			get { return base.ZO_ACROSSHighValueProductAuditAction.IsEmpty ? new ZString(ProductAuditActions.Codes.RegistryDefault) : base.ZO_ACROSSHighValueProductAuditAction; }
		}

		#endregion

		#region ZO_ACROSSLowValueProductAuditAction

		[List(nameof(Lookups) + "." + nameof(CAOrgImpAddInfoLookups.ProductAuditActions))]
		[BusinessObjectTestExclude]
		public override ZString ZO_ACROSSLowValueProductAuditAction
		{
			get { return base.ZO_ACROSSLowValueProductAuditAction.IsEmpty ? new ZString(ProductAuditActions.Codes.RegistryDefault) : base.ZO_ACROSSLowValueProductAuditAction; }
		}

		#endregion

		#region ZO_B3HighValueProductAuditAction

		[List(nameof(Lookups) + "." + nameof(CAOrgImpAddInfoLookups.ProductAuditActions))]
		[BusinessObjectTestExclude]
		public override ZString ZO_B3HighValueProductAuditAction
		{
			get { return base.ZO_B3HighValueProductAuditAction.IsEmpty ? new ZString(ProductAuditActions.Codes.RegistryDefault) : base.ZO_B3HighValueProductAuditAction; }
		}

		#endregion

		#region ZO_B3LowValueProductAuditAction

		[List(nameof(Lookups) + "." + nameof(CAOrgImpAddInfoLookups.ProductAuditActions))]
		[BusinessObjectTestExclude]
		public override ZString ZO_B3LowValueProductAuditAction
		{
			get { return base.ZO_B3LowValueProductAuditAction.IsEmpty ? new ZString(ProductAuditActions.Codes.RegistryDefault) : base.ZO_B3LowValueProductAuditAction; }
		}

		#endregion

		public override ZBool ZO_IsCSAApprovedImporter
		{
			get => base.ZO_IsCSAApprovedImporter;
			set
			{
				var oldValue = base.ZO_IsCSAApprovedImporter;
				if (oldValue != value)
				{
					base.ZO_IsCSAApprovedImporter = value;
					if (!value)
					{
						ZO_AccountingTimeOption = ZString.Empty;
					}
					Validation.ValidateZO_AccountingTimeOption();
				}
			}
		}

		#region IOrgImpAddInfo Members

		public ZBool IsCSAApprovedImporter
		{
			get { return ZO_IsCSAApprovedImporter; }
		}

		public ZString CAAccountSecurityNumber
		{
			get { return base.ZO_AccountSecurityNumber; }
		}

		#endregion

		#region FreightPercentages

		[ChildEditable(true)]
		public FreightPercentageCollection FreightPercentages
		{
			get
			{
				if (fFreightPercentages == null)
				{
					fFreightPercentages = new FreightPercentageCollection(OrgHeader);
					fFreightPercentages.Load();
					RegisterEditableChildObject(fFreightPercentages);
				}
				return fFreightPercentages;
			}
		}

		FreightPercentageCollection fFreightPercentages;

		#endregion

		#region SafeFoodLicenses

		[ChildEditable(true)]
		public SafeFoodLicenseCollection SafeFoodLicenses
		{
			get
			{
				if (fSafeFoodLicenses == null)
				{
					fSafeFoodLicenses = new SafeFoodLicenseCollection(OrgHeader);
					fSafeFoodLicenses.Load();
					RegisterEditableChildObject(fSafeFoodLicenses);
				}

				return fSafeFoodLicenses;
			}
		}

		SafeFoodLicenseCollection fSafeFoodLicenses;

		#endregion

		#region
		[ChildEditable(true)]
		public CusBondDetailCollection BondDetails
		{
			get
			{
				if (fBondDetails == null)
				{
					fBondDetails = new CusBondDetailCollection(OrgHeader);
					fBondDetails.Load();
					RegisterEditableChildObject(fBondDetails);
				}
				return fBondDetails;
			}
		}
		CusBondDetailCollection fBondDetails;
		#endregion

		[ReadOnlyMember(nameof(ZO_PreventWarningOnSendingB3_ReadOnly))]
		public override ZBool ZO_PreventWarningOnSendingB3
		{
			get { return base.ZO_PreventWarningOnSendingB3; }
			set { base.ZO_PreventWarningOnSendingB3 = value; }
		}

		ZBool ZO_PreventWarningOnSendingB3_ReadOnly
		{
			get
			{
				return !ZO_PreventWarningOnSendingB3 && !ZO_IsLVSImporterDirectPayment && !ZO_IsGSTDirectPayment &&
					   !ZO_IsImporterDirectPayment;
			}
		}

		[ReadOnlyMember(nameof(ZO_AccountingTimeOption_ReadOnly))]
		[List(nameof(Lookups) + "+" + nameof(CAOrgImpAddInfoLookups.AccountingOptions))]
		public override ZString ZO_AccountingTimeOption
		{
			get { return base.ZO_AccountingTimeOption; }
			set { base.ZO_AccountingTimeOption = value; }
		}

		ZBool ZO_AccountingTimeOption_ReadOnly => !IsCSAApprovedImporter;
	}
}
