using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC037CGuaranteeReferenceProvider
	{
		public CC037CGuaranteeReferenceProvider(GuaranteeReferenceType07 guaranteeReference)
		{
			this.guaranteeReference = Argument.NotNull(guaranteeReference, nameof(guaranteeReference));
			this.guaranteeQuery = guaranteeReference.GuaranteeQuery;
			this.exposure = guaranteeReference.Exposure;
			this.guarantor = guaranteeReference.Guarantor;
			this.comprehensiveGuarantee = guaranteeReference.ComprehensiveGuarantee;
			this.individualGuaranteeByGuarantor = guaranteeReference.IndividualGuaranteeByGuarantor;
			this.individualGuaranteeVoucher = guaranteeReference.IndividualGuaranteeVoucher;
		}
		readonly GuaranteeReferenceType07 guaranteeReference;
		readonly GuaranteeQueryType guaranteeQuery;
		readonly ExposureType exposure;
		readonly GuarantorType01 guarantor;
		readonly ComprehensiveGuaranteeType comprehensiveGuarantee;
		readonly IndividualGuaranteeByGuarantorType individualGuaranteeByGuarantor;
		readonly IndividualGuaranteeVoucherType individualGuaranteeVoucher;

		public ZInt SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCached, () => int.TryParse(guaranteeReference.SequenceNumber, out var tryParseIntResult) ? tryParseIntResult : 0);
		CachedValue<int> sequenceNumberCached;

		public ZString GRN => guaranteeReference.Grn;

		public ZString GuaranteeMonitoringCode => guaranteeReference.GuaranteeMonitoringCode;

		public ZString QueryIdentifier => guaranteeQuery?.QueryIdentifier;

		public ZDate QueryPeriodFromDate => new ZDate(guaranteeQuery?.PeriodFromDate);

		public ZDate QueryPeriodToDate => new ZDate(guaranteeQuery?.PeriodToDate);

		public IReadOnlyCollection<CC037CUsageProvider> Usages => usagesCached ?? (usagesCached = guaranteeReference.Usage?.Select(x => new CC037CUsageProvider(x)).ToArray() ?? Array.Empty<CC037CUsageProvider>());
		IReadOnlyCollection<CC037CUsageProvider> usagesCached;

		public CC037CExposureProvider Exposure => exposure == null ? null : exposureCached ?? (exposureCached = new CC037CExposureProvider(exposure));
		CC037CExposureProvider exposureCached;

		public CC037GuarantorProvider Guarantor => guarantor == null ? null : guarantorCached ?? (guarantorCached = new CC037GuarantorProvider(guarantor));
		CC037GuarantorProvider guarantorCached;

		public CC037CComprehensiveGuaranteeProvider ComprehensiveGuarantee => comprehensiveGuarantee == null ? null : comprehensiveGuaranteeCached ?? (comprehensiveGuaranteeCached = new CC037CComprehensiveGuaranteeProvider(comprehensiveGuarantee));
		CC037CComprehensiveGuaranteeProvider comprehensiveGuaranteeCached;

		public CC037CIndividualGuaranteeByGuarantorProvider IndividualGuaranteeByGuarantor => individualGuaranteeByGuarantor == null ? null : individualGuaranteeByGuarantorCached ?? (individualGuaranteeByGuarantorCached = new CC037CIndividualGuaranteeByGuarantorProvider(individualGuaranteeByGuarantor));
		CC037CIndividualGuaranteeByGuarantorProvider individualGuaranteeByGuarantorCached;

		public CC037CIndividualGuaranteeVoucherProvider IndividualGuaranteeVoucher => individualGuaranteeVoucher == null ? null : individualGuaranteeVoucherCached ?? (individualGuaranteeVoucherCached = new CC037CIndividualGuaranteeVoucherProvider(individualGuaranteeVoucher));
		CC037CIndividualGuaranteeVoucherProvider individualGuaranteeVoucherCached;
	}
}
