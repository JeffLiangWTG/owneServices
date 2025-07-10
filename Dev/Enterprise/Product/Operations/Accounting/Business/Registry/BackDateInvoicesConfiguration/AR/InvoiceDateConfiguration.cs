using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class InvoiceDateConfiguration : RegistryBusinessObjectTemplate, IInvoiceDateConfiguration
	{
		#region Schema

		public abstract class Schema
		{
			public const string JobType = "JobType";
			public const string DirectionCode = "DirectionCode";
			public const string Mode = "Mode";
			public const string BrokerCode = "BrokerCode";
			public const string SignificantDateCode = "SignificantDateCode";
			public const string PriorClosedPeriod = "PriorClosedPeriod";
			public const string PriorOpenPeriod = "PriorOpenPeriod";
			public const string CurrentPeriod = "CurrentPeriod";
			public const string FuturePeriod = "FuturePeriod";
			public const string Override = "Override";
			public const string Today = "Today";
			public const string ReversalRule = "ReversalRule";
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new InvoiceDateConfiguration();
		}

		public BusinessObjectCollection ParentCollection
		{
			get { return GetParentCollection(this, typeof(InvoiceDateConfigurationCollection)); }
		}

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateJobType();
			ValidateDirectionCode();
			ValidateMode();
			ValidateBrokerCode();
			ValidateSignificantDateCode();
			ValidatePriorClosedPeriod();
			ValidatePriorOpenPeriod();
			ValidateCurrentPeriod();
			ValidateFuturePeriod();
			ValidateOverride();
			ValidateToday();
			ValidateReversalRule();
		}

		public InvoiceDateConfigurationValidation Validation
		{
			get { return new InvoiceDateConfigurationValidation(this); }
		}

		#endregion

		#region Default Values

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			JobType = InvoiceDateConfigurationLookups.JobTypeAdditionalCodes.All;
			SignificantDateCode = InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate;
			CurrentPeriod = InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.InvoiceAddDate;
			ReversalRule = InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules;
		}

		#endregion

		#region Properties

		#region JobType

		[MaxLength(3)]
		[List("JobTypeList")]
		public ZString JobType
		{
			get { return fJobType; }
			set
			{
				CheckMaximumLength(JobTypeInfo, value);
				SetNonPersistentPropertyValue(JobTypeInfo, ref fJobType, value);
				if (!IsValidationSuspended)
				{
					ValidateJobType();
				}
			}
		}

		public virtual ZPropertyInfo JobTypeInfo
		{
			get { return GetZPropertyInfo(Schema.JobType); }
		}

		public CodeDescriptionPairList JobTypeList
		{
			get { return InvoiceDateConfigurationLookups.JobTypeList; }
		}

		public void ValidateJobType()
		{
			JobTypeInfo.ClearAllNotifications();

			Validation.ValidateJobType();
		}

		ZString fJobType;

		#endregion

		#region Direction

		[MaxLength(3)]
		[List("DirectionList")]
		public ZString DirectionCode
		{
			get { return DirectionCodeInfo.ReadOnly ? ZString.Empty : fDirectionCode; }
			set
			{
				CheckMaximumLength(DirectionCodeInfo, value);
				SetNonPersistentPropertyValue(DirectionCodeInfo, ref fDirectionCode, value);
				if (!IsValidationSuspended)
				{
					ValidateDirectionCode();
				}
			}
		}

		public virtual ZPropertyInfo DirectionCodeInfo
		{
			get { return GetZPropertyInfo(Schema.DirectionCode); }
		}

		public bool DirectionCode_ReadOnly
		{
			get
			{
				return ReadOnlyHelper.DirectionCode_ReadOnly;
			}
		}

		public CodeDescriptionPairList DirectionList
		{
			get { return InvoiceDateConfigurationLookups.DirectionList; }
		}

		public void ValidateDirectionCode()
		{
			DirectionCodeInfo.ClearAllNotifications();

			Validation.ValidateDirectionCode();
		}

		ZString fDirectionCode;

		#endregion

		#region Mode

		[MaxLength(3)]
		[List("ModeList")]
		public ZString Mode
		{
			get { return ModeInfo.ReadOnly ? ZString.Empty : fMode; }
			set
			{
				CheckMaximumLength(ModeInfo, value);
				SetNonPersistentPropertyValue(ModeInfo, ref fMode, value);
				if (!IsValidationSuspended)
				{
					ValidateMode();
				}
			}
		}

		public virtual ZPropertyInfo ModeInfo
		{
			get { return GetZPropertyInfo(Schema.Mode); }
		}

		public bool Mode_ReadOnly
		{
			get
			{
				return ReadOnlyHelper.Mode_ReadOnly;
			}
		}

		public CodeDescriptionPairList ModeList
		{
			get { return InvoiceDateConfigurationLookups.ModeList; }
		}

		public void ValidateMode()
		{
			ModeInfo.ClearAllNotifications();

			Validation.ValidateMode();
		}

		ZString fMode;

		#endregion

		#region SignificantDate

		[MaxLength(3)]
		[List("SignificantDateList")]
		public ZString SignificantDateCode
		{
			get { return significantDateCode; }
			set
			{
				CheckMaximumLength(SignificantDateCodeInfo, value);
				SetNonPersistentPropertyValue(SignificantDateCodeInfo, ref significantDateCode, value);

				if (value == InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate)
				{
					PriorClosedPeriod = ZString.Empty;
					PriorOpenPeriod = ZString.Empty;
					FuturePeriod = ZString.Empty;
				}

				if (!IsValidationSuspended)
				{
					ValidateSignificantDateCode();
				}
			}
		}

		public virtual ZPropertyInfo SignificantDateCodeInfo
		{
			get { return GetZPropertyInfo(Schema.SignificantDateCode); }
		}

		public void ValidateSignificantDateCode()
		{
			SignificantDateCodeInfo.ClearAllNotifications();

			Validation.ValidateSignificantDateCode();
		}

		ZString significantDateCode;

		public ZString SignificantDateDescription
		{
			get
			{
				ICodeDescription significantDate = InvoiceDateConfigurationLookups.CompleteSignificantDateList[SignificantDateCode];
				return significantDate != null ? significantDate.Description : "";
			}
		}

		public ZPropertyInfo SignificantDateDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(SignificantDateDescription)); }
		}

		public CodeDescriptionPairList SignificantDateList
		{
			get { return InvoiceDateConfigurationLookups.SignificantDateList; }
		}

		#endregion

		#region Broker

		[MaxLength(3)]
		[List("BrokerList")]
		public ZString BrokerCode
		{
			get { return BrokerCodeInfo.ReadOnly ? ZString.Empty : fBrokerCode; }
			set
			{
				CheckMaximumLength(BrokerCodeInfo, value);
				SetNonPersistentPropertyValue(BrokerCodeInfo, ref fBrokerCode, value);
				if (!IsValidationSuspended)
				{
					ValidateBrokerCode();
				}
			}
		}

		public virtual ZPropertyInfo BrokerCodeInfo
		{
			get { return GetZPropertyInfo(Schema.BrokerCode); }
		}

		public bool BrokerCode_ReadOnly
		{
			get
			{
				return ReadOnlyHelper.BrokerCode_ReadOnly;
			}
		}

		public CodeDescriptionPairList BrokerList
		{
			get { return InvoiceDateConfigurationLookups.BrokerList; }
		}

		public void ValidateBrokerCode()
		{
			BrokerCodeInfo.ClearAllNotifications();

			Validation.ValidateBrokerCode();
		}

		ZString fBrokerCode;

		#endregion

		#region PriorClosedPeriod

		[MaxLength(3)]
		[List("PriorClosedPeriodList")]
		public ZString PriorClosedPeriod
		{
			get { return priorClosedPeriod; }
			set
			{
				CheckMaximumLength(PriorClosedPeriodInfo, value);
				SetNonPersistentPropertyValue(PriorClosedPeriodInfo, ref priorClosedPeriod, value);
				if (!IsValidationSuspended)
				{
					ValidatePriorClosedPeriod();
				}
			}
		}

		public virtual ZPropertyInfo PriorClosedPeriodInfo
		{
			get { return GetZPropertyInfo(Schema.PriorClosedPeriod); }
		}

		public CodeDescriptionPairList PriorClosedPeriodList
		{
			get { return InvoiceDateConfigurationLookups.PriorClosedPeriodList; }
		}

		public bool PriorClosedPeriod_ReadOnly
		{
			get
			{
				return ReadOnlyHelper.PriorClosedPeriod_ReadOnly;
			}
		}

		public void ValidatePriorClosedPeriod()
		{
			PriorClosedPeriodInfo.ClearAllNotifications();

			Validation.ValidatePriorClosedPeriod();
		}

		ZString priorClosedPeriod;

		#endregion

		#region PriorOpenPeriod

		[MaxLength(3)]
		[List("PriorOpenPeriodList")]
		public ZString PriorOpenPeriod
		{
			get { return priorOpenPeriod; }
			set
			{
				CheckMaximumLength(PriorOpenPeriodInfo, value);
				SetNonPersistentPropertyValue(PriorOpenPeriodInfo, ref priorOpenPeriod, value);
				if (!IsValidationSuspended)
				{
					ValidatePriorOpenPeriod();
				}
			}
		}

		public virtual ZPropertyInfo PriorOpenPeriodInfo
		{
			get { return GetZPropertyInfo(Schema.PriorOpenPeriod); }
		}

		public CodeDescriptionPairList PriorOpenPeriodList
		{
			get { return InvoiceDateConfigurationLookups.PriorOpenPeriodList; }
		}

		public bool PriorOpenPeriod_ReadOnly
		{
			get
			{
				return ReadOnlyHelper.PriorOpenPeriod_ReadOnly;
			}
		}

		public void ValidatePriorOpenPeriod()
		{
			PriorOpenPeriodInfo.ClearAllNotifications();

			Validation.ValidatePriorOpenPeriod();
		}

		ZString priorOpenPeriod;

		#endregion

		#region CurrentPeriod

		[MaxLength(3)]
		[List("CurrentPeriodList")]
		public ZString CurrentPeriod
		{
			get { return currentPeriod; }
			set
			{
				CheckMaximumLength(CurrentPeriodInfo, value);
				SetNonPersistentPropertyValue(CurrentPeriodInfo, ref currentPeriod, value);
				if (!IsValidationSuspended)
				{
					ValidateCurrentPeriod();
				}
			}
		}

		public virtual ZPropertyInfo CurrentPeriodInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentPeriod); }
		}

		public CodeDescriptionPairList CurrentPeriodList
		{
			get { return InvoiceDateConfigurationLookups.CurrentPeriodList; }
		}

		public void ValidateCurrentPeriod()
		{
			CurrentPeriodInfo.ClearAllNotifications();

			Validation.ValidateCurrentPeriod();
		}

		ZString currentPeriod;

		#endregion

		#region FuturePeriod

		[MaxLength(3)]
		[List("FuturePeriodList")]
		public ZString FuturePeriod
		{
			get { return futurePeriod; }
			set
			{
				CheckMaximumLength(FuturePeriodInfo, value);
				SetNonPersistentPropertyValue(FuturePeriodInfo, ref futurePeriod, value);
				if (!IsValidationSuspended)
				{
					ValidateFuturePeriod();
				}
			}
		}

		public virtual ZPropertyInfo FuturePeriodInfo
		{
			get { return GetZPropertyInfo(Schema.FuturePeriod); }
		}

		public CodeDescriptionPairList FuturePeriodList
		{
			get { return InvoiceDateConfigurationLookups.FuturePeriodList; }
		}

		public bool FuturePeriod_ReadOnly
		{
			get
			{
				return ReadOnlyHelper.FuturePeriod_ReadOnly;
			}
		}

		public void ValidateFuturePeriod()
		{
			FuturePeriodInfo.ClearAllNotifications();

			Validation.ValidateFuturePeriod();
		}

		ZString futurePeriod;

		#endregion

		#region Override

		public ZBool Override
		{
			get { return _override; }
			set
			{
				SetNonPersistentPropertyValue(OverrideInfo, ref _override, value);
				if (!IsValidationSuspended)
				{
					ValidateOverride();
				}
			}
		}

		public virtual ZPropertyInfo OverrideInfo
		{
			get { return GetZPropertyInfo(Schema.Override); }
		}

		public void ValidateOverride()
		{
			OverrideInfo.ClearAllNotifications();

			Validation.ValidateOverride();
		}

		ZBool _override;

		#endregion

		#region Today

		public ZBool Today
		{
			get { return today; }
			set
			{
				SetNonPersistentPropertyValue(TodayInfo, ref today, value);
				if (!IsValidationSuspended)
				{
					ValidateToday();
				}
			}
		}

		public virtual ZPropertyInfo TodayInfo
		{
			get { return GetZPropertyInfo(Schema.Today); }
		}

		public void ValidateToday()
		{
			TodayInfo.ClearAllNotifications();

			Validation.ValidateToday();
		}

		ZBool today;

		#endregion

		#region ReversalRule

		[MaxLength(3)]
		[List("ReversalRuleList")]
		public ZString ReversalRule
		{
			get { return reversalRule; }
			set
			{
				CheckMaximumLength(ReversalRuleInfo, value);
				SetNonPersistentPropertyValue(ReversalRuleInfo, ref reversalRule, value);

				if (!IsValidationSuspended)
				{
					ValidateReversalRule();
				}
			}
		}

		public virtual ZPropertyInfo ReversalRuleInfo
		{
			get { return GetZPropertyInfo(Schema.ReversalRule); }
		}

		public void ValidateReversalRule()
		{
			ReversalRuleInfo.ClearAllNotifications();

			Validation.ValidateReversalRule();
		}

		ZString reversalRule;

		public CodeDescriptionPairList ReversalRuleList
		{
			get { return InvoiceDateConfigurationLookups.ReversalRuleList; }
		}

		#endregion

		#endregion

		#region InvoiceDateConfigurationLookups

		public InvoiceDateConfigurationLookups InvoiceDateConfigurationLookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = GetNewLookups();
				}
				return fLookups;
			}
		}

		protected virtual InvoiceDateConfigurationLookups GetNewLookups()
		{
			return new InvoiceDateConfigurationLookups(this);
		}

		InvoiceDateConfigurationLookups fLookups;

		#endregion

		#region Read Only Helper

		public InvoiceDateConfigurationReadOnly ReadOnlyHelper
		{
			get
			{
				if (fReadOnlyHelper == null)
				{
					fReadOnlyHelper = new InvoiceDateConfigurationReadOnly(this);
				}

				return fReadOnlyHelper;
			}
		}

		InvoiceDateConfigurationReadOnly fReadOnlyHelper;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.JobType, JobType);
			writer.WriteElementString(Schema.DirectionCode, DirectionCode);
			writer.WriteElementString(Schema.Mode, Mode);
			writer.WriteElementString(Schema.SignificantDateCode, SignificantDateCode);
			writer.WriteElementString(Schema.BrokerCode, BrokerCode);
			writer.WriteElementString(Schema.PriorClosedPeriod, PriorClosedPeriod);
			writer.WriteElementString(Schema.PriorOpenPeriod, PriorOpenPeriod);
			writer.WriteElementString(Schema.CurrentPeriod, CurrentPeriod);
			writer.WriteElementString(Schema.FuturePeriod, FuturePeriod);
			writer.WriteElementString(Schema.Override, Override.ToString());
			writer.WriteElementString(Schema.Today, Today.ToString());
			writer.WriteElementString(Schema.ReversalRule, ReversalRule);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			JobType = reader.ReadElementString(Schema.JobType);
			DirectionCode = reader.ReadElementString(Schema.DirectionCode);
			Mode = reader.ReadElementString(Schema.Mode);
			SignificantDateCode = reader.ReadElementString(Schema.SignificantDateCode);
			BrokerCode = reader.ReadElementString(Schema.BrokerCode);

			if (BrokerCode.IsEmpty && !BrokerCodeInfo.ReadOnly)
			{
				BrokerCode = InvoiceDateConfigurationLookups.BrokerCodes.All;
			}

			PriorClosedPeriod = reader.ReadElementString(Schema.PriorClosedPeriod);
			PriorOpenPeriod = reader.ReadElementString(Schema.PriorOpenPeriod);
			CurrentPeriod = reader.ReadElementString(Schema.CurrentPeriod);
			FuturePeriod = reader.ReadElementString(Schema.FuturePeriod);
			Override = reader.ReadElementStringAsZBool(Schema.Override);
			Today = reader.ReadElementStringAsZBool(Schema.Today);
			ReversalRule = reader.ReadElementString(Schema.ReversalRule);

			if (ReversalRule.IsEmpty && !ReversalRuleInfo.ReadOnly)
			{
				ReversalRule = InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules;
			}
		}

		#endregion
	}
}