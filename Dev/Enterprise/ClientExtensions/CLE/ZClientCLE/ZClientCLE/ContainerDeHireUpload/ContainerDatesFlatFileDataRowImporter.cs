using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.CLE
{
	internal class ContainerDatesFlatFileDataRowImporter
	{
		public ContainerDatesFlatFileDataRowImporter(BusinessObjectFactoryProvider factoryProvider)
		{
			FactoryProvider = factoryProvider;
		}

		public void Import(ContainerDatesFlatFileDataRow row, NotificationBuffer notifications, IExceptionBuffer exceptions)
		{
			ImportCore(row, notifications, exceptions);
		}

		#region Implementation

		void ImportCore(ContainerDatesFlatFileDataRow row, NotificationBuffer notifications, IExceptionBuffer exceptions)
		{
			if (row != null && !row.ContainerNumber.IsEmpty)
			{
				CommonContainer[] containers = FactoryProvider.Current.Load<CommonContainer>(GetContainerFilter(row.ContainerNumber));
				if (containers.Length == 0)
				{
					exceptions.AddToExceptionReport(CLEConstants.ContainerNotFound, row);
				}
				else
				{
					foreach (CommonContainer container in containers)
					{
						if (GetIsLinkedToImportConsol(container) || GetIsLinkedToImportDeclaration(container))
						{
							UpdateContainerFromRow(container, row, exceptions);
						}
					}
				}
			}
		}

		bool GetIsLinkedToImportConsol(CommonContainer container)
		{
			return container.IsConsolContainer && container.IsImport();
		}

		bool GetIsLinkedToImportDeclaration(CommonContainer container)
		{
			BaseCusContainer cusContainer = FactoryProvider.Current.LoadTop1<BaseCusContainer>(new ZQuery(CusContainerSchema.CO_JC, container.PK));
			return cusContainer != null && cusContainer.Declaration != null && cusContainer.Declaration.IsImport;
		}

		void UpdateContainerFromRow(CommonContainer container, ContainerDatesFlatFileDataRow row, IExceptionBuffer exceptions)
		{
			ZString deliveryDateString = row.DeliveryDateString.Trim();
			ZString deHireDateString = row.DeHireDateString.Trim();

			ZDateTime deHireDate = ZDateTime.Empty;
			ZDateTime deliveryDate = ZDateTime.Empty;
			ZDateTime.TryParseISO8601Date(deliveryDateString, out deliveryDate);
			ZDateTime.TryParseISO8601Date(deHireDateString, out deHireDate);

			bool canUpdateDates = false;
			if ((!deliveryDate.IsValid && deliveryDateString == "YARD")
				|| (deliveryDateString.CompareTo("U/BOND") != 0 && deliveryDateString.CompareTo("HELD") != 0))
			{
				if (deliveryDate.IsValid && !deliveryDate.IsEmpty)
				{
					ZDateTime containerCartageComplete = container.JC_ArrivalCartageComplete;
					if (!containerCartageComplete.IsEmpty && containerCartageComplete != deliveryDate)
					{
						exceptions.AddToExceptionReport(CLEConstants.PreviouslyDelivered + " " + containerCartageComplete.ToShortDateString(), row);
					}
					if (deHireDate.IsValid && deHireDate < deliveryDate)
					{
						exceptions.AddToExceptionReport(CLEConstants.DeHirePriorToDelivery, row);
					}
					canUpdateDates = true;
				}

				if (deHireDate.IsValid && !deHireDate.IsEmpty)
				{
					ZDateTime containerReturnedOn = container.JC_ContainerYardEmptyReturnGateIn;
					if (!containerReturnedOn.IsEmpty && containerReturnedOn != deHireDate)
					{
						exceptions.AddToExceptionReport(CLEConstants.PreviouslyDeHired + " " + containerReturnedOn.ToShortDateString(), row);
					}
					if (!deliveryDate.IsValid || deliveryDate.IsEmpty || deliveryDateString.IsEmpty)
					{
						string exceptionString = (deliveryDateString == "YARD") ?
							CLEConstants.YardAndDeHireDate + " " + deHireDate.ToShortDateString() :
							CLEConstants.DeHireButNoDelivery;

						exceptions.AddToExceptionReport(exceptionString, row);
					}
					canUpdateDates = true;
				}

				if (deliveryDateString.Trim() == "YARD")
				{
					container.JC_FCLHeldInTransitStaging = true;
				}
				if (canUpdateDates)
				{
					if (deliveryDate.IsValid)
					{
						container.JC_ArrivalCartageComplete = deliveryDate;
					}
					if (deHireDate.IsValid)
					{
						container.JC_ContainerYardEmptyReturnGateIn = deHireDate;
					}
				}
			}
		}

		ZQuery GetContainerFilter(string containerNumber)
		{
			ZQuery result = new ZQuery(JobContainerSchema.JC_ContainerNum, containerNumber);
			result.AddToFilter(JobContainerSchema.JC_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddMonths(-6));
			return result;
		}

		readonly BusinessObjectFactoryProvider FactoryProvider;

		#endregion
	}
}
