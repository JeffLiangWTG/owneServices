using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using Res = Enterprise.Accounting.Business.Res;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class GLJournalApprovalThreshold : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Type = "Type";
			public const string GLAccount = "GLAccount";
			public const string ReportSection = "ReportSection";
			public const string Description = "Description";
		}

		#endregion

		public GLJournalApprovalThreshold()
			: base()
		{
		}

		public GLJournalApprovalThreshold(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var gLJournalApprovalThresholdClone = (GLJournalApprovalThreshold)clone;
			if (authorisationSettings != null)
			{
				gLJournalApprovalThresholdClone.authorisationSettings = (PaymentThreeLevelAuthorisationSettingsCollection)authorisationSettings.Clone(gLJournalApprovalThresholdClone.CurrentFallbackLevel, gLJournalApprovalThresholdClone.Factory);
				gLJournalApprovalThresholdClone.authorisationSettings.CurrentFallbackLevel = authorisationSettings.CurrentFallbackLevel;
				gLJournalApprovalThresholdClone.RegisterEditableChildObject(gLJournalApprovalThresholdClone.authorisationSettings);
				gLJournalApprovalThresholdClone.SetupAuthorizationSettingCollectionAccessibility();
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new GLJournalApprovalThreshold(fallbackLevel, null);
		}

		public BusinessObjectCollection ParentCollection
		{
			get { return GetParentCollection(this, typeof(GLJournalApprovalThresholdCollection)); }
		}

		#region Bound Properties

		#region Type

		[MaxLength(3)]
		[List("TypeList")]
		public ZString Type
		{
			get
			{
				return type;
			}
			set
			{
				CheckMaximumLength(TypeInfo, value);
				SetNonPersistentPropertyValue(TypeInfo, ref type, value);
				if (Type != TypeCodes.GLAccount)
				{
					GLAccount = ZGuid.Empty;
				}
				if (Type != TypeCodes.ReportSection)
				{
					ReportSection = ZString.Empty;
				}
				if (!IsValidationSuspended)
				{
					ValidateType();
				}
				SetupAuthorizationSettingCollectionAccessibility();
			}
		}
		ZString type;

		public ZPropertyInfo TypeInfo
		{
			get { return GetZPropertyInfo(Schema.Type); }
		}

		public CodeDescriptionPairList TypeList
		{
			get
			{
				if (typeList == null)
				{
					typeList = new CodeDescriptionPairList();
					typeList.AddPair(TypeCodes.All, TypeDescription.All);
					typeList.AddPair(TypeCodes.GLAccount, TypeDescription.GLAccount);
					typeList.AddPair(TypeCodes.ReportSection, TypeDescription.ReportSection);
					typeList.AddPair(TypeCodes.AnyChanges, TypeDescription.AnyChanges);
				}

				return typeList;
			}
		}
		CodeDescriptionPairList typeList;

		public static class TypeCodes
		{
			public const string All = "ALL";
			public const string GLAccount = "GLA";
			public const string ReportSection = "RSN";
			public const string AnyChanges = "ANY";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "code description only")]
		public static class TypeDescription
		{
			public const string All = "ALL GL Accounts";
			public const string GLAccount = "GL Account";
			public const string ReportSection = "Report Section";
			public const string AnyChanges = "Any Changes";
		}

		#endregion

		#region GLAccount

		[List("GLAccounts")]
		public ZGuid GLAccount
		{
			get { return glAccount; }
			set
			{
				SetNonPersistentPropertyValue(GLAccountInfo, ref glAccount, value);
				if (!IsValidationSuspended)
				{
					ValidateGLAccount();
				}
				AuthorisationSettings.RefreshBinding();
			}
		}
		ZGuid glAccount;

		public ZPropertyInfo GLAccountInfo
		{
			get { return GetZPropertyInfo(Schema.GLAccount); }
		}

		public bool GLAccount_ReadOnly
		{
			get { return Type != TypeCodes.GLAccount; }
		}

		public AccGLHeaderCollection GLAccounts
		{
			get
			{
				var accountTypeFilter = new ZQuery(AccGLHeaderSchema.AG_AccountType, AccountTypeComboBoxConstants.BalanceSheetAccount);
				accountTypeFilter.AddToFilter(JoinCondition.Or, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, AccountTypeComboBoxConstants.ProfitAndLossAccount);

				var collection = new AccGLHeaderCollection(CurrentFactory, accountTypeFilter);
				collection.SetOverrideNotificationWhenAdditionalFilterNotMet(GLAccountsAdditionalFilterInvalidMessage);

				return collection;
			}
		}

		internal static MultilingualString GLAccountsAdditionalFilterInvalidMessage
		{
			get { return ResString.GetMultilingualString("A68B1B4E-3E5A-44FF-A202-32875DFDC5C0", "You can only select a BSH or P&L Account."); }
		}

		#endregion

		#region ReportSection

		[MaxLength(2)]
		[List("ReportSectionList")]
		public ZString ReportSection
		{
			get { return reportSection; }
			set
			{
				CheckMaximumLength(ReportSectionInfo, value);
				SetNonPersistentPropertyValue(ReportSectionInfo, ref reportSection, value);
				if (!IsValidationSuspended)
				{
					ValidateReportSection();
				}
				AuthorisationSettings.RefreshBinding();
			}
		}
		ZString reportSection;

		public ZPropertyInfo ReportSectionInfo
		{
			get { return GetZPropertyInfo(Schema.ReportSection); }
		}

		public bool ReportSection_ReadOnly
		{
			get { return Type != TypeCodes.ReportSection; }
		}

		public CodeDescriptionPairList ReportSectionList
		{
			get
			{
				if (reportSectionList == null)
				{
					reportSectionList = AccGLHeader.BuildSectionTypeList();
				}
				return reportSectionList;
			}
		}
		CodeDescriptionPairList reportSectionList;

		#endregion

		#region Description

		[MaxLength(256)]
		public ZString Description
		{
			get
			{
				var description = ZString.Empty;
				if (Type == TypeCodes.All)
				{
					description = Res.GetString("23abfe5c-872d-4f2e-8e18-94ea4b526428", "All GL Accounts");
				}
				else if (GLAccount.IsValid)
				{
					if (!GLAccountInfo.HasErrors())
					{
						var glAccount = CurrentFactory.Load<AccGLHeader>(GLAccount);
						if (glAccount != null)
						{
							description = glAccount.AG_DescriptionMultilingual;
						}
					}
				}
				else if (!ReportSection.IsEmpty)
				{
					if (!ReportSectionInfo.HasErrors())
					{
						description = ReportSectionList.GetDescriptionFromCode(ReportSection);
					}
				}

				return description;
			}
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		#endregion

		#region AuthorisationSettings

		public PaymentThreeLevelAuthorisationSettingsCollection AuthorisationSettings
		{
			get
			{
				if (authorisationSettings == null)
				{
					authorisationSettings = new PaymentThreeLevelAuthorisationSettingsCollection();

					RegisterEditableChildObject(authorisationSettings);
				}
				authorisationSettings.CurrentFallbackLevel = CurrentFallbackLevel;

				return authorisationSettings;
			}
		}

		PaymentThreeLevelAuthorisationSettingsCollection authorisationSettings;

		ZXmlSerializer AuthorisationSettingsSerialiser
		{
			get { return authorisationSettingsSerialiser ?? (authorisationSettingsSerialiser = ZXmlSerializer.New(typeof(PaymentThreeLevelAuthorisationSettingsCollection))); }
		}

		ZXmlSerializer authorisationSettingsSerialiser;

		void SetupAuthorizationSettingCollectionAccessibility()
		{
			if (Type == TypeCodes.AnyChanges)
			{
				AuthorisationSettings.RemoveAndDeleteAll();
				AuthorisationSettings.SetReadOnlyIncludingChildren(true);
			}
			else
			{
				AuthorisationSettings.SetReadOnlyIncludingChildren(false);
			}
			AuthorisationSettings.RefreshBinding();
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateType();
			ValidateGLAccount();
			ValidateReportSection();
			Validation.ValidateRow();
		}

		public void ValidateType()
		{
			TypeInfo.ClearAllNotifications();
			Validation.ValidateType();
		}

		public void ValidateGLAccount()
		{
			GLAccountInfo.ClearAllNotifications();
			Validation.ValidateGLAccount();
		}

		public void ValidateReportSection()
		{
			ReportSectionInfo.ClearAllNotifications();
			Validation.ValidateReportSection();
		}

		public GLJournalApprovalThresholdValidation Validation
		{
			get { return new GLJournalApprovalThresholdValidation(this); }
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Type, Type);
			writer.WriteElementString(Schema.GLAccount, GLAccount.ToString());
			writer.WriteElementString(Schema.ReportSection, ReportSection);
			AuthorisationSettingsSerialiser.Serialize(writer, AuthorisationSettings);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Type = reader.ReadElementString(Schema.Type);
			GLAccount = new ZGuid(reader.ReadElementString(Schema.GLAccount));
			ReportSection = reader.ReadElementString(Schema.ReportSection);
			authorisationSettings = (PaymentThreeLevelAuthorisationSettingsCollection)AuthorisationSettingsSerialiser.Deserialize(reader);
			RegisterEditableChildObject(authorisationSettings);
		}

		#endregion
	}
}