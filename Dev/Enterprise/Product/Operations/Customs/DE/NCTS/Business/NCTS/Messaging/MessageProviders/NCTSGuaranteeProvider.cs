using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NCTSGuaranteeProvider : INCTSGuarantee
	{
		public static NCTSGuaranteeProvider NewOrNull(NctsGuarantee guarantee) => guarantee != null ? new NCTSGuaranteeProvider(guarantee) : null;

		public NCTSGuaranteeProvider(string bondType, IReadOnlyCollection<NctsGuarantee> guarantees)
		{
			this.bondType = Argument.NotNull(bondType, nameof(bondType));
			this.guarantees = Argument.NotNull(guarantees, nameof(guarantees));
		}

		NCTSGuaranteeProvider(NctsGuarantee guarantee)
		{
			this.guarantee = Argument.NotNull(guarantee, nameof(guarantee));
			bondType = guarantee.PW_BondType;
		}

		public string Type => bondType;

		public string OtherReference => bondTypesWithBondNumber.Contains(bondType) ? (string)guarantee.PW_BondNumber2 : null;

		public IReadOnlyCollection<INCTSGuaranteeReference> References => references ?? GetReferences();
		readonly IReadOnlyCollection<INCTSGuaranteeReference> references;

		IReadOnlyCollection<INCTSGuaranteeReference> GetReferences()
		{
			if (guarantees != null)
			{
				return guarantees.Select(x => new NCTSGuaranteeReferenceProvider(x)).ToArray();
			}

			if (bondType == NctsGuaranteeTypeList.Codes._3)
			{
				return new NCTSGuaranteeReferenceProvider[] { new NCTSGuaranteeReferenceProvider(guarantee) };
			}

			return Array.Empty<INCTSGuaranteeReference>();
		}

		readonly NctsGuarantee guarantee;

		readonly string bondType;

		readonly IReadOnlyCollection<NctsGuarantee> guarantees;

		readonly static ImmutableHashSet<string> bondTypesWithBondNumber = ImmutableHashSet.Create(NctsGuaranteeTypeList.Codes._8, NctsGuaranteeTypeList.Codes.R);
	}
}
