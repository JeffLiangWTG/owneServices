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
	public class SendCatalogDeactivateActionMethod : OperationalActionMethod
	{
		public SendCatalogDeactivateActionMethod() : base(new ZGuid("{66E722DE-C982-4BB6-A9E8-E97A31FB99F5}"))
		{
		}

		public override string Name => Res.GetString("{654060ED-B6DB-432B-9073-FD411A63DF78}", "Deactivate");

		public override string Description => Res.GetString("{D3061029-DF4B-459C-9E8F-17C029D5B4B1}", "Send Catalog Message - Deactivate");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings) => new SendCatalogDeactivateApplicator(factory);
	}
}
