using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public class AdditionalTariffCollection : NonPersistentBusinessObjectCollection<AdditionalTariff>
	{
		public AdditionalTariffCollection(IAdditionalTariffParent parent) : base(parent.Factory)
		{
			this.parent = parent;
		}
		readonly IAdditionalTariffParent parent;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var tariffDetail = parent.CusLineTariffDetails.AddNew() as CusLineTariffDetail;
			var legalAct = parent.LegalActInfos.AddNew();
			return new AdditionalTariff(legalAct, tariffDetail);
		}

		public BusinessObject AddNew(ZString subject)
		{
			var additionalTariff = AddNew();
			additionalTariff.LegalActSubject = subject;
			return additionalTariff;
		}

		public override void Load()
		{
			if (!IsLoaded)
			{
				RemoveAll();

				var legalActTyps = AdditionalTariff.GetLegalActSubjects(parent);

				foreach (var legalAct in parent.LegalActInfos.Cast<LegalActInfo>().Where(x => legalActTyps.ContainsCode(x.CSI_SubType)))
				{
					var tariffDetail = parent.CusLineTariffDetails.Cast<CusLineTariffDetail>().FirstOrDefault(x => x.BZ_LegalActSubject == legalAct.CSI_SubType);
					Add(new AdditionalTariff(legalAct, tariffDetail));
				}
			}

			IsLoaded = true;
		}

		public void Rebuild()
		{
			IsLoaded = false;
			Load();
		}

		public AdditionalTariff FindBySubject(ZString subject) => this.Cast<AdditionalTariff>().FirstOrDefault(x => x.LegalActSubject == subject);
	}
}
