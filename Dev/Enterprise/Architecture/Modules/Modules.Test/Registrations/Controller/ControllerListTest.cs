using System.Linq;
using CargoWise.Application;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ControllerListTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestRegisteringControllerWithMixedOverrides()
		{
			DummyControllerList list = new DummyControllerList();
		}

		public void TestClientControllersAddOn()
		{
			ControllerID controllerID1 = new ControllerID("ControllerID1");
			ControllerInfo info1 = new ControllerInfo(controllerID1, "Enterprise.ZArchitecture.Business", "Enterprise.ZArchitecture.Modules.Testing.DummyController1");
			ControllerID controllerID2 = new ControllerID("ControllerID2");
			ControllerInfo info2 = new ControllerInfo(controllerID2, "Enterprise.ZArchitecture.Business", "Enterprise.ZArchitecture.Modules.Testing.DummyController2");
			ControllerInfo[] infos = new ControllerInfo[] { info1, info2 };

			ControllerList list = new ControllerList();
			AssertNotNull("ControllerList", list);
			AssertNull(list[controllerID1, ""]);
			AssertNull(list[controllerID2, ""]);

			TestClientHook.Instance.NewClientControllersForTest = infos;
			using (ClientHookLoader.Instance.OverrideClientHookForTest(TestClientHook.Instance))
			{
				list = new ControllerList();
				AssertNotNull("ControllerList", list);
				ControllerInfo existingInfo = list[controllerID1, ""];
				AssertNotNull(existingInfo);
				AssertEquals(info1, existingInfo);

				existingInfo = list[controllerID2, ""];
				AssertNotNull(existingInfo);
				AssertEquals(info2, existingInfo);
			}

			using (ClientHookLoader.Instance.OverrideClientHookForTest(null))
			{
				list = new ControllerList();
				AssertNotNull("ControllerList", list);
				AssertNull(list[controllerID1, ""]);
				AssertNull(list[controllerID2, ""]);
			}
		}

		#region DummyControllerList

		class DummyControllerList : ControllerList
		{
			public DummyControllerList()
			{
				Add(new ControllerInfo(new ControllerID("000"), "", "", "SG"));
				Add(new ControllerInfo(new ControllerID("000"), "", "", "AU"));
				Add(new ClientOverrideControllerInfo(new ClientOverrideControllerID(new ControllerID("000")), "", "", true));
			}
		}

		#endregion

		#region ControllerList
		public void TestExporterSchemeControllerUSHasUSTerritories()
		{
			ControllerList list = new ControllerList();

			AssertEquals(list[ControllerIDs.ExporterScheme, Constants.CountryCodes.PuertoRico].ClassFullNameForTest, "Enterprise.MasterFiles.Module.ExporterSchemeControllerUS");
			AssertEquals(list[ControllerIDs.ExporterScheme, Constants.CountryCodes.VirginIslands].ClassFullNameForTest, "Enterprise.MasterFiles.Module.ExporterSchemeControllerUS");
			AssertEquals(list[ControllerIDs.ExporterScheme, Constants.CountryCodes.Guam].ClassFullNameForTest, "Enterprise.MasterFiles.Module.ExporterSchemeControllerUS");
			AssertEquals(list[ControllerIDs.ExporterScheme, Constants.CountryCodes.NorthernMarianaIslands].ClassFullNameForTest, "Enterprise.MasterFiles.Module.ExporterSchemeControllerUS");
			AssertEquals(list[ControllerIDs.ExporterScheme, Constants.CountryCodes.AmericanSamoa].ClassFullNameForTest, "Enterprise.MasterFiles.Module.ExporterSchemeControllerUS");
		}

		public void TestExporterSchemeControllerUnitedKingdom()
		{
			var list = new ControllerList();
			AssertEquals(list[ControllerIDs.ExporterScheme, Constants.CountryCodes.UnitedKingdom].ClassFullNameForTest, "Enterprise.MasterFiles.Module.ExporterSchemeControllerUK");
		}

		#endregion

		public void TestDeviceDetailsControllerIDIsTelematicsDeviceDetails()
		{
			AssertEquals("DeviceDetails should be TelematicsDeviceDetails for backwards compatibility with generated URLs.", "TelematicsDeviceDetails", ControllerIDs.DeviceDetails.Name);
		}

		public void TestPortMessagingIsAvailableForAllCountries()
		{
			var list = new ControllerList();
			var portMessaging = list[ControllerIDs.PortMessaging, Constants.CountryCodes.Germany];
			AssertNotNull(portMessaging);
			AssertEquals("Enterprise.Freight.Forwarding.PortMessaging.Module.PortMessagingController", portMessaging.ClassFullNameForTest);
			AssertEquals(string.Empty, portMessaging.CountryCode);

			portMessaging = list[ControllerIDs.PortMessaging, Constants.CountryCodes.Australia];
			AssertNotNull(portMessaging);
			AssertEquals("Enterprise.Freight.Forwarding.PortMessaging.Module.PortMessagingController", portMessaging.ClassFullNameForTest);
			AssertEquals(string.Empty, portMessaging.CountryCode);
		}

		public void TestGlobalHarbourRateIsAvailableForAllCountries()
		{
			var list = new ControllerList();
			var harbourRate = list[ControllerIDs.Customs.Universal.RefHarbourRate, Constants.CountryCodes.France];
			AssertNotNull(harbourRate);
			AssertEquals("Enterprise.Customs.Universal.Module.RefHarbourRateController", harbourRate.ClassFullNameForTest);
			AssertEquals(string.Empty, harbourRate.CountryCode);

			harbourRate = list[ControllerIDs.Customs.Universal.RefHarbourRate, Constants.CountryCodes.Martinique];
			AssertNotNull(harbourRate);
			AssertEquals("Enterprise.Customs.Universal.Module.RefHarbourRateController", harbourRate.ClassFullNameForTest);
			AssertEquals(string.Empty, harbourRate.CountryCode);

			harbourRate = list[ControllerIDs.Customs.Universal.RefHarbourRate, Constants.CountryCodes.Italy];
			AssertNotNull(harbourRate);
			AssertEquals("Enterprise.Customs.Universal.Module.RefHarbourRateController", harbourRate.ClassFullNameForTest);
			AssertEquals(string.Empty, harbourRate.CountryCode);

			harbourRate = list[ControllerIDs.Customs.Universal.RefHarbourRate, Constants.CountryCodes.Australia];
			AssertNotNull(harbourRate);
			AssertEquals("Enterprise.Customs.Universal.Module.RefHarbourRateController", harbourRate.ClassFullNameForTest);
			AssertEquals(string.Empty, harbourRate.CountryCode);
		}

		public void TestETerminalReleaseManifestPortMessagingForAllCountries()
		{
			var list = new ControllerList();
			var eTerminalReleaseManifest = list[ControllerIDs.ETerminalReleaseManifestPortMessaging, Constants.CountryCodes.China];
			AssertNotNull(eTerminalReleaseManifest);
			AssertEquals("Enterprise.Freight.Forwarding.Documents.Module.ETerminalReleaseManifestPortMessagingController", eTerminalReleaseManifest.ClassFullNameForTest);
			AssertEquals(string.Empty, eTerminalReleaseManifest.CountryCode);

			eTerminalReleaseManifest = list[ControllerIDs.ETerminalReleaseManifestPortMessaging, Constants.CountryCodes.Australia];
			AssertNotNull(eTerminalReleaseManifest);
			AssertEquals("Enterprise.Freight.Forwarding.Documents.Module.ETerminalReleaseManifestPortMessagingController", eTerminalReleaseManifest.ClassFullNameForTest);
			AssertEquals(string.Empty, eTerminalReleaseManifest.CountryCode);
		}

		public void TestTemporaryStorageIsAvailableForES()
		{
			var list = new ControllerList();
			var temporaryStorage = list[ControllerIDs.Customs.TemporaryStorage, Constants.CountryCodes.Spain];
			AssertNotNull(temporaryStorage);
			AssertEquals("Enterprise.Customs.ES.TemporaryStorage.Module.TemporaryStorageController", temporaryStorage.ClassFullNameForTest);
			AssertEquals(Constants.CountryCodes.Spain, temporaryStorage.CountryCode);
		}

		public void TestTemporaryStorageRegisterIsAvailableForES()
		{
			var list = new ControllerList();
			var temporaryStorageRegister = list[ControllerIDs.Customs.ES.TemporaryStorageRegister, Constants.CountryCodes.Spain];
			AssertNotNull(temporaryStorageRegister);
			AssertEquals("Enterprise.Customs.ES.TemporaryStorage.Module.TemporaryStorageRegisterController", temporaryStorageRegister.ClassFullNameForTest);
			AssertEquals(Constants.CountryCodes.Spain, temporaryStorageRegister.CountryCode);
		}

		public void TestExitSummaryIsAvailableForES()
		{
			var list = new ControllerList();
			var exitSummary = list[ControllerIDs.Customs.EU.ExitSummaryController, Constants.CountryCodes.Spain];
			AssertNotNull(exitSummary);
			AssertEquals("Enterprise.Customs.ES.Module.ExitSummaryController", exitSummary.ClassFullNameForTest);
			AssertEquals(Constants.CountryCodes.Spain, exitSummary.CountryCode);
		}

		public void TestTempStorageRegisterIsAvailableForFranceAndOverseasDepartmentsUnderItsCustomsJurisdiction()
		{
			var list = new ControllerList();
			foreach (var countryCode in Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				var tempStorageRegisterController = list[ControllerIDs.Customs.EU.TempStorageRegister, countryCode];
				AssertNotNull(tempStorageRegisterController);
				AssertEquals("Enterprise.Customs.FR.Module.TempStorageRegisterController", tempStorageRegisterController.ClassFullNameForTest);
				AssertEquals(countryCode, tempStorageRegisterController.CountryCode);
			}
		}

		public void TestTemporaryStorageIsAvailableForPL()
		{
			var list = new ControllerList();
			var temporaryStorage = list[ControllerIDs.Customs.TemporaryStorage, Constants.CountryCodes.Poland];
			AssertNotNull(temporaryStorage);
			AssertEquals("Enterprise.Customs.PL.Module.TemporaryStorageController", temporaryStorage.ClassFullNameForTest);
			AssertEquals(Constants.CountryCodes.Poland, temporaryStorage.CountryCode);
		}

		public void TestTemporaryStorageIsAvailableForEUCountries()
		{
			var list = new ControllerList();
			foreach (var euCustomsMember in ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers())
			{
				if (ControllerList.EuCountriesThatHaveTheirOwnUCC6TemporaryStorageController.Contains(euCustomsMember))
				{
					continue;
				}
				var temporaryStorageUCC6 = list[ControllerIDs.Customs.EU.UCC6TemporaryStorage, euCustomsMember];
				AssertNotNull(temporaryStorageUCC6);
				AssertEquals("Enterprise.Customs.EU.TemporaryStorage.Module.UCC6TemporaryStorageController", temporaryStorageUCC6.ClassFullNameForTest);
				AssertEquals(euCustomsMember, temporaryStorageUCC6.CountryCode);
			}
		}

		public void TestTemporaryStoragePremisesIsAvailableForEUCountries()
		{
			var list = new ControllerList();
			foreach (var euCustomsMember in ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers())
			{
				if (ControllerList.EuCountriesThatHaveTheirOwnTemporaryStoragePremisesController.Contains(euCustomsMember))
				{
					continue;
				}
				var temporaryStoragePremises = list[ControllerIDs.Customs.EU.TempStoragePremises, euCustomsMember];
				AssertNotNull(temporaryStoragePremises);
				AssertEquals("Enterprise.Customs.EU.TemporaryStorage.Module.TempStoragePremisesController", temporaryStoragePremises.ClassFullNameForTest);
				AssertEquals(euCustomsMember, temporaryStoragePremises.CountryCode);
			}
		}

		public void TestTemporaryStoragePremisesIsAvailableForES()
		{
			var list = new ControllerList();
			var temporaryStoragePremises = list[ControllerIDs.Customs.EU.TempStoragePremises, Constants.CountryCodes.Spain];
			AssertNotNull(temporaryStoragePremises);
			AssertEquals("Enterprise.Customs.ES.TemporaryStorage.Module.TempStoragePremisesController", temporaryStoragePremises.ClassFullNameForTest);
			AssertEquals(Constants.CountryCodes.Spain, temporaryStoragePremises.CountryCode);
		}

		public void TestUCC6TemporaryStorageIsAvailableForIE()
		{
			var list = new ControllerList();
			var temporaryStorageUCC6 = list[ControllerIDs.Customs.EU.UCC6TemporaryStorage, Constants.CountryCodes.Ireland];
			AssertNotNull(temporaryStorageUCC6);
			AssertEquals("Enterprise.Customs.IE.Module.UCC6TemporaryStorageController", temporaryStorageUCC6.ClassFullNameForTest);
			AssertEquals(Constants.CountryCodes.Ireland, temporaryStorageUCC6.CountryCode);
		}

		public void TestUCC6TemporaryStorageIsAvailableForIT()
		{
			var list = new ControllerList();
			var temporaryStorageUCC6 = list[ControllerIDs.Customs.EU.UCC6TemporaryStorage, Constants.CountryCodes.Italy];
			AssertNotNull(temporaryStorageUCC6);
			AssertEquals("Enterprise.Customs.IT.TemporaryStorage.Module.UCC6TemporaryStorageController", temporaryStorageUCC6.ClassFullNameForTest);
			AssertEquals(Constants.CountryCodes.Italy, temporaryStorageUCC6.CountryCode);
		}

		public void TestTempStorageRegisterIsAvailableForIT()
		{
			var countryCode = Constants.CountryCodes.Italy;
			var list = new ControllerList();

			var tempStorageRegister = list[ControllerIDs.Customs.EU.TempStorageRegister, countryCode];
			AssertNotNull(tempStorageRegister);
			AssertEquals("Enterprise.Customs.IT.TemporaryStorage.Module.TempStorageRegisterController", tempStorageRegister.ClassFullNameForTest);
			AssertEquals(countryCode, tempStorageRegister.CountryCode);
		}

		public void TestG5V1TemporaryStorageIsAvailableForES()
		{
			var list = new ControllerList();
			var temporaryStorageG5V1 = list[ControllerIDs.Customs.EU.UCC6TemporaryStorage, Constants.CountryCodes.Spain];
			AssertNotNull(temporaryStorageG5V1);
			AssertEquals("Enterprise.Customs.ES.TemporaryStorage.Module.G5V1TemporaryStorageController", temporaryStorageG5V1.ClassFullNameForTest);
			AssertEquals(Constants.CountryCodes.Spain, temporaryStorageG5V1.CountryCode);
		}

		public void TestCusStatementController()
		{
			ControllerList list = new ControllerList();

			AssertEquals("Enterprise.Customs.Module.StatementController", list[ControllerIDs.Customs.CustomsStatement, ""].ClassFullNameForTest);
			AssertEquals("Enterprise.Customs.CA.Module.ARLStatementOfAccountController", list[ControllerIDs.Customs.CustomsStatement, Constants.CountryCodes.Canada].ClassFullNameForTest);
			AssertEquals("Enterprise.Customs.ZA.Module.CusStatementController", list[ControllerIDs.Customs.CustomsStatement, Constants.CountryCodes.SouthAfrica].ClassFullNameForTest);
			AssertEquals("Enterprise.Customs.US.Module.StatementController", list[ControllerIDs.Customs.CustomsStatement, Constants.CountryCodes.UnitedStates].ClassFullNameForTest);
			foreach (var countryCode in Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				AssertEquals("Enterprise.Customs.FR.Module.StatementController", list[ControllerIDs.Customs.CustomsStatement, countryCode].ClassFullNameForTest);
			}
			AssertEquals("Enterprise.Customs.KR.Module.CusStatementController", list[ControllerIDs.Customs.CustomsStatement, Constants.CountryCodes.KoreaSouth].ClassFullNameForTest);
			AssertEquals("Enterprise.Customs.TR.Module.StatementsStampDutyController", list[ControllerIDs.Customs.CustomsStatement, Constants.CountryCodes.Turkey].ClassFullNameForTest);

			AssertEquals("Enterprise.Customs.CA.Module.DailyNoticeReconciliationController", list[ControllerIDs.Customs.CA.CADailyNoticeReconciliation, ""].ClassFullNameForTest);
		}

		public void TestControllerIdLength()
		{
			var controllerIdList = new ControllerList();

			foreach (var info in controllerIdList.All)
			{
				Assert($"The length of controller id: {info.ID.Name} should less than 70, but now is {info.ID.Name.Length}.", info.ID.Name.Length < 70);
			}
		}

		public void TestNctsMovementControllerForNL()
		{
			var list = new ControllerList();
			var nctsMovementControllerForNL = list[ControllerIDs.Customs.EU.NctsMovementController, Constants.CountryCodes.Netherlands];
			AssertNotNull(nctsMovementControllerForNL);
			AssertEquals("Enterprise.Customs.NL.NCTS.Module.NctsMovementController", nctsMovementControllerForNL.ClassFullNameForTest);
			AssertEquals(Constants.CountryCodes.Netherlands, nctsMovementControllerForNL.CountryCode);
		}

		public void TestTemporaryStorageControllerForNorway()
		{
			var list = new ControllerList();
			AssertController(list, ControllerIDs.Customs.NO.TemporaryStorageRegister, "Enterprise.Customs.NO.Module.SumARegisterController", Constants.CountryCodes.Norway);
			AssertController(list, ControllerIDs.Customs.NO.TemporaryStorageRegisterReadOnly, "Enterprise.Customs.NO.Module.SumARegisterReadOnlyController", Constants.CountryCodes.Norway);
		}

		public void TestSumARegisterController()
		{
			var list = new ControllerList();
			AssertController(list, ControllerIDs.Customs.SumARegister, "Enterprise.Customs.EFTA.TemporaryStorageRegister.Module.SumARegisterController");
			AssertController(list, ControllerIDs.Customs.SumARegisterReadOnly, "Enterprise.Customs.EFTA.TemporaryStorageRegister.Module.SumARegisterReadOnlyController");
		}

		public void TestControllersForCH()
		{
			var list = new ControllerList();
			var listForCH = list.All.Where(x => x.CountryCode == Constants.CountryCodes.Switzerland);
			AssertEquals("# of controllers for CH" + string.Join("\n", listForCH.Select(x => x.IDForTest + ":" + x.ClassFullNameForTest)), 10, listForCH.Count());
			AssertController(list, ControllerIDs.CommercialInvoice, "Enterprise.Customs.CH.Module.CommercialInvoiceController", Constants.CountryCodes.Switzerland);
			AssertController(list, ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.CH.Module.JobDeclarationController", Constants.CountryCodes.Switzerland);
			AssertController(list, ControllerIDs.Customs.SingleTariffClassification, "Enterprise.Customs.CH.Module.CusClassificationController", Constants.CountryCodes.Switzerland);
			AssertController(list, ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, "Enterprise.Customs.CH.Module.JobDeclarationShipmentController", Constants.CountryCodes.Switzerland);
			AssertController(list, ControllerIDs.Customs.Permits, "Enterprise.Customs.CH.Module.CusPermitController", Constants.CountryCodes.Switzerland);
			AssertController(list, ControllerIDs.Customs.EU.NctsMovementController, "Enterprise.Customs.CH.NCTS.Module.NctsMovementController", Constants.CountryCodes.Switzerland);
			AssertController(list, ControllerIDs.Customs.CH.EntryHeader, "Enterprise.Customs.CH.Module.EntryHeaderController", Constants.CountryCodes.Switzerland);
			AssertController(list, ControllerIDs.Customs.CH.CustomsSummary, "Enterprise.Customs.CH.Module.CustomsSummaryController", Constants.CountryCodes.Switzerland);
			AssertController(list, ControllerIDs.Customs.CH.DeclarationActivation, "Enterprise.Customs.CH.DeclarationActivation.Module.DeclarationActivationController", Constants.CountryCodes.Switzerland);
		}

		static void AssertController(ControllerList list, ControllerID controllerId, string expectedClassFullName, string countryCode = "")
		{
			var controllerName = controllerId.Name;
			var controller = list[controllerId, countryCode];
			AssertNotNull($"Controller Entry for {controllerName}", controller);
			CombineAssertions($"Controller Data: {controllerName}", () =>
			{
				AssertEquals("ClassFullNameForTest", expectedClassFullName, controller.ClassFullNameForTest);
				AssertEquals("CountryCode", countryCode, controller.CountryCode);
			});
		}
	}
}
