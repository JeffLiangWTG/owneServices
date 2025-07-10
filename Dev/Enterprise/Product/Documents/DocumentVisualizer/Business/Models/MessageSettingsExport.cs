using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.Models
{
	public sealed class MessageSettingsExport : NonPersistentBusinessObject, IObsoleteValidation
	{
		public MessageSettingsExport(BusinessObjectFactory factory, ZString workflowType)
			: base(factory)
		{
			Argument.NotNull(factory, nameof(factory));
			this.workflowType = workflowType;
		}

		readonly ZString workflowType;

		#region Recipient Type

		[List("RecipientTypeList")]
		[MaxLength(3)]
		public ZString RecipientType
		{
			get => recipientType;
			set
			{
				if (SetNonPersistentPropertyValue(RecipientTypeInfo, ref recipientType, value)
					&& !IsValidationSuspended)
				{
					ValidateRecipientType();
				}
			}
		}
		ZString recipientType;

		public ZPropertyInfo RecipientTypeInfo => GetZPropertyInfo(nameof(RecipientType));

		public CodeDescriptionPairList RecipientTypeList => Factory.GetCachedValue(string.Concat(workflowType, "4e657e91-3f99-4db8-8332-e2bc81d3b5a0"), GetRecipientTypeList);

		CodeDescriptionPairList GetRecipientTypeList()
		{
			var workflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(workflowType);
			return WorkflowListHelper.GetRecipientTypeList(workflowDescriptor);
		}

		#endregion

		#region Purpose Code

		[List("PurposeCodeList")]
		[MaxLength(3)]
		public ZString PurposeCode
		{
			get => purposeCode;
			set
			{
				if (SetNonPersistentPropertyValue(PurposeCodeInfo, ref purposeCode, value)
					&& !IsValidationSuspended)
				{
					ValidatePurposeCode();
				}
			}
		}
		ZString purposeCode;

		public ZPropertyInfo PurposeCodeInfo => GetZPropertyInfo(nameof(PurposeCode));

		public CodeDescriptionPairList PurposeCodeList => WorkflowListHelper.GetCachedPurposeCodeList(Factory);

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			this.ValidateRecipientType();
			this.ValidatePurposeCode();
		}

		public void ValidateRecipientType()
		{
			RecipientTypeInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(RecipientTypeInfo);

			if (!RecipientTypeInfo.HasNotifications())
			{
				ListValidation.ErrorIfInvalidCode(RecipientTypeInfo);
			}
		}

		public void ValidatePurposeCode()
		{
			PurposeCodeInfo.ClearAllNotifications();

			if (!PurposeCodeInfo.HasNotifications())
			{
				ListValidation.ErrorIfInvalidCode(PurposeCodeInfo);
			}
		}
		#endregion
	}
}
