using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	/// <summary>
	/// Wraps a Run Sheet Instruction
	/// Documents group by the Instruction (Sequence), so create multiple Run Sheet Instruction Wrappers for each Confirmation
	/// </summary>
	public class RunSheetInstructionWrapper : InstructionWrapper
	{
		public RunSheetInstructionWrapper(DtbConsignmentRunSheetInstruction groupedInstruction, IConsignmentAction perAction, BusinessObjectFactory factory)
			: base(groupedInstruction ?? factory.GetNull<DtbConsignmentRunSheetInstruction>(), perAction ?? factory.GetNull<DtbConsignmentConfirmation>(), factory)
		{
		}

		#region Instruction Properties

		#region GetAddress

		protected override AddressWrapper GetAddress()
		{
			return RunSheetInstructionBO.IsOwnDepot ? new AddressWrapper("DEPOT", Factory) : new AddressWrapper(RunSheetInstructionBO.Address, Factory);
		}

		#endregion

		#region GetDropMode

		protected override CodeAndDescriptionWrapper GetDropMode()
		{
			return ConsignmentInstructionBO != null
				? new CodeAndDescriptionWrapper(ConsignmentInstructionBO.DropMode, new TransportBindToLists(Factory).DropModes, Factory)
				: CodeAndDescriptionWrapper.Empty;
		}

		#endregion

		#region GetInstructionType

		protected override ZString GetInstructionType()
		{
			var result = ZString.Empty;
			var hasPickups = RunSheetInstructionBO.ConsignmentActions.Any(a => a.IsPickUp);
			var hasDeliveries = RunSheetInstructionBO.ConsignmentActions.Any(a => a.IsDelivery);

			if (hasPickups && hasDeliveries)
			{
				result = InstructionTypes.Descriptions.Multi;
			}
			else if (hasPickups)
			{
				result = InstructionTypes.Descriptions.PickUp;
			}
			else if (hasDeliveries)
			{
				result = InstructionTypes.Descriptions.Delivery;
			}

			return result;
		}

		#endregion

		#region GetStatus

		protected override ZString GetStatus()
		{
			return ConsignmentInstructionBO != null ? ConsignmentInstructionBO.Status : ZString.Empty;
		}

		#endregion

		#region GetServiceInstruction

		protected override ZString GetServiceInstruction()
		{
			return ConsignmentInstructionBO != null ? ConsignmentInstructionBO.ServiceInstruction : ZString.Empty;
		}

		#endregion

		#region GetEquipment

		protected override ZString GetEquipment()
		{
			return ConsignmentInstructionBO != null && ConsignmentInstructionBO.Equipment != null ? ConsignmentInstructionBO.Equipment.RQ_DescriptionMultilingual : ZString.Empty;
		}

		#endregion

		#region GetSequence

		protected override ZInt GetSequence()
		{
			return RunSheetInstructionBO.K1_Sequence;
		}

		#endregion

		#region GetReceivedBy

		protected override ZString GetReceivedBy()
		{
			return ConsignmentConfirmationBO != null ? ConsignmentConfirmationBO.ReceivedBy : ZString.Empty;
		}

		#endregion

		#region GetReceivedBySignature

		protected override Image GetReceivedBySignature()
		{
			return ConsignmentConfirmationBO != null ? new SignatureDrawer(ConsignmentConfirmationBO.ReceivedBySignature).Image : null;
		}

		#endregion

		#region GetRequiredFromLabelValuePairCore

		protected override LabelValuePairWrapper GetRequiredFromLabelValuePairCore()
		{
			return FormatRequiredTime(Res.GetString("RouteInstructionWrapper|RequiredFrom", "Required From"), ConsignmentConfirmationBO.RequiredFrom);
		}

		LabelValuePairWrapper FormatRequiredTime(string caption, ZDateTime time)
		{
			return new LabelValuePairWrapper(caption, time, Factory);
		}

		#endregion

		#region GetRequiredToLabelValuePairCore

		protected override LabelValuePairWrapper GetRequiredToLabelValuePairCore()
		{
			return FormatRequiredTime(Res.GetString("RouteInstructionWrapper|RequiredTo", "Required To"), ConsignmentConfirmationBO.RequiredTo);
		}

		#endregion

		#region GetTimeIn

		protected override ZDateTime GetTimeIn()
		{
			return RunSheetInstructionBO != null ? RunSheetInstructionBO.K1_TimeIn.ToLocalZDateTime() : ZDateTime.Empty;
		}

		#endregion

		#region GetTimeOut

		protected override ZDateTime GetTimeOut()
		{
			return RunSheetInstructionBO != null ? RunSheetInstructionBO.K1_TimeOut.ToLocalZDateTime() : ZDateTime.Empty;
		}

		#endregion

		#endregion

		#region Package Properties

		#region GetPackageDivotQuantity

		protected override ZInt GetPackageDivotQuantity()
		{
			return Packages.Sum(p => p.KP_PackageQty);
		}

		#endregion

		#region GetPackageDivotSequence

		protected override ZInt GetPackageDivotSequence()
		{
			return 0; // consignments don't support Package Divots, so never group by instruction per Divot
		}

		#endregion

		#region GetPackageType

		protected override ZString GetPackageType()
		{
			var result = ZString.Empty;

			var packages = Packages;
			if (packages.Any())
			{
				var firstPackage = packages.FirstOrDefault();
				result = packages.All(p => p.KP_F3_NKPackType == firstPackage.KP_F3_NKPackType)
						? firstPackage.KP_F3_NKPackType.ToString()
						: Res.GetString("RouteInstructionWrapper|PackageType", "Many");
			}

			return result;
		}

		#endregion

		#region GetWeight

		protected override WeightWrapper GetWeight()
		{
			var total = new ZWeight(0, Core.Constants.Weight.Kilograms);

			var packages = Packages;
			if (packages.Any())
			{
				var firstPackage = packages.FirstOrDefault();
				total = new ZWeight(firstPackage.KP_Weight, firstPackage.KP_WeightUQ);

				foreach (var package in packages.Skip(1))
				{
					total += new ZWeight(package.KP_Weight, package.KP_WeightUQ);
				}
			}

			return new WeightWrapper(total.Amount, total.Unit, 1, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		#endregion

		#region GetVolume

		protected override VolumeWrapper GetVolume()
		{
			var total = new ZVolume(0, Core.Constants.Volume.CubicMetres);

			var packages = Packages;
			if (packages.Any())
			{
				var firstPackage = packages.FirstOrDefault();
				total = new ZVolume(firstPackage.KP_Volume, firstPackage.KP_VolumeUQ);
				foreach (var package in packages.Skip(1))
				{
					total += new ZVolume(package.KP_Volume, package.KP_VolumeUQ);
				}
			}

			return new VolumeWrapper(total.Amount, total.Unit, new CodeDescriptionPairList(OLookUpEditType.Volume), Factory);
		}

		#endregion

		#region GetPackageID

		protected override ZString GetPackageID()
		{
			return Packages.Count() == 1 ? Packages.Single().KP_PackageID : ZString.Empty;
		}

		#endregion

		#region GetPackageDivotID

		protected override ZString GetPackageDivotID()
		{
			return PackageDivotQuantity != 0 ? ZString.Format((NoResString)"{0}x {1}", PackageDivotQuantity, PackageType) : ZString.Empty;
		}

		#endregion

		#region GetPackageDimensions

		protected override ZString GetPackageDimensions()
		{
			var result = ZString.Empty;

			if (Packages.Count() == 1)
			{
				var package = Packages.Single();
				if (package.IsContainer)
				{
					var containerType = package.Container.ContainerType;
					result = ZString.Format("{0} - {1}", containerType.RC_Code, containerType.RC_DescriptionMultilingual);
				}
				else
				{
					result = package.ToStringDimensionSummary();
				}
			}

			return result;
		}

		#endregion

		#region GetHasSingleConfirmationForWholePackage

		protected override ZBool GetHasSingleConfirmationForWholePackage()
		{
			return true; // currently Consignments only support this
		}

		#endregion

		#region GetUNDGsSummary

		protected override ZString GetUNDGsSummary()
		{
			var builder = new ZStringBuilder();

			foreach (var package in Packages)
			{
				foreach (var undg in package.UNDGs)
				{
					var undgWrapper = new UNDGSubstanceWrapper(undg, Factory);
					undgWrapper.ContainingPackage = new PackageWrapperFromPkgPackage(package, Factory);
					builder.Append(undgWrapper.SummaryWithContainingPackageID);
				}
			}

			return builder.ToStringWithDelimiterBetweenAppends(System.Environment.NewLine);
		}

		#endregion

		#endregion

		#region Implementation

		DtbConsignmentRunSheetInstruction RunSheetInstructionBO
		{
			get { return (DtbConsignmentRunSheetInstruction)WrappedBO; }
		}

		IConsignmentAddress ConsignmentInstructionBO
		{
			get { return ConsignmentConfirmationBO?.ConsignmentAddress; }
		}

		IConsignmentAction ConsignmentConfirmationBO
		{
			get { return ActionBO; }
		}

		IEnumerable<PkgPackage> Packages
		{
			get { return ConsignmentInstructionBO != null ? ConsignmentInstructionBO.GetPackages : new List<PkgPackage>(); }
		}

		#endregion
	}
}
