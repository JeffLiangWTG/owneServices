using System.Xml.Linq;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IVisualizerDocumentData
	{
		bool HasChanges { get; }

		string Name { get; set; }
		object Parent { get; set; }

		XDocument ReadXml();
		void WriteXml(XDocument document);

		void Reload();

		void ReloadSafe();

		void Save();
	}
}
