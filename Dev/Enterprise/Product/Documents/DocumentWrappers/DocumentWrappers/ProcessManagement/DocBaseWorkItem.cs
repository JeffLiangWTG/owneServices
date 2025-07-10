using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.DocumentWrappers.ProcessManagement
{
	public class DocBaseWorkItem : DocBaseWrapperWithJobHeader
	{
		protected DocBaseWorkItem(WorkItemCommon workItem, BusinessObjectFactory factory)
			: base(workItem, factory)
		{
		}

		public static DocBaseWorkItem New(WorkItemCommon workItem, BusinessObjectFactory factoryToWrap)
		{
			return (workItem == null) ? null : new DocBaseWorkItem(workItem, factoryToWrap);
		}

		WorkItemCommon WorkItem
		{
			get { return (WorkItemCommon)WrappedObject; }
		}

		public override DocJobHeader JobHeader
		{
			get { return DocJobHeader.New(WorkItem.Job, Factory); }
		}

		#region Properties

		public ZString WorkItemType
		{
			get { return WorkItem.WKI_WorkItemType; }
		}

		public ZString WorkItemArea
		{
			get { return WorkItem.WKI_WorkItemArea; }
		}

		#endregion
	}
}
