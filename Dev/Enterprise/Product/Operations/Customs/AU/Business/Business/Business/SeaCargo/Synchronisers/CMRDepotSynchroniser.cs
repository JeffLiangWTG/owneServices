using System.Collections;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRDepotSynchroniser
	{
		public CMRDepotSynchroniser(CFSLoadListConsol loadList)
		{
			if (loadList == null)
			{
				ErrorReporter.ReportOnce("JimmyMcHat_LoadListNull_in_CMRDepotSynchroniser_ctor", "Object is null");
			}

			if (loadList != null && (loadList.Shipments == null || loadList.Containers == null))
			{
				if (loadList.Shipments == null)
				{
					ErrorReporter.ReportOnce("JimmyMcHat_Loadlist_shipments_isnull", "Object is null");
				}

				if (loadList.Containers == null)
				{
					ErrorReporter.ReportOnce("JimmyMcHat_loadlist_conts_isnull", "Object is null");
				}
			}
			this.LoadList = loadList;
			LoadChildSynchronisers();
		}

		#region Properties

		public readonly CFSLoadListConsol LoadList;

		protected ArrayList UnderbondSynchronisers
		{
			get
			{
				if (fUnderbondSynchronisers == null)
				{
					fUnderbondSynchronisers = new ArrayList();
				}
				return fUnderbondSynchronisers;
			}
		}
		ArrayList fUnderbondSynchronisers;

		#endregion

		public void Synchronise(Customs.Business.SynchroniseAction action)
		{
			OnSynchronise(new Customs.Business.SynchroniseEventArgs(action));
		}

		#region Implementation

		protected void LoadChildSynchronisers()
		{
			foreach (CFSContainer container in LoadList.Containers)
			{
				LoadContainerSynchroniser(container);
			}
			foreach (CFSShipment shipment in LoadList.Shipments)
			{
				LoadShipmentSynchroniser(shipment);
			}
		}

		protected void LoadContainerSynchroniser(CFSContainer container)
		{
			CusUnderbond[] underbonds = LoadUnderbondsForBusinessObject(container);
			foreach (CusUnderbond underbond in underbonds)
			{
				CMRDepotContainerUnderbondSynchroniser synchroniser = new CMRDepotContainerUnderbondSynchroniser(underbond, container);
				UnderbondSynchronisers.Add(synchroniser);
			}
		}

		protected void LoadShipmentSynchroniser(CFSShipment shipment)
		{
			CusUnderbond[] underbonds = LoadUnderbondsForBusinessObject(shipment);
			foreach (CusUnderbond underbond in underbonds)
			{
				CMRDepotShipmentUnderbondSynchroniser synchroniser = new CMRDepotShipmentUnderbondSynchroniser(underbond, shipment);
				UnderbondSynchronisers.Add(synchroniser);
			}
		}

		protected CusUnderbond[] LoadUnderbondsForBusinessObject(BusinessObject parent)
		{
			ZQuery filter = new ZQuery(CusUnderbondSchema.C4_ParentID, parent.PK);
			return (CusUnderbond[])parent.Factory.Load(typeof(CusUnderbond), filter);
		}

		protected void OnSynchronise(Customs.Business.SynchroniseEventArgs e)
		{
			foreach (Customs.Business.BusinessObjectSynchroniser synchroniser in UnderbondSynchronisers.ToArray(typeof(Customs.Business.BusinessObjectSynchroniser)))
			{
				synchroniser.Synchronise(e);
			}
		}

		#endregion
	}
}
