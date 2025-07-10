using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Module
{
	public class SendCatalogUpdateDraftActionMethod : OperationalActionMethod
	{
		public SendCatalogUpdateDraftActionMethod() : base(new ZGuid("F9B63D42-D96B-4E08-8A41-0CE3B549725F"))
		{
		}

		public override string Name => Res.GetString("683A40F8-EB7C-44B8-9B7F-A3C1979D1674", "Update Draft");

		public override string Description => Res.GetString("4BF35954-BBE5-40B2-AD67-08CAA305CECE", "Send Catalog Message - Update Draft");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings) => new SendCatalogUpdateDraftApplicator(factory);
	}
}
