using CargoWise.Types;

namespace Enterprise.Accounting.Business.EInvoicing.KoreaSouth
{
	public interface IInvoicerPartyAdditionalInfo
	{
		ZString ID { get; }

		ZString TypeCode { get; }

		ZString NameText { get; }

		ZString ClassificationCode { get; }

		ZString TaxRegistrationID { get; }

		ZString SpecifiedPersonNameText { get; }

		ZString DefinedContactPersonName { get; }

		ZString DefinedContactTel { get; }

		ZString DefinedContactURICommunication { get; }

		ZString SpecifiedAddressLineOneText { get; }
	}
}
