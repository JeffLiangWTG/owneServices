using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	[TestedType(typeof(BatchScheduleValueObjectDataAdapter))]
	sealed class BatchScheduleValueObjectDataAdapterTest : ScheduleXmlDataAdapterTest
	{
		protected override ValueObjectDataAdapter<JobVoyage, Xsd.Schedule> GetNewBizObjXmlDataAdapter()
		{
			return new BatchScheduleValueObjectDataAdapter();
		}
	}
}
