using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class CusGoodsCatalogValidation : Customs.Business.CusGoodsCatalogValidation
	{
		public CusGoodsCatalogValidation(AutoCusGoodsCatalog parent) : base(parent)
		{
		}

		protected new CusGoodsCatalog Parent
		{
			get { return (CusGoodsCatalog)base.Parent; }
		}

		protected override void CheckCGC_AuthorityIdentifier()
		{
			base.CheckCGC_AuthorityIdentifier();
			if (!Parent.CGC_AuthorityIdentifier.IsNumbersOnlyOrEmpty)
			{
				Parent.CGC_AuthorityIdentifierInfo.AddError(Res.GetString("FF9A8288-500E-4D65-B418-1B06636B0955", "Authority Identifier should be only numbers."));
			}
		}

		protected override void CheckCGC_AuthorityVersion()
		{
			base.CheckCGC_AuthorityVersion();
			if (!Parent.CGC_AuthorityVersion.IsNumbersOnlyOrEmpty)
			{
				Parent.CGC_AuthorityVersionInfo.AddError(Res.GetString("763831FB-C255-47E6-9B57-41DFAD206C3C", "Authority Version should be only numbers."));
			}
		}

		protected override void CheckCGC_AuthorityStatus()
		{
			base.CheckCGC_AuthorityStatus();
			ListValidation.ErrorIfInvalidCode(Parent.CGC_AuthorityStatusInfo);
		}

		protected override void CheckCGC_Tariff()
		{
			base.CheckCGC_Tariff();

			var targetInfo = Parent.CGC_TariffInfo;
			MandatoryValidation.CheckEntered(targetInfo);

			if (!Parent.CGC_Tariff.IsEmpty && Parent.UniversalTariff == null)
			{
				targetInfo.AddMessageError(Res.GetString("96032A1A-A01A-4E81-B843-FF2328D46EFE", "The Tariff Code entered is not valid for the current context."));
			}
		}

		protected override void CheckCGC_OH_Owner()
		{
			base.CheckCGC_OH_Owner();

			if (Parent.Owner != null)
			{
				var catalogRootCnpj = Parent.Owner.GetRootCNPJ();

				if (catalogRootCnpj.IsEmpty)
				{
					Parent.CGC_OH_OwnerInfo.AddMessageError(Res.GetString("142E64B7-BCEB-4517-9075-51DD93FC7FAF", "Please enter a Root CNPJ in this organization to continue."));
				}

				if (BROrgImpAddInfo.Get(Parent.Owner) is BROrgImpAddInfo orgImpAddInfo)
				{
					if (orgImpAddInfo.Broker == null)
					{
						Parent.CGC_OH_OwnerInfo.AddMessageError(Res.GetString("42DF8E83-2B65-496B-885F-F849D25A9DA8", "No Catalog Manager has been assigned for this Owner. Please verify it on Details > Brazil > Catalog Manager."));
					}
					else if (!orgImpAddInfo.BrokerCertificate?.IsValidCertificate ?? true)
					{
						Parent.CGC_OH_OwnerInfo.AddMessageError(Res.GetString("1EEDD075-B63F-4D35-9B33-7148FED1D2DF", "The digital certificate for the Catalog Manager is missing, expired, or invalid. Please verify it on Details > Brazil > Catalog Manager."));
					}
				}
			}
		}
	}
}
