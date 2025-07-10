using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocTaxRate : DocumentWrapper
	{
		DocTaxRate(AccTaxRate accTaxRate, BusinessObjectFactory factoryToWrap)
			: base(accTaxRate, factoryToWrap)
		{
		}

		public static DocTaxRate New(AccTaxRate accTaxRate, BusinessObjectFactory factoryToWrap)
		{
			if (accTaxRate == null)
			{
				return null;
			}
			else
			{
				return factoryToWrap.GetCachedValue(accTaxRate.PK.ToStringKey(), delegate
				{ return new DocTaxRate(accTaxRate, factoryToWrap); }, CacheStalenessPolicy.StaleOnFactorySave);
			}
		}

		public AccTaxRate AccTaxRate
		{
			get { return (AccTaxRate)WrappedObject; }
		}

		public override string ToString()
		{
			return NoDefaultPropertyErrorMessage;
		}

		public ZString Code
		{
			get { return AccTaxRate.AT_Code; }
		}

		public ZString Description
		{
			get { return AccTaxRate.AT_Description; }
		}

		public ZString Type
		{
			get { return AccTaxRate.AT_Type; }
		}

		public ZString ExtraType
		{
			get { return AccTaxRate.AT_ExtraTaxRateType; }
		}

		public DocARInvoiceTaxMessageCollection ARInvoiceTaxMessages
		{
			get
			{
				var result = new DocARInvoiceTaxMessageCollection(Factory);
				if (AccTaxRate.DefaultVatClass != null)
				{
					var messageToAdd = DocARInvoiceTaxMessage.New(AccTaxRate.DefaultVatClass, Factory);
					result.Add(messageToAdd);
				}
				return result;
			}
		}

		public ZBool IsActive
		{
			get { return AccTaxRate.AT_IsActive; }
		}

		public ZBool IsNonReportable => AccTaxRate.IsNonReportable;

		public static bool IsRatedTax(ZString taxType)
		{
			return taxType == AccTaxRate.Types.Rated
					|| taxType == AccTaxRate.Types.IntegratedGST
					|| (taxType == AccTaxRate.Types.ServiceTax && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.India)
					|| (taxType == AccTaxRate.Types.CapitalRated && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Congo);
		}
	}
}
