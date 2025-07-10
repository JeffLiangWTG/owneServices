using System.Data;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.TNT.Testing
{
	public class EDNExporterForTesting : EDNExporter
	{
		public EDNExporterForTesting() : base(new NotificationBuffer(null))
		{
		}

		public new DataTable GetDeclarationCusEntryNumbers()
		{
			return base.GetDeclarationCusEntryNumbers();
		}

		public new string GetDeclarationCusEntryNumbersSelectCommand()
		{
			return base.GetDeclarationCusEntryNumbersSelectCommand();
		}
	}
}
