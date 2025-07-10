using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class NotificationDataRegistry : RegistryItemSet
	{
		NotificationDataRegistry()
		{
		}

		public override bool IsForProductivityWise => true;

		#region Update AP AR Account Balances Process Notification Group

		public GuidRegistryItem UpdateAPARAccountBalancesProcessNotificationGroupItem
		{
			get
			{
				return GetItem<GuidRegistryItem>("UpdateAPARAccountBalancesProcessNotificationGroupItem", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"UpdateAPARAccountBalancesProcessNotificationGroupItem",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("78e6a863-a424-4214-8e9b-872f1db02e75", "Update Account Balances Import Notification Group"),
						ResString.GetMultilingualString("ac04c122-987b-48a8-b720-61fd2de0b0b6", "The staff group that will be notified about the Result of Update Account Balances Import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						RegistryFactory.Instance.GetGroupPK("ALL")
					);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region OrganisationImportNotificationGroup

		public GuidRegistryItem OrganisationImportNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("OrganisationImportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"OrganisationImportNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("e310c3af-76ba-47bb-abb6-004ad44426d6", "Organization Import Notification Group"),
						ResString.GetMultilingualString("02a0e729-b474-4014-bf2f-e4b32311e6d1", "The staff group that will be notified about the Result of Organization XML Import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						RegistryFactory.Instance.GetGroupPK("ALL")
					);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Instance

		public static NotificationDataRegistry Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new NotificationDataRegistry();
				}
				return fInstance;
			}
		}

		[ThreadStatic]
		static NotificationDataRegistry fInstance;

		#endregion

		#region Fax Delivery Failures (Fallback)

		public GuidRegistryItem FaxDeliveryFailuresFallback
		{
			get
			{
				return GetItem<GuidRegistryItem>("FaxDeliveryFailuresFallback", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"FaxDeliveryFailuresFallback",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("3feb71a4-beab-4898-8c00-c1acaad8c07d", "Fax Delivery Failures (Fallback)"),
						ResString.GetMultilingualString("1ce14992-02ec-45f7-8cf1-8e22574870cf", "If a user sends a fax, and that user does not have an email address, delivery failure messages will be sent to this group instead. If the user does have an email address, delivery failures will only be sent to the user, not to this group."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region POD Import Notification Group

		public GuidRegistryItem PODImportNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("PODImportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"PODImportNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("52158cc6-89c6-4238-be19-35c9c28a0691", "POD Import Notification Group"),
						ResString.GetMultilingualString("b175a086-7d30-4380-b353-174d69a6c4f3", "The staff group that will be notified about the Result of POD CSV Import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Core.Constants.Groups.AllPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Product Import Notification Group

		public GuidRegistryItem ProductImportNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("ProductImportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ProductImportNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("69f56327-d40e-48aa-a3ee-dc9d3118ef0d", "Product Import Notification Group"),
						ResString.GetMultilingualString("63abb316-1bb1-4822-866e-2f28c9571e99", "The staff group that will be notified about the Result of Product CSV Import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						Core.Constants.Groups.AllPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Schedule Import Notification Group

		public GuidRegistryItem SchedulesImportNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("SchedulesImportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SchedulesImportNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("7492011a-72f4-43bd-82fa-faa97c76453b", "Schedules Import Notification Group"),
						ResString.GetMultilingualString("e0755a43-db39-4eb8-abe1-89bdc8a77bc3", "The staff group that will be notified about the Result of Schedule XML Import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						Core.Constants.Groups.AllPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region ImportedOrderChangesNotificationGroup

		public GuidRegistryItem ImportedOrderChangesNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("ImportedOrderChangesNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ImportedOrderChangesNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("2f8661f0-82e2-4de6-9867-952450ea8606", "Imported Order Changes Notification Group"),
						ResString.GetMultilingualString("f10ce040-5925-4a4e-b0d1-88b47806b723", "The staff group that will be notified about which changes were made to orders and order lines after a data import."),
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsValueOptional | RegistryOptions.PreserveTestValue,
						Guid.Empty
					);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region ContainerEventsImportNotificationGroup

		public GuidRegistryItem ContainerEventsImportNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("ContainerEventsImportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ContainerEventsImportNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("702dba13-4280-48d2-89d3-a18b20b42d86", "Container Event Import Notification Group"),
						ResString.GetMultilingualString("e0fdd2b4-84f2-4d71-86aa-52ad7bdc5693", "The staff group that will be notified about the Result of Container Events XML Import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						RegistryFactory.Instance.GetGroupPK("ALL")
					);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Customs Declaration Import Notification Group

		public GuidRegistryItem CustomsDeclarationImportNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("CustomsDeclarationImportNotificationGroup", delegate
				{
					return new GuidRegistryItem(
						"CustomsDeclarationImportNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("af285968-dc44-4096-a6c1-9440fe3f1812", "Customs Declaration Import Notification Group"),
						ResString.GetMultilingualString("f3549bce-c519-4323-b1d9-ea19cace04c2", "The staff group that will be notified about the Result of Customs Declaration XML Import."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						Core.Constants.Groups.AllPK);
				});
			}
		}

		#endregion

		#region Commercial Invoice Import Notification Group

		public GuidRegistryItem CommercialInvoiceImportNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("CommercialInvoiceImportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"CommercialInvoiceImportNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("20181d84-ecba-4546-b612-5b89d4895ca6", "Commercial Invoice Import Notification Group"),
						ResString.GetMultilingualString("1d3f4eea-dcca-45de-8be7-a6eb23527b47", "The staff group that will be notified about the Result of Commercial Invoice XML Import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Core.Constants.Groups.AllPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region EDIMessageDelivery Fail Group

		public GuidRegistryItem EDIMessageDeliveryFailNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("EDIMessageDeliveryFailNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"EDIMessageDeliveryFailNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("7d0bce0c-6fc7-49b9-b08b-4628942322b7", "EDI Communication mode delivery fail notification group"),
						ResString.GetMultilingualString("f6c932d9-90d3-41a9-91b1-87c80e62d1e8", "The staff group that will be notified when an EDI Communication mode delivery fails."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						Core.Constants.Groups.AllPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Consol Shipment Import Notification Group

		public GuidRegistryItem ConsolShipmentImportNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("ConsolShipmentImportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ConsolShipmentImportNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("73f1bee3-48aa-4203-a521-201d8255093e", "Consol Shipment Import Notification Group"),
						ResString.GetMultilingualString("7991569a-0688-4b1f-a8c1-ddef8520db6f", "The staff group that will be notified about the Result of Consol & Shipment XML Import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						Core.Constants.Groups.AllPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Booking Import Notification Group

		public GuidRegistryItem BookingImportNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("BookingImportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"BookingImportNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("f51d4165-7d65-44b4-af70-541abe542361", "Booking Import Notification Group"),
						ResString.GetMultilingualString("1e9e7cc9-f826-424e-aa0a-e2693ca2505b", "The staff group that will be notified about the Result of Booking XML Import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Core.Constants.Groups.AllPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Warehouse Import Notification Group

		public GuidRegistryItem WarehouseImportNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("WarehouseImportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"WarehouseImportNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("39755cba-3ec2-4419-aea7-bafcb35baa81", "Warehouse Import Notification Group"),
						ResString.GetMultilingualString("d3ed6554-28c5-4e7d-8488-5f0dda14e117", "The staff group that will be notified about the Result of Warehouse XML Import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Core.Constants.Groups.AllPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Order Import Notification Group

		public GuidRegistryItem OrderImportNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("OrderImportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"OrderImportNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("e409639d-7105-4919-a376-a809b0c2cc69", "Order Import Notification Group"),
						ResString.GetMultilingualString("dd9bb0c1-6698-46a6-b1d1-7395f059110d", "The staff group that will be notified about the Result of Order XML Import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Core.Constants.Groups.AllPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Agency Bill Of Lading Import Notification Group

		public GuidRegistryItem AgencyBillOfLadingImportNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("AgencyBillOfLadingImportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"AgencyBillOfLadingImportNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("A9DC6A43-7A5D-4AE8-9A88-AFCDB41AB91B", "Agency Bill Of Lading Import Notification Group"),
						ResString.GetMultilingualString("D3987E1F-8771-4B46-84D9-28D19DF0BDBF", "The staff group that will be notified about the Result of Agency Bill Of Lading XML Import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Core.Constants.Groups.AllPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Financial Transaction Import Notification Group

		public GuidRegistryItem FinancialTransactionImportNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("FinancialTransactionImportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"FinancialTransactionImportNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("64326BEF-C730-4D79-B878-27E434CBEF28", "Financial Transaction Import Notification Group"),
						ResString.GetMultilingualString("937A0E82-D286-4443-B2C2-447AF54B464A", "The staff group that will be notified about the Result of Financial Transaction XML Import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						Core.Constants.Groups.AllPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region

		public GuidRegistryItem NettingClearingJournalImportNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("NettingClearingJournalImportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"NettingClearingJournalImportNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("cf856e45-e38b-4320-b6b3-66cac386a5f8", "Netting Clearing Journal Import Notification Group"),
						ResString.GetMultilingualString("7c2df17f-ef48-4619-924f-99ae16713f91", "The staff group that will be notified about the Result of Automatic Netting Clearing Journal Import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						Core.Constants.Groups.AllPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Bank Statement Import Notification Group

		public GuidRegistryItem BankStatementImportNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("BankStatementImportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"BankStatementImportNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("c71234d9-fd2f-4fc9-b24d-266e27234d29", "Bank Statement Import Notification Group"),
						ResString.GetMultilingualString("c30c0154-5a6f-4ada-806a-11b415e32134", "The staff group that will be notified about the Result of Bank Statement XML Import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						Core.Constants.Groups.AllPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Container Movement Import Notification Group

		public GuidRegistryItem ContainerMovementImportNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("ContainerMovementImportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ContainerMovementImportNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("0B958393-B442-4C26-A9D4-FF393C4FB13A", "Container Movement Import Notification Group"),
						ResString.GetMultilingualString("522100CB-D803-4EA0-9A95-0DE15D34B739", "The staff group that will be notified about the Result of Container Movement XML Import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsValueOptional,
						Core.Constants.Groups.AllPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Local Cartage Notification Group

		public GuidRegistryItem LocalCartageNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("LocalCartageNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"LocalCartageNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("eec8a732-b124-4139-b703-0c62422b7ce7", "Port Transport Notification Group"),
						ResString.GetMultilingualString("c2b21b69-08e1-4c34-b1b1-ee01178fb6fa", "The staff group that will be notified about the Result of Port Transport XML file import/export."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						Core.Constants.Groups.AllPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region ediWebPrint

		public GuidRegistryItem WebPrintNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("WebPrintNotificationGroup", delegate
				{
					return new GuidRegistryItem(
						"WebPrintNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("2813ead6-2f61-44ba-93d4-e31f405ffe26", "WebPrint Notification Group"),
						ResString.GetMultilingualString("5dc8baf0-6357-4492-9d65-7a4cf675605f", "Staff group to notify when some significant actions with WebPrint service are occurred."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueOptional,
						Core.Constants.Groups.AllPK);
				});
			}
		}

		#endregion

		#region Database Health Check Notification Group

		public GuidRegistryItem DatabaseHealthCheckNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("DatabaseHealthCheckNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"DatabaseHealthCheckNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("b058ef0d-ee24-4b88-ab6e-ccde719e87ff", "Database Health Check Notification Group"),
						ResString.GetMultilingualString("eab2159e-fe3e-4f0b-a4a2-c14d84c64f2d", "The staff group that will receive Database Health Warning notification emails."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region BI Admin Notification Group

		public GuidRegistryItem BiAdminNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("BiAdminNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"BiAdminNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("94E2C104-AFC5-484C-BCF1-2C7C02E44034", "BI Admin Notification Group"),
						ResString.GetMultilingualString("F4305BDE-BC58-4446-A4FD-FAF319067A39", "The staff group that will receive BI notification emails."),
						RegistryStorageFlags.System,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Event Import Notification Group

		public GuidRegistryItem EventImportNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("EventImportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"EventImportNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("3a3d1324-0735-4aea-a752-c987b395ee19", "Event Import Notification Group"),
						ResString.GetMultilingualString("0c933915-5c8f-4465-bd64-b487d4f58cb7", "The staff group that will be notified about the Result of Event XML Import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						Core.Constants.Groups.AllPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Database Backup and Maintenance Notification Group

		public GuidRegistryItem DatabaseBackupAndMaintenanceNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("DatabaseBackupAndMaintenanceNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"DatabaseBackupAndMaintenanceNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("c1de20e2-600a-4dc9-923c-95452de4ee4c", "Database Backup and Maintenance Notification Group"),
						ResString.GetMultilingualString("3d696fe9-4b58-4bb4-9234-8887e8fc0900", "The staff group that will be notified about the Database Backups and Maintenance."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						Core.Constants.Groups.PostMastersGroupPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region System Service Tasks Notification Group

		public GuidRegistryItem SystemServiceTasksNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("SystemServiceTasksNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SystemServiceTasksNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("5970d722-705c-4309-b976-fc6d37eeec4f", "System Service Tasks Notification Group"),
						ResString.GetMultilingualString("7e2f1de2-ed0d-4eaf-bc67-f5e6aa0cbf57", "The staff group that will be notified about the issues with System Service Tasks."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						Core.Constants.Groups.PostMastersGroupPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Transactions Pending Allocation XML Posting Failure Notification Group

		public GuidRegistryItem TransactionsPendingAllocationXMLPostingFailureNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("TransactionsPendingAllocationXMLPostingFailureNotificationGroup", () =>
				{
					var result = new GuidRegistryItem(
						"TransactionsPendingAllocationXMLPostingFailureNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("FECA89F8-743C-4637-ADC3-FE8A58802939", "Transactions Pending Allocation XML Posting Failure Notification Group"),
						ResString.GetMultilingualString("5DD02106-C60D-4C09-A2DF-E6E826F676A6", "The staff group that will be notified about imported AP transactions that have failed to post due to errors found in the imported XML whilst running the ATP trigger event."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						Core.Constants.Groups.AllPK
					);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region XMS failure fallback notification group

		public GuidRegistryItem XMSFailureFallBackNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("XMLFailureFallbackNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"XMLFailureFallbackNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("8D7DBA51-2B8E-44E7-8A9D-29B89BF90B40", "XML Failure Fallback Notification Group"),
						ResString.GetMultilingualString("B4592C56-61C5-4AFD-B8E0-5D0F48D64FA6", "If there is a failure in an XMS import and the import message has no receiving group, a fallback message will be sent to this group."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						Core.Constants.Groups.AllPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion
	}
}
