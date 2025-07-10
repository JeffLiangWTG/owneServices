using System.Collections.Generic;

using CargoWise.ComponentModel;

using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public partial class ImportMetaData : IDataObject
	{
		public ImportMetaData()
		{
		}

		public ImportMetaData(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		[Mandatory]
		public InstructionType? Instruction { get; set; }
		public PostingInstruction? PostingInstruction { get; set; }
		public List<MatchingCriteria> MatchingCriteriaCollection { get; private set; }
	}
}
