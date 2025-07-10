using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ArchiveManager.Module.Schedule
{
	public partial class ArchiveScheduleFilterControl : ZFilterStripControl
	{
		[Obsolete("Use the constructor that takes a collection and filter strip, this constructor is just for the designer", true)]
		public ArchiveScheduleFilterControl()
		{
			InitializeComponent();
		}

		public ArchiveScheduleFilterControl(IBusinessObjectCollection collection, ArchiveScheduleFilterBusinessObject strip)
			: base(collection, strip)
		{
			InitializeComponent();
		}
	}
}
