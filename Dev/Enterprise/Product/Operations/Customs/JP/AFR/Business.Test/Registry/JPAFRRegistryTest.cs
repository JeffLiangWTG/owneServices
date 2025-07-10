using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(JPAFRRegistry))]
	sealed class JPAFRRegistryTest : RegistryItemSetTestCaseWithFactory<JPAFRRegistry>
	{
		public void TestSendMessageAcknowledgements()
		{
			TestRegistryItem(ItemSet.SendMessageAcknowledgements,
				"AFRSendMessageAcknowledgements",
				JPAFRRegistry.Categories.Customs_Japan_AFR,
				"Send AFR Message Acknowledgements",
				"Send AFR message acknowledgements to staff member, nominated group or combination of both",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				new CodeDescriptionPairList(OLookUpEditType.EmailTo),
				Enterprise.Core.Constants.EmailTo.StaffMember);
		}

		public void TestSendMessageAcknowledgementsToGroup()
		{
			TestRegistryItem(ItemSet.SendMessageAcknowledgementsToGroup,
				"AFRSendMessageAcknowledgementsToGroup",
				JPAFRRegistry.Categories.Customs_Japan_AFR,
				"Send AFR Message Acknowledgements To Group",
				"Send AFR message acknowledgements to selected group",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryFindBoxCollection.GlbGroup,
				RegistryConstants.GroupPKs.Notification);
		}

		public void TestSendMessageErrors()
		{
			TestRegistryItem(ItemSet.SendMessageErrors,
				"AFRSendMessageErrors",
				JPAFRRegistry.Categories.Customs_Japan_AFR,
				"Send AFR Message Errors",
				"Send AFR message errors to staff member, nominated group or combination of both",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				new CodeDescriptionPairList(OLookUpEditType.EmailTo),
				Enterprise.Core.Constants.EmailTo.StaffMember);
		}

		public void TestSendMessageErrorsToGroup()
		{
			TestRegistryItem(ItemSet.SendMessageErrorsToGroup,
				"AFRSendMessageErrorsToGroup",
				JPAFRRegistry.Categories.Customs_Japan_AFR,
				"Send AFR Message Errors To Group",
				"Send AFR message errors to selected group",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryFindBoxCollection.GlbGroup,
				RegistryConstants.GroupPKs.Notification);
		}

		public void TestAFRShowBLLFunctions()
		{
			TestGenericRegistryItem(ItemSet.AFRShowBLLFunctions,
				"AFRShowBLLFunctions",
				JPAFRRegistry.Categories.Customs_Japan,
				"Show BLL Functions",
				"BLL Functions will work only if 'Yes'",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true
			);
		}

		public void TestAFR2017EffectiveLiveDate()
		{
			TestGenericRegistryItem(ItemSet.AFR2017EffectiveLiveDate,
				"AFR2017EffectiveLiveDate",
				JPAFRRegistry.Categories.Customs_Japan,
				"AFR 2017 Effective Live Date",
				"The new function bulk Vessel Change message(CMV) will work after AFR 2017 Effective Live Date",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController,
				new DateTime(2017, 10, 7)
				);
		}

		public void TestIncludeAsmCldClbSubShipments()
		{
			TestGenericRegistryItem(ItemSet.IncludeAsmCldClbSubShipmentsInManifest,
				"IncludeAsmCldClbSubShipmentsInManifest",
				JPAFRRegistry.Categories.Customs_Japan_AFR,
				"Include ASM/CLD/CLB Sub Shipments in Manifest",
				"Include ASM/CLD/CLB Sub Shipments in Manifest",
				RegistryStorageFlags.System,
				false);
		}

		public void TestIncludeBCNSubShipmentsInManifest()
		{
			TestGenericRegistryItem(ItemSet.IncludeBCNSubShipmentsInManifest,
				"IncludeBCNSubShipmentsInManifest",
				JPAFRRegistry.Categories.Customs_Japan_AFR,
				"Include BCN Sub Shipments in Manifest",
				"Include BCN Sub Shipments in Manifest",
				RegistryStorageFlags.System,
				true);
		}

		public void TestAFRAddSCACtoBillsDuringSync()
		{
			CombineAssertions(() =>
			{
				TestRegistryItem(ItemSet.AFRAddSCACtoBillsDuringSync,
					"AFRAddSCACtoBillsDuringSync",
					JPAFRRegistry.Categories.Customs_Japan_AFR,
					"Add Carrier Code to Master Bill during Consol synchronization or Sailing Bills importing?",
					"For Forwarder Manifesting\r\n\r\nIf 'Yes' it assumed that for SEA shipments, the master bill number of your consol doesn't have the four letter Carrier Code at the beginning. When synchronizing data to the AFR from Consol, in order to meet JP AFR (Advance Filing Rules) message requirement, the corresponding carrier code of designated Shipping Line in the Consol will be added automatically at the beginning of the master bill if the master bill doesn't start with carrier code. You can specify the carrier code of the Shipping Line in the corresponding (Organization -> Config -> Registration Numbers) with 'Country/Region Of Issue' set as 'JP' and 'Type' set as 'CCC'. If the master bill in the Consol is longer than 31 characters, the last 31 characters will be combined with the carrier code as the master bill number in the AFR message.\r\n\r\nFor Carrier Manifesting\r\n\r\nIf 'Yes' it assumes that the Ocean Bills for the corresponding Sailing Schedule doesn't have the four letter Carrier Code at the beginning. When importing bills from the linked sailing schedule, in order to meet JP AFR message requirement, the corresponding carrier code will be added automatically at the beginning of the Ocean Bill Number.",
					RegistryStorageFlags.Company,
					true);
				AssertEquals("AFRAddSCACtoBillsDuringSync.CountryFilterPKs should be Empty", Enumerable.Empty<Guid>(), ItemSet.AFRAddSCACtoBillsDuringSync.CountryFilterPKs);
			});
		}

		public void TestAFRNVOCCIDtoAddtoBillDuringSync()
		{
			CombineAssertions(() =>
			{
				TestGenericRegistryItem(ItemSet.AFRNVOCCIDtoAddtoBillDuringSync,
				"AFRNVOCCIDtoAddtoBillDuringSync",
				JPAFRRegistry.Categories.Customs_Japan_AFR,
				"House Bill NVOCC Code",
				"JP AFR (Advance Filing Rules) messages require the house bill to start with NVOCC code assigned by 'NACCS Reporter ID Issuance System'. If the value is put in the below text box, it is assumed that for SEA shipments, the house bill doesn't start with the NVOCC code. When synchronizing data to AFR bill from Shipment, the value specified below will be added automatically at the beginning of the house bill to meet JP AFR message requirement if current bill number doesn't start with NVOCC code. If your house bill in the corresponding shipment is longer than 31 characters, the last 31 characters will be combined with the NVOCC code specified below to form house bill number in the AFR message.",
				RegistryStorageFlags.Company,
				""
				);
				AssertEquals("AFRAddSCACtoBillsDuringSync.CountryFilterPKs should be Empty", Enumerable.Empty<Guid>(), ItemSet.AFRAddSCACtoBillsDuringSync.CountryFilterPKs);
				AssertEquals("CharacterCase should be Upper", CharacterCase.Upper, ((StringRegistryDataType)ItemSet.AFRNVOCCIDtoAddtoBillDuringSync.DataType).CharacterCase);
				AssertEquals("MinLength should be 3", 3, ((StringRegistryDataType)ItemSet.AFRNVOCCIDtoAddtoBillDuringSync.DataType).MinLength);
				AssertEquals("MaxLength should be 4", 4, ((StringRegistryDataType)ItemSet.AFRNVOCCIDtoAddtoBillDuringSync.DataType).MaxLength);
			});
		}

		public void TestAFRReporterIDForDocument()
		{
			TestGenericRegistryItem(ItemSet.AFRReporterIDForDocument,
				"AFRReporterIDForDocument",
				JPAFRRegistry.Categories.Customs_Japan_AFR,
				"AFR Reporter ID",
				"The AFR Reporter ID specified in this registry Item will be used in the electronic messaging of AFR reporting and the Document printing on AFR Job. The reporter ID should be a 5 character string which you get from 'NACCS Reporter ID Issuance System'.",
				RegistryStorageFlags.Company | RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController
				);
		}

		public void TestOnAllValuesSavedAction_OnlyActiveCompanyIncluded()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "TS1";
			company1.GC_IsActive = true;

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "TS2";
			company2.GC_IsActive = true;

			var company3 = Factory.NewWithValidTestData<GlbCompany>();
			company3.GC_Code = "TS3";
			company3.GC_IsActive = false;

			company1.SetReporter(new AFRReporterID { ReporterID = "JJA01", Password = "TEST" });
			company2.SetReporter(new AFRReporterID { ReporterID = "JJA02", Password = "TEST" });
			company3.SetReporter(new AFRReporterID { ReporterID = "JJA03", Password = "Test" });

			Factory.Save();

			var registryItem = JPAFRRegistry.Instance.AFRReporterIDForDocument;
			JPAFRRegistry.Instance.OnAllValuesSavedAction();
			var config = GetConfigurationFromLastInterchange();
			var companyGroups = config.Group[0].Items;

			CombineAssertions(() =>
			{
				AssertEquals(2, companyGroups.Length);
				AssertNotNull(companyGroups.Where(x => (x as Group).Reference == "TS1"));
				AssertNotNull(companyGroups.Where(x => (x as Group).Reference == "TS2"));
			});
		}

		public void TestOnAllValuesSavedAction()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "TS1";

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "TS2";

			var company3 = Factory.NewWithValidTestData<GlbCompany>();
			company3.GC_Code = "TS3";

			company1.SetReporter(new AFRReporterID { ReporterID = "JJA01", Password = "TEST" });
			company2.SetReporter(new AFRReporterID { ReporterID = "JJA02", Password = "TEST" });
			company3.SetReporter(new AFRReporterID { ReporterID = "JJA03", Password = "TEST" });

			Factory.Save();

			var registryItem = JPAFRRegistry.Instance.AFRReporterIDForDocument;
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AFRReporterID { ReporterID = "JJA00", Password = "TEST" }))
			{
				JPAFRRegistry.Instance.OnAllValuesSavedAction();

				var config = GetConfigurationFromLastInterchange();

				CombineAssertions(() =>
				{
					var systemGroup = config.Group[0];
					AssertEquals("System Group Type", "System", systemGroup.Type);
					AssertEquals("System Group Reference", "EDIDAT", systemGroup.Reference);
					AssertEquals("System Credential UserName", "JJA00", ((Credential)systemGroup.Items[0]).UserName);

					var companyGroups = systemGroup.Items.OfType<Group>();
					AssertArrayEqualsByElements("Company Group Reference + Company Credential UserName", new[] { "TS1+JJA01", "TS2+JJA02", "TS3+JJA03" }, companyGroups.Select(c => GetCompanyGroupInfo(c)).OrderBy(c => c).ToArray());
				});

				var emptyReportID = new AFRReporterID();

				using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, emptyReportID))
				{
					JPAFRRegistry.Instance.OnUpdateAction(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, emptyReportID);
					JPAFRRegistry.Instance.OnUpdateAction(company3.PK.ToGuid(), Guid.Empty, Guid.Empty, emptyReportID);

					company1.SetReporter(emptyReportID);
					company3.SetReporter(emptyReportID);

					JPAFRRegistry.Instance.OnAllValuesSavedAction();
					config = GetConfigurationFromLastInterchange();

					CombineAssertions(() =>
					{
						var systemGroup = config.Group[0];
						AssertEquals("System Group Type", "System", systemGroup.Type);
						AssertEquals("System Group Reference", "EDIDAT", systemGroup.Reference);
						AssertEquals("System Empty Credential", 0, systemGroup.Items.OfType<Credential>().Count());

						var companyGroups = systemGroup.Items.Cast<Group>();
						AssertArrayEqualsByElements("Company Group Reference + Company Credential UserName", new[] { "TS1+NULL", "TS2+JJA02", "TS3+NULL" }, companyGroups.Select(c => GetCompanyGroupInfo(c)).OrderBy(c => c).ToArray());
					});
				}
			}

			string GetCompanyGroupInfo(Group group)
			{
				var sb = new ZStringBuilder();
				sb.Append(group.Reference);
				sb.Append("+");

				var items = group.Items;
				if (items == null || items.Length == 0)
				{
					sb.Append("NULL");
				}
				else
				{
					var credential = (Credential)items[0];
					sb.Append(credential.UserName);
				}

				return sb.ToString();
			}
		}

		Configuration GetConfigurationFromLastInterchange()
		{
			var query = new ZQuery(EDIInterchangeSchema.EI_To, "eHub");
			query.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";

			var interchange = Factory.LoadTop1<EDIInterchange>(query);
			AssertNotNull("Should send an email with these changed credential information.", interchange);

			using (var reader = new StringReader(interchange.EI_BodyText))
			{
				return ConfigurationExtension.DeserializeToConfiguration(reader);
			}
		}
	}
}
