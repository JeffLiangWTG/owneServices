using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.CLE.OrdersDataImport
{
	public class CLEOrderDataImporter : FlatFileDataImporter
	{
		protected override IValueObject CreateXsd()
		{
			return new Xsd.Orders();
		}

		protected override IFlatFileConverter CreateConverter(INotifications notifications)
		{
			return new CLEOrderDataConverter(notifications, FactoryProvider.Current);
		}

		protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
		{
			bool result = false;

			try
			{
				result = base.ImportDataToFactoryCore(dataReader, attachmentFileName, notifications, out additionalTransactionActions);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				additionalTransactionActions = null;
			}

			return result;
		}

		public ZString ErrorOrderNumbers
		{
			get
			{
				return errorOrderNumbers;
			}
		}
		ZString errorOrderNumbers;

		protected override bool ExtractToDataAdapter(IValueObject xsd, INotifications notifications)
		{
			Xsd.Orders ordersValue = (Xsd.Orders)xsd;
			Dictionary<ZGuid, ImportedOrder> importedOrdersDictionary = NewImportedOrdersDictionary();
			Adapter.SetOrdersCount(ordersValue.Order.Count);

			foreach (Xsd.Order orderValue in ordersValue.Order)
			{
				var interchange = new Xsd.XmlInterchange();
				interchange.InterchangeInfo.EDIOrganisation.OwnerCode = orderValue.OrderDetail.Custom.Text5;

				var importContext = new ValueObjectImportContext(FactoryProvider.Current, interchange, notifications);

				Order importedOrder = null;
				try
				{
					importedOrder = Adapter.CreateOrUpdateFromValueObject(orderValue, importContext);
					if (!importedOrdersDictionary.ContainsKey(importedOrder.PK))
					{
						Order order = importedOrder;
						if (!order.CanBeUpdatedByImport)
						{
							var context = new ValueObjectImportContext(EmptyFactory, interchange, new NotificationBuffer());
							order = GetOrderChanges(orderValue, context);
						}

						if (order == null)
						{
							importedOrdersDictionary.Add(importedOrder.PK, new ImportedOrder(importedOrder));
						}
						else
						{
							importedOrdersDictionary.Add(order.PK, new ImportedOrder(order));
						}
					}
					if (!importedOrder.IsDeleted && importedOrder.IsInDatabase && !importedOrder.IsDeclarationAttached)
					{
						CheckIfNeedToCancelExistingOrder(importedOrder);
					}

					if (importedOrdersDictionary.Count >= CLEDataRegistry.Instance.MaximumOrdersToDeliver || ordersValue.Order[ordersValue.Order.Count - 1] == orderValue)
					{
						FactoryProvider.SaveCurrentAndCreateNew();
						ImportedOrderChangesDocumentManager.DeliverAllImportedOrderChanges(new List<ImportedOrder>(importedOrdersDictionary.Values));

						importedOrdersDictionary.Clear();
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					notifications.AddError("Error: Order " + orderValue.OrderIdentifier.OrderNumber + " cannot be created");
					notifications.AddError(e.Message);
					if (errorOrderNumbers.IsEmpty)
					{
						errorOrderNumbers = orderValue.OrderIdentifier.OrderNumber;
					}
					else
					{
						errorOrderNumbers += ", " + orderValue.OrderIdentifier.OrderNumber;
					}
					throw;
				}
			}

			return true;
		}

		void CheckIfNeedToCancelExistingOrder(Order importedOrder)
		{
			var orderlines = importedOrder.OrderLines.Count(line => !line.IsDeleted && (line.JO_Quantity.IsEmpty || line.JO_LineStatus == Core.Constants.OrderStatus.Cancelled));

			if (importedOrder.OrderLines.Count == 0 || orderlines == importedOrder.OrderLines.Count)
			{
				importedOrder.JD_OrderStatus = Core.Constants.OrderStatus.Cancelled;
			}
		}

		CLEOrder GetOrderChanges(Xsd.Order orderValue, ValueObjectImportContext importContext)
		{
			CLEOrder order = Adapter.FindOrder(orderValue, importContext);
			if (order != null)
			{
				Adapter.ImportFromValueObject(order, orderValue, importContext);
			}
			return order;
		}

		BusinessObjectFactory EmptyFactory
		{
			get
			{
				return emptyFactory ?? (emptyFactory = new BusinessObjectFactory());
			}
		}
		BusinessObjectFactory emptyFactory;

		protected override IFlatFileFormat FlatFileFormat
		{
			get
			{
				return new CsvFlatFileFormat();
			}
		}

		CLEOrderValueObjectDataAdapter Adapter
		{
			get
			{
				return adapter ?? (adapter = new CLEOrderValueObjectDataAdapter());
			}
		}
		CLEOrderValueObjectDataAdapter adapter;

		protected virtual Dictionary<ZGuid, ImportedOrder> NewImportedOrdersDictionary()
		{
			return new Dictionary<ZGuid, ImportedOrder>();
		}
	}
}

#region Set up
#endregion
