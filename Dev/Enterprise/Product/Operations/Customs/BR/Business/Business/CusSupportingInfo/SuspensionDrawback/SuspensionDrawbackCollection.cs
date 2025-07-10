using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class SuspensionDrawbackCollection : Customs.Business.CusSupportingInfoCollection<SuspensionDrawback>
	{
		public SuspensionDrawbackCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.SuspensionDrawback)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var line = (SuspensionDrawback)child;
			line.CSI_Tariff = line.Parent?.JI_Tariff ?? ZString.Empty;
		}
	}
}
