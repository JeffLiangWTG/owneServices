using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.Module
{
	public partial class UCC6TemporaryStorageFilterControl : ZFilterStripControl
	{
		[Obsolete("Use the constructor that takes a collection and filter strip, this constructor is just for the designer", true)]
		public UCC6TemporaryStorageFilterControl()
		{
			InitializeComponent();
		}

		public UCC6TemporaryStorageFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
