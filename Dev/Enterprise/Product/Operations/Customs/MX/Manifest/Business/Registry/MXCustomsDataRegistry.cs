using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Manifest.Business
{
	public sealed class MXCustomsDataRegistry : RegistryItemSet, Integration.Customs.MX.IMXCustomsDataRegistry
	{
		#region Construction

		public static MXCustomsDataRegistry Instance
		{
			get { return instance ?? (instance = new MXCustomsDataRegistry()); }
		}
		[ThreadStatic]
		static MXCustomsDataRegistry instance;

		public MXCustomsDataRegistry()
		{
		}

		#endregion

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_Mexico { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("CEE8E25C-CC4B-4DAC-9DDE-5B5DC722CC7E", "Mexico")); } }
			public static MultilingualString Customs_Mexico_Manifest { get { return CombineCategories(Customs_Mexico, ResString.GetMultilingualString("15DC48F8-5635-4075-9D57-ECA3AA60E46F", "Manifest")); } }
		}

		#endregion

		public override bool IsForProductivityWise => false;

		public BooleanRegistryItem MXTestingSystem
		{
			get
			{
				return GetItem("IsMXTesting", delegate
				{
					return new BooleanRegistryItem(
						"IsMXTesting",
						Categories.Customs_Mexico,
						ResString.GetMultilingualString("AB959820-0896-4164-9616-34760020F427", "Is Test Mode?"),
						ResString.GetMultilingualString("4FF42CBC-3F46-491C-A923-25873A9AB49F", "Should MX messages be sent to Test System rather than Production System?"),
						RegistryStorageFlags.Company,
						EnableMXManifests.Value ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden,
						IsTestLicence);
				});
			}
		}

		public ManifestGroupNotificationRegistryItem MXMANGroupNotification
		{
			get
			{
				return GetItem("MXMANGroupNotification", delegate
				{
					var result = new ManifestGroupNotificationRegistryItem(
						"MXMANGroupNotification",
						Categories.Customs_Mexico_Manifest,
						ResString.GetMultilingualString("38D691FC-ABDD-43E0-820E-589FFD5205E9", "Notification Group"),
						ResString.GetMultilingualString("BA41A25E-AD75-4DBD-8625-2552B965ED0D", "Group to receive Manifest Messages sent by Customs. if 'Send Error Only' ticked, Only when an error occurs then send the notification email."),
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						EnableMXManifests.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						ManifestGroupNotification.Default);

					result.CountryFilterPKs = CountryFilterPKs.Mexico;
					return result;
				}
				);
			}
		}

		public ZBool IsMXTestingSystem => Instance.MXTestingSystem.Value;

		public BooleanRegistryItem MXManifestShowAIRFunctions =>
				GetItem("MXManifestShowAIRFunctions", () => new BooleanRegistryItem(
				"MXManifestShowAIRFunctions",
				Categories.Customs_Mexico,
				ResString.GetMultilingualString("F96101E8-3BBE-435C-B7DF-C256C198E0C9", "Show Air Manifest Functions"),
				ResString.GetMultilingualString("07E09EAB-7FDD-46B2-B0D9-948AA75C9C9B", "Air Manifest Functions will work only if 'Yes'"),
				RegistryStorageFlags.System,
				EnableMXManifests.Value ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden,
				false)
		);

		public MXWsVucemRegistryItem WSVucem
		{
			get
			{
				return GetItem("WSVucem",
					() => new MXWsVucemRegistryItem(
						"WSVucem",
						Categories.Customs_Mexico,
						ResString.GetMultilingualString("38FBEB9B-BC3C-4D07-B6E3-E0C8A705D1A9", "Services for the VUCEM"),
						ResString.GetMultilingualString("1FF2364F-2D5B-45AB-9D7C-E0BC85B5B0D9", "URLs for the reception of responses from the VUCEM"),
						RegistryStorageFlags.Company,
						EnableMXManifests.Value ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden,
						new MXWsVucem() { }
					)
					{
						CountryFilterPKs = CountryFilterPKs.Mexico
					}
				);
			}
		}

		public BooleanRegistryItem EnableMXManifests
		{
			get
			{
				return GetItem("EnableMXManifests", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableMXManifests",
						Categories.Customs_Mexico,
						ResString.GetMultilingualString("8C975AD0-1104-400E-8307-C09230A9A4A7", "Enable Mexico Manifest"),
						ResString.GetMultilingualString("F43EBC27-A819-469D-A1CB-808E41259F89", "Enable Mexico Manifest?"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						false);
					return result;
				});
			}
		}

		IRegistryItem Integration.Customs.MX.IMXCustomsDataRegistry.EnableMXManifests => EnableMXManifests;

		public ZBool IsAirMXTestingSystem => Instance.MXManifestShowAIRFunctions.Value;

		IRegistryItem Integration.Customs.MX.IMXCustomsDataRegistry.MXTestingSystem => MXTestingSystem;

		public bool IsTestLicence => (RegKey?.DatabaseType ?? string.Empty) != DatabaseTypes.Codes.Production;

		IProductRegistrationKey RegKey => ObjectFactory.Get<IProductRegistration>()?.Key;
	}
}

