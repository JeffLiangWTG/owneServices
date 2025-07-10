using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.BR.Module
{
	class GoodsCatalogOperationalActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			return new OperationalActionMethod[]
			{
				new SendCatalogCreateDraftActionMethod(),
				new SendCatalogUpdateDraftActionMethod(),
				new SendCatalogActivateActionMethod(),
				new SendCatalogCreateNewVersionActionMethod(),
				new SendCatalogDeactivateActionMethod()
			};
		}
	}
}
