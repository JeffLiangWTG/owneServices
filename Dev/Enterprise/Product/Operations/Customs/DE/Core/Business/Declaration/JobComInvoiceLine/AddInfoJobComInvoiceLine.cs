using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class AddInfoJobComInvoiceLine : EU.Business.Declaration.AddInfoJobComInvoiceLine
	{
		public AddInfoJobComInvoiceLine(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.IdentificationMeansTypeList))]
		public override ZString ZG_IdentificationMeansType
		{
			get => base.ZG_IdentificationMeansType;
			set => base.ZG_IdentificationMeansType = value;
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.CessionFlagList))]
		public override ZString ZG_CessionFlag
		{
			get => base.ZG_CessionFlag;
			set => base.ZG_CessionFlag = value;
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.EconomicConditionsList))]
		public override ZString ZG_EconomicConditions
		{
			get => base.ZG_EconomicConditions;
			set => base.ZG_EconomicConditions = value;
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.CustomsUQList))]
		public override ZString ZG_QuotaUQ
		{
			get => base.ZG_QuotaUQ;
			set => base.ZG_QuotaUQ = value;
		}

		public new AddInfoJobComInvoiceLineValidation Validation => (AddInfoJobComInvoiceLineValidation)base.Validation;

		public new AddInfoJobComInvoiceLineLookups Lookups => (AddInfoJobComInvoiceLineLookups)base.Lookups;

		protected JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)Parent;

		protected override EUAddInfoValidation GetNewValidation()
		{
			var invoiceLine = InvoiceLine;
			if (invoiceLine.IsImport)
			{
				return new ImportAddInfoJobComInvoiceLineValidation(this);
			}
			else if (invoiceLine.IsExport)
			{
				return new ExportAddInfoJobComInvoiceLineValidation(this);
			}
			else if (invoiceLine.IsWarehouseAdjustment)
			{
				return new WarehouseAdjustmentAddInfoJobComInvoiceLineValidation(this);
			}
			else
			{
				return new AddInfoJobComInvoiceLineValidation(this);
			}
		}

		protected override EUAddInfoLookups GetNewLookups() => new AddInfoJobComInvoiceLineLookups(this);
	}
}
