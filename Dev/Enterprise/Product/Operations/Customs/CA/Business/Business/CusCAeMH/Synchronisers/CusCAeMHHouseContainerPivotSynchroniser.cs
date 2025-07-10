using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHHouseContainerPivotSynchroniser : BusinessObjectSynchroniser
	{
		public CusCAeMHHouseContainerPivotSynchroniser(CusCAeMHHouseContainerPivot destination, PackLine source)
			: base(destination, source)
		{ }

		protected new PackLine Source
		{
			get { return (PackLine)base.Source; }
		}

		protected new CusCAeMHHouseContainerPivot Destination
		{
			get { return (CusCAeMHHouseContainerPivot)base.Destination; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (Source != null && !Source.IsDeleted && Destination != null && !Destination.IsDeleted)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.BPA_BQ_ContainerInfo, GetContainerPK, () => { return new[] { Source.JL_JCInfo }; }));
				Destination.MasterBill.Containers.CountChanged -= Container_CountChanged;
				Destination.MasterBill.Containers.CountChanged += Container_CountChanged;
			}
		}

		void Container_CountChanged(object sender, EventArgs e)
		{
			Synchronise();
		}

		protected override void UnHookSynchronisers()
		{
			base.UnHookSynchronisers();
			if (Destination != null && !Destination.IsDeleted)
			{
				Destination.MasterBill.Containers.CountChanged -= Container_CountChanged;
			}
		}

		IZType GetContainerPK()
		{
			var masterBill = Destination.HouseBill.MasterBill;
			CusCAeMHContainer container = null;
			if (masterBill != null)
			{
				var containerNum = Source.ContainerNumberForConsol(masterBill.Consol);
				if (containerNum.IsEmpty)
				{
					containerNum = Core.Constants.ContainerModes.NonContainerised;
				}

				container = masterBill.Containers.FirstOrDefault(x => x.BQ_ContainerNumber == containerNum);
			}
			return container != null ? container.PK : ZGuid.Empty;
		}
	}
}
