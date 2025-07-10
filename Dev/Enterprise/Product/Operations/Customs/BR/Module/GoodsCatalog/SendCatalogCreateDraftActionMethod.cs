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
	public class SendCatalogCreateDraftActionMethod : OperationalActionMethod
	{
		public SendCatalogCreateDraftActionMethod() : base(new ZGuid("894CCDE9-6CC2-46F3-9FE1-16162E18CAE4"))
		{
		}

		public override string Name => Res.GetString("C868D8DB-CB33-480B-82D0-279088B973D2", "Create Draft");

		public override string Description => Res.GetString("2E035B3D-30F3-49E0-B338-86CD5EF05391", "Send Catalog Message - Create Draft");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings) => new SendCatalogCreateDraftApplicator(factory);
	}
}
