using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ClientSharedComponents;
using Enterprise.Integration;

namespace Enterprise.Client.ELG
{
	public abstract class SagAccountsExportDirector : AccountsExportDirectorARAP
	{
		public SagAccountsExportDirector(BusinessObjectFactory factory, INotifications notificationSubscriber)
			: base(factory, notificationSubscriber)
		{
		}

		protected override string ExportPathName
		{
			get { return ELGDataRegistry.Instance.SagExportDirectory; }
		}

		protected override System.Guid RecipientGroupPK
		{
			get { return ELGDataRegistry.Instance.SagExportNotifyGroup.ToGuid(); }
		}

		protected override IRegistryItem RecipientGroup
		{
			get { return ELGDataRegistry.Instance.SagDataTransferSwitchRegistryItem; }
		}

		protected override AccountsExporterARAP NewARExporter(int batchNumber, BusinessObjectFactory factory)
		{
			return new SagARExporter(batchNumber, factory, notifications);
		}

		protected override AccountsExporterARAP NewAPExporter(int batchNumber, BusinessObjectFactory factory)
		{
			return new SagAPExporter(batchNumber, factory, notifications);
		}

		protected override string AccountingPackageName
		{
			get { return "Sage "; }
		}
	}
}
