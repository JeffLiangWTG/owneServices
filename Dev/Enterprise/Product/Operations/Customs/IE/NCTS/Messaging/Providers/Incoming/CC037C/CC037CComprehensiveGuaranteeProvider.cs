using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC037CComprehensiveGuaranteeProvider
	{
		public CC037CComprehensiveGuaranteeProvider(ComprehensiveGuaranteeType comprehensiveGuarantee)
		{
			this.comprehensiveGuarantee = Argument.NotNull(comprehensiveGuarantee, nameof(comprehensiveGuarantee));
		}
		readonly ComprehensiveGuaranteeType comprehensiveGuarantee;

		public ZDecimal ReferenceAmount => comprehensiveGuarantee.ReferenceAmount ?? ZDecimal.Zero;

		public ZString PercentageOfReferenceAmount => comprehensiveGuarantee.PercentageOfReferenceAmount;

		public ZDecimal GuaranteeAmount => comprehensiveGuarantee.GuaranteeAmount;

		public ZString Currency => comprehensiveGuarantee.Currency;

		public ZString NumberOfCertificates => comprehensiveGuarantee.NumberOfCertificates;

		public ZDate ValidityStartDate => new ZDate(comprehensiveGuarantee.ValidityStartDate);

		public ZDate ValidityEndDate => new ZDate(comprehensiveGuarantee.ValidityEndDate);

		public ZString InvalidityReasonCode => comprehensiveGuarantee.InvalidityReasonCode;

		public ZString InvalidityReasonText => comprehensiveGuarantee.InvalidityReasonText;

		public ZDate LiabilityLiberationDate => new ZDate(comprehensiveGuarantee.LiabilityLiberationDate);

		public ZString RestrictedUseForSuspendedGoods => comprehensiveGuarantee.RestrictedUseForSuspendedGoods.GetXmlEnumAttributeValue();

		public IReadOnlyCollection<CC037CValidityLimitationProvider> ValidityLimitations => validityLimitationsCached ?? (validityLimitationsCached = comprehensiveGuarantee.ValidityLimitation?.Select(x => new CC037CValidityLimitationProvider(x)).ToArray() ?? Array.Empty<CC037CValidityLimitationProvider>());
		IReadOnlyCollection<CC037CValidityLimitationProvider> validityLimitationsCached;
	}
}
