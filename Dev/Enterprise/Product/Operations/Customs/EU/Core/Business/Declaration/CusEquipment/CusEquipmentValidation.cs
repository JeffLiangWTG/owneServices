using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusEquipmentValidation : Customs.Business.CusEquipmentValidation
	{
		public CusEquipmentValidation(CusEquipment parent) : base(parent)
		{
		}

		public new CusEquipment Parent
		{
			get { return (CusEquipment)base.Parent; }
		}

		protected override void CheckCEQ_IdentificationNumber()
		{
			base.CheckCEQ_IdentificationNumber();
			var parent = Parent;
			var identificationNumber = parent.CEQ_IdentificationNumber;
			if (!identificationNumber.IsEmpty && parent.Declaration is JobDeclaration declaration && declaration.EquipmentsRequired
				&& !parent.Declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any((invoiceLine => invoiceLine?.PackagesPivot?.Cast<InvoiceLinePackagePivot>().Any(pivot => pivot.Package.CW_ContainerNoOrEquipmentNo.EqualsIgnoringCase(identificationNumber)) ?? false)))
			{
				parent.CEQ_IdentificationNumberInfo.AddWarning(EquipmentShouldLinkToOnePackageWarning);
			}
		}

		protected virtual string EquipmentShouldLinkToOnePackageWarning => Res.GetString("6AEF3AFE-6E6F-423E-9BA4-555F2C33677A", "An Equipment should be linked to at least one package line. Please go to the Packaging tab -> sub tab Packing Details and select appropriate Equipments(s)");
	}
}
