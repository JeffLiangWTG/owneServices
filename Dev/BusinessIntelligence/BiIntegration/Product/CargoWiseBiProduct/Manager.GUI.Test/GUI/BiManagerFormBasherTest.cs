using System.Windows.Forms;
using CargoWise.Bi.Product.Manager.Business;
using CargoWise.Data;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.GUI.Testing
{
	[TestedType(typeof(BiManagerForm))]
	public class BiManagerFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
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

		#endregion
	}
}
