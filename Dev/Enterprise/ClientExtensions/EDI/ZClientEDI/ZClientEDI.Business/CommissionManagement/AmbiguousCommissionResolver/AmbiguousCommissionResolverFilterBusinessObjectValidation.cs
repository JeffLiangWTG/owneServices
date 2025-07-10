using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.CommissionManagement.Business
{
	public sealed class AmbiguousCommissionResolverFilterBusinessObjectValidation : AutoAmbiguousCommissionResolverFilterBusinessObjectValidation
	{
		public AmbiguousCommissionResolverFilterBusinessObjectValidation(AutoAmbiguousCommissionResolverFilterBusinessObject parent)
			: base(parent)
		{
		}

		public new AmbiguousCommissionResolverFilterBusinessObject Parent
		{
			get { return (AmbiguousCommissionResolverFilterBusinessObject)base.Parent; }
		}

		protected override void CheckCompanyPk()
		{
			base.CheckCompanyPk();

			if (Parent.IsResolvingFiltered)
			{
				MandatoryValidation.CheckEntered(Parent.CompanyPkInfo);
				if (!Parent.CompanyPk.IsEmpty && OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.Value)
				{
					if (Parent.CompanyPk != GlbCompany.CurrentCompany.PK)
					{
						Parent.CompanyPkInfo.AddError(Res.GetString("13166589-121e-4bc5-bce4-9a73e0e91908", "Must be current login company"));
					}
				}
			}
		}

		protected override void CheckInvoicePkToResolve()
		{
			base.CheckInvoicePkToResolve();
			if (Parent.IsResolvingFiltered && Parent.FilterByPkIsAllowed)
			{
				MandatoryValidation.CheckEntered(Parent.InvoicePkToResolveInfo);
			}
		}

		protected override void CheckInvoiceNumberToResolve()
		{
			base.CheckInvoiceNumberToResolve();
			if (Parent.IsResolvingFiltered && !Parent.FilterByPkIsAllowed)
			{
				MandatoryValidation.CheckEntered(Parent.InvoiceNumberToResolveInfo);
			}
		}
	}
}
