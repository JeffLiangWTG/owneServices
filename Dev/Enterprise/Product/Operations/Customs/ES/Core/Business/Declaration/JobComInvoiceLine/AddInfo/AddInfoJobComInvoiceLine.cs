using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class AddInfoJobComInvoiceLine : EU.Business.Declaration.AddInfoJobComInvoiceLine
	{
		public AddInfoJobComInvoiceLine(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.ExciseCodeList))]
		public override ZString ZG_ExciseCode { get => base.ZG_ExciseCode; set => base.ZG_ExciseCode = value; }

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.ExciseExemptionList))]
		public override ZString ZG_ExciseExemption { get => base.ZG_ExciseExemption; set => base.ZG_ExciseExemption = value; }

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.MethodOfPaymentList))]
		public override ZString ZG_MethodOfPayment2 { get => base.ZG_MethodOfPayment2; set => base.ZG_MethodOfPayment2 = value; }

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.REAProductCodeList))]
		public override ZString ZG_REAProductCode { get => base.ZG_REAProductCode; set => base.ZG_REAProductCode = value; }

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.AIEMTypeCodeList))]
		public override ZString ZG_AIEMType { get => base.ZG_AIEMType; set => base.ZG_AIEMType = value; }

		[MaxLength(3)]
		public override ZString ZG_RegionOfDestination { get => base.ZG_RegionOfDestination; set => base.ZG_RegionOfDestination = value; }

		public JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)Parent;

		public new AddInfoJobComInvoiceLineValidation Validation => (AddInfoJobComInvoiceLineValidation)base.Validation;

		protected override EUAddInfoValidation GetNewValidation() => new AddInfoJobComInvoiceLineValidation(this);

		public new AddInfoJobComInvoiceLineLookups Lookups => (AddInfoJobComInvoiceLineLookups)base.Lookups;

		protected override EUAddInfoLookups GetNewLookups() => new AddInfoJobComInvoiceLineLookups(this);
	}
}
