using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class DocumentDeliveriesProvider : IDocumentDeliveriesProvider
	{
		public IReadOnlyCollection<IDocumentDelivery> GetDocumentDeliveries(IStmMenuItem menuItem, BusinessObject bizObj)
		{
			if (menuItem == null
				|| bizObj == null)
			{
				return Array.Empty<IDocumentDelivery>();
			}

			var builder = new DocumentInfoBuilder(bizObj, menuItem);

			var res = builder
				.CreateDocumentInfos()
				.Where(info => info.Descriptor?.PrintInstructions != null)
				.SelectMany(info => info
					.Descriptor
					.PrintInstructions
					.DeliveryModes
					.Select(deliveryMode => new PrintDocumentDelivery(info, deliveryMode)))
				.ToArray();

			return res;
		}
	}
}
