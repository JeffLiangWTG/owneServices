using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CN.Business
{
	public class AdditionalElementWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AdditionalElementWrapper(ZZRefCusCodeListCombined additionalElement, ZString value, IAdditionalInformationWrapperParent master, EnteringOrExiting isEnteringOrExiting, bool isRequired = true)
			: this(additionalElement.Factory, additionalElement.ZZD_Code, additionalElement.ZZD_Description, value, master, isEnteringOrExiting, isRequired)
		{
		}

		public AdditionalElementWrapper(BusinessObjectFactory factory, ZString elementCode, ZString elementName, ZString value, IAdditionalInformationWrapperParent master, EnteringOrExiting isEnteringOrExiting, bool isRequired = true) : base(factory)
		{
			this.master = Argument.NotNull(master, nameof(master));
			this.isEnteringOrExiting = isEnteringOrExiting;
			ElementCode = elementCode;
			ElementName = elementName;
			ElementValue = value.Left(ElementValueInfo.MaxLength);
			IsRequired = isRequired;
		}
		readonly EnteringOrExiting isEnteringOrExiting;
		readonly IAdditionalInformationWrapperParent master;

		public bool IsRequired { get; private set; }

		#region Schema

		public static class Schema
		{
			public const string ElementCode = "ElementCode";
			public const string ElementName = "ElementName";
			public const string ElementInstruction = "ElementInstruction";
			public const string ElementValue = "ElementValue";
		}

		#endregion

		#region Element Code & Name

		public ZString ElementCode { get; private set; }

		[ResourceStringData("Enterprise.Customs.CN.Business.AdditionalElementValue|ElementName", Caption = "Name")] // 申报要素名称
		public ZString ElementName { get; private set; }

		public ZPropertyInfo ElementNameInfo => GetZPropertyInfo(Schema.ElementName);

		#endregion

		#region ElementValue

		[MaxLength("ElementValue_MaxLength")]
		[ReadOnlyMember(nameof(ElementValue_ReadOnly))]
		[List(nameof(ElementValue_List))]
		[ResourceStringData("Enterprise.Customs.CN.Business.AdditionalElementValue|ElementValue", Caption = "Value")] // 申报要素值
		public ZString ElementValue
		{
			get => fElementValue;
			set
			{
				if (value.IsEmpty)
				{
					value = DefaultValue;
				}
				SetNonPersistentPropertyValue(ElementValueInfo, ref fElementValue, value);
				ElementValueInfo.RefreshBinding();
				ValidateElementValue();
			}
		}
		ZString fElementValue;

		public ZPropertyInfo ElementValueInfo => GetZPropertyInfo(Schema.ElementValue);

		void ValidateElementValue()
		{
			ElementValueInfo.ClearAllNotifications();

			if (IsRequired)
			{
				if (AdditionalElementStrategy.IsMandatory)
				{
					MandatoryValidation.MessageErrorIfNotEntered(ElementValueInfo);
				}

				if (AdditionalElementStrategy.ProvideList)
				{
					ListValidation.MessageErrorIfInvalidCode(ElementValueInfo);
				}

				if (ElementValue.IndexOf(AdditionalInformationHelper.AddInfoElementDelimeter) > -1)
				{
					ElementValueInfo.AddError(Res.GetString("ee132d50-ca11-4a19-9c17-272477337b9e", "The value has special character '{0}'.", AdditionalInformationHelper.AddInfoElementDelimeter));
				}

				var message = AdditionalElementStrategy.GetFormatCheckMessage(ElementValue);
				if (!message.IsEmpty)
				{
					ElementValueInfo.AddWarning(message);
				}

				AdditionalElementStrategy.ValidateAdditionalElement(master, ElementValue, ElementValueInfo);
			}
			else
			{
				ElementValueInfo.AddWarning(Res.GetString("8efcbd74-34ba-4f0c-a028-c40134f00516", "This additional information is not available for the selected Tariff, will be deleted after saving."));
			}
		}

		public ZString ElementValue_FieldType => AdditionalElementStrategy.ProvideList ? nameof(FieldType.TextDropEdit) : nameof(FieldType.Text);

		public int ElementValue_MaxLength => AdditionalElementStrategy.MaxLength;

		public ICodeDescriptionPairList ElementValue_List => AdditionalElementStrategy.GetList(Factory, isEnteringOrExiting);

		public bool ElementValue_ReadOnly => !IsRequired;

		#endregion

		#region Implementation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateElementValue();
		}

		IAdditionalElementStrategy AdditionalElementStrategy => fAdditionalElementStrategy ?? (fAdditionalElementStrategy = AdditionalElementStrategyProvider.GetAdditionalElementStrategy(ElementCode));
		IAdditionalElementStrategy fAdditionalElementStrategy;

		ZString DefaultValue => AdditionalElementStrategy.IsMandatory ? AdditionalElementStrategy.GetDefaultValue(isEnteringOrExiting) : ZString.Empty;

		#endregion
	}
}
