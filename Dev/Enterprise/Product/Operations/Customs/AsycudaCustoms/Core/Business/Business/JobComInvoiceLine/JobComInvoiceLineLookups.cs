using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class JobComInvoiceLineLookups : Customs.Business.JobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine InvoiceLine
		{
			get { return (JobComInvoiceLine)base.InvoiceLine; }
		}

		protected new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
		}

		public override CodeDescriptionPairList CustomsUQList
		{
			get { return RefCusCodeListTypes.GetCachedList(Factory, InvoiceLine.Declaration?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, ZDateTime.Today); }
		}

		public override CodeDescriptionPairList BondedWhsUnitQtyList => InvoiceUQList;

		public override ICodeDescriptionPairList Procedures
		{
			get
			{
				var procedures = base.Procedures;
				var ceiStyle = InvoiceLine.EntryInstruction?.CEI_Style ?? ZString.Empty;
				if (!ceiStyle.IsEmpty)
				{
					((IFilterBusinessObjectDefaultsProvider)procedures).FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Group", "Property", ceiStyle, false)); // English only filtername
				}
				return procedures;
			}
		}

		public override ZString TaxOrFeeType => Core.Constants.Customs.CusEntryFeeTypes.VAT;
	}
}
