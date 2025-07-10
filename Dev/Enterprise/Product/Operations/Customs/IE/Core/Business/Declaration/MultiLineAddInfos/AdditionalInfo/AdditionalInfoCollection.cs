using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class AdditionalInfoCollection : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection
	{
		public AdditionalInfoCollection(BusinessObject parent) : base(parent)
		{
		}

		public new AdditionalInfo this[int index] => (AdditionalInfo)base[index];

		public new AdditionalInfo AddNew() => (AdditionalInfo)base.AddNew();

		public bool ContainsAdditionalInfoOfCode(ZString subType, IEnumerable<ZString> codes)
		{
			return this.Cast<AdditionalInfo>().Any(additionalInfo => additionalInfo.CSI_SubType == subType && codes.Contains(additionalInfo.CSI_Code));
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (child is AdditionalInfo addInfo && addInfo.IsUCC5AndIsImport)
			{
				((AdditionalInfo)child).CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			}
		}
	}
}
