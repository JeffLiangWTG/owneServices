using System;
using System.Linq;
using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class NFEImportObjectValidation : ZValidation
	{
		public NFEImportObjectValidation(NFEImportObject parent)
			: base(parent)
		{
			Parent = parent;
		}

		public NFEImportObject Parent;

		public override Type AutoValidationType
		{
			get { return typeof(NFEImportObjectValidation); }
		}

		public override void ValidateAll()
		{
			ValidateIncoterm();
			ValidateCurrencyCode();
			ValidateExchangeRate();
		}

		public void ValidateIncoterm()
		{
			ValidateCalculatedProperty(Parent.IncotermInfo);
		}

		protected void CheckIncoterm()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.IncotermInfo);

			if (Parent.InvoiceHeaderPK.IsValid)
			{
				if (Parent.ObjectParent?.NFEImportObjectCollection?.Cast<NFEImportObject>().Any(x => x.InvoiceHeaderPK == Parent.InvoiceHeaderPK && x.Incoterm != Parent.Incoterm) ?? false)
				{
					Parent.IncotermInfo.AddError(Res.GetString("e737ec1b-7194-4574-b0fb-b3269c68beba", "You can not enter different Incoterms in NF-e linked to the same Invoice No."));
				}
			}
		}

		public void ValidateCurrencyCode()
		{
			ValidateCalculatedProperty(Parent.CurrencyCodeInfo);
		}

		protected void CheckCurrencyCode()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CurrencyCodeInfo);

			if (Parent.InvoiceHeaderPK.IsValid)
			{
				if (Parent.ObjectParent?.NFEImportObjectCollection?.Cast<NFEImportObject>().Any(x => x.InvoiceHeaderPK == Parent.InvoiceHeaderPK && x.CurrencyCode != Parent.CurrencyCode) ?? false)
				{
					Parent.CurrencyCodeInfo.AddError(Res.GetString("13c5c465-cb36-4810-8c07-d4b877aadee8", "You can not enter different Currencies in NF-e linked to the same Invoice No."));
				}
			}
		}

		public void ValidateExchangeRate()
		{
			ValidateCalculatedProperty(Parent.ExchangeRateInfo);
		}

		protected void CheckExchangeRate()
		{
			MandatoryValidation.WarnIfIsZero(Parent.ExchangeRateInfo);
		}

		public void ValidateInvoiceHeaderPK()
		{
			ValidateCalculatedProperty(Parent.InvoiceHeaderPKInfo);
		}

		protected void CheckInvoiceHeaderPK()
		{
			ListValidation.ErrorIfInvalidPK(Parent.InvoiceHeaderPKInfo);
		}

		public void ValidateEntryInstructionPK()
		{
			ValidateCalculatedProperty(Parent.EntryInstructionPKInfo);
		}

		protected void CheckEntryInstructionPK()
		{
			if (Parent.Declaration != null && Parent.Declaration.IsPersistent)
			{
				ListValidation.ErrorIfInvalidPK(Parent.EntryInstructionPKInfo);
			}
		}
	}
}
