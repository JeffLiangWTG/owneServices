namespace VSLangProj
{
	using System.Runtime.InteropServices;

	/// <summary>Microsoft Internal Use Only.</summary>
	[ComVisible(false), ComEventInterface(typeof(_dispBuildManagerEvents), typeof(_dispBuildManagerEvents_EventProvider))]
	internal interface _dispBuildManagerEvents_Event
	{
		/// <summary>Microsoft Internal Use Only.</summary>
		event _dispBuildManagerEvents_DesignTimeOutputDeletedEventHandler DesignTimeOutputDeleted;

		/// <summary>Microsoft Internal Use Only.</summary>
		event _dispBuildManagerEvents_DesignTimeOutputDirtyEventHandler DesignTimeOutputDirty;
	}
}

