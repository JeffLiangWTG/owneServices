using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class ImportJobComInvoiceHeaderValidation : JobComInvoiceHeaderValidation
	{
		public ImportJobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		public override void ValidateSupplierOrgPK(ZPropertyInfo propertyInfo)
		{
			base.ValidateSupplierOrgPK(propertyInfo);

			if (Parent.IsImportOnly && Parent.JZ_OH_Supplier.IsValid && Parent.IsAttachedToPersistentDeclaration && BRCustomsDataRegistry.Instance.EnableForeignOperator.Value)
			{
				var foreignOperator = Parent.ForeignOperator;
				if (foreignOperator == null)
				{
					propertyInfo.AddMessageError(Res.GetString("12C04209-C173-48E1-BBA7-C149ECA53266", "Supplier is not a Foreign Operator."));
				}
				else if (foreignOperator.BFR_AuthorityIdentifier.IsEmpty)
				{
					propertyInfo.AddMessageError(Res.GetString("2C16759A-DDE7-494B-A7CE-21405984CD72", "Supplier choose is a Foreign Operator but has no Authority Identifier."));
				}
				else if (foreignOperator.BFR_MessageStatus == BRMessageStatusList.Codes.NotSent)
				{
					propertyInfo.AddMessageError(Res.GetString("14F5C426-46E9-48E7-98D5-CA044D7FDCC7", "Supplier should not be used because there might be messages that need to be sent (Message Status is currently NST – Not Sent)."));
				}
				else if (foreignOperator.BFR_MessageStatus == BRMessageStatusList.Codes.AwaitingResponse)
				{
					propertyInfo.AddMessageError(Res.GetString("7E9BF14B-225F-4855-BBAD-282B8CF92BDD", "Supplier choose is a Foreign Operator but there are pending messages (Message Status is currently AWA - Awaiting Response)."));
				}
			}
		}

		protected override void CheckJZ_RelatedIndicator()
		{
			base.CheckJZ_RelatedIndicator();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JZ_RelatedIndicatorInfo);
		}

		protected override void CheckJZ_OA_SupplierAddress()
		{
			base.CheckJZ_OA_SupplierAddress();

			if (Parent.IsImportSiscomex)
			{
				AddressValidationHelper.CheckAddressStatusAndStreetNumber(Parent.JZ_OA_SupplierAddressInfo, Parent.SupplierAddress);
			}
		}

		protected override void CheckJZ_ValuationCode()
		{
			base.CheckJZ_ValuationCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JZ_ValuationCodeInfo);
		}

		protected override void CheckJZ_SupplierAuthorityIdentifier()
		{
			base.CheckJZ_SupplierAuthorityIdentifier();
			if (Parent.IsImportOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_SupplierAuthorityIdentifierInfo);
			}
		}

		protected override void CheckJZ_SupplierAuthorityVersion()
		{
			base.CheckJZ_SupplierAuthorityVersion();
			if (Parent.IsImportOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_SupplierAuthorityVersionInfo);
			}
		}

		protected override void CheckJZ_AdditionalTerms()
		{
			base.CheckJZ_AdditionalTerms();

			if (Parent.JZ_IncoTerm == BRIncoTermList.Codes.OCV)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_AdditionalTermsInfo, Res.GetString("4F356896-428E-4805-8933-15BECFA5DE4C", "The selected Incoterm is OCV - Other Condition of Sale, but no Complement has been entered."));
			}
		}

		protected override void CheckExchangeRateDate()
		{
			var propertyInfo = Parent.ExchangeRateDateInfo;
			if (Parent.JobDeclaration.Invoices.Cast<JobComInvoiceHeader>().Any(x => x.ExchangeRateDate != Parent.ExchangeRateDate && x.PK != Parent.PK))
			{
				propertyInfo.AddMessageError(Res.GetString("06453f47-df40-4f3d-80d7-bf548827ea6b", "The Exchange Rate Date is different from the other invoices of this declaration."));
			}
		}
	}
}
