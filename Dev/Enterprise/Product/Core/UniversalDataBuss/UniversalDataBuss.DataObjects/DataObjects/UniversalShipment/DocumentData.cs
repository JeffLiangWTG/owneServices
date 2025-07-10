using System.Collections.Generic;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public partial class DocumentData : IDataObject
	{
		public DocumentData()
		{
		}

		public DocumentData(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		public List<SystemDefinedData> SystemDefinedDataCollection { get; private set; }

		public List<UserDefinedData> UserDefinedDataCollection { get; private set; }
	}
}
