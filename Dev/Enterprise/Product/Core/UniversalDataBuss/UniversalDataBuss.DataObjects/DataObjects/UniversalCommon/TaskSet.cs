using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public partial class TaskSet : IDataObject, ITaskCollectionParent, ITaskSetCollectionParent
	{
		public TaskSet()
		{
		}

		public TaskSet(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		[Mandatory, MaxLength(UniversalXmlInfo.MaxStringLength)]
		public ZString? Description { get; set; }

		[Mandatory]
		public CodeDescriptionPair Type { get; set; }

		[MaxLength(UniversalXmlInfo.MaxStringLength)]
		public ZString? CurrentStatus { get; set; }

		[MaxLength(UniversalXmlInfo.MaxStringLength)]
		public ZString? Notes { get; set; }

		public ZBool? IsActive { get; set; }

		public ZDateTimeOffset? EarliestStartDateUTC { get; set; }

		public ZDateTimeOffset? AgreedDeliveryDateUTC { get; set; }

		public CodeDescriptionPair DateAcceptability { get; set; }

		public Group ReleaseGroup { get; set; }

		public CodeDescriptionPair TaskSetStatus { get; set; }

		public List<Task> TaskCollection { get; private set; }

		public List<TaskSet> TaskSetCollection { get; private set; }
	}
}
