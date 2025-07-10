using System.Windows.Forms;
using CargoWise.Bi.Product.Manager.Business;
using CargoWise.Bi.Product.Manager.GUI;
using CargoWise.Data;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Controller.Testing
{
	[TestedType(typeof(BiManagerModule))]
	public class BiManagerModuleTest : ZPopupModuleBasherTest
	{
		public override Form GetFormToBash()
		{
			var bo = new BiMonitorBusinessObject();
			bo.RefreshCdcInformation();
			bo.AuditInformation = new AuditInformation(Db.ServerName, Db.AuditDatabaseName);
			bo.AuditInformation.RefreshInfo();
			bo.EdwInformation = new EdwInformation(Db.ServerName, Db.EdwDatabaseName);
			bo.EdwInformation.RefreshInfo();
			bo.AnalysisServerInfo = new AnalysisServerInformation(Db.ServerName, Db.ServerName);
			bo.AnalysisServerInfo.RefreshInfo();
			return new BiManagerForm(bo);
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.BiManager;
		}
	}
}
