using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.MasterFiles;

namespace Enterprise.Customs.EU.Module
{
	public partial class CusClassificationFilterControl : Customs.Module.CusClassificationFilterControl
	{
		[System.Obsolete("Use the constructor that takes a collection and/or business object, this constructor is just for the designer")]
		public CusClassificationFilterControl()
		{
			InitializeComponent();
		}

		public CusClassificationFilterControl(IBusinessObjectCollection gridCollection, CusClassificationFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			FilteredGrid.SetAvailability(false, CusClassification.Schema.CC_TariffNum);
			base.SetDataBinding(dataSource, dataMember);
		}
	}
}
