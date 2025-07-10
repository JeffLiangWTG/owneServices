using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public partial class JobDeclaration
	{
		[MaxLength(3)]
		public override ZString JE_SpecificCircumstanceIndicator
		{
			get => base.JE_SpecificCircumstanceIndicator;
			set
			{
				base.JE_SpecificCircumstanceIndicator = value;
				Invoices.MarkAsNeedingValidation();
			}
		}

		[MaxLength(3)]
		public override ZString JE_VATDeferType
		{
			get => base.JE_VATDeferType;
			set => base.JE_VATDeferType = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.VATAccountNumberList))]
		[MaxLength(10)]
		public override ZString JE_VATDeferNumber
		{
			get => base.JE_VATDeferNumber;
			set => base.JE_VATDeferNumber = value;
		}

		public override ZBool JE_IsHighValueOvrd
		{
			get => base.JE_IsHighValueOvrd;
			set
			{
				base.JE_IsHighValueOvrd = value;
				InvoiceLines.MarkAsNeedingValidation();
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.BorderTransportMeansList))]
		public override ZString JE_BorderTransportMeans
		{
			get => base.JE_BorderTransportMeans;
			set => base.JE_BorderTransportMeans = value;
		}
	}
}
