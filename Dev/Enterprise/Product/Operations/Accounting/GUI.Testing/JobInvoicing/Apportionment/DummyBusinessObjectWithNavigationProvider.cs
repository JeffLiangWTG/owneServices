using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting.Testing
{
	class DummyBusinessObjectWithNavigationProvider : DummyBusinessObject, INavigationControllerIDProvider, IJobCostingPlugIn
	{
		public DummyBusinessObjectWithNavigationProvider(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region IJobCostingPlugIn

		public ZString JK_UniqueConsignRef => null;

		public RefUNLOCO LoadPort => null;

		public RefUNLOCO DischargePort => null;

		public JobProfitLossCollection ProfitLossContainer => null;

		public decimal ConsolExchangeRate => 0m;

		public RefCurrency ConsolCurrency => null;

		public bool IsMasterCollect => false;

		public OrgHeader ReceivingAgent => null;

		public OrgHeader ReceivingAgentAPInvoicingParty => null;

		public OrgHeader ReceivingAgentARInvoicingParty => null;

		public OrgHeader SendingAgent => null;

		public OrgHeader SendingAgentAPInvoicingParty => null;

		public OrgHeader SendingAgentARInvoicingParty => null;

		public ZString ContainerMode => ZString.Empty;

		public ZString ConsolType => ZString.Empty;

		public ZString Direction => ZString.Empty;

		public ZString Module => throw new NotImplementedException();

		public CodeDescriptionPairList PrepaidCollectList => null;

		public IGenericJobCostSupporter CostSupporter => costSupporter ?? (costSupporter = new DummyObjectWithGenericJobCostSupporter());

		IGenericJobCostSupporter costSupporter;

		public decimal ExchangeRateForCurrency(RefCurrency currency, ZGuid currentJobConsolCostPK) => 0m;

		public void AddNewToLogs(Event @event, ZString reference) => throw new NotImplementedException();

		public ZString GetPrepaidCollect(IJobInvoicingPlugIn apportionableJob) => ZString.Empty;

		public ZString TransportMode => throw new NotImplementedException();

		#endregion

		#region INavigationControllerIDProvider

		public ControllerID GetValidControllerID(object dataSource)
		{
			return DummyControllerIDs.Dummy;
		}

		public bool ShouldLoadBusinessObject { get => false; }

		#endregion
	}

	class DummyObjectWithGenericJobCostSupporter : IGenericJobCostSupporter
	{
		public DummyObjectWithGenericJobCostSupporter()
		{
		}

		public ZGuid PK => ZGuid.NewZGuid();

		public ZString Type => ZString.Empty;

		public ZGuid[] ShipmentsListPKs => null;

		public IJobInvoicingPlugIn[] ShipmentsList => null;

		public bool HasChanges => false;

		public bool IsInDatabase => false;

		public DocumentSupporter DocumentSupporter => null;

		public ZString MasterBillNum => ZString.Empty;

		public ZString TransportMode => ZString.Empty;

		public ZString TotalChargeableUnit => ZString.Empty;

		public Directions Direction => throw new NotImplementedException();

		public bool IsBuyersConsol => false;

		public ZString PortOfLoading => ZString.Empty;

		public ZString PortOfDischarge => ZString.Empty;

		public ZString ConsolMode => ZString.Empty;

		public OrgHeader SendingForwarder => null;

		public OrgHeader ReceivingForwarder => null;

		public ManyToManyBusinessObjectCollection Shipments => null;

		public ZDateTime ETD => ZDateTime.Now;

		public ZDateTime ETA => ZDateTime.Now;

		public IEnumerable<ZString> ExcludedApportionmentMethods => null;

		public bool IsApportionmentFilterEnabled => false;

		public ZDecimal FreeSpace => 0m;

		public SecurityCheckpoint JobConsolCostingCheckPoint => Env.Security.None;

		public ZGuid GetCreditorPK(ZString chargeGroup, ZGuid rateProviderOrgPK)
		{
			throw new NotImplementedException();
		}
	}
}
