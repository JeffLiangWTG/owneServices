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
	public class SendCatalogCreateNewVersionActionMethod : OperationalActionMethod
	{
		public SendCatalogCreateNewVersionActionMethod() : base(new ZGuid("55FF9F51-8D56-423E-8A00-77C11030E715"))
		{
		}

		public override string Name => Res.GetString("086CF003-E78C-4794-98D8-3FFA8F1CC4AC", "Create New Version");

		public override string Description => Res.GetString("18DF4B0A-B93D-41AA-A16F-FBA27FC8D896", "Send Catalog Message - Create New Version");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings) => new SendCatalogCreateNewVersionApplicator(factory);
	}
}
