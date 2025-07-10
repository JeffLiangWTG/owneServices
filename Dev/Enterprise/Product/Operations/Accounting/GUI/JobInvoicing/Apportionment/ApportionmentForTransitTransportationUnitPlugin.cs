using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public class ApportionmentForTransitTransportationUnitPlugin : ApportionmentPlugin
	{
		public ApportionmentForTransitTransportationUnitPlugin(IBusiness hostEntity) : base(hostEntity)
		{
			if (!(hostEntity is IWhsItemReceiveTransportationUnit || hostEntity is IWhsItemDispatchTransportationUnit))
			{
				throw new NotSupportedException("This Apportionment only be used in the Transit Transportation Unit.");
			}
		}

		public override ResourceString MenuName => ResString.GetMultilingualString("f662d623-a819-42c3-88cd-dc8d53a7611a", "&Costing");
	}
}
