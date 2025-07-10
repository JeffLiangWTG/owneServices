using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocLoadListPackLine : DocBaseWrapper
	{
		DocLoadListPackLine(LoadListPackLine packLine, BusinessObjectFactory factoryToWrap) : base(packLine, factoryToWrap) { }

		public static DocLoadListPackLine New(LoadListPackLine packLine, BusinessObjectFactory factoryToWrap)
		{
			return (packLine != null) ? new DocLoadListPackLine(packLine, factoryToWrap) : null;
		}

		#region LoadList Document

		public ZString ContainerCode
		{
			get { return (Container != null) ? Container.ContainerNumberOrTypeCount : ZString.Empty; }
		}

		ZString fConsignorAndConsigneeForLoadList;
		public ZString ConsignorAndConsigneeForLoadList
		{
			get { return fConsignorAndConsigneeForLoadList; }
			set { fConsignorAndConsigneeForLoadList = value; }
		}

		ZString fCustomsBrokerForLoadList;
		public ZString CustomsBrokerForLoadList
		{
			get { return fCustomsBrokerForLoadList; }
			set { fCustomsBrokerForLoadList = value; }
		}

		public ZDecimal ContainerTotalManifestWeight
		{
			get { return fContainerTotalManifestWeight; }
			set { fContainerTotalManifestWeight = value; }
		}
		ZDecimal fContainerTotalManifestWeight;

		public ZString ContainerTotalManifestWeightUnit
		{
			get { return containerTotalManifestWeightUnit; }
			set { containerTotalManifestWeightUnit = value; }
		}
		ZString containerTotalManifestWeightUnit;

		public ZDecimal ContainerTotalManifestVolume
		{
			get { return fContainerTotalManifestVolume; }
			set { fContainerTotalManifestVolume = value; }
		}
		ZDecimal fContainerTotalManifestVolume;

		public ZString ContainerTotalManifestVolumeUnit
		{
			get { return containerTotalManifestVolumeUnit; }
			set { containerTotalManifestVolumeUnit = value; }
		}
		ZString containerTotalManifestVolumeUnit;

		#endregion

		#region Fields
		public override string ToString()
		{
			return (Shipment != null) ? Shipment.ShipmentNumber : ZString.Empty;
		}

		public ZInt PackageCount
		{
			get { return PackLine.PackageCount; }
		}

		public ZString PackType
		{
			get { return PackLine.PackType; }
		}

		public ZString WeightUQ
		{
			get { return PackLine.WeightUQ; }
		}

		public ZString VolumeUQ
		{
			get { return PackLine.VolumeUQ; }
		}

		public ZDecimal Volume
		{
			get { return PackLine.Volume; }
		}

		public ZDecimal Weight
		{
			get { return PackLine.Weight; }
		}

		public ZDecimal TotalHazVolume
		{
			get { return PackLine.TotalHazVolume; }
		}

		public ZDecimal TotalHazWeight
		{
			get { return PackLine.TotalHazWeight; }
		}

		public ZString Dimensions
		{
			get { return PackLine.Dimensions; }
		}

		public ZString CargoLocationAndPacks
		{
			get { return PackLine.CargoLocationAndPacks; }
		}

		public ZString WeightVolumeAndPacks
		{
			get
			{
				return ZString.Format("{0} {1}\n{2} {3}\n{4} {5}",
					FormatNumber(Weight), WeightUQ,
					FormatNumber(Volume), VolumeUQ,
					PackageCount, PackType);
			}
		}

		public ZString PackageDetails
		{
			get { return PackLine.PackageDetails; }
		}

		public ZString PackageDetailsWithHazCat
		{
			get { return PackLine.PackageDetailsWithHazCat; }
		}

		public ZString PackageDetailsAndHandlingInstructionNote
		{
			get
			{
				ZString result = "";
				if (IsGroupPackLine)
				{
					result = PackageDetails;
				}
				else
				{
					result = Dimensions;
				}

				if (Shipment != null && !Shipment.HandlingInstructions.IsEmpty)
				{
					result += result.IsEmpty ? "" : "\n";
					result += Shipment.HandlingInstructions;
				}
				return result;
			}
		}

		public ZBool IsGroupPackLine
		{
			get { return PackLine.IsGroupPackLine; }
		}

		public DocShipment Shipment
		{
			get { return PackLine.Shipment; }
		}

		public DocContainer Container
		{
			get { return PackLine.Container; }
		}

		public ZString ContainerNumber
		{
			get { return (Container != null) ? Container.ContainerNumber : ZString.Empty; }
		}

		public ZInt ContainerPackingOrder
		{
			get
			{
				return PackLine.PackingOrder;
			}
		}

		#endregion

		#region Implementation
		protected LoadListPackLine PackLine
		{
			get { return (LoadListPackLine)WrappedObject; }
		}

		#endregion

		internal PackLocationCollection PackLocations
		{
			get { return PackLine.PackLocations; }
		}
	}

	public class LoadListPackLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		public LoadListPackLine() { }
		public LoadListPackLine(DocPackLines docPackLine, DocContainer container)
		{
			if (docPackLine != null)
			{
				this.PackageCount = docPackLine.PackageCount;
				this.PackType = docPackLine.PackTypeDescription;
				this.PackingOrder = docPackLine.ContainerPackingOrder;

				if (docPackLine.Shipment != null)
				{
					VolumeUQ = docPackLine.Shipment.UnitOfVolume;
					WeightUQ = docPackLine.Shipment.UnitOfWeight;

					Volume = Constants.Volume.ConvertSafe(docPackLine.ActualVolume, docPackLine.ActualVolumeUQ, VolumeUQ);
					Weight = Constants.Weight.ConvertSafe(docPackLine.ActualWeight, docPackLine.ActualWeightUQ, WeightUQ);
				}
				else
				{
					VolumeUQ = docPackLine.ActualVolumeUQ;
					WeightUQ = docPackLine.ActualWeightUQ;

					Volume = docPackLine.ActualVolume;
					Weight = docPackLine.ActualWeight;
				}

				this.PackageDetails = GetPackageDetails(docPackLine);
				this.Dimensions = docPackLine.Dimensions;
				AddHazCatDetails(docPackLine, this.PackageDetails);

				this.CargoLocationAndPacks = docPackLine.CargoLocationAndPacks;
				this.Shipment = docPackLine.Shipment;
				this.PackLocations = docPackLine.PackLocations;
			}
			this.Container = container;
		}

		public void AmendExistingPackLine(DocPackLines packLineToAdd)
		{
			this.IsGroupPackLine = ZBool.True;

			PackageCount += packLineToAdd.PackageCount;
			Weight += Constants.Weight.ConvertSafe(packLineToAdd.ActualWeight, packLineToAdd.ActualWeightUQ, WeightUQ);
			Volume += Constants.Volume.ConvertSafe(packLineToAdd.ActualVolume, packLineToAdd.ActualVolumeUQ, VolumeUQ);

			if (PackType != packLineToAdd.PackTypeDescription)
			{
				PackType = Res.GetString("87176086-1b79-447b-af76-df12b9f75bce", "Packages");
			}

			if (!packLineToAdd.CargoLocationAndPacks.IsEmpty)
			{
				this.CargoLocationAndPacks += this.CargoLocationAndPacks.IsEmpty ? "" : "\n";
				this.CargoLocationAndPacks += packLineToAdd.CargoLocationAndPacks;
			}

			ZString packageDetailsToAdd = GetPackageDetails(packLineToAdd);

			this.PackageDetails += "\n" + packageDetailsToAdd;
			AddHazCatDetails(packLineToAdd, packageDetailsToAdd);
		}

		public ZInt PackageCount;
		public ZString PackType;
		public ZString WeightUQ;
		public ZString VolumeUQ;
		public ZDecimal Volume;
		public ZDecimal Weight;
		public ZDecimal TotalHazWeight;
		public ZDecimal TotalHazVolume;
		public ZString Dimensions;
		public ZString CargoLocationAndPacks;
		public ZString PackageDetails;
		public ZString PackageDetailsWithHazCat;
		public DocShipment Shipment;
		public DocContainer Container;
		public ZBool IsGroupPackLine;
		public ZInt PackingOrder;
		PackLocationCollection packLocations;

		[BusinessObjectTestExclude]
		public PackLocationCollection PackLocations
		{
			get { return packLocations; }
			set { packLocations = value; }
		}

		static ZString GetPackageDetails(ZInt packageCount, ZString packType, ZDecimal weight, ZString weightUQ, ZDecimal volume, ZString volumeUQ, ZString dimensions)
		{
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			builder.AppendFormat("{0:0.###} {1} {2:0.###} {3} {4} {5}",
				packageCount, packType,
				weight, weightUQ,
				volume, volumeUQ);

			if (!dimensions.IsEmpty)
			{
				builder.AppendFormat("\n   ");
				builder.AppendFormat(dimensions);
			}

			return builder.ToString();
		}

		static ZString GetPackageDetails(DocPackLines line)
		{
			return GetPackageDetails(
				line.PackageCount, line.PackTypeDescription,
				line.ActualWeight, line.ActualWeightUQ,
				line.ActualVolume, line.ActualVolumeUQ,
				line.Dimensions);
		}

		protected void AddHazCatDetails(DocPackLines line, ZString packageDetailsToAdd)
		{
			if (line.IsHazardous)
			{
				if (!this.PackageDetailsWithHazCat.IsEmpty)
				{
					this.PackageDetailsWithHazCat += "\n";
				}

				this.PackageDetailsWithHazCat += packageDetailsToAdd;
				this.TotalHazWeight += Constants.Weight.ConvertSafe(line.ActualWeight, line.ActualWeightUQ, WeightUQ);
				this.TotalHazVolume += Constants.Volume.ConvertSafe(line.ActualVolume, line.ActualVolumeUQ, VolumeUQ);

				foreach (UNDGSubstanceWrapper undg in line.UNDGs)
				{
					if (!this.PackageDetailsWithHazCat.IsEmpty)
					{
						this.PackageDetailsWithHazCat += "\n";
					}
					this.PackageDetailsWithHazCat += undg.SummaryWithEMSCode;
				}

				var helper = new UNDGSubstanceWrapperHelper();
				var summary = helper.GetUNDGPackagesSummary(line.UNDGs);
				if (!summary.IsEmpty)
				{
					this.PackageDetailsWithHazCat += "\n" + summary;
				}
			}
		}
	}

	public class LoadListPackLineCollection : NonPersistentBusinessObjectCollection<LoadListPackLine>
	{
		public LoadListPackLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new LoadListPackLine(null, null);
		}
	}
}
