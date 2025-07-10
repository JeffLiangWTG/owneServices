using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.TEL
{
	public sealed class TELDataRegistry : RegistryItemSet
	{
		public static TELDataRegistry Instance
		{
			get
			{
				return instance ?? (instance = new TELDataRegistry());
			}
		}
		[ThreadStatic]
		static TELDataRegistry instance;

		public override bool IsForProductivityWise => false;

		#region Orders Import Settings

		#region Service Task

		public ZGuid ConsolShipImportNotificationGroupPK
		{
			get
			{
				return ConsolShipImportNotificationGroupPKItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			}
		}

		internal GuidRegistryItem ConsolShipImportNotificationGroupPKItem
		{
			get
			{
				return GetItem("ConsolShipImportNotificationGroupPKItem", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ConsolShipImportNotificationGroupPKItem",
						(NoResString)consolShipmentImportCategory,
						(NoResString)"Notification Group",
						(NoResString)"A nominated user group to get advised of data import notifications.",
						RegistryStorageFlags.System,
						RegistryOptions.NotCached);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public ZString ConsolShipManifestEmailSubjectIdentifier
		{
			get
			{
				return ConsolShipManifestEmailSubjectIdentifierItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			}
		}

		internal StringRegistryItem ConsolShipManifestEmailSubjectIdentifierItem
		{
			get
			{
				return GetItem("ConsolShipManifestEmailSubjectIdentifierItem", delegate
				{
					return new StringRegistryItem(
						"ConsolShipManifestEmailSubjectIdentifierItem",
						(NoResString)consolShipmentImportCategory,
						(NoResString)"Email Subject Identifier",
						(NoResString)"Enter the Email Subject that will identify the email containing the Consol/Shipment Manifest.",
						RegistryStorageFlags.System,
						RegistryOptions.NotCached);
				});
			}
		}

		#endregion

		#endregion

		#region Implementation
		const string category = "TEL Client Extensions";
		const string consolShipmentImportCategory = category + @"/Import of W.T. Sea Air Manifest xml-Files";
		#endregion
	}
}


