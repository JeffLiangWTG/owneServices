using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EMCSAddInfoJobDeclaration : EU.EMCS.Business.EMCSAddInfoJobDeclaration
	{
		public EMCSAddInfoJobDeclaration(EMCSJobDeclaration declaration) : base(declaration)
		{
		}

		public override ZString ZG_DeferredSubmission
		{
			get => base.ZG_DeferredSubmission;
			set
			{
				bool hasChanged = ZG_DeferredSubmission != value;
				base.ZG_DeferredSubmission = value;
				if (!IsCopying && hasChanged)
				{
					Declaration.CusContainers.MarkAsNeedingValidation();
				}
			}
		}

		public new EMCSAddInfoJobDeclarationValidation Validation => (EMCSAddInfoJobDeclarationValidation)base.Validation;
		protected override EUEMCSAddInfoValidation GetNewValidation() => new EMCSAddInfoJobDeclarationValidation(this);

		public new EMCSAddInfoJobDeclarationLookups Lookups => (EMCSAddInfoJobDeclarationLookups)base.Lookups;
		protected override EUEMCSAddInfoLookups GetNewLookups() => new EMCSAddInfoJobDeclarationLookups(this);
	}
}
