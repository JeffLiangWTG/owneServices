using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class EMCSJobDeclarationLookups : EU.EMCS.Business.EMCSJobDeclarationLookups
	{
		public EMCSJobDeclarationLookups(EU.EMCS.Business.EMCSJobDeclaration parent) : base(parent) { }

		protected new EMCSJobDeclaration Parent => (EMCSJobDeclaration)base.Parent;

		public CodeDescriptionPairList Credentials => Parent.EMCSCredentialCollection;
	}
}
