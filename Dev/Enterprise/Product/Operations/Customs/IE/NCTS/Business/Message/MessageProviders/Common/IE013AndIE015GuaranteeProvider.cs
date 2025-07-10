using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces.Common;
using Enterprise.Customs.IE.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE013AndIE015GuaranteeProvider : IIE013AndIE015Guarantee
	{
		public IE013AndIE015GuaranteeProvider(NctsGuarantee guarantee)
		{
			this.guarantee = guarantee;
		}
		readonly NctsGuarantee guarantee;

		public string GuaranteeType => guarantee.PW_BondType;

		public string OtherGuaranteeReference
		{
			get
			{
				string result = null;
				if (GuaranteeType is string guaranteeType &&
					(
						guaranteeType == NctsGuaranteeTypeList.Codes.CashDepositGuarantee ||
						guaranteeType == NctsGuaranteeTypeList.Codes.GuaranteeNotRequiredForCertainPublicBodies
					))
				{
					result = guarantee.PW_BondNumber2;
				}
				return result;
			}
		}

		public IReadOnlyCollection<IIE013AndIIE015GuaranteeReference> GuaranteeReferences
		{
			get
			{
				if (guaranteeReferences is null && MessageStaticHelper.IsGuaranteeTypeWithReferences(guarantee.PW_BondType.ToString()))
				{
					guaranteeReferences = new[] { new IE013AndIE015GuaranteeReference(guarantee) };
				}
				return guaranteeReferences;
			}
		}
		IReadOnlyCollection<IIE013AndIIE015GuaranteeReference> guaranteeReferences;
	}
}
