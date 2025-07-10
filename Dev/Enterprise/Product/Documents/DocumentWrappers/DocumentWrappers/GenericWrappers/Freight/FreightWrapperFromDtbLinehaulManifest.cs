using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Money;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportConsignment.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class FreightWrapperFromDtbLinehaulManifest : FreightWrapper
	{
		public FreightWrapperFromDtbLinehaulManifest(DtbLinehaulManifest linehaulManifest, BusinessObjectFactory factory)
			: base(linehaulManifest, factory)
		{
			Argument.NotNull(factory, "factory");
			LinehaulManifest = linehaulManifest ?? Factory.GetNull<DtbLinehaulManifest>();
		}

		readonly DtbLinehaulManifest LinehaulManifest;

		#region JobNumberHeading

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("b1ea2759-da2d-4ff3-b122-51322fc594b9", "Linehaul Manifest");
		}

		#endregion

		#region JobNumber

		protected override ZString GetJobNumber()
		{
			return LinehaulManifest.LHM_ManifestID;
		}

		#endregion

		#region Costs

		protected override CostWrapperCollection GetCosts()
		{
			return new CostWrapperCollection(new JobConsolCostCollection(Factory, LinehaulManifest), Factory);
		}

		#endregion

		#region Consignor

		protected override OrganisationWrapper GetConsignor()
		{
			return new OrganisationWrapper(OrganisationUsageType.Consignor, LinehaulManifest.OriginDepot, ContactType.Consignor, Factory);
		}

		#endregion

		#region Consignee

		protected override OrganisationWrapper GetConsignee()
		{
			return new OrganisationWrapper(OrganisationUsageType.Consignee, LinehaulManifest.DestinationDepot, ContactType.Consignee, Factory);
		}

		#endregion

		#region Charges

		protected override ChargeWrapperCollection GetCharges()
		{
			var result = new ChargeWrapperCollection(Factory);

			if (Job != null)
			{
				foreach (Charge charge in Job.Charges)
				{
					result.Add(new ChargeWrapper(charge, Factory));
				}
			}

			return result;
		}

		#endregion

		#region RunSheet

		protected override RunSheetWrapper GetRunSheet()
		{
			return new RunSheetWrapperFromLinehaulManifest(LinehaulManifest, Factory);
		}

		#endregion

		#region BookingInstructions

		protected override InstructionWrapperCollection GetBookingInstructions()
		{
			var collection = new InstructionWrapperCollection(Factory);

			var pickupInstruction = new LinehaulManifestPickupInstructionStrategy(LinehaulManifest, Factory);
			var deliveryInstruction = new LinehaulManifestDeliveryInstructionStrategy(LinehaulManifest, Factory);

			AddWrappersFromInstructionConfirmations(collection, LinehaulManifest.Packages, pickupInstruction);
			AddWrappersFromInstructionConfirmations(collection, LinehaulManifest.Packages, deliveryInstruction);

			return collection;
		}

		void AddWrappersFromInstructionConfirmations(InstructionWrapperCollection collection, DtbLinehaulManifestPackageCollection packages, LinehaulManifestCommonInstructionStrategy instruction)
		{
			foreach (var package in packages)
			{
				var instructionWrapper = new LinehaulManifestInstructionWrapper(package, instruction, Factory);
				collection.Add(instructionWrapper);
			}
		}

		#endregion
	}
}
