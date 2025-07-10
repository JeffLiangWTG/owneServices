using CargoWise.Types;

namespace Enterprise.Accounting.Business.EInvoicing.KoreaSouth
{
	public interface IInvoiceePartyAdditionalInfo
	{
		ZString ID { get; }

		ZString TypeCode { get; }

		ZString NameText { get; }

		ZString ClassificationCode { get; }

		ZString TaxRegistrationID { get; }

		ZString SpecifiedPersonNameText { get; }

		ZString PrimaryDefinedContactPersonName { get; }

		ZString PrimaryDefinedContactTel { get; }

		ZString PrimaryDefinedContactURICommunication { get; }

		ZString SecondaryDefinedContactPersonName { get; }

		ZString SecondaryDefinedContactTel { get; }

		ZString SecondaryDefinedContactURICommunication { get; }

		ZString SpecifiedAddressLineOneText { get; }

		ZString BusinessTypeCode { get; }
	}
}
