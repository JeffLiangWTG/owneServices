using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsGuaranteeValueSetStrategy : IValueSetStrategy
	{
		readonly NctsGuarantee nctsGuarantee;
		public NctsGuaranteeValueSetStrategy(NctsGuarantee nctsGuarantee)
		{
			this.nctsGuarantee = nctsGuarantee;
		}

		void IValueSetStrategy.ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			switch (valueThatHasChanged.Name)
			{
				case NctsGuarantee.Schema.PW_BondType:
				case NctsGuarantee.Schema.PW_SuretyCode:
					ApportionedAmountToGuaranteesLiabilityAmount();
					break;
			}
		}

		void ApportionedAmountToGuaranteesLiabilityAmount()
		{
			var header = nctsGuarantee.NctsHeader;
			if (header != null)
			{
				header.ApportionedAmountToGuaranteesLiabilityAmount();
			}
		}
	}
}
