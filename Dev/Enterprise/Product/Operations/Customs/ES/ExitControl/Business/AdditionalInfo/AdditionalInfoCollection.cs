using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class AdditionalInfoCollection : EU.ExitControl.Business.AdditionalInfoCollection<AdditionalInfo>
	{
		public AdditionalInfoCollection(BusinessObject parent)
		: base(parent)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newElement = (AdditionalInfo)child;
			newElement.CSI_ItemNumber = Count > ZShort.Zero ? this.Cast<AdditionalInfo>().Max(x => x.CSI_ItemNumber) + 1 : (ZShort)1;
		}
	}
}
