using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ResourceStrings.Module
{
	public partial class LocalLanguagesFilterControl : ZFilterStripControl
	{
		[Obsolete("Use the constructor that takes a collection and filter strip, this constructor is just for the designer", true)]
		public LocalLanguagesFilterControl()
		{
			InitializeComponent();
		}

		public LocalLanguagesFilterControl(IBusinessObjectCollection collection, FilterStripBusinessObject strip) : base(collection, strip)
		{
			InitializeComponent();
		}
	}
}
