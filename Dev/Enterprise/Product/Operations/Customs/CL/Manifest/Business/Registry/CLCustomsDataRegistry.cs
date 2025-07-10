using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public sealed class CLCustomsDataRegistry : RegistryItemSet, Integration.Customs.CL.ICLCustomsRegistry
	{
		#region Construction

		public static CLCustomsDataRegistry Instance
		{
			get { return instance ?? (instance = new CLCustomsDataRegistry()); }
		}

		[ThreadStatic]
		static CLCustomsDataRegistry instance;

		public CLCustomsDataRegistry()
		{
		}

		#endregion

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_Chile { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("94A82C0F-8246-41A8-A8C6-D80CCEB855ED", "Chile")); } }
			public static MultilingualString Customs_Chile_Manifest { get { return CombineCategories(Customs_Chile, ResString.GetMultilingualString("FB9297DD-A77B-4798-AA3E-37DD59F7F3F4", "Manifest")); } }
		}

		#endregion

		public override bool IsForProductivityWise => false;

		public BooleanRegistryItem EnableGlobalManifestForChile
		{
			get
			{
				return GetItem("EnableGlobalManifestForChile", delegate
				{
					return new BooleanRegistryItem(
						"EnableGlobalManifestForChile",
						Categories.Customs_Chile,
						ResString.GetMultilingualString("AA051D52-8C3C-4FCC-8FC1-53E3788830C0", "Enable Global Manifest for Chile?"),
						ResString.GetMultilingualString("81E4D192-64CF-4690-8F8A-3D88B5EB33C1", "Enable Global Manifest for Chile?"),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public CLSMSMessageSendingRegistryItem CLSMSMessageSending
		{
			get
			{
				return GetItem("CLSMSMessageSending",
					() => new CLSMSMessageSendingRegistryItem(
						name: "CLSMSMessageSending",
						category: Categories.Customs_Chile,
						caption: ResString.GetMultilingualString("C0E17872-CE08-4D25-9B29-31AE92A81959", "SMS Message Sending Configuration"),
						hint: ResString.GetMultilingualString("77100B18-FC72-47DE-B00A-F7FA04AF3A6D", "The settings are for CL SMS Client Application which is a standalone tool installed on the client’s local machine. The tool sends and receives messages through the CW1 Remote Printing Client Software."),
						storage: RegistryStorageFlags.Company,
						options: IsGlobalManifestForChileEnabled ? RegistryOptions.Default : RegistryOptions.IsHidden,
						defaultValue: Business.CLSMSMessageSending.GetDefault()
					)
					{
						CountryFilterPKs = CountryFilterPKs.Chile
					}
				);
			}
		}

		public ManifestGroupNotificationRegistryItem CLMANGroupNotification
		{
			get
			{
				return GetItem("CLMANGroupNotification", delegate
				{
					var result = new ManifestGroupNotificationRegistryItem(
						"CLMANGroupNotification",
						Categories.Customs_Chile_Manifest,
						ResString.GetMultilingualString("DC9771FA-411D-4630-BB80-2F2CEEED81AE", "Notification Group"),
						ResString.GetMultilingualString("6B923537-A4C6-42EE-8682-2BAC7BF6910D", "Group to receive Manifest Messages sent by Customs. if 'Send Error Only' ticked, Only when an error occurs then send the notification email."),
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						IsGlobalManifestForChileEnabled ? RegistryOptions.Default : RegistryOptions.IsHidden,
						ManifestGroupNotification.Default);

					result.CountryFilterPKs = CountryFilterPKs.Chile;
					return result;
				}
				);
			}
		}

		public ZBool IsGlobalManifestForChileEnabled => Instance.EnableGlobalManifestForChile.Value;

		public BooleanRegistryItem CLManifestShowAIRFunctions =>
				GetItem("CLManifestShowAIRFunctions", () => new BooleanRegistryItem(
				"CLManifestShowAIRFunctions",
				Categories.Customs_Chile,
				ResString.GetMultilingualString("ACD123A3-1D4F-4066-93DF-C54BC724FA33", "Show Air Manifest Functions"),
				ResString.GetMultilingualString("ED57ED79-B46E-4AA7-B9F3-BF42417B434B", "Air Manifest Functions will work only if 'Yes'"),
				RegistryStorageFlags.System,
				IsGlobalManifestForChileEnabled ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden,
				false)
		);

		public ZBool IsAirCLTestingSystem => Instance.CLManifestShowAIRFunctions.Value;

		public BooleanRegistryItem CLTestingSystem
		{
			get
			{
				return GetItem("IsCLTesting", delegate
				{
					return new BooleanRegistryItem(
						"IsCLTesting",
						Categories.Customs_Chile,
						ResString.GetMultilingualString("5506BF90-FCF7-43ED-B0BB-48458D8B5E8E", "Is Test Mode?"),
						ResString.GetMultilingualString("FB2DD249-2CF4-4A6C-A508-C2B0AAD523F6", "Should CL messages be sent to Test System rather than Production System?"),
						RegistryStorageFlags.Company,
						IsGlobalManifestForChileEnabled ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden,
						false);
				});
			}
		}

		#region ICLCustomsRegistry Members

		IRegistryItem Integration.Customs.CL.ICLCustomsRegistry.EnableGlobalManifest => EnableGlobalManifestForChile;

		#endregion
	}
}
