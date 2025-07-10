using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class LinehaulManifestInstructionWrapper : InstructionWrapper
	{
		public LinehaulManifestInstructionWrapper(PkgPackage package, LinehaulManifestCommonInstructionStrategy instruction, BusinessObjectFactory factory)
			: base(package, factory)
		{
			Argument.NotNull(package, "Package");
			Argument.NotNull(instruction, "Instruction");
			Package = package;
			Instruction = instruction;
		}

		readonly PkgPackage Package;
		readonly LinehaulManifestCommonInstructionStrategy Instruction;

		#region Empty

		#region DropMode

		protected override CodeAndDescriptionWrapper GetDropMode()
		{
			return CodeAndDescriptionWrapper.Empty;
		}

		#endregion

		#region Status

		protected override ZString GetStatus()
		{
			return ZString.Empty;
		}

		#endregion

		#region ServiceInstruction

		protected override ZString GetServiceInstruction()
		{
			return ZString.Empty;
		}

		#endregion

		#region Equipment

		protected override ZString GetEquipment()
		{
			return ZString.Empty;
		}

		#endregion

		#region GetTimeIn

		protected override ZDateTime GetTimeIn()
		{
			return ZDateTime.Empty;
		}

		#endregion

		#region GetTimeOut

		protected override ZDateTime GetTimeOut()
		{
			return ZDateTime.Empty;
		}

		#endregion

		#endregion

		#region Address

		protected override AddressWrapper GetAddress()
		{
			return Instruction.GetAddress();
		}

		#endregion

		#region InstructionType

		protected override ZString GetInstructionType()
		{
			return Instruction.GetInstructionType();
		}

		#endregion

		#region GetSequence

		protected override ZInt GetSequence()
		{
			return Instruction.GetSequence();
		}

		#endregion

		#region Packages

		#region PackageDivotQuantity

		protected override ZInt GetPackageDivotQuantity()
		{
			return Package.KP_PackageQty;
		}

		#endregion

		#region PackageType

		protected override ZString GetPackageType()
		{
			return Package.KP_F3_NKPackType;
		}

		#endregion

		#region Weight

		protected override WeightWrapper GetWeight()
		{
			var weight = new ZWeight(Package.KP_Weight, Package.KP_WeightUQ);
			return new WeightWrapper(weight.Amount, weight.Unit, 3, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		#endregion

		#region Volume

		protected override VolumeWrapper GetVolume()
		{
			var volume = new ZVolume(Package.KP_Volume, Package.KP_VolumeUQ);
			return new VolumeWrapper(volume.Amount, volume.Unit, new CodeDescriptionPairList(OLookUpEditType.Volume), Factory);
		}

		#endregion

		#region ConsignorOrConsigneeAddress

		protected override ZString ConsignorOrConsigneeAddressCore
		{
			get
			{
				var result = ZString.Empty;
				if (Transport != null)
				{
					var consignoree = InstructionType == (ZString)InstructionTypes.Descriptions.PickUp ? Transport.Consignee : Transport.Consignor;
					if (consignoree != null)
					{
						result = GetOrganizationAddress(consignoree);
					}
				}

				return result;
			}
		}

		#endregion

		#region Transport

		protected override FreightWrapper TransportCore
		{
			get
			{
				FreightWrapper result = null;
				var packageJob = Package.PackageJob;
				if (packageJob != null)
				{
					var consignment = PkgPackageJob.LoadParent<IPackingParent>(packageJob);
					if (consignment != null)
					{
						result = FreightWrapper.New(consignment as BusinessObject, Factory)[0];
					}
				}

				return result;
			}
		}

		#endregion

		#region PackageID

		protected override ZString GetPackageID()
		{
			return Package.KP_PackageID;
		}

		#endregion

		#region PackageDivotSequence

		protected override ZInt GetPackageDivotSequence()
		{
			return ZInt.Zero; // manifest don't support Package Divots, so never group by instruction per Divot
		}

		#endregion

		#region PackageDivotID

		protected override ZString GetPackageDivotID()
		{
			return PackageDivotQuantity != 0 ? PackageType : ZString.Empty;
		}

		#endregion

		#region PackageDimensions

		protected override ZString GetPackageDimensions()
		{
			if (Package.IsContainer)
			{
				var containerType = Package.Container.ContainerType;
				return ZString.Format("{0} - {1}", containerType.RC_Code, containerType.RC_DescriptionMultilingual);
			}

			return Package.ToStringDimensionSummary();
		}

		#endregion

		#region HasSingleConfirmationForWholePackage

		protected override ZBool GetHasSingleConfirmationForWholePackage()
		{
			return true;
		}

		#endregion

		#region UNDGsSummary

		protected override ZString GetUNDGsSummary()
		{
			var builder = new ZStringBuilder();

			foreach (var undg in Package.UNDGs)
			{
				var undgWrapper = new UNDGSubstanceWrapper(undg, Factory);
				undgWrapper.ContainingPackage = new PackageWrapperFromPkgPackage(Package, Factory);
				builder.Append(undgWrapper.SummaryWithContainingPackageID);
			}

			return builder.ToStringWithDelimiterBetweenAppends(System.Environment.NewLine);
		}

		#endregion

		#endregion
	}
}
