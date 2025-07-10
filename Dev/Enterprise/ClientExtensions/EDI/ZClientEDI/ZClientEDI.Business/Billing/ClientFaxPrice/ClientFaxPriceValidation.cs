//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientFaxPriceValidation
//
//    This class should be used for overriding validation in AutoClientFaxPriceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Billing.Business
{
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Schema;
	using Enterprise.ZArchitecture.Schema;

	public class ClientFaxPriceValidation : AutoClientFaxPriceValidation
	{
		public ClientFaxPriceValidation(AutoClientFaxPrice parent) : base(parent)
		{
		}

		protected override void CheckCFP_RX_NKCurrencyCode()
		{
			base.CheckCFP_RX_NKCurrencyCode();
			MandatoryValidation.CheckEntered(Parent.CFP_RX_NKCurrencyCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CFP_RX_NKCurrencyCodeInfo);
			if (!Parent.CFP_RX_NKCurrencyCodeInfo.HasErrors() && !IsUniqueForThatMonthAndYear(ClientFaxPriceSchema.CFP_RX_NKCurrencyCode, Parent.CFP_RX_NKCurrencyCode))
			{
				Parent.CFP_RX_NKCurrencyCodeInfo.AddError("Currency must be unique in the chosen period.");
			}
		}

		bool IsUniqueForThatMonthAndYear(SchemaColumn column, object fieldValue)
		{
			ClientFaxPriceCollection clientFaxPrices = new ClientFaxPriceCollection(Parent.Factory);
			ZQuery query = new ZQuery(column, fieldValue);
			query.AddToFilter(ClientFaxPriceSchema.CFP_Month, Parent.CFP_Month);
			query.AddToFilter(ClientFaxPriceSchema.CFP_Year, Parent.CFP_Year);
			query.AddToFilter(ClientFaxPriceSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			clientFaxPrices.AdditionalFilter = query;
			return clientFaxPrices.Count == 0;
		}

		protected override void CheckCFP_PageRate()
		{
			base.CheckCFP_PageRate();
			if (!Parent.CFP_PageRateInfo.HasErrors() && Parent.CFP_PageRate <= 0)
			{
				Parent.CFP_PageRateInfo.AddError("Page Rate must be a positive value.");
			}
		}
	}
}

