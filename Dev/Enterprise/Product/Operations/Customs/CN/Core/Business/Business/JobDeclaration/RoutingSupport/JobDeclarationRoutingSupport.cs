using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.CN.Business
{
	partial class JobDeclaration : ITransportParent
	{
		TransportSupporter ITransportParent.TransportSupporter => new JobDeclarationTransportSupporter(this);

		ZString GetLastPortBeforeEntry()
		{
			var routingCollection = ((IRoutingSupport)this).TransportsIncludingRelated.Cast<Transport>();
			var lastPortBeforeEntry = routingCollection
							.Where(x => x.JW_RL_NKDiscPort.Left(2) == Core.Constants.CountryCodes.China && x.JW_RL_NKLoadPort.Left(2) != Core.Constants.CountryCodes.China)
							.OrderBy(x => x.JW_LegOrder).LastOrDefault()?.JW_RL_NKLoadPort ?? ZString.Empty;

			if (lastPortBeforeEntry.IsEmpty)
			{
				lastPortBeforeEntry = routingCollection
							 .Where(x => x.JW_RL_NKDiscPort.Left(2) != Core.Constants.CountryCodes.China && x.JW_RL_NKLoadPort.Left(2) != Core.Constants.CountryCodes.China)
							 .OrderBy(x => x.JW_LegOrder).LastOrDefault()?.JW_RL_NKDiscPort ?? ZString.Empty;
			}

			return lastPortBeforeEntry.IsEmpty ? JE_RL_NKPortOfLoading : lastPortBeforeEntry;
		}

		protected override RoutingCollection GetNewTransportsIncludingRelated()
		{
			var result = base.GetNewTransportsIncludingRelated();
			result.CountChanged += TransportsIncludingRelated_CountChanged;
			return result;
		}

		void TransportsIncludingRelated_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			DefaultLastPortBeforeEntryIfNeeded();
		}

		internal void DefaultLastPortBeforeEntryIfNeeded()
		{
			if (IsImport)
			{
				var oldLastPort = flastPortBeforeEntry.HasValue ? JE_LastPortBeforeEntry : null;
				flastPortBeforeEntry = null;
				if (oldLastPort != JE_LastPortBeforeEntry)
				{
					LastPortBeforeEntryDefaulter.DefaultPort();
				}
			}
		}
	}
}
