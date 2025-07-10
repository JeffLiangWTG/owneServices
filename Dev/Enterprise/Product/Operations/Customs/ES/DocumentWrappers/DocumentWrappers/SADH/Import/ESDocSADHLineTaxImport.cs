using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.EU;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH
{
	public class ESDocSADHLineTaxImport : DocSADHLineTax
	{
		public static ESDocSADHLineTaxImport New(IESDocSADHLineTaxBoxSupporter taxSupporter, BusinessObjectFactory factory)
		{
			return taxSupporter == null ? null : new ESDocSADHLineTaxImport(taxSupporter, factory);
		}

		protected ESDocSADHLineTaxImport(IESDocSADHLineTaxBoxSupporter taxSupporter, BusinessObjectFactory factory)
			: base(taxSupporter, factory)
		{
		}

		const string ChargeType3IG = "3IG";
		const string DeferredMoPCode = "D";
		protected override ZString G4_TypeCore
		{
			get
			{
				if (taxSupporter.Type.Equals(RefCusRateCodes.Vat) && ((IESDocSADHLineTaxBoxSupporter)taxSupporter).DestinationStateIsCanaryIsland)
				{
					return ChargeType3IG;
				}
				else
				{
					return base.G4_TypeCore;
				}
			}
		}

		protected override ZString G4_RateOverrideCore
		{
			get
			{
				if (!taxSupporter.RateDuty.Equals(Enterprise.Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage))
				{
					return string.Format("E{0}", taxSupporter.RateDuty);
				}
				else
				{
					return taxSupporter.RateDuty;
				}
			}
		}

		protected override ZString G4_MethodOfPaymentCore
		{
			get
			{
				if (taxSupporter.MethodOfPayment.Equals(FeeMethodOfPayment.Deferred))
				{
					return DeferredMoPCode;
				}
				else if (taxSupporter.Type.StartsWith("3") || (taxSupporter.Type == RefCusRateCodes.Vat && ((IESDocSADHLineTaxBoxSupporter)taxSupporter).DestinationStateIsCanaryIsland))
				{
					return ((IESDocSADHLineTaxBoxSupporter)taxSupporter).EntryLineMethodOfPayment2;
				}
				else
				{
					return ((IESDocSADHLineTaxBoxSupporter)taxSupporter).EntryLineMethodOfPayment;
				}
			}
		}
	}
}
