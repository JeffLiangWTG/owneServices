using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Module;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class AUSendTestCustomsMessageController : SendDiagnosticMessageController
	{
		protected override DiagnosticConsol GetNewBusinessEntity(BusinessObjectFactory factory)
		{
			return new AUDiagnosticConsol(factory);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
