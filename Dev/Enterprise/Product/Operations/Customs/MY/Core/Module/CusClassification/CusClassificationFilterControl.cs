using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.MY.Module
{
	public partial class CusClassificationFilterControl : Customs.Module.CusClassificationFilterControl
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer")]
		public CusClassificationFilterControl()
		{
			InitializeComponent();
		}

		public CusClassificationFilterControl(IBusinessObjectCollection gridCollection, CusClassificationFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
