using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Integration.Schedule;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class TransportCollection : Freight.Business.TransportCollection
{
	public TransportCollection(ITransportParentCommon parent) : base(parent)
	{
	}

	protected override void SetDefaultsForNewChild(BusinessObject newChild)
	{
		base.SetDefaultsForNewChild(newChild);

		if (newChild is Transport newTransport)
		{
			DefaultLegOrder(newTransport);
		}
	}

	#region Implementation

	void DefaultLegOrder(Transport newTransport)
	{
		var maxLegOrder = this.Cast<Transport>().MaxOrDefault(x => x.JW_LegOrder);

		var legOrder = maxLegOrder < byte.MaxValue
			? (byte)(maxLegOrder + 1)
			: byte.MaxValue;

		newTransport.JW_LegOrder = legOrder;
	}

	#endregion
}
