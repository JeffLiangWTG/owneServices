using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.TrustedMessaging.Business
{
	public class EdiTrustedMessagingConfigGlobalCollection : ActiveBusinessObjectCollection<EdiTrustedMessagingConfig>
	{
		public EdiTrustedMessagingConfigGlobalCollection(BusinessObjectFactory factory)
			: base(factory, GetGlobalTrustedMessagingConfigQuery())
		{
		}

		public EdiTrustedMessagingConfigGlobalCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		public static ZQuery GetGlobalTrustedMessagingConfigQuery()
		{
			var trustedServiceCodes = EDIDataRegistry.Instance.MyAccountTrustedServices.Value.GetActiveCodeDescriptionPairList().Cast<CodeDescriptionPair>().Select(x => x.Code);

			var query = new ZQuery(EdiTrustedMessagingConfigSchema.ETM_CertificateType, SQLComparisonOperator.NotEqual, CertificateTypeList.Codes.TrustedSystemCertificate);
			query.AddToFilter(JoinCondition.Or, EdiTrustedMessagingConfigSchema.ETM_Product, trustedServiceCodes);
			return query;
		}

		protected override void SetDefaultsForNewElementCore(EdiTrustedMessagingConfig newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.ETM_CertificateType = CertificateTypeList.Codes.CentralSystemCertificate;
		}
	}
}
