using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentVisualizer.Delivery
{
	public interface IDocumentDeliveriesProvider
	{
		IReadOnlyCollection<IDocumentDelivery> GetDocumentDeliveries(IStmMenuItem menuItem, BusinessObject bizObj);
	}
}
