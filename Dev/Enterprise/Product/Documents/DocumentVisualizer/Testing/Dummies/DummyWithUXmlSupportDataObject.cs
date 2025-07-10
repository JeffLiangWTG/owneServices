using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyWithUXmlSupportDataObject : TopLevelDataObject
	{
		public override IDataContextDataObject DataContext { get; set; }

		public ZString Code { get; set; }
		public ZString Description { get; set; }

		public List<DummyWithUXmlSupportDataObject> DummyCollection { get; set; }
	}
}