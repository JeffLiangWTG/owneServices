using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class JobComInvoiceHeaderValidation : AutoBRJobComInvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		protected override void CheckJZ_InvoiceAmount()
		{
			base.CheckJZ_InvoiceAmount();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_InvoiceAmountInfo);
		}

		protected override void CheckJZ_NetWeight()
		{
			base.CheckJZ_NetWeight();
			var netWeight = Parent.JZ_NetWeight;
			if (!netWeight.IsEmpty)
			{
				var invoiceHeaderNetWeightUQ = Parent.JZ_NetWeightUQ;
				var totalInvoiceLineNetWeightInHeaderNetWeightUQ = ((ZDecimal)Parent.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => Core.Constants.Weight.ConvertSafe(x.JI_NetWeight, x.JI_NetWeightUQ, invoiceHeaderNetWeightUQ))).Round(3).Normalize();
				if (netWeight != totalInvoiceLineNetWeightInHeaderNetWeightUQ)
				{
					Parent.JZ_NetWeightInfo.AddMessageError(Res.GetString("F3B0EA49-2448-4183-B46F-C945D030312F", "The total Net Weight from the Invoice Lines ({0} {2}) differs from the Net Weight entered against this Invoice Header ({1} {2}).", totalInvoiceLineNetWeightInHeaderNetWeightUQ, Parent.JZ_NetWeight, Parent.JZ_NetWeightUQ));
				}
			}
		}

		protected override void CheckJZ_OA_SupplierAddress()
		{
			base.CheckJZ_OA_SupplierAddress();

			if (Parent.SupplierAddressIsAvailable)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_OA_SupplierAddressInfo, Res.GetString("9A4F7F87-AA72-433A-BC92-24069C5BC0D8", "Supplier Address"));
				Parent.JZ_OA_SupplierAddress_ZAddress.ValidateOrgPK();
			}
		}

		public virtual void ValidateSupplierOrgPK(ZPropertyInfo propertyInfo)
		{
			MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, JobComInvoiceHeader.SupplierCaption.Caption);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateExchangeRateDate();
		}

		public void ValidateExchangeRateDate()
		{
			ValidateCalculatedProperty(Parent.ExchangeRateDateInfo);
		}

		protected virtual void CheckExchangeRateDate()
		{
		}

		protected override bool ShouldCheckRelatedHouseBillEntered => base.ShouldCheckRelatedHouseBillEntered && !Parent.IsImportLicense && !Parent.IsLPCO;
	}
}
