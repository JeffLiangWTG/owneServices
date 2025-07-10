using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusSCAPivot : BaseCusSCAPivot, ISCRLine, ICanDelete
	{
		#region Constants
		const string AssociatedContainerNotSet = "NotSet";
		#endregion

		#region Schema
		public new class Schema : AutoCusSCAPivot.Schema
		{
			public const string CV_AssociatedContainer = "CV_AssociatedContainer";
			public const string CN_ContainerType = "CN_ContainerType";
			public const string CN_RN_NKCountryOfRegistration = "CN_RN_NKCountryOfRegistration";
			public const string CN_ContainerSizeOrISOCode = "CN_ContainerSizeOrISOCode";
			public const string CN_ContainerMode = "CN_ContainerMode";
		}
		#endregion

		public CusSCAPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public static new readonly TypeDecider TypeDecider = new CusSCAPivotTypeDecider();

		protected override Customs.Business.CusSCAPivotLookups GetNewLookups()
		{
			return new CusSCAPivotLookups(this);
		}

		public new CusSCAPivotLookups Lookups
		{
			get { return (CusSCAPivotLookups)base.Lookups; }
		}

		bool ShouldSynchronizeWithShipment => HouseBill?.ShouldSynchronizeWithShipment ?? false;

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get { return !ShouldSynchronizeWithShipment; }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("23014987-4398-48e4-8d1f-f7137e4669fb", "You may not delete pack lines while they are synchronized with the shipment."); }
		}

		#endregion

		#region BaseCusSCAPivot property override

		public new CusSCAPivotValidation Validation
		{
			get { return (CusSCAPivotValidation)base.Validation; }
		}

		protected override Customs.Business.CusSCAPivotValidation GetNewValidation()
		{
			return new CusSCAPivotValidation(this);
		}

		public override ZGuid CV_CA
		{
			get => base.CV_CA;
			set
			{
				var oldHouseBill = HouseBill;
				base.CV_CA = value;
				if (!IsCopying)
				{
					var houseBill = HouseBill;
					if (oldHouseBill != houseBill)
					{
						oldHouseBill?.MarkAsNeedingValidation();
						houseBill?.MarkAsNeedingValidation();
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusSCAPivotLookups.AcrossPackageTypes))]
		[ReadOnlyMember(ShouldSynchronizeWithShipmentConst)]
		public override ZString CV_PackageType
		{
			get { return base.CV_PackageType; }
		}

		[List(nameof(Lookups) + "." + nameof(CusSCAPivotLookups.ACIWeightUnits))]
		[ReadOnlyMember(ShouldSynchronizeWithShipmentConst)]
		public override ZString CV_WeightUQ
		{
			get { return base.CV_WeightUQ; }
		}

		[List(nameof(Lookups) + "." + nameof(CusSCAPivotLookups.VolumeUQList))]
		[ReadOnlyMember(ShouldSynchronizeWithShipmentConst)]
		public override ZString CV_VolumeUQ
		{
			get { return base.CV_VolumeUQ; }
		}

		[DecimalPlaces(3)]
		[ReadOnlyMember(ShouldSynchronizeWithShipmentConst)]
		[MeasureUnit(Schema.CV_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal CV_Weight
		{
			get { return base.CV_Weight; }
			set { base.CV_Weight = value; }
		}

		[DecimalPlaces(3)]
		[ReadOnlyMember(ShouldSynchronizeWithShipmentConst)]
		public override ZDecimal CV_Volume
		{
			get { return base.CV_Volume; }
			set { base.CV_Volume = value; }
		}

		[ReadOnlyMember(ShouldSynchronizeWithShipmentConst)]
		public override ZInt CV_PackageCount
		{
			get { return base.CV_PackageCount; }
		}

		[ReadOnlyMember(ShouldSynchronizeWithShipmentConst)]
		public override ZString CV_HarmonisedTariffNums
		{
			get { return base.CV_HarmonisedTariffNums; }
		}

		[ReadOnlyMember(ShouldSynchronizeWithShipmentConst)]
		public override ZString CV_GoodsDescription
		{
			get { return base.CV_GoodsDescription; }
		}

		[ReadOnlyMember(ShouldSynchronizeWithShipmentConst)]
		public override ZString CV_MarksAndNumbers
		{
			get { return base.CV_MarksAndNumbers; }
		}

		#endregion

		#region OceanBill
		public CusSCAOceanBill OceanBill
		{
			get
			{
				CusSCAOceanBill result = null;
				var houseBill = HouseBill;
				if (houseBill != null)
				{
					result = houseBill.OceanBill;
				}
				else
				{
					var container = Container;
					if (container != null)
					{
						result = container.OceanBill;
					}
				}
				return result;
			}
		}
		#endregion

		#region HouseBill
		public CusSCAHouse HouseBill => Factory.Load<CusSCAHouse>(CV_CA);

		#endregion

		#region Container

		public new CusSCAContainer Container => Factory.Load<CusSCAContainer>(CV_CN);

		[BusinessObjectTestExclude()]
		[List(nameof(Lookups) + "." + nameof(CusSCAPivotLookups.CV_OceanBillContainers_List))]
		[ReadOnlyMember(ShouldSynchronizeWithShipmentConst)]
		[MaxLength(12)]
		public ZString CV_AssociatedContainer
		{
			get { return Container?.CN_ContainerNumber ?? CusSCAHouse.NonContaineriseID; }
			set
			{
				CusSCAContainer oldContainer = Container;
				ZString previousValue = AssociatedContainerNotSet;
				if (CV_CN.IsValid)
				{
					previousValue = CV_AssociatedContainer;
				}
				if (value.IsEmpty)
				{
					CV_CN = ZGuid.Empty;
				}
				else
				{
					if (value != CV_AssociatedContainer)
					{
						CusSCAContainer newContainer = null;
						if (OceanBill != null)
						{
							OceanBill.HouseBills.MarkAsNeedingValidationIncludingChildren();
							OceanBill.Containers.MarkAsNeedingValidationIncludingChildren();
							newContainer = OceanBill.FindContainerByNumber(value);
						}
						if (newContainer != null)
						{
							CV_CN = newContainer.PK;
						}
						else
						{
							CV_CN = ZGuid.Empty;
						}
						CV_LineNo = 0;
						if (!IsValidationSuspended)
						{
							Validation.ValidateCV_AssociatedContainer();
						}
					}
				}
				CV_AssociatedContainerInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CV_AssociatedContainerInfo
		{
			get { return GetZPropertyInfo(Schema.CV_AssociatedContainer); }
		}

		[BusinessObjectTestExclude()]
		[MaxLength(3)]
		public ZString CN_ContainerMode
		{
			get { return Container != null ? Container.CN_ContainerMode : ZString.Empty; }
			set
			{
				ZString oldValue = CN_ContainerMode;
				if (Container != null)
				{
					Container.CN_ContainerMode = value;
				}
				CN_ContainerModeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CN_ContainerModeInfo
		{
			get { return GetZPropertyInfo(Schema.CN_ContainerMode); }
		}

		#endregion

		const string ShouldSynchronizeWithShipmentConst = "ShouldSynchronizeWithShipment";

		#region ISCRLine members

		ZInt ISCRLine.NumberOfPackages
		{
			get { return CV_PackageCount; }
		}

		ZString ISCRLine.TypeOfPackages
		{
			get { return CV_PackageType; }
		}

		ZString ISCRLine.GoodsDescription
		{
			get { return CV_GoodsDescription; }
		}

		ZDecimal ISCRLine.GrossWeight
		{
			get { return CV_Weight; }
		}

		ZString ISCRLine.WeightUnits
		{
			get { return CV_WeightUQ; }
		}

		ZDecimal ISCRLine.Volume
		{
			get { return CV_Volume; }
		}

		ZString ISCRLine.VolumeUnits
		{
			get { return CV_VolumeUQ; }
		}

		ZString ISCRLine.ContainerNumber
		{
			get { return CV_AssociatedContainer == CusSCAHouse.NonContaineriseID ? ZString.Empty : CV_AssociatedContainer; }
		}

		ZString ISCRLine.DGCodes
		{
			get
			{
				if (CV_HazardousGoods && (HouseBill?.Shipment?.IsSea ?? false))
				{
					return "MHB";
				}
				else
				{
					return string.Join(",", UNDGs.Cast<UNDGDataItem>().Select(x => x.UNDGSubstance != null ? x.UNDGSubstance.DG_Code : ZString.Empty));
				}
			}
		}

		ZString ISCRLine.ShippingMarks
		{
			get { return CV_MarksAndNumbers; }
		}

		ZString ISCRLine.TariffNumbers
		{
			get { return CV_HarmonisedTariffNums; }
		}

		#endregion
	}
}
