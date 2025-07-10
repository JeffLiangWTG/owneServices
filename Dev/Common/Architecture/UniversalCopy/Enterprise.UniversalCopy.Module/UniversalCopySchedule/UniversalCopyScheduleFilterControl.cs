using System;
using Enterprise.UniversalCopy.Module.UniversalCopySchedule;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.UniversalCopy.Module
{
	public partial class UniversalCopyScheduleFilterControl : ZFilterStripControl
	{
		[Obsolete("Use the constructor that takes a collection and filter strip, this constructor is just for the designer", true)]
		public UniversalCopyScheduleFilterControl()
		{
			InitializeComponent();
		}

		public UniversalCopyScheduleFilterControl(StmUniversalCopyCollection collection, FilterStripBusinessObject strip)
			: base(collection, strip)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
			=> new UniversalCopyScheduleFilterStrip();
	}
}
