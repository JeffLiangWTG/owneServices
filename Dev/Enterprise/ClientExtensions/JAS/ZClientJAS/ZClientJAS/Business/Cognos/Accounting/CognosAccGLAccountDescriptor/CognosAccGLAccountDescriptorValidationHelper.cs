
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class CognosAccGLAccountDescriptorValidationHelper : AccGLAccountDescriptorValidationHelper
	{
		public CognosAccGLAccountDescriptorValidationHelper(CognosAccGLAccountDescriptor account, ZString language, ZString countryCode, BusinessObjectFactory factory)
			: base(account, language, countryCode, factory)
		{
		}

		protected override bool ShouldEnsureConsolidationAccountNumGreaterThanAccountNum
		{
			get { return (Language != Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger || !CognosGLAccount.RequiresCognosConsolidationAccount); }
		}

		CognosAccGLAccountDescriptor CognosGLAccount
		{
			get { return (CognosAccGLAccountDescriptor)GLAccount; }
		}
	}
}
