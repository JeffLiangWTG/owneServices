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
	public class SendCatalogActivateActionMethod : OperationalActionMethod
	{
		public SendCatalogActivateActionMethod() : base(new ZGuid("47BB0DB1-AEAD-45A4-8A5F-03ECF95AC736"))
		{
		}

		public override string Name => Res.GetString("F28B0543-4E75-47BC-BD8C-3E5D6F72D86C", "Activate");

		public override string Description => Res.GetString("84E10A8A-DF59-47BB-BDEC-A10A7AA9D0D3", "Send Catalog Message - Activate");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings) => new SendCatalogActivateApplicator(factory);
	}
}
