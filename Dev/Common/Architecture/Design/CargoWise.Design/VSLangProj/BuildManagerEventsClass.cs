namespace VSLangProj
{
	using System;
	using System.Runtime.InteropServices;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	/// <summary>Refer to <see cref="T:VSLangProj.BuildManagerEvents" /> for this functionality. Do not instantiate from this class.</summary>
	[ComImport, ClassInterface((short)0), ComSourceInterfaces("VSLangProj._dispBuildManagerEvents\0"), Guid("66923B02-677B-4920-A319-F8925A0BA8A8"), TypeLibType(2)]
	internal abstract class BuildManagerEventsClass : _BuildManagerEvents, BuildManagerEvents, _dispBuildManagerEvents_Event
	{
		/// <summary>Raised when a project item that generates a portable executable is deleted from the project.</summary>
		public abstract event _dispBuildManagerEvents_DesignTimeOutputDeletedEventHandler DesignTimeOutputDeleted;

		/// <summary>Raised when a custom tool that results in a portable executable being generated or updated is run on a project item.</summary>
		public abstract event _dispBuildManagerEvents_DesignTimeOutputDirtyEventHandler DesignTimeOutputDirty;
	}
}

