using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CN.Business
{
	public interface IAdditionalInformationWrapperParent : IValidationModeProvider
	{
		TariffView UniversalTariff { get; }
		ZString CountryOfOrigin { get; }
		bool ElementValueAllowEmpty { get; }
		AdditionalInformationCollection AdditionalInformationCodes { get; }
	}

	public class AdditionalInformationWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AdditionalInformationWrapper(IAdditionalInformationWrapperParent parent, ZString nameOfGoods, ZString goodsSpecModel, EnteringOrExiting isEnteringOrExiting) : base((parent as BusinessObject)?.Factory)
		{
			this.parent = Argument.NotNull(parent, nameof(parent));
			tariff = parent.UniversalTariff;
			this.isEnteringOrExiting = isEnteringOrExiting;
			BuildAdditionalElementValues(nameOfGoods, goodsSpecModel, this.parent.AdditionalInformationCodes);
		}
		readonly TariffView tariff;
		readonly IAdditionalInformationWrapperParent parent;
		readonly EnteringOrExiting isEnteringOrExiting;

		void BuildAdditionalElementValues(ZString nameOfGoods, ZString goodsSpecModel, AdditionalInformationCollection additionalInformationCodes)
		{
			fAdditionalElementValues = new AdditionalElementWrapperCollection(this);

			if (tariff != null)
			{
				var attributes = tariff.GetSortedAdditionalInfoAttributes(isEnteringOrExiting);
				foreach (var attribute in attributes)
				{
					AddNewAdditionalElementWrapper(attribute.ZZ3_Value, ZString.Empty);
				}
				fAdditionalElementValues.SetNameOfGoods(nameOfGoods);
				fAdditionalElementValues.SetGoodsSpecModel(tariff, isEnteringOrExiting, goodsSpecModel);

				if (additionalInformationCodes != null)
				{
					foreach (AdditionalInformation additionalInformation in additionalInformationCodes)
					{
						if (!attributes.Any(attr => attr.ZZ3_Value == additionalInformation.CY_Code))
						{
							AddNewAdditionalElementWrapper(additionalInformation.CY_Code, additionalInformation.CY_Data, false);
						}
					}
				}
			}
		}

		public AdditionalElementWrapperCollection AdditionalElementValues
		{
			get
			{
				if (!IsRegisteredEditableChildObject(fAdditionalElementValues))
				{
					RegisterEditableChildObject(fAdditionalElementValues);
				}
				return fAdditionalElementValues;
			}
		}
		AdditionalElementWrapperCollection fAdditionalElementValues;

		void AddNewAdditionalElementWrapper(ZString additionalElementCode, ZString additionalElementValue, bool isRequired = true)
		{
			var additionalElementDescription = tariff.GetAdditionalElementDescription(additionalElementCode);
			if (!additionalElementDescription.IsEmpty)
			{
				var additionalElementWrapper = new AdditionalElementWrapper(tariff.Factory, additionalElementCode, additionalElementDescription, additionalElementValue, parent, isEnteringOrExiting, isRequired);
				additionalElementWrapper.ElementValueInfo.ValueChanged += ElementValueInfo_ValueChanged;
				fAdditionalElementValues.Add(additionalElementWrapper);
			}
		}

		void ElementValueInfo_ValueChanged(object sender, System.EventArgs e)
		{
			NameOfGoodsInfo.RefreshBinding();
			GoodsSpecModelInfo.RefreshBinding();

			if (!IsValidationSuspended)
			{
				ValidateGoodsSpecModel();
			}
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.AdditionalInformationWrapper|NameOfGoods", Caption = "Name of Goods")]
		public ZString NameOfGoods => AdditionalElementValues.GetNameOfGoods();
		public ZPropertyInfo NameOfGoodsInfo => GetZPropertyInfo(nameof(NameOfGoods));

		[ResourceStringData("Enterprise.Customs.CN.Business.AdditionalInformationWrapper|GoodsSpecModel", Caption = "Specification & Model", ShortCaption = "Spec & Model")]
		public ZString GoodsSpecModel => AdditionalElementValues.GetGoodsSpecModel(tariff, isEnteringOrExiting);
		public ZPropertyInfo GoodsSpecModelInfo => GetZPropertyInfo(nameof(GoodsSpecModel));

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateGoodsSpecModel();
		}

		public void ValidateGoodsSpecModel()
		{
			GoodsSpecModelInfo.ClearAllNotifications();

			var actualLength = GoodsSpecModel.Length;
			var maxLength = AdditionalInformationHelper.GoodsSpecModelMaxLength;
			if (actualLength > maxLength)
			{
				GoodsSpecModelInfo.AddError(ValidationHelper.GetExceedsMaxLengthMessage(GoodsSpecModelInfo.HumanReadableName, actualLength, maxLength));
			}
		}
	}
}
