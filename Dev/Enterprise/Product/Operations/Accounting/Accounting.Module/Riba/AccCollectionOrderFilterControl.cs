using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Riba;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class AccCollectionOrderFilterControl : ZFilterStripControl
	{
		protected override ZFilterStrip NewZFilterStrip()
		{
			return new ZFilterStrip();
		}

		public AccCollectionOrderFilterControl(IBusinessObjectCollection gridCollection, AccCollectionOrderFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		public AccCollectionOrderCollection OrderCollection => GridCollection as AccCollectionOrderCollection;
	}
}

