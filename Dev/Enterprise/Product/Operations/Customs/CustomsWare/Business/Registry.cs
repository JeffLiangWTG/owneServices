using System;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using ICustomsWareRegistry = Enterprise.Integration.Customs.CustomsWare.ICustomsWareRegistry;

namespace Enterprise.Customs.CustomsWare.Business
{
	public sealed class CustomsWareRegistry : RegistryItemSet, ICustomsWareRegistry
	{
		#region Construction

		public static CustomsWareRegistry Instance
		{
			get { return instance ?? (instance = new CustomsWareRegistry()); }
		}

		[ThreadStatic]
		static CustomsWareRegistry instance;

		CustomsWareRegistry()
		{
		}

		#endregion

		public override bool IsForProductivityWise => false;

		#region Notifications

		public LocalCountryCustomsInterfaceMutualExclusiveBooleanRegistryItem SendNotifications
		{
			get
			{
				return GetItem("CustomsWareSendNotifications", delegate
				{
					return new LocalCountryCustomsInterfaceMutualExclusiveBooleanRegistryItem(
						"CustomsWareSendNotifications",
						CustomsDataRegistry.Categories.Customs_Integration_CustomsWare,
						ResString.GetMultilingualString("eaceb07a-9a62-47c2-8a98-50b7e22526a2", "Send Response Notifications"),
						ResString.GetMultilingualString("eaceb08a-9a61-47c2-8a98-50b7e22526a2", "If ticked, A notification email will be sent for each successfully processed response."),
						RegistryStorageFlags.Company,
						true);
				});
			}
		}

		#endregion

		#region WebService

		public LocalCountryCustomsInterfaceMutualExclusiveStringRegistryItem CustomsWareSiteID
		{
			get
			{
				return GetItem("CustomsWareSiteID", delegate
				{
					var result = new LocalCountryCustomsInterfaceMutualExclusiveStringRegistryItem(
						"CustomsWareSiteID",
						CustomsDataRegistry.Categories.Customs_Integration_CustomsWare,
						ResString.GetMultilingualString("edbd8304-fcee-4501-90c2-a16f89fef390", "Site ID"),
						ResString.GetMultilingualString("edbd8304-fcee-4501-90c2-a16f89fef390", "Site ID"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter);

					return result;
				});
			}
		}

		IRegistryItem ICustomsWareRegistry.CustomsWareSiteID
		{
			get { return CustomsWareSiteID; }
		}

		public StringRegistryItem URI
		{
			get
			{
				return GetItem("CustomsWareURI", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"CustomsWareURI",
						CustomsDataRegistry.Categories.Customs_Integration_CustomsWare_WebService,
						ResString.GetMultilingualString("eaceb08a-9a62-47c2-8a98-50b7e32526a2", "URI"),
						ResString.GetMultilingualString("eaceb08a-9a62-47c2-8a98-50b7e42526a2", "URI of web service"),
						RegistryStorageFlags.System,
						"http://83.141.75.90:1120/CustomsForceWebService.asmx");

					return result;
				});
			}
		}

		public LocalCountryCustomsInterfaceMutualExclusiveStringRegistryItem UserName
		{
			get
			{
				return GetItem("CustomsWareUserName", delegate
				{
					var result = new LocalCountryCustomsInterfaceMutualExclusiveStringRegistryItem(
						"CustomsWareUserName",
						CustomsDataRegistry.Categories.Customs_Integration_CustomsWare_WebService,
						ResString.GetMultilingualString("eaceb08a-9b62-47c2-8a98-50c7e22526a2", "User Name"),
						ResString.GetMultilingualString("eaceb08a-9b62-47c2-8a98-50c7e22526a2", "User Name"),
						RegistryStorageFlags.Company);

					return result;
				});
			}
		}

		IRegistryItem ICustomsWareRegistry.UserName
		{
			get { return UserName; }
		}

		public LocalCountryCustomsInterfaceMutualExclusiveStringRegistryItem Password
		{
			get
			{
				return GetItem("CustomsWarePassword", delegate
				{
					var result = new LocalCountryCustomsInterfaceMutualExclusiveStringRegistryItem(
						"CustomsWarePassword",
						CustomsDataRegistry.Categories.Customs_Integration_CustomsWare_WebService,
						ResString.GetMultilingualString("ebceb08a-9a62-47c2-8a98-50b7e22526a2", "Password"),
						ResString.GetMultilingualString("ebceb08a-9a62-47c2-8a98-50b7e22526a2", "Password"),
						RegistryStorageFlags.Company);

					return result;
				});
			}
		}

		IRegistryItem ICustomsWareRegistry.Password
		{
			get { return Password; }
		}

		public StringRegistryItem ApplicationID
		{
			get
			{
				return GetItem("CustomsWareApplicationID", delegate
				{
					var result = new StringRegistryItem(
						"CustomsWareApplicationID",
						CustomsDataRegistry.Categories.Customs_Integration_CustomsWare_WebService,
						ResString.GetMultilingualString("eaceb08a-9a62-46c2-8a98-50b7e22526a2", "Application ID"),
						ResString.GetMultilingualString("eaceb08a-9a62-46c2-8a98-50b7e22526a2", "Application ID"),
						RegistryStorageFlags.System,
						"CWAPIEXTERNAL");

					return result;
				});
			}
		}

		public BooleanRegistryItem SubmitOutOfLine => GetItem(nameof(SubmitOutOfLine), () => new BooleanRegistryItem(
				nameof(SubmitOutOfLine),
				CustomsDataRegistry.Categories.Customs_Integration_CustomsWare_WebService,
				ResString.GetMultilingualString("6ee0b454-547f-4813-9816-292803aa31e2", "Submit Declaration out of line"),
				ResString.GetMultilingualString("e1508c0b-d791-44f1-b4f0-83e0536c7cec", "Submit Declaration out of line"),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true));

		bool ICustomsWareRegistry.SubmitOutOfLine => SubmitOutOfLine.Value;

		#endregion
	}
}
