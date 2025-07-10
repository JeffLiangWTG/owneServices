using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.Rohlig
{
	sealed class RohDataRegistry : RegistryItemSet
	{
		RohDataRegistry()
		{
		}

		#region Instance

		public static RohDataRegistry Instance
		{
			get { return instance ?? (instance = new RohDataRegistry()); }
		}
		[ThreadStatic]
		static RohDataRegistry instance;

		#endregion

		public override bool IsForProductivityWise => false;

		#region CoreFreight

		const string CoreFreightCategory = "CoreFreight Interface";

		#region CoreFreightEmailAddress

		public ZString CoreFreightEmailAddress
		{
			get
			{
				return CoreFreightEmailAddressRaw.Value;
			}
			set
			{
				CoreFreightEmailAddressRaw.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value.ToString());
			}
		}

		internal StringRegistryItem CoreFreightEmailAddressRaw
		{
			get
			{
				return GetItem("Email Recipient", delegate
				{
					return new StringRegistryItem(
						(NoResString)"Email Recipient",
						(NoResString)CoreFreightCategory,
						(NoResString)"Email Recipient",
						(NoResString)String.Empty,
						RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#endregion

		#region HouseBillSignedBy

		const string HouseBillsCatergory = Category + "/House Bills";

		public ZString HouseBillSignedBy
		{
			get
			{
				return HouseBillSignedByRaw.Value;
			}
			set
			{
				HouseBillSignedByRaw.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value.ToString());
			}
		}

		internal StringRegistryItem HouseBillSignedByRaw
		{
			get
			{
				return GetItem("House Bill Signed By", delegate
				{
					return new StringRegistryItem(
						(NoResString)"House Bill Signed By",
						(NoResString)HouseBillsCatergory,
						(NoResString)"House Bill Signed By",
						(NoResString)"This will be used as \"Signed By\" on House Bill of Ladings",
						RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#region Enable Client Specific Invoice Layout

		public bool UseRohligSpecificInvoiceLayout
		{
			get { return UseRohligSpecificInvoiceLayoutItem.Value; }
			set { UseRohligSpecificInvoiceLayoutItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value); }
		}

		internal BooleanRegistryItem UseRohligSpecificInvoiceLayoutItem
		{
			get
			{
				return GetItem("UseRohligSpecificInvoiceLayoutItem", delegate
				{
					return new BooleanRegistryItem(
						(NoResString)"UseRohligSpecificInvoiceLayoutItem",
						(NoResString)Category,
						(NoResString)"Use Rohlig-Specific Invoice Layout",
						(NoResString)ZString.Empty,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Implementation

		#region Constants

		const string Category = "Rohlig Client Extensions";

		#endregion

		#endregion
	}
}
