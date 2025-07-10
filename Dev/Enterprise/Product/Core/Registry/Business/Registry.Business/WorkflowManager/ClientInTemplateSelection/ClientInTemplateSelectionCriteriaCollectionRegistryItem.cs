using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class ClientInTemplateSelectionCriteriaCollectionRegistryItem : StronglyTypedRegistryItem<ClientInTemplateSelectionCriteriaCollection>
	{
		public ClientInTemplateSelectionCriteriaCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ClientInTemplateSelectionCriteriaCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ClientInTemplateSelectionCriteriaCollectionRegistryDataType(defaultValue), storage, defaultValue))
		{
		}

		#region Default Values

		public static ClientInTemplateSelectionCriteriaCollection DefaultCollectionValue
		{
			get
			{
				var result = new ClientInTemplateSelectionCriteriaCollection();

				foreach (CodeDescriptionPair pair in DefaultProcessTaskList)
				{
					Argument.NotNull(pair, nameof(pair));
					var criteria = new ClientInTemplateSelectionCriteria();
					criteria.ProcessTaskCode = pair.Code;

					foreach (CodeDescriptionBool orgTypeDefault in GetDefaultOrgTypesByCode(criteria.ProcessTaskCode))
					{
						Argument.NotNull(orgTypeDefault, nameof(orgTypeDefault));
						var orgType = new ClientInTemplateSelectionCriteriaOrgType(orgTypeDefault.Code);

						if (orgTypeDefault.Bool)
						{
							criteria.SelectedItems.Add(orgType);
						}
						else
						{
							criteria.AvailableItems.Add(orgType);
						}
					}

					result.Add(criteria);
				}

				return result;
			}
		}

		public static ReadOnlyCodeDescriptionPairList DefaultProcessTaskList
		{
			get
			{
				if (defaultProcessTaskList == null)
				{
					var list = new ClientInTemplateSelectionProcessTypeList();

					Interlocked.CompareExchange(ref defaultProcessTaskList, new ReadOnlyCodeDescriptionPairList(list), null); // Init var at the end to avoid a rare race condition.
				}

				return defaultProcessTaskList;
			}
		}
#if DEBUG
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
		public
#endif
		static ReadOnlyCodeDescriptionPairList defaultProcessTaskList;

		public static CodeDescriptionBoolCollection GetDefaultOrgTypesByCode(string processTaskCode)
		{
			switch (processTaskCode)
			{
				case ClientInTemplateSelectionProcessTypeList.Codes.Shipment:
					return CreateOrgTypeCollection(ShipmentOrgTypes);

				case ClientInTemplateSelectionProcessTypeList.Codes.QuotedBooking:
					return CreateOrgTypeCollection(QuotedBookingOrgTypes);

				case ClientInTemplateSelectionProcessTypeList.Codes.ForwardingOrder:
					return CreateOrgTypeCollection(ForwardingOrderOrgTypes);

				case ClientInTemplateSelectionProcessTypeList.Codes.LandTransportConsignment:
					return CreateOrgTypeCollection(LandTransportConsignmentOrgTypes);

				default:
					return new CodeDescriptionBoolCollection();
			}
		}

		static CodeDescriptionBoolCollection CreateOrgTypeCollection(IEnumerable<(string Code, bool ShouldBeSelected)> settingsForProcessType)
		{
			var allCodesAndDescriptions = new ClientInTemplateSelectionOrgTypeList();
			var result = new CodeDescriptionBoolCollection();

			foreach (var (code, shouldBeSelected) in settingsForProcessType)
			{
				var description = allCodesAndDescriptions.GetMultilingualDescriptionFromCode(code);
				result.Add(code, description, shouldBeSelected);
			}

			return result;
		}

		static IEnumerable<(string, bool)> ShipmentOrgTypes => new[]
		{
			(ClientInTemplateSelectionOrgTypeList.Codes.ConsigneeConsignor, true),
			(ClientInTemplateSelectionOrgTypeList.Codes.LocalClient, true),
			(ClientInTemplateSelectionOrgTypeList.Codes.ControllingCustomer, false),
		};

		static IEnumerable<(string, bool)> QuotedBookingOrgTypes => new[]
		{
			(ClientInTemplateSelectionOrgTypeList.Codes.Client, true),
			(ClientInTemplateSelectionOrgTypeList.Codes.ControllingCustomer, false),
		};

		static IEnumerable<(string, bool)> ForwardingOrderOrgTypes => new[]
		{
			(ClientInTemplateSelectionOrgTypeList.Codes.ControllingCustomer, true),
			(ClientInTemplateSelectionOrgTypeList.Codes.BuyerSupplier, true),
		};

		static IEnumerable<(string, bool)> LandTransportConsignmentOrgTypes => new[]
		{
			(ClientInTemplateSelectionOrgTypeList.Codes.BookingParty, true),
			(ClientInTemplateSelectionOrgTypeList.Codes.LocalClient, true),
			(ClientInTemplateSelectionOrgTypeList.Codes.PickupAddressOrganization, true),
			(ClientInTemplateSelectionOrgTypeList.Codes.DeliveryAddressOrganization, true),
		};

		#endregion
	}

	[RegistryEditor("Enterprise.Registry.GUI.ClientInTemplateSelectionRegistryItemEditor, Enterprise.Registry.GUI")]
	class ClientInTemplateSelectionCriteriaCollectionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ClientInTemplateSelectionCriteriaCollection>
	{
		public ClientInTemplateSelectionCriteriaCollectionRegistryDataType(ClientInTemplateSelectionCriteriaCollection defaultValue)
			: base(defaultValue)
		{
		}

		public override bool IsDefaultValueImmutable => true;

		protected override ClientInTemplateSelectionCriteriaCollection DeserialiseCore(byte[] value)
		{
			var result = base.DeserialiseCore(value);
			result.AddMissingProcessTypesFromDefaultsIfMissing();

			return result;
		}
	}
}
