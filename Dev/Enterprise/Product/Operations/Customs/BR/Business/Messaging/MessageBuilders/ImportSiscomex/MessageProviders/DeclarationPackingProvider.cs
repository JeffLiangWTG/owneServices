using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ImportSiscomex.Outgoing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public class DeclarationPackingProvider : IDeclarationPacking
	{
		public DeclarationPackingProvider(IEnumerable<BasePackage> packages)
		{
			Argument.NotNull(packages, nameof(packages));
			Argument.GreaterThanZero(packages.Count(), nameof(packages));

			this.packages = packages;
		}

		readonly IEnumerable<BasePackage> packages;

		public static DeclarationPackingProvider New(IEnumerable<BasePackage> packages)
		{
			return packages == null && !packages.Any() ? null : new DeclarationPackingProvider(packages);
		}

		public string PackingTypeCode => packages.First().CW_PackType;

		public int PackingQty => packages.Sum(pack => pack.CW_PackQty);
	}
}


