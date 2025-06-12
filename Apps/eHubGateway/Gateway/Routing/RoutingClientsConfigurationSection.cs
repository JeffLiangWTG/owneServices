using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace CargoWise.eHub.Gateway.Routing
{
	public class RoutingClientsConfigurationSection : IConfigurationSectionHandler
	{
		public object Create(object parent, object configContext, XmlNode section)
		{
			bool.TryParse(section.Attributes["Enabled"]?.Value, out var enabled);

			var clientList = new List<RoutingClientConfigurationItem>();

			foreach (XmlNode childNode in section.ChildNodes)
			{
				string clientId = childNode.Attributes["Id"]?.Value;
				if (clientId is null)
				{
					throw new ConfigurationErrorsException("Invalid Client Id.", childNode);
				}
				string routeId = childNode.Attributes["RouteId"]?.Value;
				if (routeId is null)
				{
					routeId = clientId;
				}
				RoutingMethod routingMethod;
				if (childNode.Attributes["RoutingMethod"] is null)
				{
					routingMethod = RoutingMethod.Standard;
				}
				else if (!Enum.TryParse(childNode.Attributes["RoutingMethod"]?.Value, out routingMethod))
				{
					throw new ConfigurationErrorsException("Invalid RoutingMethod.", childNode);
				}

				var cl = new RoutingClientConfigurationItem()
				{
					ClientId = clientId,
					RoutingMethod = routingMethod,
					RouteId = routeId
				};
				try
				{
					clientList.Add(cl);
				}
				catch (ArgumentException)
				{
					throw new ConfigurationErrorsException("A RoutingClient entry already exists for the specified client.", childNode);
				}
			}
			return new RoutingClientsConfiguration(enabled, clientList);
		}
	}
}
