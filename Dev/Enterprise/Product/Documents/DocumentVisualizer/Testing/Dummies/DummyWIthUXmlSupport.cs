using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DocumentVisualizer.Testing
{
	[VisualizableDocumentsSupportable(typeof(DummyVisualizableDocumentSupporter))]
	[UniversalDataContext(DataContextType.DummyBusinessObject)]
	sealed class DummyWithUXmlSupport : DummyWithWorkflow, IDocumentSupportable
	{
		public DummyWithUXmlSupport(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		public new List<DummyBusinessObject> Collection => children ?? (children = new List<DummyBusinessObject>());
		List<DummyBusinessObject> children;

		public IMessagingExtensions MessagingExtensions { get; set; }

		#region IDocumentSupportable members

		DocumentSupporter IDocumentSupportable.DocumentSupporter => supporter ?? (supporter = new DummyWithUXmlSupportDocumentSupporter(this));
		DummyWithUXmlSupportDocumentSupporter supporter;

		string IDocumentSupportable.TableName => TableName;

		public string MessageBroker { get; set; } = string.Empty;
		public bool ShouldUseDraftWatermark { get; set; }

		#endregion
	}
}
