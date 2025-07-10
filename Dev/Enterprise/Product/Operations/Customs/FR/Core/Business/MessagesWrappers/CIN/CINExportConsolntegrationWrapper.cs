using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.CIN
{
	public class CINExportConsolntegrationWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CINExportConsolntegrationWrapper(ForwardingConsol consol)
			: base(consol.Factory)
		{
			this.ForwardingConsol = consol;
		}
		public CINExportConsolntegrationWrapper()
		{
		}

		public ForwardingConsol ForwardingConsol
		{
			get;
			private set;
		}

		bool IsExportConsol => ForwardingConsol?.IsExport() ?? ZBool.False;

		public bool IsCINExportMenuEnabled => IsExportConsol &&
				ForwardingConsol.JK_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.France, StringComparison.Ordinal) && ForwardingConsol.IsAir;
	}
}
