using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.AR.Manifest.Business
{
	public sealed class ARCustomsDataRegistry : RegistryItemSet, Integration.Customs.AR.IARCustomsDataRegistry
	{
		#region Construction

		public static ARCustomsDataRegistry Instance
		{
			get { return instance ?? (instance = new ARCustomsDataRegistry()); }
		}
		[ThreadStatic]
		static ARCustomsDataRegistry instance;

		public ARCustomsDataRegistry()
		{
		}

		#endregion

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_Argentina { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("915BBC9D-4E28-4804-BFDC-F7769D852B50", "Argentina")); } }
			public static MultilingualString Customs_Argentina_Manifest { get { return CombineCategories(Customs_Argentina, ResString.GetMultilingualString("07FD6F0F-7643-49DB-A5F9-A8B89B1EABCE", "Manifest")); } }
		}

		#endregion

		public override bool IsForProductivityWise => false;

		public BooleanRegistryItem EnableARManifests
		{
			get
			{
				return GetItem("EnableARManifests", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableARManifests",
						Categories.Customs_Argentina,
						ResString.GetMultilingualString("BF552389-C6D7-4009-BFF3-50032DE2B649", "Enable Argentina Manifest"),
						ResString.GetMultilingualString("A6C0FBC1-A058-43B3-B01C-57E7A9B6DAEF", "Enable Argentina Manifest?"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						false);
					return result;
				});
			}
		}

		IRegistryItem Integration.Customs.AR.IARCustomsDataRegistry.EnableARManifests => EnableARManifests;

		public BooleanRegistryItem ARManifestShowAIRFunctions =>
				GetItem("ARManifestShowAIRFunctions", () => new BooleanRegistryItem(
				"ARManifestShowAIRFunctions",
				Categories.Customs_Argentina,
				ResString.GetMultilingualString("EC529066-27A0-4613-B790-A26B19FDC34E", "Show Air Manifest Functions"),
				ResString.GetMultilingualString("C7812932-17A9-40B1-AA38-5C4F20D972CE", "Air Manifest Functions will work only if 'Yes'"),
				RegistryStorageFlags.System,
				EnableARManifests.Value ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden,
				false)
		);

		public ZBool IsARManifestShowAIRFunctions => Instance.ARManifestShowAIRFunctions.Value;

		public BooleanRegistryItem ARTestingSystem
		{
			get
			{
				return GetItem("IsARTesting", delegate
				{
					return new BooleanRegistryItem(
						"IsARTesting",
						Categories.Customs_Argentina,
						ResString.GetMultilingualString("86A70C56-8D8C-4BF3-82E2-F3B32987F5B6", "Is Test Mode?"),
						ResString.GetMultilingualString("46626DEF-1605-48E3-BDD7-E6A39F99C11A", "Should AR messages be sent to Test System rather than Production System?"),
						RegistryStorageFlags.Company,
						EnableARManifests.Value ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden,
						false);
				});
			}
		}

		public ManifestGroupNotificationRegistryItem ARMANGroupNotification
		{
			get
			{
				return GetItem("ARMANGroupNotification", delegate
				{
					var result = new ManifestGroupNotificationRegistryItem(
						"ARMANGroupNotification",
						Categories.Customs_Argentina_Manifest,
						ResString.GetMultilingualString("D427EBD2-93BD-492F-949C-D9F7E1F71348", "Notification Group"),
						ResString.GetMultilingualString("F6EFDAB5-C4D9-4906-83E1-DF1D96D1462E", "Group to receive Manifest Messages sent by Customs. if 'Send Error Only' ticked, Only when an error occurs then send the notification email."),
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						ManifestGroupNotification.Default);

					result.CountryFilterPKs = CountryFilterPKs.Argentina;
					return result;
				});
			}
		}
	}
}
