using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class AddInfoJobComInvoiceHeader : AddInfo
	{
		public AddInfoJobComInvoiceHeader(ZPropertyInfo addInfoPropertyInfo)
			: base(addInfoPropertyInfo)
		{
		}

		public new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.TransportChargesMethodOfPaymentList))]
		public override ZString ZG_TransportChargesMethodOfPayment
		{
			get => base.ZG_TransportChargesMethodOfPayment;
			set => base.ZG_TransportChargesMethodOfPayment = value;
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.AgreedPlaceCodeList))]
		[ResourceStringData("70E15581-7B1B-49BB-B0E2-D2018C483703", Caption = "Incoterm Place Code")]
		public override ZString ZG_AgreedPlaceCode
		{
			get => base.ZG_AgreedPlaceCode;
			set
			{
				bool hasChanged = ZG_AgreedPlaceCode != value;
				base.ZG_AgreedPlaceCode = value;
				if (!IsCopying && hasChanged)
				{
					if (Parent is JobComInvoiceHeader invoice)
					{
						if (ShouldClearJZ_IncoTermPlace)
						{
							invoice.JZ_IncoTermPlace = ZString.Empty;
						}
						invoice.MarkAsNeedingValidation();
					}
				}
			}
		}
		internal ZBool IsAgreedUnloco => (ZG_AgreedPlaceCode.Length == 5 ? new RefUNLOCO.Loader(Factory).Load(ZG_AgreedPlaceCode) : null) != null;

		protected virtual ZBool ShouldClearJZ_IncoTermPlace => IsAgreedUnloco
			&& Parent is JobComInvoiceHeader invoice
			&& invoice.AgreedPlaceCodeSupport;

		public override ZString ZG_RelatedIndicator2
		{
			get => base.ZG_RelatedIndicator2;
			set
			{
				base.ZG_RelatedIndicator2 = value;
				if (InvoiceLines is JobComInvoiceLineViewCollection invoiceLines)
				{
					invoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.ZG_RelatedIndicator2Info.RefreshBinding());
				}
			}
		}

		public virtual ZBool RelatedIndicator2
		{
			get => ZG_RelatedIndicator2 == RelatedIndicatorList.Codes.Yes;
			set
			{
				ZG_RelatedIndicator2 = value ? RelatedIndicatorList.Codes.Yes : RelatedIndicatorList.Codes.No;
			}
		}
		public bool RelatedIndicator2_ReadOnly => !RelatedIndicator2 && InvoiceLines != null && InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.RelatedIndicator2);

		public override ZString ZG_RelatedIndicator3
		{
			get => base.ZG_RelatedIndicator3;
			set
			{
				base.ZG_RelatedIndicator3 = value;
				if (InvoiceLines is JobComInvoiceLineViewCollection invoiceLines)
				{
					invoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.ZG_RelatedIndicator3Info.RefreshBinding());
				}
			}
		}

		public virtual ZBool RelatedIndicator3
		{
			get => ZG_RelatedIndicator3 == RelatedIndicatorList.Codes.Yes;
			set
			{
				ZG_RelatedIndicator3 = value ? RelatedIndicatorList.Codes.Yes : RelatedIndicatorList.Codes.No;
			}
		}
		public bool RelatedIndicator3_ReadOnly => !RelatedIndicator3 && InvoiceLines != null && InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.RelatedIndicator3);

		public override ZString ZG_RelatedIndicator4
		{
			get => base.ZG_RelatedIndicator4;
			set
			{
				base.ZG_RelatedIndicator4 = value;
				if (InvoiceLines is JobComInvoiceLineViewCollection invoiceLines)
				{
					invoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.ZG_RelatedIndicator4Info.RefreshBinding());
				}
			}
		}

		public virtual ZBool RelatedIndicator4
		{
			get => ZG_RelatedIndicator4 == RelatedIndicatorList.Codes.Yes;
			set
			{
				ZG_RelatedIndicator4 = value ? RelatedIndicatorList.Codes.Yes : RelatedIndicatorList.Codes.No;
			}
		}
		public bool RelatedIndicator4_ReadOnly => !RelatedIndicator4 && InvoiceLines != null && InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.RelatedIndicator4);

		public new AddInfoJobComInvoiceHeaderLookups Lookups => (AddInfoJobComInvoiceHeaderLookups)base.Lookups;

		public new AddInfoJobComInvoiceHeaderValidation Validation => (AddInfoJobComInvoiceHeaderValidation)base.Validation;

		protected override EUAddInfoLookups GetNewLookups() => new AddInfoJobComInvoiceHeaderLookups(this);

		protected override EUAddInfoValidation GetNewValidation() => new AddInfoJobComInvoiceHeaderValidation(this);

		JobComInvoiceLineViewCollection InvoiceLines => Parent.InvoiceLines;
	}
}
