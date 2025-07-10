using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.JP.Manifest.Business;

[DependentBusinessObject(typeof(AsycudaManifestHeader), nameof(AsycudaManifestHeader.Containers))]
public class AsycudaContainer : ASYCUDA.Business.AsycudaContainer, Integration.Customs.ASYCUDA.JPManifest.IAsycudaContainer
{
	public AsycudaContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new partial class Schema : ManifestBase.AutoAsycudaContainer.Schema
	{
		public const string NACCSContainerSize = nameof(AsycudaContainer.NACCSContainerSize);
		public const string NACCSContainerType = nameof(AsycudaContainer.NACCSContainerType);
		public const string CustomsTareWeight = nameof(AsycudaContainer.CustomsTareWeight);
		public const string CustomsWeightUQ = nameof(AsycudaContainer.CustomsWeightUQ);
		public const string ACN_MoveOutDate = nameof(AsycudaContainer.ACN_MoveOutDate);
		public const string VanningLocationCode = nameof(AsycudaContainer.VanningLocationCode);
	}

	public override ZGuid ACN_RC_ContainerType
	{
		get => base.ACN_RC_ContainerType;
		set
		{
			if (base.ACN_RC_ContainerType != value)
			{
				base.ACN_RC_ContainerType = value;
				var containerTareWeight = ContainerType?.RC_TareWeight ?? ZDecimal.Zero;
				if (!containerTareWeight.IsEmpty)
				{
					if (CustomsWeightUQ.IsEmpty)
					{
						CustomsWeightUQ = Weight.Kilograms;
					}
					if (CustomsTareWeight.IsEmpty)
					{
						CustomsTareWeight = Weight.ConvertSafe(containerTareWeight, Weight.Kilograms, CustomsWeightUQ);
					}
				}
			}
		}
	}

	[ResourceStringData("JPAsycudaContainer.ACN_MoveOutDate", Caption = "Move Out Date", MediumCaption = "Move Out")]
	public ZDateTime ACN_MoveOutDate
	{
		get => this.GetSystemDefinedValue<ZDateTime>(Constants.GenAddOnColumnFieldName.ACN_MoveOutDate);
		set
		{
			var oldValue = ACN_MoveOutDate;
			if (oldValue != value)
			{
				this.SetSystemDefinedValue(Constants.GenAddOnColumnFieldName.ACN_MoveOutDate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateACN_MoveOutDate();
				}
				ACN_MoveOutDateInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo ACN_MoveOutDateInfo => GetZPropertyInfo(nameof(ACN_MoveOutDate));

	[MaxLength(5)]
	[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.VanningLocationCodeCollection))]
	[ResourceStringData("Enterprise.Customs.JP.Manifest.Business.AsycudaContainer|VanningLocationCode", Caption = "Vanning Location Code", MediumCaption = "Vanning Loc.", ShortCaption = "Van Loc.", FullDescription = "Vanning Location Code ")]
	public ZString VanningLocationCode
	{
		get => this.GetSystemDefinedValue<ZString>(Schema.VanningLocationCode);
		set
		{
			var oldValue = VanningLocationCode;
			if (value != oldValue)
			{
				CheckMaximumLength(VanningLocationCodeInfo, value);
				this.SetSystemDefinedValue(Schema.VanningLocationCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateVanningLocationCode();
				}
				VanningLocationCodeInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo VanningLocationCodeInfo => GetZPropertyInfo(Schema.VanningLocationCode);

	[ResourceStringData("Enterprise.Customs.JP.Manifest.Business.AsycudaContainer|NACCSContainerType", Caption = "NACCS Container Type", MediumCaption = "NACCS Cont. Type", ShortCaption = "NACCS Type")]
	public ZString NACCSContainerType => Factory.GetValue(ref naccsContainerType, () => ContainerHelper.GetNACCSContainerType(ContainerType));
	CachedProperty<ZString> naccsContainerType;

	[ResourceStringData("Enterprise.Customs.JP.Manifest.Business.AsycudaContainer|NACCSContainerSize", Caption = "NACCS Container Size", MediumCaption = "NACCS Cont. Size", ShortCaption = "NACCS Size")]
	public ZString NACCSContainerSize => Factory.GetValue(ref naccsContainerSize, () => ContainerHelper.GetNACCSContainerSize(ContainerType));
	CachedProperty<ZString> naccsContainerSize;

	[ChildEditable]
	public CusSealCollection AdditionalSeals
	{
		get
		{
			if (additionalSeals == null)
			{
				additionalSeals = new CusSealCollection(this);
				RegisterEditableChildObject(additionalSeals);
			}
			return additionalSeals;
		}
	}
	CusSealCollection additionalSeals;

	internal ShortSequenceNumberGenerator SealsSequenceNumberGenerator => sealsSequenceNumberGeneratorCache ??= new ShortSequenceNumberGenerator(() => AdditionalSeals);
	ShortSequenceNumberGenerator sealsSequenceNumberGeneratorCache;

	[DecimalPlaces(3)]
	[ResourceStringData("JPAsycudaContainer.CustomsTareWeight", Caption = "Customs Tare Weight", MediumCaption = "Tare Weight", ShortCaption = "Tare Wgt.")]
	public ZDecimal CustomsTareWeight
	{
		get => this.GetSystemDefinedValue<ZDecimal>(Constants.GenAddOnColumnFieldName.ACN_CustomsTareWeight);
		set
		{
			var oldValue = CustomsTareWeight;
			if (oldValue != value)
			{
				this.SetSystemDefinedValue(Constants.GenAddOnColumnFieldName.ACN_CustomsTareWeight, value);
				CustomsTareWeightInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo CustomsTareWeightInfo => GetZPropertyInfo(Schema.CustomsTareWeight);

	[MaxLength(2)]
	[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.WeightCodes))]
	[ResourceStringData("JPAsycudaContainer.CustomsWeightUQ", Caption = "Customs Weight UQ", MediumCaption = "Weight UQ", ShortCaption = "UQ")]
	public ZString CustomsWeightUQ
	{
		get => this.GetSystemDefinedValue<ZString>(Constants.GenAddOnColumnFieldName.ACN_CustomsWeightUQ);
		set
		{
			var oldValue = CustomsWeightUQ;
			if (oldValue != value)
			{
				CheckMaximumLength(CustomsWeightUQInfo, value);
				this.SetSystemDefinedValue(Constants.GenAddOnColumnFieldName.ACN_CustomsWeightUQ, value);
				CustomsWeightUQInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo CustomsWeightUQInfo => GetZPropertyInfo(Schema.CustomsWeightUQ);

	internal bool HasSealNumber => Factory.GetValue(ref hasSealNumber, () => !(ACN_Seal1.IsEmpty && ACN_Seal2.IsEmpty && ACN_Seal3.IsEmpty && AdditionalSeals.All(seal => seal.BK_SealNumber.IsEmpty)));
	CachedProperty<bool> hasSealNumber;

	ContainerHelper ContainerHelper => containerHelper ??= new ContainerHelper(Factory);
	ContainerHelper containerHelper;

	protected override ManifestBase.AsycudaContainerValidation GetNewValidation() => new AsycudaContainerValidation(this);

	public new AsycudaContainerValidation Validation => (AsycudaContainerValidation)base.Validation;

	public new AsycudaContainerLookups Lookups => (AsycudaContainerLookups)base.Lookups;

	protected override ManifestBase.AsycudaContainerLookups GetNewLookups() => new AsycudaContainerLookups(this);

	public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
}
