using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI
{
	public class CostConfirmationDocTypeBizo : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CostConfirmationDocTypeBizo()
		{
			CostConfirmationDocType = AccountingConfigurationRegistry.Instance.CostConfirmationDocumentSettings.Value;
		}

		[MaxLength(3)]
		[List("CostConfirmationDocTypeList")]
		public ZString CostConfirmationDocType
		{
			get { return CostConfirmationDocType_innerValue; }
			set
			{
				CheckMaximumLength(CostConfirmationDocTypeInfo, value);
				CostConfirmationDocType_innerValue = value;
				if (!IsValidationSuspended)
				{
					ValidateCostConfirmationDocType();
				}
				CostConfirmationDocTypeInfo.RefreshBinding();
			}
		}
		ZString CostConfirmationDocType_innerValue;

		public ZPropertyInfo CostConfirmationDocTypeInfo
		{
			get { return GetZPropertyInfo(nameof(CostConfirmationDocType)); }
		}

		public void ValidateCostConfirmationDocType()
		{
			CostConfirmationDocTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CostConfirmationDocTypeInfo);
			ListValidation.ErrorIfInvalidCode(CostConfirmationDocTypeInfo, CostConfirmationDocTypeList);
		}

		public CodeDescriptionPairList CostConfirmationDocTypeList
		{
			get
			{
				return CostConfirmationDocTypeList_innerValue ?? (CostConfirmationDocTypeList_innerValue = AccountingConstants.CostConfirmationDocumentSettingList);
			}
		}
		CodeDescriptionPairList CostConfirmationDocTypeList_innerValue;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateCostConfirmationDocType();
		}
	}
}
