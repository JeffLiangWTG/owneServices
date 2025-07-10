using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderDocumentSupporter : DocumentSupporter
	{
		public NctsHeaderDocumentSupporter(NctsHeader parent) : base(parent)
		{
			this.nctsHeader = parent;
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			var result = new List<DataContext>();
			result.Add(DataContext.EuNcts);
			result.Add(DataContext.GenericFreightJob);
			result.Add(DataContext.GenericFreightJobInvoice);
			result.Add(DataContext.EuNcts5TAD);
			return result.ToArray();
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			switch (filterName)
			{
				case DocumentFilters.NCTSTransitDocumentsSupport:
					return GetNctsTransitDocumentsSupportFilterValue();
			}
			return base.GetFilterValue(filterName);
		}

		DocumentWrapper[] GetGenericWrappers(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, nctsHeader);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}
			return null;
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var result = new List<DocumentWrapper>();
			ZString classAndNamespaceName = null;
			switch (dataContext)
			{
				case DataContext.EuNcts:
					classAndNamespaceName = "Enterprise.DocumentWrappers.Customs.EU.NCTS.NctsHeaderDocumentWrapper";
					break;
				case DataContext.GenericFreightJob:
				case DataContext.GenericFreightJobInvoice:
					result = GetGenericWrappers(dataContext, commandBeingRun).ToList();
					break;
				case DataContext.EuNcts5TAD:
					classAndNamespaceName = "Enterprise.DocumentWrappers.Customs.EU.NCTS.Phase5NctsHeaderTADDocumentWrapper";
					break;
					// Other cases go here as needed...
			}

			if (!classAndNamespaceName.IsEmpty)
			{
				var wrapper = DocumentWrapperFactory.CreateWrapperWithoutException(classAndNamespaceName, nctsHeader);
				if (wrapper != null)
				{
					result.Add(wrapper);
				}
			}
			return result.ToArray();
		}

		public override string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider dataProvider)
		{
			switch (filterType)
			{
				case MenuTemplateFilterType.Security:
					if (nctsHeader != null)
					{
						return nctsHeader.IsSecurityDeclaration.ToString();
					}
					break;
			}
			return base.GetMenuTemplateFilterValue(filterType, dataProvider);
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CusInBondHeader; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Environment.Env.Security.None; }
		}

		public override MultilingualString CustomWatermarkText => WatermarkText;

		public MultilingualString WatermarkText { get; set; }

		#region Implementation

		readonly NctsHeader nctsHeader;

		string GetNctsTransitDocumentsSupportFilterValue()
		{
			if (nctsHeader == null)
			{
				return ZBool.False.ToString();
			}
			return NctsConfiguration.GetConfiguration(Factory, nctsHeader.BrokerageCountryCode).DocDataPlugInSupport(nctsHeader).ToString();
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			IDocumentDeliveryContact result = null;
			if (contactType == ContactType.Consignee)
			{
				result = new OrgHeaderContact(nctsHeader?.Consignee?.Address?.Header, nctsHeader?.Consignor?.Address?.Header, nctsHeader?.Consignee?.Address);
			}
			else if (contactType == ContactType.Consignor)
			{
				result = new OrgHeaderContact(nctsHeader?.Consignor?.Address?.Header, nctsHeader?.Consignee?.Address?.Header, nctsHeader?.Consignor?.Address);
			}
			else if (contactType == ContactType.Principal)
			{
				result = new OrgHeaderContact(nctsHeader?.Principal?.Address?.Header, nctsHeader?.Principal?.Address);
			}
			else if (contactType == ContactType.Declarant)
			{
				result = new OrgHeaderContact(nctsHeader?.MovementHeader?.Representative?.Organisation?.MainAddress?.Header, nctsHeader?.MovementHeader?.Representative?.Organisation?.MainAddress);
			}
			else if (contactType == ContactType.Warehouse)
			{
				result = new OrgHeaderContact(nctsHeader?.MovementHeader?.WarehouseAddress?.Header, nctsHeader?.MovementHeader?.WarehouseAddress);
			}
			else if (contactType == ContactType.TransportServices || contactType == ContactType.LocalTransport || contactType == ContactType.ShippingLine)
			{
				result = new OrgHeaderContact(nctsHeader?.MovementHeader?.Carrier?.Organisation?.MainAddress?.Header, nctsHeader?.MovementHeader?.Carrier?.Organisation?.MainAddress);
			}
			else if (contactType == ContactType.Receivables || contactType == ContactType.LocalClient || contactType == ContactType.Payables)
			{
				var job = nctsHeader?.Job;
				if (job != null)
				{
					result = new OrgHeaderContact(job.LocalCharges, job.LocalChargesAddr);
				}
				else
				{
					var jobHeaderForeignKeyLink = GetJobHeaderForeignKeyLink(nctsHeader);

					if (jobHeaderForeignKeyLink != null)
					{
						result = new OrgHeaderContact(jobHeaderForeignKeyLink.LocalCharges, null);
					}
				}
			}
			else if (contactType == ContactType.ImportBroker || contactType == ContactType.ExportBroker)
			{
				var shipment = nctsHeader?.Shipment;
				if (shipment != null && (shipment.IsExport() || shipment.IsImport()))
				{
					result = shipment.IsExport() ? new OrgHeaderContact(shipment.ExportBroker, null) : new OrgHeaderContact(shipment.ImportBroker, null);
				}
				else
				{
					result = nctsHeader?.Company != null ? new OrgHeaderContact(nctsHeader?.Company?.OrgProxy, null) : null;
				}
			}
			return result;
		}

		public static JobHeader GetJobHeaderForeignKeyLink(NctsHeader header)
		{
			var filter = new ZQuery();
			filter.AddToFilter(JobHeaderSchema.JH_ParentID, header.PK);
			filter.AddToFilter(JobHeaderSchema.JH_GC, header.Branch.GB_GC);
			filter.AddToFilter(JobHeaderSchema.JH_ParentTableCode, CusInBondHeaderSchema.Constants.Prefix);
			return header.Factory.LoadTop1<JobHeader>(filter);
		}
		#endregion
	}
}
