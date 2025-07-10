using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class InvoiceCharge : TypeSafeInvoiceCharge, Integration.Customs.EU.IInvoiceCharge, IEUCommonInvoiceCharge
	{
		public InvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("EU.Business.Declaration.JobComInvHeaderCharge|J7_RX_NKCurrency", Caption = "Curr.")]
		[ResourceStringData("18B7DE32-B304-450D-AA50-540A3D845F19", Caption = "Curr.", FullDescription = "[14 05 000 000] Invoice currency", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString J7_RX_NKCurrency
		{
			get => base.J7_RX_NKCurrency;
			set => base.J7_RX_NKCurrency = value;
		}

		[ResourceStringData("EU.Business.Declaration.JobComInvHeaderCharge|Export|J7_Percentage", Caption = "Fixed %", MultipleKey = JobDeclaration.CaptionKeyChargesExport)]
		public override ZDecimal J7_Percentage { get => base.J7_Percentage; set => base.J7_Percentage = value; }

		[ResourceStringData("EU.Business.Declaration.JobComInvHeaderCharge|Export|J7_IsIncludedInITOT", Caption = "Include in Line?", MultipleKey = JobDeclaration.CaptionKeyChargesExport)]
		public override ZBool J7_IsIncludedInITOT { get => base.J7_IsIncludedInITOT; set => base.J7_IsIncludedInITOT = value; }

		[ResourceStringData("EU.Business.Declaration.JobComInvHeaderCharge|Export|J7_IsStatisticalValueApplicable", Caption = "Stat. Value appl.", MultipleKey = JobDeclaration.CaptionKeyChargesExport)]
		public override ZBool J7_IsStatisticalValueApplicable { get => base.J7_IsStatisticalValueApplicable; set => base.J7_IsStatisticalValueApplicable = value; }

		public ZDecimal AmountCorrection => ZDecimal.Zero;
	}
}
