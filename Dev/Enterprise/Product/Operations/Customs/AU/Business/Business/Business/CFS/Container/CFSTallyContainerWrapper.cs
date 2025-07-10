using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IFindOrCreateOutturnUI
	{
		void ShowNoConsolError();
		void ShowMultipleMatchesError();
		void ShowOutturnAlreadyLinked();
		void ShowShipmentOutturnsAlreadyLinked(int countOfAlreadyLinked);
		bool ShouldCreateAndLinkWhenNoMatchesFound();
		bool ShouldCreateAndLinkWhenOnlyHeaderFound();
		bool ShouldLinkWhenOutturnFound();
	}

	public class CFSTallyContainerWrapper : CFSContainerWrapper, IOutturnableLine, IObsoleteValidation, ICusUnderbondDependentCollectionParent, ICusUnderbondUnionCollectionParent
	{
		protected CFSTallyContainerWrapper(TallyContainer container)
			: base(container)
		{
			this.Container = container;
		}

		public readonly new TallyContainer Container;

		public static CFSTallyContainerWrapper Load(TallyContainer parent)
		{
			return (CFSTallyContainerWrapper)Load(typeof(CFSTallyContainerWrapper), parent);
		}

		#region Properties

		#region Header

		public TallyOutturnHeader Header
		{
			get
			{
				if (Outturn != null)
				{
					return Outturn.Header;
				}
				return null;
			}
		}

		#endregion

		#region Outturn

		public TallyOutturn Outturn
		{
			get
			{
				foreach (TallyOutturn outturn in Outturns)
				{
					var parent = outturn.Header;
					if (parent != null && parent.C6_OutturningPremiseID == LoaderOrCreator.CurrentBranchPremiseID)
					{
						return outturn;
					}
				}
				return Outturns.Count > 0 ? Outturns[0] : null;
			}
		}

		#endregion

		#region Outturns

		public CFSTallyContainerOutturnCollection Outturns
		{
			get
			{
				if (outturns == null)
				{
					outturns = new CFSTallyContainerOutturnCollection(this);
					RegisterEditableChildObject(outturns);
					outturns.Load();
				}
				return outturns;
			}
		}
		CFSTallyContainerOutturnCollection outturns;

		#endregion

		#endregion

		#region Find Or Create Outturn

		TallyOutturnLoaderOrCreator LoaderOrCreator
		{
			get
			{
				return loaderOrCreator ?? (loaderOrCreator = new TallyOutturnLoaderOrCreator(Container));
			}
		}
		TallyOutturnLoaderOrCreator loaderOrCreator;

		public TallyOutturn GetOutturnForTallyPlugin(IFindOrCreateOutturnUI ui)
		{
			TallyOutturn result = Outturn;

			if (result == null)
			{
				try
				{
					result = FindOrCreateOutturn(ui);
					if (result != null)
					{
						Outturns.Add(result);
						LinkShipments(ui);
					}
				}
				catch (TallyOutturnLoaderOrCreator.ContainerHasNoConsolException)
				{
					ui.ShowNoConsolError();
				}
				catch (TallyOutturnLoaderOrCreator.FoundTooManyMatchesException)
				{
					ui.ShowMultipleMatchesError();
				}
				catch (TallyOutturnLoaderOrCreator.AlreadyLinkedException)
				{
					ui.ShowOutturnAlreadyLinked();
				}
			}

			return result;
		}

		void LinkShipments(IFindOrCreateOutturnUI ui)
		{
			int shipmentAlreadyLinkedCount = 0;

			foreach (PackUnpackShipment shipment in Container.PackUnpackShipments)
			{
				try
				{
					CFSShipmentWrapper.Load(shipment).CreateAndLinkOutturnIfMissing(Header);
				}
				catch (DepotCusOutturnLoaderOrCreator.FoundLineAlreadyLinked)
				{
					shipmentAlreadyLinkedCount++;
				}
			}

			if (shipmentAlreadyLinkedCount > 0)
			{
				ui.ShowShipmentOutturnsAlreadyLinked(shipmentAlreadyLinkedCount);
			}
		}

		TallyOutturn FindOrCreateOutturn(IFindOrCreateOutturnUI ui)
		{
			TallyOutturn result = null;
			ZString question = ZString.Empty;

			result = LoaderOrCreator.FindOutturn();

			if (result == null)
			{
				TallyOutturnHeader header = LoaderOrCreator.FindHeader();

				if (header == null)
				{
					if (ui.ShouldCreateAndLinkWhenNoMatchesFound())
					{
						result = LoaderOrCreator.CreateOutturnAndHeader();
					}
				}
				else
				{
					if (ui.ShouldCreateAndLinkWhenOnlyHeaderFound())
					{
						result = LoaderOrCreator.CreateOutturn(header);
					}
				}
			}
			else
			{
				if (!ui.ShouldLinkWhenOutturnFound())
				{
					result = null;
				}
			}

			return result;
		}

		#endregion

		#region IOutturnableLine Members

		ZString IOutturnableLine.CargoStatus
		{
			get { return ZString.Empty; }
		}

		ZInt IOutturnableLine.PackagesManifested
		{
			get { return Container.JC_Calc_TotalPackages; }
		}

		#endregion
	}
}
