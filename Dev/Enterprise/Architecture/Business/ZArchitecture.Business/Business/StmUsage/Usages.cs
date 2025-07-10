using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.ZArchitecture.Business.Statistics.Xml
{
	public interface IUsages : IEnumerable<IUsage>
	{
		string AsXml();
		IEnumerable<IUsage> Children { get; }
	}

	class Usages : XmlSerializableSetting, IUsages
	{
		readonly List<Usage> nodes;

		public Usages()
		{
			nodes = new List<Usage>();
		}

		public void AddNode(Usage node) => nodes.Add(node);
		public void RemoveNode(Usage node) => nodes.Remove(node);

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		[XmlElement("Usage")]
		public Usage[] Nodes
		{
			get
			{
				return nodes.ToArray();
			}
			set
			{
				foreach (var node in Nodes)
				{
					RemoveNode(node);
				}

				foreach (var node in value)
				{
					AddNode(node);
				}
			}
		}

		IEnumerable<IUsage> IUsages.Children => Nodes;

		public virtual IEnumerator<IUsage> GetEnumerator()
		{
			var usages = new List<IUsage>();
			nodes.ForEach(n => usages.AddRange(n));
			return usages.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
