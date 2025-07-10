using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocFormedPagesTopLevelPack : NonPersistentBusinessObject, IObsoleteValidation, IDocPackageDetails
	{
		public DocFormedPagesTopLevelPack(AgencyShipmentContainer agencyContainer)
		{
			if (agencyContainer != null)
			{
				Mode = agencyContainer.JC_ContainerMode;
				ReferenceNumber = agencyContainer.JC_ContainerNum;
				PackType = agencyContainer.JC_F3_NKPackType;

				VehicleColour = agencyContainer.JC_VehicleColor;
				VehicleMake = agencyContainer.JC_VehicleMake;
				VehicleModel = agencyContainer.JC_VehicleModel;
				VehicleNumberOfDoors = agencyContainer.JC_VehicleNumberOfDoors;
				VehicleTransmission = agencyContainer.JC_VehicleTransmission;
				VehicleYear = agencyContainer.JC_VehicleYear;

				Count = agencyContainer.JC_ContainerCount;
				Description = agencyContainer.JC_Description;
				MarksAndNumbers = agencyContainer.JC_MarksAndNumbers;

				Weight = agencyContainer.JC_GrossWeight;
				WeightUnit = agencyContainer.JC_GrossWeightUQ;

				Volume = agencyContainer.JC_GrossVolume;
				VolumeUnit = agencyContainer.JC_GrossVolumeUQ;

				Length = agencyContainer.JC_TotalLength;
				Height = agencyContainer.JC_TotalHeight;
				Width = agencyContainer.JC_TotalWidth;
				DimensionUnit = agencyContainer.JC_TotalUnitOfMeasure;

				Commodity = DocCommodity.New(agencyContainer.Factory, agencyContainer.JC_RH_NKContainerCommodityCode);
				HarmonisedCode = agencyContainer.JC_HarmonisedCode;

				var undgs = new List<UNDGSubstanceWrapper>();
				foreach (var undg in agencyContainer.UNDGs)
				{
					undgs.Add(new UNDGSubstanceWrapper(undg, agencyContainer.Factory));
				}

				UNDGs = undgs.ToArray();
			}
		}

		#region Properties

		public ZString Mode { get; private set; }
		public ZString ReferenceNumber { get; private set; }
		public ZString PackType { get; private set; }

		public ZString VehicleColour { get; private set; }
		public ZString VehicleMake { get; private set; }
		public ZString VehicleModel { get; private set; }
		public ZByte VehicleNumberOfDoors { get; private set; }
		public ZString VehicleTransmission { get; private set; }
		public ZShort VehicleYear { get; private set; }

		public ZInt Count { get; private set; }
		public ZString Description { get; private set; }
		public ZString MarksAndNumbers { get; private set; }

		public ZDecimal Weight { get; private set; }
		public ZString WeightUnit { get; private set; }

		public ZDecimal Volume { get; private set; }
		public ZString VolumeUnit { get; private set; }

		public ZDecimal Length { get; private set; }
		public ZDecimal Height { get; private set; }
		public ZDecimal Width { get; private set; }
		public ZString DimensionUnit { get; private set; }

		public DocCommodity Commodity { get; private set; }
		public ZString HarmonisedCode { get; private set; }

		public UNDGSubstanceWrapper[] UNDGs { get; private set; }

		#endregion

		#region IDocPackageDetails Members

		ZString IDocPackageDetails.ReferenceNumber
		{
			get { return ReferenceNumber; }
		}

		ZInt IDocPackageDetails.Count
		{
			get { return Count; }
		}

		ZString IDocPackageDetails.PackType
		{
			get { return PackType; }
		}

		ZString IDocPackageDetails.Description
		{
			get { return Description; }
		}

		ZString IDocPackageDetails.DetailedDescription
		{
			get { return Description; }
		}

		ZString IDocPackageDetails.MarksAndNumbers
		{
			get { return MarksAndNumbers; }
		}

		ZDecimal IDocPackageDetails.Weight
		{
			get { return Weight; }
		}

		ZString IDocPackageDetails.WeightUnit
		{
			get { return WeightUnit; }
		}

		ZDecimal IDocPackageDetails.Volume
		{
			get { return Volume; }
		}

		ZString IDocPackageDetails.VolumeUnit
		{
			get { return VolumeUnit; }
		}

		ZDecimal IDocPackageDetails.Length
		{
			get { return Length; }
		}

		ZDecimal IDocPackageDetails.Height
		{
			get { return Height; }
		}

		ZDecimal IDocPackageDetails.Width
		{
			get { return Width; }
		}

		ZString IDocPackageDetails.DimensionUnit
		{
			get { return DimensionUnit; }
		}

		DocCommodity IDocPackageDetails.Commodity
		{
			get { return Commodity; }
		}

		ZString IDocPackageDetails.HarmonisedCode
		{
			get { return HarmonisedCode; }
		}

		UNDGSubstanceWrapper[] IDocPackageDetails.UNDGs
		{
			get { return UNDGs; }
		}

		#endregion
	}
}
