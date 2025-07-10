using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class InvoiceApportionCharge : TypeSafeInvoiceApportionCharge, Integration.Customs.EU.IInvoiceApportionCharge, IEUCommonInvoiceCharge
	{
		public InvoiceApportionCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("EU.Business.Declaration.InvoiceApportionCharge|J7_Percentage|Export", Caption = "Fixed %", MultipleKey = JobDeclaration.CaptionKeyChargesExport)]
		public override ZDecimal J7_Percentage { get => base.J7_Percentage; set => base.J7_Percentage = value; }

		[ResourceStringData("EU.Business.Declaration.InvoiceApportionCharge|J7_IsIncludedInITOT|Export", Caption = "Include in Line?", MultipleKey = JobDeclaration.CaptionKeyChargesExport)]
		public override ZBool J7_IsIncludedInITOT { get => base.J7_IsIncludedInITOT; set => base.J7_IsIncludedInITOT = value; }

		[ResourceStringData("EU.Business.Declaration.InvoiceApportionCharge|J7_IsStatisticalValueApplicable|Export", Caption = "Stat. Value appl.", MultipleKey = JobDeclaration.CaptionKeyChargesExport)]
		public override ZBool J7_IsStatisticalValueApplicable { get => base.J7_IsStatisticalValueApplicable; set => base.J7_IsStatisticalValueApplicable = value; }

		public ZDecimal AmountCorrection => ZDecimal.Zero;
	}
}
