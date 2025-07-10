using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentVisualizer.Delivery
{
	public sealed class FormDeliverablesProvider : IFormDeliverablesProvider
	{
		public IReadOnlyCollection<IDeliverable> GetDeliverables(IStmMenuItem menuItem, IDocumentSupportable documentSupportable)
		{
			if (menuItem == null
				|| !(documentSupportable is BusinessObject bizObj))
			{
				return Array.Empty<IDeliverable>();
			}

			var provider = ObjectFactory.Get<IDocumentDeliveriesProvider>();
			var deliveries = provider.GetDocumentDeliveries(menuItem, bizObj);

			if (deliveries == null)
			{
				return Array.Empty<IDeliverable>();
			}

			var res = deliveries
				.Select(documentDelivery => new DocumentDeliverable(bizObj.Factory, documentDelivery))
				.ToArray();

			return res;
		}
	}
}
