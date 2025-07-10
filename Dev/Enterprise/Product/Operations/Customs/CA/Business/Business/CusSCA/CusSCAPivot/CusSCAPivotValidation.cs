using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusSCAPivotValidation : Customs.Business.CusSCAPivotValidation
	{
		public CusSCAPivotValidation(Customs.Business.BaseCusSCAPivot parent)
			: base(parent)
		{
		}

		public CusSCAPivot PackLine
		{
			get { return Parent; }
		}

		protected new CusSCAPivot Parent
		{
			get { return (CusSCAPivot)base.Parent; }
		}

		public void ValidateCV_AssociatedContainer()
		{
			ValidateCalculatedProperty(PackLine.CV_AssociatedContainerInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCV_AssociatedContainer();
		}

		protected void CheckCV_AssociatedContainer()
		{
			var container = Parent.Container;
			if (container != null && container.CN_ContainerMode == Core.Constants.ContainerModes.Empty)
			{
				Parent.CV_AssociatedContainerInfo.AddWarning(PackLineAssociatedWithAnEmptyContainer);
			}
		}
		internal static string PackLineAssociatedWithAnEmptyContainer
		{
			get { return Res.GetString("148DD3A1-3D42-435A-B569-7F894CC74C33", "This pack line will not be reported as this is an empty container."); }
		}

		protected override void CheckCV_PackageCount()
		{
			base.CheckCV_PackageCount();
			MandatoryValidation.MessageErrorIfNotEntered(PackLine.CV_PackageCountInfo);
			if (PackLine.CV_PackageCount > 9999999)
			{
				PackLine.CV_PackageCountInfo.AddMessageError(MaximumPackageCount);
			}

			ValidateCV_PackageType();
			ValidateCV_GoodsDescription();
			ValidateCV_Weight();
		}
		internal static string MaximumPackageCount
		{
			get { return Res.GetString("ee9502dd-ea1f-4b95-9b75-acdea93a79bd", "The maximum value allowed for Package Count is 9,999,999."); }
		}

		protected override void CheckCV_PackageType()
		{
			base.CheckCV_PackageType();
			ListValidation.MessageErrorIfInvalidCode(PackLine.CV_PackageTypeInfo);
			if (!PackLine.CV_PackageCount.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(PackLine.CV_PackageTypeInfo);
			}
		}

		protected override void CheckCV_Weight()
		{
			base.CheckCV_Weight();
			if (!PackLine.CV_PackageCount.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(PackLine.CV_WeightInfo);
			}
			ValidateCV_WeightUQ();
		}

		protected override void CheckCV_WeightUQ()
		{
			base.CheckCV_WeightUQ();
			ListValidation.MessageErrorIfInvalidCode(PackLine.CV_WeightUQInfo);
			if (!PackLine.CV_Weight.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(PackLine.CV_WeightUQInfo);
			}
		}

		protected override void CheckCV_Volume()
		{
			base.CheckCV_Volume();
			ValidateCV_VolumeUQ();
		}

		protected override void CheckCV_VolumeUQ()
		{
			base.CheckCV_VolumeUQ();
			ListValidation.MessageErrorIfInvalidCode(PackLine.CV_VolumeUQInfo);
			if (!PackLine.CV_Volume.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(PackLine.CV_VolumeUQInfo);
			}
		}

		protected override void CheckCV_HazardousGoods()
		{
			base.CheckCV_HazardousGoods();

			if (PackLine.CV_HazardousGoods)
			{
				bool hasDG = false;
				foreach (UNDGDataItem dgItem in PackLine.UNDGs)
				{
					if (!dgItem.DI_DG.IsEmpty)
					{
						hasDG = true;
					}
				}

				if (hasDG)
				{
					PackLine.CV_HazardousGoodsInfo.AddMessageError(HazWarning);
				}
			}
		}
		internal static string HazWarning
		{
			get { return Res.GetString("bfd36eb3-5d27-4b25-867d-7f1ff63ec1c9", "Please enter either Dangerous Goods Codes or Tick Hazardous Only in Bulk, but not both."); }
		}

		protected override void CheckCV_GoodsDescription()
		{
			base.CheckCV_GoodsDescription();
			if (!PackLine.CV_PackageCount.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(PackLine.CV_GoodsDescriptionInfo);
			}
		}

		protected override void CheckCV_HarmonisedTariffNums()
		{
			base.CheckCV_HarmonisedTariffNums();
			ZString[] tariffNumbers = PackLine.CV_HarmonisedTariffNums.Split(',');
			if (PackLine.CV_HarmonisedTariffNums.IsEmpty)
			{
				PackLine.CV_HarmonisedTariffNumsInfo.AddWarning(TariffWarning);
			}
			else if (tariffNumbers.Length > 5)
			{
				PackLine.CV_HarmonisedTariffNumsInfo.AddMessageError(TariffWarning);
			}
			else
			{
				foreach (ZString tariffNumber in tariffNumbers)
				{
					var tariffNumberKeepNumerics = tariffNumber.KeepNumericCharacters().Trim();
					if (!tariffNumberKeepNumerics.IsEmpty && (tariffNumberKeepNumerics.Length < 2 || tariffNumberKeepNumerics.Length > 10))
					{
						PackLine.CV_HarmonisedTariffNumsInfo.AddMessageError(TariffWarning);
						break;
					}
				}
			}
		}
		internal static string TariffWarning
		{
			get
			{
				return Res.GetString("57DD953B-4938-4966-BD89-2996F601ED7D", "Tariff codes should be entered if available. If entered, up to 5 Tariff codes may be specified, separated by commas (','). Each Tariff Code must contain at least 2 numeric digits, but cannot contain more than 10 digits.");
			}
		}
	}
}
