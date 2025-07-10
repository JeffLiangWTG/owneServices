using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class NEXDOCEventLineDecoder
	{
		public NEXDOCEventLineDecoder(Context context)
		{
			this.context = context;
			subContextCollection = this.context.SubContextCollection;
		}

		readonly Context context;
		readonly List<Context> subContextCollection;

		public bool IsValid => LineNumber > 0 && subContextCollection != null && subContextCollection.Any();

		public ZShort LineNumber => (lineNumber ?? (lineNumber = ZShort.ParseSafe(context.Value.GetValueOrDefault(), 0))).Value;
		ZShort? lineNumber;

		public ZString HealthCertificateDescription => (healthCertificateDescription ?? (healthCertificateDescription = GetContextValue(Constants.EventContextTypes.HealthCertificateDescription))).Value;
		ZString? healthCertificateDescription;

		public ZString PrimaryCertificateTemplateCode => (primaryCertificateTemplateCode ?? (primaryCertificateTemplateCode = GetContextValue(Constants.EventContextTypes.PrimaryCertificateTemplateCode))).Value;
		ZString? primaryCertificateTemplateCode;

		public ZString PrimaryCertificateEndorsementNumber => (primaryCertificateEndorsementNumber ?? (primaryCertificateEndorsementNumber = GetContextValue(Constants.EventContextTypes.PrimaryCertificateEndorsementNumber))).Value;
		ZString? primaryCertificateEndorsementNumber;

		public ZString SecondaryCertificateTemplateCode => (secondaryCertificateTemplateCode ?? (secondaryCertificateTemplateCode = GetContextValue(Constants.EventContextTypes.SecondaryCertificateTemplateCode))).Value;
		ZString? secondaryCertificateTemplateCode;

		public ZString SecondaryCertificateEndorsementNumber => (secondaryCertificateEndorsementNumber ?? (secondaryCertificateEndorsementNumber = GetContextValue(Constants.EventContextTypes.SecondaryCertificateEndorsementNumber))).Value;
		ZString? secondaryCertificateEndorsementNumber;

		ZString GetContextValue(ZString type)
		{
			var context = subContextCollection?.FirstOrDefault(x => IsContextType(x, type));
			return context?.Value.GetValueOrDefault() ?? ZString.Empty;
		}

		bool IsContextType(Context context, ZString type)
		{
			return context.Type.Type.GetValueOrDefault().EqualsIgnoringCase(type);
		}
	}
}
