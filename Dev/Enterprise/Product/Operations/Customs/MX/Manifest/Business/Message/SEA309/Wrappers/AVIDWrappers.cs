using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class AVIDWrappers : IAVID
	{
		readonly AsycudaContainer container;
		readonly AsycudaBill bill;

		public AVIDWrappers(AsycudaContainer container, AsycudaBill bill)
		{
			this.container = Argument.NotNull(container, "asycudaContainer cannot be null");
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}

		IVIDConveyanceIdentification IAVID.VID => vid ?? (vid = new VIDWrappers(container));
		IVIDConveyanceIdentification vid;

		IReadOnlyCollection<IAN1> IAVID.AN1 => new IAN1[] { new AN1Wrapper(bill, container) };
	}

	internal class AN1Wrapper : IAN1
	{
		readonly AsycudaBill bill;
		readonly AsycudaContainer container;

		public AN1Wrapper(AsycudaBill bill, AsycudaContainer container)
		{
			this.container = Argument.NotNull(container, "asycudaContainer cannot be null");
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}

		IReadOnlyCollection<IAN10> IAN1.AN10
		{
			get
			{
				var result = new List<IAN10>();

				var packs = from s in bill.Packs.OfType<AsycudaPack>()
							where s.ContainerPK == container.PK
							select s;

				foreach (AsycudaPack pack in packs)
				{
					result.Add(new AN10Wrapper(pack));
				}

				return result.ToArray();
			}
		}
	}

	internal class AN10Wrapper : IAN10
	{
		readonly AsycudaPack pack;

		public AN10Wrapper(AsycudaPack pack)
		{
			this.pack = Argument.NotNull(pack, "asycudaPack cannot be null");
		}

		IN10QuantityDescription IAN10.N10 => n10 ?? (n10 = new N10Wrapper(pack));
		IN10QuantityDescription n10;

		IReadOnlyCollection<IAH1> IAN10.AH => new IAH1[1] { new AH1Wrapper(pack) };
	}

	internal class AH1Wrapper : IAH1
	{
		readonly AsycudaPack pack;

		public AH1Wrapper(AsycudaPack pack)
		{
			this.pack = Argument.NotNull(pack, "asycudaPack cannot be null");
		}

		IHazardousInfo IAH1.H1
		{
			get
			{
				var uNDG = pack.UNDGs?.FirstOrDefault();

				if (uNDG == null || uNDG.UNDGSubstance == null)
				{
					return null;
				}
				else
				{
					return new H1Wrapper(uNDG.UNDGSubstance);
				}
			}
		}
	}

	internal class H1Wrapper : IHazardousInfo
	{
		readonly UNDGSubstance uNDG;

		public H1Wrapper(UNDGSubstance undg)
		{
			uNDG = undg;
		}

		string IHazardousInfo.HazardousMaterialCode => uNDG.DG_UNNO;

		string IHazardousInfo.HazardousMaterialClassCode => uNDG.DG_Class;

		string IHazardousInfo.HazardousMaterialCodeQualifier => SEA309Constants.Separator;

		string IHazardousInfo.HazardousMaterialDescription => uNDG.DG_PSN;

		string IHazardousInfo.FlashpointTemperature => SEA309Helper.CalculateFlashPointNumeric(uNDG.DG_FlashPoint);

		string IHazardousInfo.UnitorBasisForMeasurement => SEA309Constants.UnitorMeasurementCode;
	}

	internal class N10Wrapper : IN10QuantityDescription
	{
		readonly AsycudaPack pack;
		readonly string weightUQ;

		public N10Wrapper(AsycudaPack pack)
		{
			this.pack = Argument.NotNull(pack, "asycudaPack cannot be null");
			weightUQ = SEA309Helper.WeightUnitCodeCalculator(this.pack.APA_WeightUQ);
		}

		string IN10QuantityDescription.Quantity => pack.APA_PackQty.ToString();

		string IN10QuantityDescription.Description => pack.APA_GoodsDescription;

		string IN10QuantityDescription.MarksAndNumbers => pack.APA_MarksAndNumbers;

		string IN10QuantityDescription.CommodityCodeQualifier => SEA309Constants.CommodityCode;

		string IN10QuantityDescription.CommodityCode => pack.APA_CommodityCode;

		string IN10QuantityDescription.CustomsShipmentValue => pack.LinePrice.ToString();

		string IN10QuantityDescription.WeightUnitCode => weightUQ;

		string IN10QuantityDescription.Weight
		{
			get
			{
				return weightUQ == WeightConstants.Kilograms
					? (ZString)(Core.Constants.Weight.ConvertSafe(pack.APA_Weight, pack.APA_WeightUQ, Core.Constants.Weight.Kilograms)).ToString()
					: (ZString)pack.APA_Weight.ToString();
			}
		}

		string IN10QuantityDescription.ReferenceIdentification => ZString.Empty;

		string IN10QuantityDescription.ManifestUnitCode
		{
			get
			{
				var query = new ZQuery(RefPacksSchema.RP_CommercialPack, this.pack.APA_PackUQ);
				query.AddToFilter(RefPacksSchema.RP_CustomsCountry, Core.Constants.CountryCodes.Mexico);
				var pack = this.pack.Factory.LoadTop1<CusRefPacks>(query);

				return pack?.RP_CustomsPack ?? this.pack.APA_PackUQ;
			}
		}

		string IN10QuantityDescription.CountryCode => pack.Bill.ABL_RL_NKOrigin.SubstringSafe(0, 2);

		string IN10QuantityDescription.CountryCode2 => ZString.Empty;

		string IN10QuantityDescription.CurrencyCode => ZString.Empty;
	}

	internal class VIDWrappers : IVIDConveyanceIdentification
	{
		readonly AsycudaContainer container;
		readonly RefContainer containerType;
		readonly ZString equipmentType;

		public VIDWrappers(AsycudaContainer container)
		{
			this.container = Argument.NotNull(container, "asycudaContainer cannot be null");
			containerType = container.ContainerType;
			equipmentType = containerType?.RC_ISOType ?? ZString.Empty;
		}

		string IVIDConveyanceIdentification.EquipmentDescriptionCode
		{
			get
			{
				if (!containerType.IsNull)
				{
					var query = new ZQuery(RefContainerCodeMapSchema.RCM_RC_Container, containerType.PK);
					query.AddToFilter(RefContainerCodeMapSchema.RCM_RN_NKCountry, Core.Constants.CountryCodes.Mexico);
					var containercodemap = container.Factory.LoadTop1<RefContainerCodeMap>(query);

					return containercodemap?.RCM_Code ?? SEA309Constants.ContainerInitials;
				}
				else
				{
					return SEA309Constants.ContainerInitials;
				}
			}
		}

		string IVIDConveyanceIdentification.EquipmentInitial => SEA309Constants.ContainerName;

		string IVIDConveyanceIdentification.EquipmentNumber => container.ACN_ContainerNumber;

		string IVIDConveyanceIdentification.SealNumber => container.ACN_Seal1;

		string IVIDConveyanceIdentification.SealNumber2 => container.ACN_Seal2;

		string IVIDConveyanceIdentification.EquipmentLenght => equipmentType.IsEmpty ? containerType?.RC_Length.Round(0).ToString() ?? ZString.Empty : ZString.Empty;

		string IVIDConveyanceIdentification.Height => equipmentType.IsEmpty ? containerType?.RC_Height.Round(0).ToString() ?? ZString.Empty : ZString.Empty;

		string IVIDConveyanceIdentification.Width => equipmentType.IsEmpty ? containerType?.RC_Width.Round(0).ToString() ?? ZString.Empty : ZString.Empty;

		string IVIDConveyanceIdentification.EquipmentType => equipmentType;

		string IVIDConveyanceIdentification.StatusCode
		{
			get
			{
				switch (container.ACN_EmptyFullIndicator)
				{
					case Core.Constants.ContainerModes.FCL:
						return EmptyFullIndicator.FullContainer;
					case Core.Constants.ContainerModes.LCL:
						return EmptyFullIndicator.Loaded;
					default:
						return EmptyFullIndicator.Empty;
				}
			}
		}

		string IVIDConveyanceIdentification.TypeOfServiceCode => ZString.Empty;

		string IVIDConveyanceIdentification.LocationIdentifier => ZString.Empty;

		string IVIDConveyanceIdentification.StandardCarrierAlphaCode => ZString.Empty;

		string IVIDConveyanceIdentification.ReferenceIdentification => ZString.Empty;

		string IVIDConveyanceIdentification.StateCode => ZString.Empty;

		string IVIDConveyanceIdentification.CountryCode => ZString.Empty;

		string IVIDConveyanceIdentification.ReferenceIdentification2 => ZString.Empty;

		string IVIDConveyanceIdentification.CountrySubdivision => ZString.Empty;

		string IVIDConveyanceIdentification.ImpoExpoCode => ZString.Empty;

		string IVIDConveyanceIdentification.EquipmentNumberCheckDigit => ZString.Empty;
	}
}
