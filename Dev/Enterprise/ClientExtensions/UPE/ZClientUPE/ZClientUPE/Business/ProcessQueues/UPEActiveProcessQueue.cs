using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business
{
	public class UPEActiveProcessQueue : ActiveProcessQueue
	{
		#region Constructor

		public UPEActiveProcessQueue(ProcessQueue processQueue)
			: base(processQueue)
		{
		}

		public new static ActiveProcessQueue New(ProcessQueue processQueue)
		{
			return new UPEActiveProcessQueue(processQueue);
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		internal static bool IsSubTypeRegistered
		{
			get { return OverridableNewDelegate.IsOverriden; }
		}

		#endregion

		public override MultilingualString StatusCaption
		{
			get { return (NoResString)"Reason"; }
		}

		public override MultilingualString SubStatusCaption
		{
			get { return (NoResString)"Status"; }
		}

		public override MultilingualString ReasonCaption
		{
			get { return (NoResString)"Remarks"; }
		}

		public override bool IncludeLogsWithEmptyQueueName
		{
			get { return false; }
		}
	}
}
