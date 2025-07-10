using System.Collections.Immutable;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class JobComInvoiceLineTax : EU.Business.Declaration.JobComInvoiceLineTax, Integration.Customs.DE.IJobComInvoiceLineTax
	{
		public JobComInvoiceLineTax(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		protected override ZString HumanReadableNameCore => Res.GetString("eb95b940-72f7-43af-a15c-6bbc0a683d98", "Special Case");

		public new class Schema : EU.Business.Declaration.JobComInvoiceLineTax.Schema
		{
			public const int JLT_RateDecimalPlaces = 5;
		}

		[ResourceStringData("ECEEFECD-E29F-4690-8935-D30577DCD81A", Caption = "Group")]
		public override ZString JLT_Type
		{
			get => base.JLT_Type;
			set => base.JLT_Type = value;
		}

		[ResourceStringData("1F7F24B6-0B3F-4D66-872E-A87B7E045A0A", Caption = "Type")]
		public override ZString JLT_MethodOfCalculation
		{
			get => base.JLT_MethodOfCalculation;
			set
			{
				var oldValue = JLT_MethodOfCalculation;
				base.JLT_MethodOfCalculation = value;
				if (JLT_MethodOfCalculation != oldValue)
				{
					JLT_RateInfo.RefreshBinding();
					if (JLT_Rate_ReadOnly)
					{
						JLT_Rate = ZDecimal.Zero;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(JLT_Rate_ReadOnly))]
		[DecimalPlaces(Schema.JLT_RateDecimalPlaces)]
		[ResourceStringData("42B81993-CB45-4453-AB79-28528286317C", Caption = "Rate, Amount or Factor")]
		public override ZDecimal JLT_Rate
		{
			get => base.JLT_Rate;
			set => base.JLT_Rate = value;
		}

		public bool JLT_Rate_ReadOnly => Factory.GetCachedValue("C21F2540-120E-40F8-8642-8E689CC8DD70", () => ImmutableHashSet.Create(SpecialCaseGroupList.Codes._08, SpecialCaseGroupList.Codes._09)).Contains(JLT_MethodOfCalculation);

		public new JobComInvoiceLineTaxLookups Lookups => (JobComInvoiceLineTaxLookups)base.Lookups;

		protected override Customs.Business.JobComInvoiceLineTaxLookups GetNewLookups() => new JobComInvoiceLineTaxLookups(this);

		public new JobComInvoiceLineTaxValidation Validation => (JobComInvoiceLineTaxValidation)base.Validation;

		protected override Customs.Business.JobComInvoiceLineTaxValidation GetNewValidation() => new JobComInvoiceLineTaxValidation(this);
	}
}
