using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class AddInfoJobComInvoiceHeader : EU.Business.Declaration.AddInfoJobComInvoiceHeader
	{
		public AddInfoJobComInvoiceHeader(ZPropertyInfo addInfoPropertyInfo)
			: base(addInfoPropertyInfo)
		{
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.ValuationMethodList))]
		public override ZString ZG_ValuationMethod
		{
			get { return base.ZG_ValuationMethod; }
			set
			{
				var oldvalue = ZG_ValuationMethod;

				if (oldvalue != value && !IsCopying)
				{
					if (Parent.IsInvoiceLinesLoaded())
					{
						foreach (JobComInvoiceLine invoiceLine in Parent.InvoiceLines)
						{
							if (invoiceLine.JI_ValuationCode.IsEmpty || invoiceLine.JI_ValuationCode == oldvalue)
							{
								invoiceLine.JI_ValuationCode = value;
							}
						}
					}
					base.ZG_ValuationMethod = value;
				}
			}
		}

		protected override ZBool ShouldClearJZ_IncoTermPlace => false;

		public override ZString ZG_AgreedPlaceCode
		{
			get => base.ZG_AgreedPlaceCode;
			set
			{
				var oldvalue = ZG_ValuationMethod;
				base.ZG_AgreedPlaceCode = value;

				if (!IsCopying && oldvalue != value)
				{
					Parent.JobDeclaration?.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.IncotermCountries))]
		public override ZString ZG_IncotermCountry
		{
			get => base.ZG_IncotermCountry;
			set
			{
				var oldvalue = ZG_IncotermCountry;
				base.ZG_IncotermCountry = value;

				if (!IsCopying && oldvalue != value)
				{
					Parent.JobDeclaration?.MarkAsNeedingValidation();
				}
			}
		}

		public new AddInfoJobComInvoiceHeaderLookups Lookups => (AddInfoJobComInvoiceHeaderLookups)base.Lookups;

		public new AddInfoJobComInvoiceHeaderValidation Validation => (AddInfoJobComInvoiceHeaderValidation)base.Validation;

		protected override EUAddInfoValidation GetNewValidation() => Parent.JobDeclaration?.ApplicationExtender.GetAddInfoJobComInvoiceHeaderValidation(this) ?? new DeltaGAddInfoJobComInvoiceHeaderValidation(this);

		protected override EUAddInfoLookups GetNewLookups() => new AddInfoJobComInvoiceHeaderLookups(this);

		internal new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;
	}
}
