using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("Description"), WrapperTypeName("Milestone")]
	public class WorkflowItemWrapper : GenericWrapper
	{
		#region Constructors

		public WorkflowItemWrapper(ProcessTask task, BusinessObjectFactory factoryToWrap)
			: base(task, factoryToWrap)
		{ }

		public static WorkflowItemWrapper New(ProcessTask task, BusinessObjectFactory factoryToWrap)
		{
			return task != null ? new WorkflowItemWrapper(task, factoryToWrap) : null;
		}

		#endregion

		public ProcessTask WrappedTask
		{
			get { return (ProcessTask)WrappedObject; }
		}

		#region Fields

		public ZString Description
		{
			get
			{
				return WrappedTask.P9_Description;
			}
		}

		public ZString Type
		{
			get
			{
				return WrappedTask.P9_Type;
			}
		}

		public ZString TypeDescription
		{
			get
			{
				return WrappedTask.TypeDescription;
			}
		}

		public ZDateTime ScheduledDate => WrappedTask.P9_ScheduledDate.ToZDateTime();

		public ZDateTime ActualDate => WrappedTask.P9_ActualDate.ToZDateTime();

		public ZDateTime DateTime
		{
			get
			{
				return ActualDate.IsEmpty && DocumentsDataRegistry.Instance.IncludeEstimatedMilesonesOnPODDocument.Value ? ScheduledDate : ActualDate;
			}
		}

		#endregion

		#region Flags

		public ZBool EstimatedToBeShown
		{
			get { return ActualDate.IsEmpty && DocumentsDataRegistry.Instance.IncludeEstimatedMilesonesOnPODDocument.Value; }
		}

		public ZString EstimatedIsShownFlag
		{
			get
			{
				return EstimatedToBeShown ? new ZString("*") : ZString.Empty;
			}
		}

		#endregion
	}
}
