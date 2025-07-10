using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class CertificateOfOriginLookups : Customs.Business.CusSupportingInfoLookups
	{
		public CertificateOfOriginLookups(CertificateOfOrigin parent)
			: base(parent)
		{
		}

		public CertificateOfOrigin CertificateOfOrigin
		{
			get { return Parent; }
		}

		protected new CertificateOfOrigin Parent
		{
			get { return (CertificateOfOrigin)base.Parent; }
		}

		public CodeDescriptionPairList CertificateOfOriginIssuedCodeList => Factory.GetCachedValue<CertificateOfOriginIssuedCodeList>();
		public CodeDescriptionPairList CountryOfOriginDeterminationRuleCodeList => Factory.GetCachedValue<CountryOfOriginDeterminationRuleCodeList>();
		public CodeDescriptionPairList CertificateOfOriginSplitCodeList => Factory.GetCachedValue<CertificateOfOriginSplitCodeList>();
	}
}
