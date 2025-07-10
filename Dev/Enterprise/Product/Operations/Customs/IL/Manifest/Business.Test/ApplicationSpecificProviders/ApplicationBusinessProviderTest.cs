using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Business;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(ApplicationBusinessProvider))]
	sealed class ApplicationBusinessProviderTest : ASYCUDA.Business.Testing.ApplicationBusinessProviderAbstractTest<ApplicationBusinessProvider, AsycudaManifestHeader>
	{
		public override void TestAsycudaManifestHeaderType()
		{
			var header = CreateNewManifest();
			AssertEquals(typeof(AsycudaManifestHeader), header.ApplicationBusinessProvider.AsycudaManifestHeaderType);
		}

		public void TestManifestTypes_ImportManifest_WhenAll()
		{
			using (ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL"))
			{
				var header = CreateNewManifestOnly();
				var provider = header.ApplicationBusinessProvider;
				AssertContainsExactElementsInAnyOrder(new ManifestType.EqualityComparer(), new[] { new ILManifestTypes().AllManifest }, provider.AllManifestTypes);
				AssertContainsExactElementsInAnyOrder(new ManifestType.EqualityComparer(), new[] { new ILManifestTypes().AllManifest }, provider.ManifestTypes);
				AssertEquals(true, provider.ManifestTypes.Any(x => x.Code == "785"));
				AssertEquals(true, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.Sea, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(true, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.Road, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.Air, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.Rail, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.Mail, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.OwnPropulsion, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.FixedTransportInstallations, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.InlandWaterwayTransport, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
			}
		}

		public void TestManifestTypes_ImportManifest_WhenImport()
		{
			using (ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "IMPORT"))
			{
				var header = CreateNewManifestOnly();
				var provider = header.ApplicationBusinessProvider;
				AssertContainsExactElementsInAnyOrder(new ManifestType.EqualityComparer(), new[] { new ILManifestTypes().ImportManifest }, provider.AllManifestTypes);
				AssertContainsExactElementsInAnyOrder(new ManifestType.EqualityComparer(), new[] { new ILManifestTypes().ImportManifest }, provider.ManifestTypes);
				AssertEquals(true, provider.ManifestTypes.Any(x => x.Code == "785"));
				AssertEquals(true, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.Sea, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(true, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.Road, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.Air, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.Rail, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.Mail, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.OwnPropulsion, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.FixedTransportInstallations, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.InlandWaterwayTransport, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
			}
		}

		public void TestManifestTypes_ImportManifest_WhenExport()
		{
			using (ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "EXPORT"))
			{
				var header = CreateNewManifestOnly();
				var provider = header.ApplicationBusinessProvider;
				AssertContainsExactElementsInAnyOrder(new ManifestType.EqualityComparer(), new[] { new ILManifestTypes().ExportManifest }, provider.AllManifestTypes);
				AssertContainsExactElementsInAnyOrder(new ManifestType.EqualityComparer(), new[] { new ILManifestTypes().ExportManifest }, provider.ManifestTypes);
				AssertEquals(true, provider.ManifestTypes.Any(x => x.Code == "785"));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.Sea, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(true, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.Road, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.Air, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.Rail, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.Mail, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.OwnPropulsion, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.FixedTransportInstallations, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.InlandWaterwayTransport, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
			}
		}

		public void TestManifestTypes_ImportManifest_WhenNone()
		{
			using (ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "NONE"))
			{
				var header = CreateNewManifestOnly();
				var provider = header.ApplicationBusinessProvider;
				AssertContainsExactElementsInAnyOrder(new ManifestType.EqualityComparer(), new[] { new ILManifestTypes().AllManifest }, provider.AllManifestTypes);
				AssertContainsExactElementsInAnyOrder(new ManifestType.EqualityComparer(), Array.Empty<IManifestType>(), provider.ManifestTypes);
				AssertEquals(false, provider.ManifestTypes.Any(x => x.Code == "785"));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.Sea, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.Road, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.Air, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.Rail, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.Mail, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.OwnPropulsion, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.FixedTransportInstallations, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, TransportTypeList.Codes.InlandWaterwayTransport, ApplicationCodeTypeList.Codes.Consolidator).Any(code => code == Core.Constants.CountryCodes.Israel));
			}
		}

		public void TestDescription()
		{
			var header = CreateNewManifest();
			var provider = header.ApplicationBusinessProvider;
			var manifestDescriptions = provider.GetManifestDescriptions(null, null, null).ToList();
			AssertEquals("Count", 1, manifestDescriptions.Count);
			AssertEquals("Israel", manifestDescriptions[0].Description);
		}

		#region AcceptableTransportModes

		public void TestAcceptableTransportModes_WhenAll()
		{
			using (ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL"))
			{
				var header = CreateNewManifestOnly();
				var provider = header.ApplicationBusinessProvider;
				RunTestAcceptableTransportModesScenario("ALL", provider, Directions.Unknown, new[] { Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Road });
				RunTestAcceptableTransportModesScenario("ALL", provider, Directions.Import, new[] { Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Road });
				RunTestAcceptableTransportModesScenario("ALL", provider, Directions.Export, new[] { Core.Constants.TransportModes.Road });
				RunTestAcceptableTransportModesScenario("ALL", provider, Directions.CrossTrade, Array.Empty<string>());
				RunTestAcceptableTransportModesScenario("ALL", provider, Directions.Domestic, Array.Empty<string>());
			}
		}

		public void TestAcceptableTransportModes_WhenImport()
		{
			using (ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "IMPORT"))
			{
				var header = CreateNewManifestOnly();
				var provider = header.ApplicationBusinessProvider;
				RunTestAcceptableTransportModesScenario("IMPORT", provider, Directions.Unknown, new[] { Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Road });
				RunTestAcceptableTransportModesScenario("IMPORT", provider, Directions.Import, new[] { Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Road });
				RunTestAcceptableTransportModesScenario("IMPORT", provider, Directions.Export, Array.Empty<string>());
				RunTestAcceptableTransportModesScenario("IMPORT", provider, Directions.CrossTrade, Array.Empty<string>());
				RunTestAcceptableTransportModesScenario("IMPORT", provider, Directions.Domestic, Array.Empty<string>());
			}
		}

		public void TestAcceptableTransportModes_WhenExport()
		{
			using (ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "EXPORT"))
			{
				var header = CreateNewManifestOnly();
				var provider = header.ApplicationBusinessProvider;
				RunTestAcceptableTransportModesScenario("EXPORT", provider, Directions.Unknown, new[] { Core.Constants.TransportModes.Road });
				RunTestAcceptableTransportModesScenario("EXPORT", provider, Directions.Import, Array.Empty<string>());
				RunTestAcceptableTransportModesScenario("EXPORT", provider, Directions.Export, new[] { Core.Constants.TransportModes.Road });
				RunTestAcceptableTransportModesScenario("EXPORT", provider, Directions.CrossTrade, Array.Empty<string>());
				RunTestAcceptableTransportModesScenario("EXPORT", provider, Directions.Domestic, Array.Empty<string>());
			}
		}

		public void TestAcceptableTransportModes_WhenNone()
		{
			using (ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "NONE"))
			{
				var header = CreateNewManifestOnly();
				var provider = header.ApplicationBusinessProvider;
				RunTestAcceptableTransportModesScenario("NONE", provider, Directions.Unknown, Array.Empty<string>());
				RunTestAcceptableTransportModesScenario("NONE", provider, Directions.Import, Array.Empty<string>());
				RunTestAcceptableTransportModesScenario("NONE", provider, Directions.Export, Array.Empty<string>());
				RunTestAcceptableTransportModesScenario("NONE", provider, Directions.CrossTrade, Array.Empty<string>());
				RunTestAcceptableTransportModesScenario("NONE", provider, Directions.Domestic, Array.Empty<string>());
			}
		}

		void RunTestAcceptableTransportModesScenario(string registryValue, ASYCUDA.Business.ApplicationBusinessProvider provider, Directions direction, string[] expectedTransportModes)
		{
			var transportModes = provider.GetAcceptableTransportModesFromAsycudaManifestHeader(Factory, Core.Constants.CountryCodes.Israel, ApplicationCodeTypeList.Codes.Consolidator, direction);
			if (expectedTransportModes.Length > 0)
			{
				AssertContainsExactElementsInAnyOrder($"Transport modes should meet expectation when registry set to {registryValue} and direction is {direction}", expectedTransportModes, transportModes);
			}
			else
			{
				AssertEquals($"No transport modes are expected when registry set to {registryValue} and direction is {direction}", 0, transportModes.Count());
			}
		}

		#endregion AcceptableTransportModes

		#region Test ApplicableCountryCodes

		public void TestApplicableCountryCodes_WhenAll()
		{
			using (ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL"))
			{
				var header = CreateNewManifestOnly();
				var provider = header.ApplicationBusinessProvider;

				RunTestApplicableCountryCodesScenario("ALL", provider, Directions.Unknown, new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Road, "" }, new[] { Core.Constants.CountryCodes.Israel });
				RunTestApplicableCountryCodesScenario("ALL", provider, Directions.Unknown, new[] { TransportTypeList.Codes.Rail, TransportTypeList.Codes.Air, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Mail, TransportTypeList.Codes.OwnPropulsion }, Array.Empty<string>());
				RunTestApplicableCountryCodesScenario("ALL", provider, Directions.Import, new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Road, "" }, new[] { Core.Constants.CountryCodes.Israel });
				RunTestApplicableCountryCodesScenario("ALL", provider, Directions.Import, new[] { TransportTypeList.Codes.Rail, TransportTypeList.Codes.Air, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Mail, TransportTypeList.Codes.OwnPropulsion }, Array.Empty<string>());
				RunTestApplicableCountryCodesScenario("ALL", provider, Directions.Export, new[] { TransportTypeList.Codes.Road, "" }, new[] { Core.Constants.CountryCodes.Israel });
				RunTestApplicableCountryCodesScenario("ALL", provider, Directions.Export, new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Air, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Mail, TransportTypeList.Codes.OwnPropulsion }, Array.Empty<string>());
				RunTestApplicableCountryCodesScenario("ALL", provider, Directions.CrossTrade, new[] { TransportTypeList.Codes.Road, TransportTypeList.Codes.Sea, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Air, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Mail, TransportTypeList.Codes.OwnPropulsion, "" }, Array.Empty<string>());
				RunTestApplicableCountryCodesScenario("ALL", provider, Directions.Domestic, new[] { TransportTypeList.Codes.Road, TransportTypeList.Codes.Sea, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Air, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Mail, TransportTypeList.Codes.OwnPropulsion, "" }, Array.Empty<string>());
			}
		}

		public void TestApplicableCountryCodes_WhenImport()
		{
			using (ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "IMPORT"))
			{
				var header = CreateNewManifestOnly();
				var provider = header.ApplicationBusinessProvider;
				RunTestApplicableCountryCodesScenario("IMPORT", provider, Directions.Unknown, new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Road, "" }, new[] { Core.Constants.CountryCodes.Israel });
				RunTestApplicableCountryCodesScenario("IMPORT", provider, Directions.Unknown, new[] { TransportTypeList.Codes.Rail, TransportTypeList.Codes.Air, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Mail, TransportTypeList.Codes.OwnPropulsion }, Array.Empty<string>());
				RunTestApplicableCountryCodesScenario("IMPORT", provider, Directions.Import, new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Road, "" }, new[] { Core.Constants.CountryCodes.Israel });
				RunTestApplicableCountryCodesScenario("IMPORT", provider, Directions.Import, new[] { TransportTypeList.Codes.Rail, TransportTypeList.Codes.Air, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Mail, TransportTypeList.Codes.OwnPropulsion }, Array.Empty<string>());
				RunTestApplicableCountryCodesScenario("IMPORT", provider, Directions.Export, new[] { TransportTypeList.Codes.Road, TransportTypeList.Codes.Sea, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Air, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Mail, TransportTypeList.Codes.OwnPropulsion, "" }, Array.Empty<string>());
				RunTestApplicableCountryCodesScenario("IMPORT", provider, Directions.CrossTrade, new[] { TransportTypeList.Codes.Road, TransportTypeList.Codes.Sea, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Air, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Mail, TransportTypeList.Codes.OwnPropulsion, "" }, Array.Empty<string>());
				RunTestApplicableCountryCodesScenario("IMPORT", provider, Directions.Domestic, new[] { TransportTypeList.Codes.Road, TransportTypeList.Codes.Sea, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Air, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Mail, TransportTypeList.Codes.OwnPropulsion, "" }, Array.Empty<string>());
			}
		}

		public void TestApplicableCountryCodes_WhenExport()
		{
			using (ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "EXPORT"))
			{
				var header = CreateNewManifestOnly();
				var provider = header.ApplicationBusinessProvider;
				RunTestApplicableCountryCodesScenario("EXPORT", provider, Directions.Unknown, new[] { TransportTypeList.Codes.Road, "" }, new[] { Core.Constants.CountryCodes.Israel });
				RunTestApplicableCountryCodesScenario("EXPORT", provider, Directions.Unknown, new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Air, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Mail, TransportTypeList.Codes.OwnPropulsion }, Array.Empty<string>());
				RunTestApplicableCountryCodesScenario("EXPORT", provider, Directions.Import, new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Road, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Air, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Mail, TransportTypeList.Codes.OwnPropulsion, "" }, Array.Empty<string>());
				RunTestApplicableCountryCodesScenario("EXPORT", provider, Directions.Export, new[] { TransportTypeList.Codes.Road, "" }, new[] { Core.Constants.CountryCodes.Israel });
				RunTestApplicableCountryCodesScenario("EXPORT", provider, Directions.Export, new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Air, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Mail, TransportTypeList.Codes.OwnPropulsion }, Array.Empty<string>());
				RunTestApplicableCountryCodesScenario("EXPORT", provider, Directions.CrossTrade, new[] { TransportTypeList.Codes.Road, TransportTypeList.Codes.Sea, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Air, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Mail, TransportTypeList.Codes.OwnPropulsion, "" }, Array.Empty<string>());
				RunTestApplicableCountryCodesScenario("EXPORT", provider, Directions.Domestic, new[] { TransportTypeList.Codes.Road, TransportTypeList.Codes.Sea, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Air, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Mail, TransportTypeList.Codes.OwnPropulsion, "" }, Array.Empty<string>());
			}
		}

		public void TestApplicableCountryCodes_WhenNone()
		{
			using (ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "NONE"))
			{
				var header = CreateNewManifestOnly();
				var provider = header.ApplicationBusinessProvider;
				RunTestApplicableCountryCodesScenario("NONE", provider, Directions.Unknown, new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Road, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Air, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Mail, TransportTypeList.Codes.OwnPropulsion, "" }, Array.Empty<string>());
				RunTestApplicableCountryCodesScenario("NONE", provider, Directions.Import, new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Road, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Air, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Mail, TransportTypeList.Codes.OwnPropulsion, "" }, Array.Empty<string>());
				RunTestApplicableCountryCodesScenario("NONE", provider, Directions.Export, new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Road, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Air, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Mail, TransportTypeList.Codes.OwnPropulsion, "" }, Array.Empty<string>());
				RunTestApplicableCountryCodesScenario("NONE", provider, Directions.CrossTrade, new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Road, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Air, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Mail, TransportTypeList.Codes.OwnPropulsion, "" }, Array.Empty<string>());
				RunTestApplicableCountryCodesScenario("NONE", provider, Directions.Domestic, new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Road, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Air, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Mail, TransportTypeList.Codes.OwnPropulsion, "" }, Array.Empty<string>());
			}
		}

		void RunTestApplicableCountryCodesScenario(
			string registryValue,
			ASYCUDA.Business.ApplicationBusinessProvider provider,
			Directions direction,
			string[] transportModes,
			string[] expectedCountryCodes)
		{
			foreach (var transportMode in transportModes)
			{
				var countryCodes = provider.ApplicableCountryCodes(direction, transportMode);
				if (expectedCountryCodes.Length > 0)
				{
					AssertContainsExactElementsInAnyOrder($"Applicable Country Codes should meet expectation when registry set to {registryValue}, direction is {direction}, and transport mode is {transportMode}", expectedCountryCodes, countryCodes);
				}
				else
				{
					AssertEquals($"No Applicable Country Codes are expected when registry set to {registryValue}, direction is {direction} and transport mode is {transportMode}", 0, countryCodes.Count);
				}
			}
		}

		#endregion Test ApplicableCountryCodes

		public void TestPackedItemTariffDataGrouping()
		{
			var header = CreateNewManifest();
			var provider = header.ApplicationBusinessProvider;
			var packedItemTariffDataGrouping = provider.PackedItemTariffDataGrouping;
			var expectedDataGrouping = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Israel) ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AssertEquals(expectedDataGrouping, packedItemTariffDataGrouping);
		}

		public void TestPackedItemTariffType()
		{
			var header = CreateNewManifest();
			var provider = header.ApplicationBusinessProvider;
			var packedItemTariffType = provider.PackedItemTariffType;
			AssertEquals(Universal.Constants.TariffTypes.Import, packedItemTariffType);
		}

		protected override IEnumerable<IManifestType> ExpectedManifestTypes => new[] { new ILManifestTypes().AllManifest };

		protected override Type ExpectedMessagingProviderType => typeof(MessagingProvider);

		protected override Type ExpectedFeatureProviderType => typeof(FeatureProvider);

		protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(AsycudaForCustomsDeclarationDataObjectWriter<ASYCUDA.Business.AsycudaBill, AsycudaPack, AsycudaPackedItem>);

		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => typeof(AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>);

		protected override AsycudaManifestHeader CreateNewManifest()
		{
			disposableAction = ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL");
			return CreateNewManifestOnly();
		}
		protected override void TearDown()
		{
			base.TearDown();
			disposableAction?.Dispose();
		}

		AsycudaManifestHeader CreateNewManifestOnly()
		{
			var result = base.CreateNewManifest();
			result.AMA_RN_NKCountry = "IL";
			result.AMA_ApplicationCode = "NVC";
			result.AMA_ManifestType = "785";
			return result;
		}
		IDisposable disposableAction;
	}
}
