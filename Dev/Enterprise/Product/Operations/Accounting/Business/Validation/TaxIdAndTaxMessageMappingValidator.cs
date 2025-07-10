using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public abstract class TaxIdAndTaxMessageMappingValidator
	{
		public TaxIdAndTaxMessageMappingValidator(TaxIdAndTaxMessageCombinationRulesCollection rules)
		{
			Rules = rules.Cast<TaxIdAndTaxMessageCombinationRules>().ToArray();
		}

		protected IEnumerable<TaxIdAndTaxMessageCombinationRules> Rules { get; }

		public abstract bool IsValidateMapping(string[] lineTypes, AccTaxRate taxId, AccInvMsg taxMessage);

		protected IEnumerable<string> ValidLineTypeList
		{
			get
			{
				if (validLineTypeList == null)
				{
					validLineTypeList = new List<string>() { TransactionLineTypes.Cost, TransactionLineTypes.Revenue, TransactionTypes.DirectPayment, TransactionTypes.DirectReceipt };
				}
				return validLineTypeList;
			}
		}

		IEnumerable<string> validLineTypeList;
	}
}
