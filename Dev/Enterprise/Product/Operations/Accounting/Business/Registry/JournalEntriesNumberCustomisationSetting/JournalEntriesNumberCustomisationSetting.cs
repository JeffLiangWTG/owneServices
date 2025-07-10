using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;
using static Enterprise.Accounting.Registry.Business.TransactionNumberSequenceCustomisation;
using Res = Enterprise.Accounting.Business.Res;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class JournalEntriesNumberCustomisationSetting : RegistryBusinessObjectTemplate
	{
		public JournalEntriesNumberCustomisationSetting()
		{
		}

		public JournalEntriesNumberCustomisationSetting(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public static class Schema
		{
			public const string NumberRule = "NumberRule";
			public const string AllocationOption = "AllocationOption";
			public const string SequenceResetOption = "SequenceResetOption";
			public const string NumberSequenceCustomisations = "NumberSequenceCustomisations";
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateNumberRule();
			ValidateAllocationOption();
			ValidateSequenceResetOption();
			ValidateTotalLength();
		}

		public bool IsOptionGen => AllocationOption.Equals(AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN);

		#region Properties

		#region Number Rule

		ZString fNumberRule;

		[MaxLength(3)]
		[List("NumberRuleList")]
		public ZString NumberRule
		{
			get { return fNumberRule; }
			set
			{
				SetNonPersistentPropertyValue(NumberRuleInfo, ref fNumberRule, value);
				if (!IsValidationSuspended)
				{
					ValidateNumberRule();
				}
			}
		}

		public ZPropertyInfo NumberRuleInfo
		{
			get { return GetZPropertyInfo(Schema.NumberRule, "Number Rule"); }
		}

		void ValidateNumberRule()
		{
			NumberRuleInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(NumberRuleInfo);
			ListValidation.ErrorIfInvalidCode(NumberRuleInfo);

			if (NumberRule == AccountingConstants.JournalEntriesNumberCustomisationNumberRule.GRP
				&& AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroup
					.GetFallBackValueAtAllLevels(CurrentFallbackLevel.CompanyPK(true), Guid.Empty, Guid.Empty)
					.Cast<JournalEntriesClassificationGroup>()
					.Any(x => string.IsNullOrEmpty(x.GroupCode)))
			{
				NumberRuleInfo.AddError(ResString.GetMultilingualString("F682E195-9732-4A6D-8A43-565F171E384D", "Group Code in registry 'Journal Entries Classification Group' should be set when Number Rule is GRP."));
			}

			var sequenceNumberCustomisation = NumberSequenceCustomisations.Cast<JournalEntriesNumberCustomisation>().FirstOrDefault(x => x.ElementName == ElementNames.SequenceNumber);
			if (sequenceNumberCustomisation != null)
			{
				sequenceNumberCustomisation.ValidateLength();
			}
		}

		#endregion

		#region Allocation Option

		ZString fAllocationOption;

		[MaxLength(3)]
		[List("AllocationOptionList")]
		public ZString AllocationOption
		{
			get { return fAllocationOption; }
			set
			{
				SetNonPersistentPropertyValue(AllocationOptionInfo, ref fAllocationOption, value);
				if (!IsValidationSuspended)
				{
					ValidateAllocationOption();
				}
			}
		}

		public ZPropertyInfo AllocationOptionInfo
		{
			get { return GetZPropertyInfo(Schema.AllocationOption, "Allocation Option"); }
		}

		void ValidateAllocationOption()
		{
			AllocationOptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AllocationOptionInfo);
			ListValidation.ErrorIfInvalidCode(AllocationOptionInfo);
			if (AllocationOption == AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN
				&& !AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions.GetFallBackValueAtAllLevels(CurrentFallbackLevel?.CompanyPK(false) ?? Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AllocationOptionInfo.AddError(Res.GetString("E4E3D3A0-6274-4889-8BE5-C4B16A52BB1D", "Please ensure that 'Generate and Store Journal Entries for Posted Accounting Transactions' has been enabled."));
			}
		}

		#endregion

		#region Sequence Reset Option

		ZString fSequenceResetOption;

		[MaxLength(3)]
		[List("SequenceResetOptionList")]
		public ZString SequenceResetOption
		{
			get { return fSequenceResetOption; }
			set
			{
				SetNonPersistentPropertyValue(SequenceResetOptionInfo, ref fSequenceResetOption, value);
				if (!IsValidationSuspended)
				{
					ValidateSequenceResetOption();
				}
			}
		}

		public ZPropertyInfo SequenceResetOptionInfo
		{
			get { return GetZPropertyInfo(Schema.SequenceResetOption, "Sequence Reset Option"); }
		}

		void ValidateSequenceResetOption()
		{
			SequenceResetOptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(SequenceResetOptionInfo);
			ListValidation.ErrorIfInvalidCode(SequenceResetOptionInfo);

			if (SequenceResetOption == AccountingConstants.JournalEntriesNumberCustomisationSequenceResetOption.MONTH && !ShouldSequenceResetOptionChanged(CurrentFallbackLevel))
			{
				SequenceResetOptionInfo.AddError(Res.GetString("354FCAE1-9A34-4DE7-9FA3-8B15F76AA753", "'MTH' Sequence Reset Option can only be used in China login companies, please change the value back to 'YR'."));
			}
		}

		#endregion

		#region NumberSequenceCustomisations

		const int SequenceNumberOrder = 50;

		JournalEntriesNumberCustomisationCollection fNumberSequenceCustomisations;
		public JournalEntriesNumberCustomisationCollection NumberSequenceCustomisations
		{
			get
			{
				if (fNumberSequenceCustomisations == null)
				{
					fNumberSequenceCustomisations = new JournalEntriesNumberCustomisationCollection(this, CurrentFallbackLevel);
					fNumberSequenceCustomisations.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits;
					var accountingYearAsDigits = fNumberSequenceCustomisations.AddNew();
					accountingYearAsDigits.ElementName = TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits;
					accountingYearAsDigits.Code = new ZString("4");
					fNumberSequenceCustomisations.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsLetter;
					fNumberSequenceCustomisations.AddNew().ElementName = JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.CalendarMonthAs2Digits;
					fNumberSequenceCustomisations.AddNew().ElementName = JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.CalendarMonthAsLetter;
					var calendarYearAsDigits = fNumberSequenceCustomisations.AddNew();
					calendarYearAsDigits.ElementName = JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.CalendarYearAsDigits;
					calendarYearAsDigits.Code = new ZString("4");
					fNumberSequenceCustomisations.AddNew().ElementName = JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.CalendarYearAsLetter;
					fNumberSequenceCustomisations.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.CustomElement1;
					fNumberSequenceCustomisations.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.CustomElement2;
					fNumberSequenceCustomisations.AddNew().ElementName = JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.JournalEntriesBranchCode;
					fNumberSequenceCustomisations.AddNew().ElementName = JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.JournalEntriesClassificationGroupCode;
					fNumberSequenceCustomisations.AddNew().ElementName = JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.JournalEntriesDepartmentCode;
					var sequenceNumber = fNumberSequenceCustomisations.AddNew();
					sequenceNumber.ElementName = TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber;
					sequenceNumber.Order = SequenceNumberOrder;
					sequenceNumber.Include = true;
					sequenceNumber.Length = 10;
					sequenceNumber.Fountain = true;
					fNumberSequenceCustomisations.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.TransactionTypePrefix;
					fNumberSequenceCustomisations.AddNew().ElementName = JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.JournalEntriesClassificationGroupCodePrefix;

					RegisterEditableChildObject(fNumberSequenceCustomisations);
				}
				fNumberSequenceCustomisations.Parent = this;
				fNumberSequenceCustomisations.CurrentFallbackLevel = this.CurrentFallbackLevel;
				return fNumberSequenceCustomisations;
			}
		}

		ZXmlSerializer fNumberSequenceCustomisationsSerialiser;
		ZXmlSerializer NumberSequenceCustomisationsSerialiser
		{
			get
			{
				return fNumberSequenceCustomisationsSerialiser ?? (fNumberSequenceCustomisationsSerialiser = ZXmlSerializer.New(typeof(JournalEntriesNumberCustomisationCollection)));
			}
		}

		void ValidateTotalLength()
		{
			ClearRowNotifications();
			if (NumberSequenceCustomisations.TotalLength > 40)
			{
				AddRowError(ResString.GetMultilingualString("758DEAE0-ADA6-4295-A9FE-83ADC2A7921F", "Total length of all data elements must not exceed 40 characters"));
			}
		}

		#endregion

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JournalEntriesNumberCustomisationSetting(fallbackLevel);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var journalEntriesNumberCustomisationClone = (JournalEntriesNumberCustomisationSetting)clone;
			if (fNumberSequenceCustomisations != null)
			{
				journalEntriesNumberCustomisationClone.fNumberSequenceCustomisations = (JournalEntriesNumberCustomisationCollection)fNumberSequenceCustomisations.Clone(journalEntriesNumberCustomisationClone.CurrentFallbackLevel, journalEntriesNumberCustomisationClone.Factory);
				journalEntriesNumberCustomisationClone.fNumberSequenceCustomisations.CurrentFallbackLevel = fNumberSequenceCustomisations.CurrentFallbackLevel;
				journalEntriesNumberCustomisationClone.fNumberSequenceCustomisations.Parent = fNumberSequenceCustomisations.Parent;
				journalEntriesNumberCustomisationClone.RegisterEditableChildObject(journalEntriesNumberCustomisationClone.fNumberSequenceCustomisations);
			}
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			NumberRule = AccountingConstants.JournalEntriesNumberCustomisationNumberRule.ALL;
			AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.NON;
			SequenceResetOption = AccountingConstants.JournalEntriesNumberCustomisationSequenceResetOption.YEAR;
		}

		#region Lists

		CodeDescriptionPairList fNumberRuleList;
		public CodeDescriptionPairList NumberRuleList
		{
			get
			{
				if (fNumberRuleList == null)
				{
					fNumberRuleList = new CodeDescriptionPairList();
					fNumberRuleList.AddPair(AccountingConstants.JournalEntriesNumberCustomisationNumberRule.ALL, Res.GetString("C55E3C9B-A20F-48A9-8793-C7EB861F8062", "One Number Sequence for All Transaction Types"));
					fNumberRuleList.AddPair(AccountingConstants.JournalEntriesNumberCustomisationNumberRule.GRP, Res.GetString("46978EB8-9017-4350-BC8A-3B3CFFB69687", "Separate Number Sequence Per Journal Entries Classification Group"));
					fNumberRuleList.AddPair(AccountingConstants.JournalEntriesNumberCustomisationNumberRule.TRN, Res.GetString("B2147B06-4787-4D3C-86D3-64D0AD088D32", "Separate Number Sequence Per Transaction Type"));
				}
				return fNumberRuleList;
			}
		}

		CodeDescriptionPairList fAllocationOptionList;
		public CodeDescriptionPairList AllocationOptionList
		{
			get
			{
				if (fAllocationOptionList == null)
				{
					fAllocationOptionList = new CodeDescriptionPairList();
					fAllocationOptionList.AddPair(AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.NON, Res.GetString("B4726401-18E4-42D6-BDDF-307E5E8B3539", "No Allocation"));
					fAllocationOptionList.AddPair(AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN, Res.GetString("B16B8A3D-AA1C-4EE9-8A77-30319616D132", "Allocate Number when Journal Entries are Generated"));
				}
				return fAllocationOptionList;
			}
		}

		CodeDescriptionPairList fSequenceResetOptionList;
		public CodeDescriptionPairList SequenceResetOptionList
		{
			get
			{
				if (fSequenceResetOptionList == null)
				{
					fSequenceResetOptionList = new CodeDescriptionPairList();
					fSequenceResetOptionList.AddPair(AccountingConstants.JournalEntriesNumberCustomisationSequenceResetOption.YEAR, Res.GetString("9431E798-FC11-4129-BF4D-4896A823A187", "Reset from 1 on yearly basis"));
					fSequenceResetOptionList.AddPair(AccountingConstants.JournalEntriesNumberCustomisationSequenceResetOption.MONTH, Res.GetString("5DA1327E-8518-42F1-AB2E-755C1C409BF8", "Reset from 1 on monthly basis"));
				}
				return fSequenceResetOptionList;
			}
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.NumberRule, NumberRule);
			writer.WriteElementString(Schema.AllocationOption, AllocationOption);
			writer.WriteElementString(Schema.SequenceResetOption, SequenceResetOption);
			NumberSequenceCustomisationsSerialiser.Serialize(writer, NumberSequenceCustomisations);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			NumberRule = reader.ReadElementString(Schema.NumberRule);
			AllocationOption = reader.ReadElementString(Schema.AllocationOption);
			SequenceResetOption = reader.ReadElementString(Schema.SequenceResetOption);
			fNumberSequenceCustomisations = (JournalEntriesNumberCustomisationCollection)NumberSequenceCustomisationsSerialiser.Deserialize(reader);
			fNumberSequenceCustomisations.Parent = this;
			fNumberSequenceCustomisations.CurrentFallbackLevel = this.CurrentFallbackLevel;
			RegisterEditableChildObject(fNumberSequenceCustomisations);
		}

		#endregion

		public bool ShouldSequenceResetOptionChanged(FallbackLevel fallbackLevel)
		{
			if (fallbackLevel == null)
			{
				return false;
			}

			var factory = new BusinessObjectFactory();
			var fallbackCompany = factory.Load<GlbCompany>(fallbackLevel.CompanyPK(false));
			return fallbackCompany.GC_RN_NKCountryCode == Constants.CountryCodes.China;
		}
	}
}
