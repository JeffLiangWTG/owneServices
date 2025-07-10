using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Forwarding.Business;
using Enterprise.Customs.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Forwarding.Module
{
	class ForwardingConsolModuleCustomsColumnsProvider : GridCustomColumnsProvider
	{
		public ForwardingConsolModuleCustomsColumnsProvider()
		{
		}

		protected override ZGridCustomColumnsInitializer GetCustomColumnsInitializer(ZGrid grid, IBusinessObjectCollection collection, ICustomPropertyContainer propertyContainer)
		{
			return new ForwardingZGridCustomColumnsInitializer(grid, collection, GroupName, propertyContainer);
		}

		protected override CustomPropertyContainer GetCustomPropertyContainer()
		{
			var propertyContainer = new CustomPropertyContainer();

			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingConsolCustomsColumnConstants.Schema.CustomsCargoStatus, ForwardingConsolCustomsColumnConstants.Captions.CustomsCargoStatusDescription));

			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingConsolCustomsColumnConstants.Schema.AMSBillStatus, ForwardingConsolCustomsColumnConstants.Captions.AMSBillStatusDescription));
			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingConsolCustomsColumnConstants.Schema.AMSBillStatusDescription, ForwardingConsolCustomsColumnConstants.Captions.AMSBillStatusDescriptionDescription));

			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingConsolCustomsColumnConstants.Schema.LatestAMSDispositionCode, ForwardingConsolCustomsColumnConstants.Captions.LatestAMSDispositionCode));
			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingConsolCustomsColumnConstants.Schema.LatestAMSDispositionDesc, ForwardingConsolCustomsColumnConstants.Captions.LatestAMSDispositionDesc));

			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingConsolCustomsColumnConstants.Schema.AFRBillStatus, ForwardingConsolCustomsColumnConstants.Captions.AFRBillStatusDescription));
			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingConsolCustomsColumnConstants.Schema.AFRBillStatusDescription, ForwardingConsolCustomsColumnConstants.Captions.AFRBillStatusDescriptionDescription));

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.NewZealand)
			{
				propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingConsolCustomsColumnConstants.Schema.OutwardReportStatus, ForwardingConsolCustomsColumnConstants.Captions.OutwardReportStatusDescription));
				propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingConsolCustomsColumnConstants.Schema.OutwardReportStatusDescription, ForwardingConsolCustomsColumnConstants.Captions.OutwardReportStatusDescriptionDescription));
				propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingConsolCustomsColumnConstants.Schema.OutwardReportEntryNumber, ForwardingConsolCustomsColumnConstants.Captions.OutwardReportEntryNumberDescription));
			}

			propertyContainer.AddCustomProperty(GetCustomProperty(ForwardingConsolCustomsColumnConstants.Schema.AsycudaRegistrationStatus, ForwardingConsolCustomsColumnConstants.Captions.AsycudaRegistrationStatusDescription));

			return propertyContainer;
		}

		CustomPropertyImplementation<BusinessObject> GetCustomProperty(string propertyName, MultilingualString caption)
		{
			return new CustomPropertyImplementation<BusinessObject>(propertyName, caption, typeof(ZString), bo => GetCustomsProperyValue(bo, propertyName));
		}

		object GetCustomsProperyValue(BusinessObject businessObject, string propertyName)
		{
			var consol = businessObject as ForwardingConsol;

			if (consol == null)
			{
				return null;
			}

			ForwardingConsolCustomsInformation customsInformation;

			if (customsInformationsCache.ContainsKey(consol.PK))
			{
				customsInformation = customsInformationsCache[consol.PK];
			}
			else
			{
				customsInformation = new ForwardingConsolCustomsInformation(consol);
				customsInformationsCache.Add(consol.PK, customsInformation);
			}

			return customsInformation[propertyName];
		}

		protected override void AddFetchHintForView(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			foreach (var column in columns)
			{
				foreach (var businessObject in businessObjects)
				{
					if (column.ColumnName == ForwardingConsolCustomsInformation.Schema.OutwardReportStatusDescription)
					{
						businessObject.Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, businessObject.PK);
					}
				}
			}
		}

		readonly Dictionary<ZGuid, ForwardingConsolCustomsInformation> customsInformationsCache = new Dictionary<ZGuid, ForwardingConsolCustomsInformation>();
	}
}
