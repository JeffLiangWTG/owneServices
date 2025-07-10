using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionImportWizard : ImportWizard
	{
		public CommissionImportWizard(CommissionImportCollectionInfo collectionInfo, ISettingsStorage settingsStorage, IFileMapper fileMapper)
			: base(collectionInfo, settingsStorage, fileMapper)
		{
		}

		#region Properties

		#region ImportType

		[List("ImportTypes")]
		[ResourceStringData("CommissionImportWizard|ImportType", Caption = "Import Type")]
		public ZString ImportType
		{
			get { return importType; }
			set
			{
				SetNonPersistentPropertyValue(ImportTypeInfo, ref importType, value);
				if (!IsValidationSuspended)
				{
					ValidateImportType();
				}
			}
		}
		ZString importType;

		public ZPropertyInfo ImportTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ImportType)); }
		}

		public ZString ImportTypeHint
		{
			get
			{
				switch (ImportType)
				{
					case CommissionImportTypeList.Codes.Add:
						return Res.GetString("d22dc851-45d4-436f-8a36-1962971e9677", "Import each line as a new entity commission amount");

					case CommissionImportTypeList.Codes.OverrideIfChangedOtherwiseSkip:
						return Res.GetString("86d60cb6-d489-4cbf-85f8-5fe6b29ce093", "Override any existing imported entity commissions that have the same 'Recognition Date' and 'Commission Type' if the 'Entity Amount' or 'Commission Currency Code' has changed otherwise it is skipped. If an existing entity commission does not exist, the line is added as a new entity commission amount.");
				}

				return ZString.Empty;
			}
		}

		public ZPropertyInfo ImportTypeHintInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ImportType), x => ImportTypeInfo); }
		}

		#endregion

		#endregion

		#region Settings

		public override Type SettingsType
		{
			get { return typeof(CommissionImportWizardSettings); }
		}

		protected override void GetSettingsCore(ImportExportWizardSettings serializableSettings)
		{
			base.GetSettingsCore(serializableSettings);

			var commissionSettings = (CommissionImportWizardSettings)serializableSettings;
			commissionSettings.ImportType = ImportType;
		}

		protected override void SetSettingsCore(ImportExportWizardSettings serializableSettings)
		{
			base.SetSettingsCore(serializableSettings);

			var commissionSettings = (CommissionImportWizardSettings)serializableSettings;
			ImportType = commissionSettings.ImportType;
		}

		#endregion

		#region Lists

		public CommissionImportTypeList ImportTypes
		{
			get { return importTypes ?? (importTypes = new CommissionImportTypeList()); }
		}
		CommissionImportTypeList importTypes;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateImportType();
		}

		void ValidateImportType()
		{
			ImportTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ImportTypeInfo);
			ListValidation.ErrorIfInvalidCode(ImportTypeInfo);
		}

		#endregion
	}
}
