using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	internal sealed class DocAgencyDetentionAdviceLine_FromMovement : DocAgencyDetentionAdviceLine
	{
		public DocAgencyDetentionAdviceLine_FromMovement(ContainerMovement movement, BusinessObjectFactory factory)
			: base(movement, factory) { }

		protected override ZString GetDetentionType()
		{
			return "EXP";
		}

		protected override ZString GetContainerNo()
		{
			RefContainerStock stock;
			if ((stock = WrappedMovement.Stock) != null)
			{
				return stock.R6_ContainerNum;
			}
			else
			{
				return ZString.Empty;
			}
		}

		protected override ContainerTypeWrapper GetContainerType()
		{
			RefContainerStock stock;
			RefContainer type;

			if ((stock = WrappedMovement.Stock) != null && (type = stock.Container) != null)
			{
				return new ContainerTypeWrapper(type, Factory);
			}
			else
			{
				return null;
			}
		}

		protected override ZString GetVesselName()
		{
			JobVoyage voyage;

			if ((voyage = WrappedMovement.Voyage) != null)
			{
				return voyage.JV_RV_NKVessel;
			}
			else
			{
				return ZString.Empty;
			}
		}

		protected override ZString GetVoyageNo()
		{
			JobVoyage voyage;

			if ((voyage = WrappedMovement.Voyage) != null)
			{
				return voyage.JV_VoyageFlight;
			}
			else
			{
				return ZString.Empty;
			}
		}

		protected override LocationWrapper GetDetentionPort()
		{
			OrgAddress depot;
			RefUNLOCO port;

			if ((depot = WrappedMovement.Depot) != null && (port = depot.EffectiveRelatedPortCode) != null)
			{
				return new LocationWrapper(port.RL_Code, Factory);
			}
			else
			{
				return null;
			}
		}

		protected override ZString GetBillNumber()
		{
			return ZString.Empty;
		}

		protected override ZDateTime GetReleaseDate()
		{
			return WrappedMovement.E9_MovementDate;
		}

		protected override ZDateTime GetRequiredDate()
		{
			ZDateTime release = ReleaseDate;

			if (!release.IsEmpty)
			{
				return release.AddDays(WrappedMovement.RelatedInfo.ExportDetentionFreeDays);
			}
			else
			{
				return ZDateTime.Empty;
			}
		}

		protected override ZDateTime GetPickupDate()
		{
			return ZDateTime.Empty;
		}

		#region Implementation

		ContainerMovement WrappedMovement
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ContainerMovement)WrappedObject; }
		}

		#endregion

	}
}
