using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public static class IncompleteInvoiceBOIsSavedByFactoryServiceProvider
	{
		public static void RegisterInvoiceToSaveOnlyInvoiceHeader(InvoicingBase allowedInvoiceHeader)
		{
			var provider = new InvoiceBOIsSavedByFactoryServiceProvider(BusinessContext.IncompleteInvoiceSaving);
			provider.RegisterInvoiceToSaveOnlyInvoiceHeader(allowedInvoiceHeader);
		}

		public static void RegisterInvoiceAndRelatedBizoNotToBeSaved(BusinessObjectFactory factory)
		{
			var provider = new InvoiceBOIsSavedByFactoryServiceProvider(BusinessContext.IncompleteInvoiceSaving);
			provider.RegisterInvoiceAndRelatedBizoNotToBeSaved(factory);
		}

		public static void DeregisterInvoiceFromSaveOnlyInvoiceHeader(InvoicingBase disallowedInvoiceToBeSaved)
		{
			var provider = new InvoiceBOIsSavedByFactoryServiceProvider(BusinessContext.IncompleteInvoiceSaving);
			provider.DeregisterInvoiceFromSaveOnlyInvoiceHeader(disallowedInvoiceToBeSaved);
		}
	}

	public static class PreviewInvoiceIsNotSavedByFactoryServiceProvider
	{
		public static void Register(BusinessObjectFactory factory)
		{
			var provider = new InvoiceBOIsSavedByFactoryServiceProvider(BusinessContext.PreviewInvoice);
			provider.RegisterInvoiceAndRelatedBizoNotToBeSaved(factory);
		}

		public static void Deregister(BusinessObjectFactory factory)
		{
			var provider = new InvoiceBOIsSavedByFactoryServiceProvider(BusinessContext.PreviewInvoice);
			provider.DeregisterContext(factory);
		}
	}

	public class InvoiceBOIsSavedByFactoryServiceProvider
	{
		public InvoiceBOIsSavedByFactoryServiceProvider(BusinessContext context)
		{
			this.context = context;
		}
		readonly BusinessContext context;

		public void RegisterInvoiceToSaveOnlyInvoiceHeader(InvoicingBase allowedInvoiceHeader)
		{
			var factory = allowedInvoiceHeader.Factory;
			var service = CreateBizosSavedByFactoryService(factory);
			service.AddAllowedInvoicePK(allowedInvoiceHeader.PK);
		}

		public void RegisterInvoiceAndRelatedBizoNotToBeSaved(BusinessObjectFactory factory)
		{
			CreateBizosSavedByFactoryService(factory);
		}

		InvoiceBOIsSavedByFactoryService CreateBizosSavedByFactoryService(BusinessObjectFactory factory)
		{
			var service = (InvoiceBOIsSavedByFactoryService)factory.ServiceContainer.GetService<IBOIsSavedByFactoryService>();
			if (service == null)
			{
				service = new InvoiceBOIsSavedByFactoryService();
				factory.ServiceContainer.AddService<IBOIsSavedByFactoryService>(service);
				factory.SetContext(context);
			}
			return service;
		}

		public void DeregisterInvoiceFromSaveOnlyInvoiceHeader(InvoicingBase disallowedInvoiceToBeSaved)
		{
			var factory = disallowedInvoiceToBeSaved.Factory;
			var service = (InvoiceBOIsSavedByFactoryService)factory.ServiceContainer.GetService<IBOIsSavedByFactoryService>();
			if (service != null)
			{
				service.RemoveAllowedInvoicePK(disallowedInvoiceToBeSaved.PK);
				if (!service.AnyAllowedInvoicePKs())
				{
					DeregisterContext(factory);
				}
			}
		}

		public void DeregisterContext(BusinessObjectFactory factory)
		{
			factory.RemoveContext(context);
			factory.ServiceContainer.RemoveService<IBOIsSavedByFactoryService>();
		}

		class InvoiceBOIsSavedByFactoryService : IBOIsSavedByFactoryService
		{
			public void AddAllowedInvoicePK(ZGuid invoicePK)
			{
				allowedInvoicePKs.Add(invoicePK);
			}

			public void RemoveAllowedInvoicePK(ZGuid invoicePK)
			{
				allowedInvoicePKs.Remove(invoicePK);
			}

			public bool AnyAllowedInvoicePKs()
			{
				return allowedInvoicePKs.Any();
			}

			public bool IsBOSavedByFactory(BusinessObject businessObject)
			{
				var excludedTableList = new[] {
					JobHeaderSchema.Constants.TableName,
					JobConsolCostSchema.Constants.TableName,
					JobExRateSchema.Constants.TableName
				};

				var includedTableList = new[] {
					AccDraftInvoiceHeaderSchema.Constants.TableName,
					AccTransactionHeaderReferenceSchema.Constants.TableName,
					JobChargeAttribSchema.Constants.TableName,
					JobConsolCostAttribSchema.Constants.TableName
				};

				return allowedInvoicePKs.Contains(businessObject.PK) ||
					!(
						businessObject.TableName.StartsWith((NoResString)"Acc", StringComparison.OrdinalIgnoreCase) ||
						businessObject.TableName.StartsWith("JobCharge", StringComparison.OrdinalIgnoreCase) ||
						businessObject.TableName.StartsWith((NoResString)"Dummy", StringComparison.OrdinalIgnoreCase) ||
						businessObject.TableName == OrgPatternMatchOverrideSchema.Constants.TableName ||         // Code mapping updater keeps modified mapping rule in memory for transaction organization for UXML cross ledger import
						excludedTableList.Contains(businessObject.TableName
					)) || includedTableList.Contains(businessObject.TableName);
			}

			readonly HashSet<ZGuid> allowedInvoicePKs = new HashSet<ZGuid>();
		}
	}
}
