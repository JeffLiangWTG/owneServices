using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHHouseContainerPivotCollectionSynchroniser : GenericCollectionSynchroniser<CusCAeMHHouseContainerPivotSynchroniser>
	{
		public CusCAeMHHouseContainerPivotCollectionSynchroniser(ForwardingShipment source, CusCAeMHHouse destination, ForwardingConsol consol)
			: base(source, destination, source.OuterPackLines, destination.Pivots)
		{
			this.consol = consol;
		}
		readonly ForwardingConsol consol;

		public new ForwardingShipment Source
		{
			get { return (ForwardingShipment)base.Source; }
		}

		#region Implementation

		protected override bool CompareBizosEqual(BusinessObject source, BusinessObject destination)
		{
			var packLine = (PackLine)source;
			var pivot = (CusCAeMHHouseContainerPivot)destination;
			var containerNum = packLine.ContainerNumberForConsol(consol);
			return containerNum == pivot.BPA_Calc_ContainerNumber || (containerNum.IsEmpty && pivot.BPA_Calc_ContainerNumber == Core.Constants.ContainerModes.NonContainerised);
		}

		protected override CusCAeMHHouseContainerPivotSynchroniser CreateNewSynchroniser(BusinessObject destination, BusinessObject source)
		{
			return new CusCAeMHHouseContainerPivotSynchroniser((CusCAeMHHouseContainerPivot)destination, (PackLine)source);
		}

		protected override void HookEvents()
		{
			base.HookEvents();
			foreach (PackLine packLine in Source.OuterPackLines)
			{
				HookPackLineChangeEvent(packLine);
			}
		}

		protected override void UnHookEvents()
		{
			base.UnHookEvents();
			foreach (PackLine packLine in Source.OuterPackLines)
			{
				UnHookPackLineChangeEvent(packLine);
			}
		}

		protected override void Collection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var packLine = e.BizObject as PackLine;
			if (packLine != null)
			{
				if (e.ItemAdded)
				{
					HookPackLineChangeEvent(packLine);
				}
				else
				{
					UnHookPackLineChangeEvent(packLine);
				}
			}
			base.Collection_CountChanged(sender, e);
		}

		void HookPackLineChangeEvent(PackLine packLine)
		{
			packLine.JL_JCInfo.ValueChanged -= JL_JCInfo_ValueChanged;
			packLine.JL_JCInfo.ValueChanged += JL_JCInfo_ValueChanged;
		}

		void UnHookPackLineChangeEvent(PackLine packLine)
		{
			packLine.JL_JCInfo.ValueChanged -= JL_JCInfo_ValueChanged;
		}

		void JL_JCInfo_ValueChanged(object sender, System.EventArgs e)
		{
			Synchronise();
		}

		protected override bool ShouldSynchronise(BusinessObject bizo)
		{
			var packLine = (ForwardingPackLine)bizo;
			return !Source.OuterPackLines.TakeWhile(x => x != packLine).Cast<ForwardingPackLine>().Any(x => x.ContainerNumberForConsol(consol) == packLine.ContainerNumberForConsol(consol));
		}

		#endregion
	}
}
