using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.Module.StandAloneFsrEnquiry
{
	public partial class StandAloneFsrEnquiryFilterStripControl : ZFilterStripControl
	{
		[Obsolete("Use the constructor that takes a collection and filter strip, this constructor is just for the designer", true)]
		public StandAloneFsrEnquiryFilterStripControl()
		{
			InitializeComponent();
		}

		public StandAloneFsrEnquiryFilterStripControl(IBusinessObjectCollection collection, FilterStripBusinessObject strip)
			: base(collection, strip)
		{
			InitializeComponent();
		}
	}
}
