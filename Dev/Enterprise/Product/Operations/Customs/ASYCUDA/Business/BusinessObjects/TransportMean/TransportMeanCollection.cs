using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Integration.Schedule;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class TransportMeanCollection : TransportCollection
	{
		public TransportMeanCollection(ITransportParentCommon parent) : base(parent)
		{
		}

		public new TransportMean this[int index] => (TransportMean)base[index];

		public new TransportMean AddNew() => (TransportMean)base.AddNew();

		protected override void SetDefaultsForNewChild(BusinessObject newChild)
		{
			base.SetDefaultsForNewChild(newChild);
			if (newChild is TransportMean newTransport)
			{
				newTransport.JW_TransportMode = Core.Constants.TransportModes.Road;
				DefaultLegOrder(newTransport);
			}
		}

		void DefaultLegOrder(Transport newTransport)
		{
			var maxLegOrder = this.Cast<TransportMean>().MaxOrDefault(x => x.JW_LegOrder);

			var legOrder = maxLegOrder < byte.MaxValue
				? (byte)(maxLegOrder + 1)
				: byte.MaxValue;

			newTransport.JW_LegOrder = legOrder;
		}
	}
}
