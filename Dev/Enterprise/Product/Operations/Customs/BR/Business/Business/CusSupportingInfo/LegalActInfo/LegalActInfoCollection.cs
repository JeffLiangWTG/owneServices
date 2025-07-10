using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class LegalActInfoCollection : CusSupportingInfoCollection<LegalActInfo>
	{
		public LegalActInfoCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.LegalAct)
		{
		}

		public LegalActInfo AddNew(ZString subject)
		{
			var legalAct = AddNew();
			using (legalAct.SuspendSettingHasChanges())
			{
				legalAct.CSI_SubType = subject;
			}
			return legalAct;
		}

		public LegalActInfo FindBySubject(ZString subject)
		{
			return this.Cast<LegalActInfo>().FirstOrDefault(x => x.CSI_SubType == subject);
		}
	}
}
