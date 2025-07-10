using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class TaxRegimeCollection : CusSupportingInfoCollection<TaxRegime>
	{
		public TaxRegimeCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.TaxRegime)
		{
		}

		public TaxRegime AddNew(ZString subject)
		{
			var taxRegime = AddNew();
			using (taxRegime.SuspendSettingHasChanges())
			{
				taxRegime.CSI_SubType = subject;
			}
			return taxRegime;
		}

		public TaxRegime FindBySubject(ZString subject)
		{
			return this.Cast<TaxRegime>().FirstOrDefault(x => x.CSI_SubType == subject);
		}

		public void DeleteBySubject(ZString subject) => FindBySubject(subject)?.Delete();
	}
}
