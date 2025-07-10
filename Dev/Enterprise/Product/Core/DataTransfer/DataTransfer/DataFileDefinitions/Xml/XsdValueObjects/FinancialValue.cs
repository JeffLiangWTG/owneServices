using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssembly("Enterprise.DataTransfer.XmlSerializers")]
	[XmlType(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRoot(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclass("Enterprise.DataTransfer.Xml.XsdVersion1.AutoFinancialValue")]
	public class FinancialValue : AutoFinancialValue
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get { return base.IsSpecified && (Value != 0 || !CurrencyCode.IsEmpty); }
		}

		public static FinancialValue FromAmountAndCurrency(INumericZType amount, RefCurrency currency)
		{
			return (currency != null) ? FromAmountAndCurrencyCode(amount, currency.RX_Code) : null;
		}

		public static FinancialValue FromAmountAndCurrencyCode(INumericZType amount, string currencyCode)
		{
			FinancialValue result = null;

			if (!amount.IsEmpty || currencyCode != null)
			{
				result = new FinancialValue();
				result.Value = new ZDecimal(amount);
				if (!string.IsNullOrEmpty(currencyCode))
				{
					result.CurrencyCode = currencyCode;
				}
			}

			return result;
		}
	}
}
