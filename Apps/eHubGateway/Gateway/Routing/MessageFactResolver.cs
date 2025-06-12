using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using eServices.eHubRoutingRuleEngine;

namespace CargoWise.eHub.Gateway.Routing
{
	public class MessageFactResolver : IFactResolver, IDisposable
	{
		private eHubGatewayMessage message;
		private XPathDocument xpDoc;
		private XPathNavigator xpNav;
		private Dictionary<string, string> factCache = new Dictionary<string, string>(20);

		public MessageFactResolver(eHubGatewayMessage message)
		{
			this.message = message ?? throw new ArgumentNullException(nameof(message));
		}

		private void ParseDocument()
		{
			message.MessageStream.Seek(0, SeekOrigin.Begin);
			var messageStream = new MemoryStream();
			message.MessageStream.DecodeAndDecompress().CopyTo(messageStream);
			messageStream.Seek(0, SeekOrigin.Begin);
			xpDoc = new XPathDocument(messageStream);
			xpNav = xpDoc.CreateNavigator();
		}

		public void Resolve(Fact[] facts)
		{
			foreach (var fact in facts)
			{
				switch (fact.Type)
				{
					case "XPATH":
					case "XPATHNAV":
						if (factCache.TryGetValue(fact.Query, out var cacheValue))
						{
							fact.Value = cacheValue;
						}
						else
						{
							if (xpNav == null) ParseDocument();
							var node = xpNav?.SelectSingleNode(fact.Query);
							var value = node != null ? node.TypedValue as string : String.Empty;
							fact.Value = value;
							factCache[fact.Name] = value;
						}
						break;
					case "XPATHNAVFUNC":
						if (factCache.TryGetValue(fact.Query, out cacheValue))
						{
							fact.Value = cacheValue;
						}
						else
						{
							if (xpNav == null) ParseDocument();
							var resultValue = xpNav?.Evaluate(fact.Query);
							var value = resultValue?.ToString() ?? string.Empty;
							fact.Value = value;
							factCache[fact.Name] = value;
						}
						break;

					case "PROPERTY" when (fact.Name == "MessageType"):
						if (factCache.TryGetValue(fact.Query, out cacheValue))
						{
							fact.Value = cacheValue;
						}
						else
						{
							if (xpNav == null) ParseDocument();
							xpNav.MoveToRoot();
							xpNav.MoveToFollowing(XPathNodeType.Element);

							var value = string.IsNullOrWhiteSpace(xpNav.NamespaceURI)
								? xpNav.LocalName
								: $"{xpNav.NamespaceURI}#{xpNav.LocalName}";
							fact.Value = value;
							factCache[fact.Query] = value;
						}
						break;
				}
			}
		}

		private bool disposedValue;
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// no disposable managed object to dispose
				}
				xpDoc = null;
				xpNav = null;
				message = null;
				factCache = null;
				disposedValue = true;
			}
		}
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}