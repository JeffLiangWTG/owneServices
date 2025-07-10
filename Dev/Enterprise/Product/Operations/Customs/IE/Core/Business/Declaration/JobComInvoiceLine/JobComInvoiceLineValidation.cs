using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class JobComInvoiceLineValidation : EU.Business.Declaration.JobComInvoiceLineValidation
	{
		public JobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateOverlappingPackSets();
		}

		public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;
		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override bool HasValidPackagePivots => Parent.PackagesPivot.Any();

		void ValidateOverlappingPackSets()
		{
			var parent = Parent;
			if (parent.Declaration?.OverlappingPackageInvoiceLines?.Contains(parent.PK) ?? false)
			{
				parent.AddRowMessageError(OverlappingPackSetsAreNotAllowedMesssage);
			}
		}

		string OverlappingPackSetsAreNotAllowedMesssage => Res.GetString("614BEFF8-21A6-4201-9214-9B7D0E0FE8D9", "Where there are Invoice Lines that have at least one common package, then the Invoice Lines must be linked to the same set of packages.");

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();

			var parent = Parent;
			if (parent.JI_CustomsUnitQty == Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram && parent.JI_CustomsQuantity > parent.GrossWeightInKG)
			{
				parent.JI_CustomsQuantityInfo.AddMessageError(Res.GetString("566DA5E5-4FAC-4263-9DDD-0CC3833AF6F4", "The Net Weight must be less than or equal to the Gross Mass"));
			}
		}

		protected override void CheckJI_Tariff_NoPackage()
		{
		}

		protected override string GetPackagesTabName() => Res.GetString("{1F00468E-E943-4862-86BC-2718720CFC9B}", "Packages");
	}
}
