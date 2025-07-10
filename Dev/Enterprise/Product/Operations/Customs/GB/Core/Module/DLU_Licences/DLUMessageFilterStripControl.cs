using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.Module.DLU
{
	public partial class DLUMessageFilterStripControl : ZFilterStripControl
	{
		[Obsolete("Use the constructor that takes a collection and filter strip, this constructor is just for the designer", true)]
		public DLUMessageFilterStripControl()
		{
			InitializeComponent();
		}

		public DLUMessageFilterStripControl(IBusinessObjectCollection collection, FilterStripBusinessObject strip)
			: base(collection, strip)
		{
			InitializeComponent();
		}
	}
}
