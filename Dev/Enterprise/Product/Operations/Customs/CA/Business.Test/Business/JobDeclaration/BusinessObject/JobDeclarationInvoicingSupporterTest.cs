using System;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(JobDeclaration.JobDeclarationInvoicingSupporter))]
	sealed class JobDeclarationInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestLVXDestination()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0497", "0497", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "ZZ");
			Factory.Save();
			var countryState = Factory.NewWithValidTestData<RefCountryStates>();
			countryState.RW_Code = "ZZ";
			countryState.RW_RN_NKCountryCode = "AU";
			var unloco = Factory.NewWithValidTestData<RefUNLOCO>(); // should be found by tax zone
			unloco.RL_RW = countryState.PK;
			unloco.RL_Code = "CAAAA";
			unloco.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var zoneHeader = Factory.NewWithValidTestData<RefZoneHeader>();
			zoneHeader.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Tax;
			var portZonePivot = Factory.New<RefZonePivot>();
			portZonePivot.F2_FZ = zoneHeader.PK;
			portZonePivot.F2_ParentID = unloco.PK;
			portZonePivot.F2_ParentTableCode = RefUNLOCOSchema.Constants.Prefix;

			var unloco2 = Factory.NewWithValidTestData<RefUNLOCO>(); // should be found by country state
			unloco2.RL_RW = countryState.PK;
			unloco2.RL_Code = "CAZZZ";
			unloco.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			Factory.Save();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TSTORG";
			var address = orgHeader.MainAddress;
			address.OA_State = CanadianProvinceList.Codes.BritishColumbia;

			var lvx = Factory.New<JobDeclaration>();
			lvx.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;

			var invoice = lvx.LVXInvoiceHeader;
			invoice.CA_PortOfClearance = "0497";
			Factory.Save();

			var supporter = lvx.InvoicingSupporter;
			var destination = supporter.Destination;
			AssertNotNull(destination);
			AssertEquals("ZZ", destination.CountryStates.RW_Code);

			lvx.ImporterDeliveryAddress.OrganisationPK = orgHeader.PK;
			Factory.Save();

			supporter = lvx.InvoicingSupporter;
			destination = supporter.Destination;
			AssertNotNull(destination);
			AssertEquals("BC", destination.CountryStates.RW_Code);
		}

		public void TestFixedPlaceOfSupplyAndDestination()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0497", "0497", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "ZZ");
			Factory.Save();
			var countryState = Factory.NewWithValidTestData<RefCountryStates>();
			countryState.RW_Code = "ZZ";
			countryState.RW_RN_NKCountryCode = "AU";
			var unloco = Factory.NewWithValidTestData<RefUNLOCO>(); // should be found by tax zone
			unloco.RL_RW = countryState.PK;
			unloco.RL_Code = "CAAAA";
			unloco.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var zoneHeader = Factory.NewWithValidTestData<RefZoneHeader>();
			zoneHeader.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Tax;
			var portZonePivot = Factory.New<RefZonePivot>();
			portZonePivot.F2_FZ = zoneHeader.PK;
			portZonePivot.F2_ParentID = unloco.PK;
			portZonePivot.F2_ParentTableCode = RefUNLOCOSchema.Constants.Prefix;

			var unloco2 = Factory.NewWithValidTestData<RefUNLOCO>(); // should be found by country state
			unloco2.RL_RW = countryState.PK;
			unloco2.RL_Code = "CAZZZ";
			unloco.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();

			var supporter = declaration.InvoicingSupporter;
			AssertNotNull(supporter);

			var destination = supporter.Destination;
			AssertNull(destination);

			var fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
			AssertNull(fixedPlaceOfSupply);

			var origin = supporter.Origin;
			AssertNull(origin);

			using (AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				declaration.JE_CustomsOffice = "0497";
				declaration.JE_RL_NKFinalDestination = "CAAAD";
				declaration.JE_RL_NKOrigin = "CAAAD";
				using (CACustomsDataRegistry.Instance.UseCasualProvinceForTaxOverrides.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					destination = supporter.Destination;
					fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
					origin = supporter.Origin;

					AssertEquals("ZZ", destination.CountryStates.RW_Code);
					AssertNull(fixedPlaceOfSupply);
					AssertNull(origin);
				}

				destination = supporter.Destination;
				fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
				origin = supporter.Origin;
				AssertEquals("MB", destination.CountryStates.RW_Code);
				AssertNull(fixedPlaceOfSupply);
				AssertEquals("MB", origin.CountryStates.RW_Code);

				invoiceLine1.CA_IsCasualImport = true;
				invoiceLine1.CA_CasualImportDestinationProvince = "BC";
				invoiceLine2.CA_IsCasualImport = true;
				invoiceLine2.CA_CasualImportDestinationProvince = "BC";
				using (CACustomsDataRegistry.Instance.UseCasualProvinceForTaxOverrides.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					destination = supporter.Destination;
					fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
					origin = supporter.Origin;
					AssertEquals("BC", destination.CountryStates.RW_Code);
					AssertNull(fixedPlaceOfSupply);
					AssertNull(origin);
				}

				destination = supporter.Destination;
				fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
				origin = supporter.Origin;
				AssertEquals("MB", destination.CountryStates.RW_Code);
				AssertNull(fixedPlaceOfSupply);
				AssertEquals("MB", origin.CountryStates.RW_Code);

				invoiceLine3.CA_IsCasualImport = true;
				invoiceLine3.CA_CasualImportDestinationProvince = "AB";
				using (CACustomsDataRegistry.Instance.UseCasualProvinceForTaxOverrides.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					destination = supporter.Destination;
					fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
					origin = supporter.Origin;
					AssertEquals("ZZ", destination.CountryStates.RW_Code);
					AssertNull(fixedPlaceOfSupply);
					AssertNull(origin);
				}

				destination = supporter.Destination;
				fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
				origin = supporter.Origin;
				AssertEquals("MB", destination.CountryStates.RW_Code);
				AssertNull(fixedPlaceOfSupply);
				AssertEquals("MB", origin.CountryStates.RW_Code);
			}

			using (AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				declaration.JE_CustomsOffice = "0497";
				declaration.JE_RL_NKFinalDestination = "CAAAD";
				using (CACustomsDataRegistry.Instance.UseCasualProvinceForTaxOverrides.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					destination = supporter.Destination;
					fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
					origin = supporter.Origin;
					AssertEquals("ZZ", destination.CountryStates.RW_Code);
					AssertEquals("ZZ", fixedPlaceOfSupply.State.RW_Code);
					AssertNull(origin);
				}

				destination = supporter.Destination;
				fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
				origin = supporter.Origin;
				AssertEquals("MB", destination.CountryStates.RW_Code);
				AssertEquals("ZZ", fixedPlaceOfSupply.State.RW_Code);
				AssertEquals("MB", origin.CountryStates.RW_Code);

				invoiceLine1.CA_CasualImportDestinationProvince = "BC";
				invoiceLine2.CA_CasualImportDestinationProvince = "BC";
				invoiceLine3.CA_CasualImportDestinationProvince = "BC";

				using (CACustomsDataRegistry.Instance.UseCasualProvinceForTaxOverrides.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					destination = supporter.Destination;
					fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
					origin = supporter.Origin;
					AssertEquals("BC", destination.CountryStates.RW_Code);
					AssertEquals("BC", fixedPlaceOfSupply.State.RW_Code);
					AssertNull(origin);
				}

				destination = supporter.Destination;
				fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
				origin = supporter.Origin;
				AssertEquals("MB", destination.CountryStates.RW_Code);
				AssertEquals("ZZ", fixedPlaceOfSupply.State.RW_Code);
				AssertEquals("MB", origin.CountryStates.RW_Code);

				var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine4.CA_IsCasualImport = true;
				invoiceLine4.CA_IsAutoDummyHSCodeCasualImportLine = true;
				invoiceLine4.CA_CasualImportDestinationProvince = "ON";
				using (CACustomsDataRegistry.Instance.UseCasualProvinceForTaxOverrides.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					destination = supporter.Destination;
					fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
					origin = supporter.Origin;
					AssertEquals("BC", destination.CountryStates.RW_Code);
					AssertEquals("BC", fixedPlaceOfSupply.State.RW_Code);
					AssertNull(origin);
				}

				destination = supporter.Destination;
				fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
				origin = supporter.Origin;
				AssertEquals("MB", destination.CountryStates.RW_Code);
				AssertEquals("ZZ", fixedPlaceOfSupply.State.RW_Code);
				AssertEquals("MB", origin.CountryStates.RW_Code);

				invoiceLine3.CA_IsCasualImport = true;
				invoiceLine3.CA_CasualImportDestinationProvince = "AB";
				using (CACustomsDataRegistry.Instance.UseCasualProvinceForTaxOverrides.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					destination = supporter.Destination;
					fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
					origin = supporter.Origin;
					AssertEquals("ZZ", destination.CountryStates.RW_Code);
					AssertEquals("ZZ", fixedPlaceOfSupply.State.RW_Code);
					AssertNull(origin);
				}

				destination = supporter.Destination;
				fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
				origin = supporter.Origin;
				AssertEquals("MB", destination.CountryStates.RW_Code);
				AssertEquals("ZZ", fixedPlaceOfSupply.State.RW_Code);
				AssertEquals("MB", origin.CountryStates.RW_Code);
			}
		}

		public void TestFixedPlaceOfSupplyAndDestination_ExcludeInActiveUNLOCO()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0497", "0497", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "ZZ");

			var officeCode2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0496", "0496", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "YY");

			Factory.Save();
			var countryState = Factory.NewWithValidTestData<RefCountryStates>();
			countryState.RW_Code = "ZZ";
			countryState.RW_RN_NKCountryCode = "AU";

			var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco.RL_RW = countryState.PK;
			unloco.RL_Code = "CAAAA";
			unloco.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			unloco.IsCancelled = true;

			var countryState2 = Factory.NewWithValidTestData<RefCountryStates>();
			countryState2.RW_Code = "YY";
			countryState2.RW_RN_NKCountryCode = "AU";

			var unloco2 = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco2.RL_RW = countryState2.PK;
			unloco2.RL_Code = "CA111";
			unloco2.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var zoneHeader = Factory.NewWithValidTestData<RefZoneHeader>();
			zoneHeader.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Tax;

			var portZonePivot = Factory.New<RefZonePivot>();
			portZonePivot.F2_FZ = zoneHeader.PK;
			portZonePivot.F2_ParentID = unloco.PK;
			portZonePivot.F2_ParentTableCode = RefUNLOCOSchema.Constants.Prefix;

			var portZonePivot2 = Factory.New<RefZonePivot>();
			portZonePivot2.F2_FZ = zoneHeader.PK;
			portZonePivot2.F2_ParentID = unloco2.PK;
			portZonePivot2.F2_ParentTableCode = RefUNLOCOSchema.Constants.Prefix;

			var unloco3 = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco3.RL_RW = countryState.PK;
			unloco3.RL_Code = "CAZZZ";
			unloco.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var supporter = declaration.InvoicingSupporter;
			AssertNotNull(supporter);

			var destination = supporter.Destination;
			AssertNull(destination);

			var fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
			AssertNull(fixedPlaceOfSupply);

			using (AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				declaration.JE_CustomsOffice = "0497";
				declaration.JE_RL_NKFinalDestination = "CAAAD";
				using (CACustomsDataRegistry.Instance.UseCasualProvinceForTaxOverrides.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					destination = supporter.Destination;
					fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
					AssertEquals("CAZZZ", destination.RL_Code);
					AssertEquals("CAZZZ", fixedPlaceOfSupply.UNLOCO.RL_Code);
				}

				destination = supporter.Destination;
				fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
				AssertEquals("CAAAD", destination.RL_Code);
				AssertEquals("CAZZZ", fixedPlaceOfSupply.UNLOCO.RL_Code);

				declaration.JE_CustomsOffice = "0496";
				using (CACustomsDataRegistry.Instance.UseCasualProvinceForTaxOverrides.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					destination = supporter.Destination;
					fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
					AssertEquals("CA111", destination.RL_Code);
					AssertEquals("CA111", fixedPlaceOfSupply.UNLOCO.RL_Code);
				}
			}
		}

		public void TestFixedPlaceOfSupplyAndDestination_MSC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Misc;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();

			var supporter = declaration.InvoicingSupporter;
			AssertNotNull(supporter);

			var destination = supporter.Destination;
			AssertNull(destination);

			var fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
			AssertNull(fixedPlaceOfSupply);

			var origin = supporter.Origin;
			AssertNull(origin);

			using (AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				declaration.JE_RL_NKFinalDestination = "CAAAD";
				declaration.JE_RL_NKOrigin = "CAAAD";
				using (CACustomsDataRegistry.Instance.UseCasualProvinceForTaxOverrides.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					destination = supporter.Destination;
					fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
					origin = supporter.Origin;
					AssertEquals("MB", destination.CountryStates.RW_Code);
					AssertNull(fixedPlaceOfSupply);
					AssertNull(origin);
				}

				destination = supporter.Destination;
				fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
				origin = supporter.Origin;
				AssertEquals("MB", destination.CountryStates.RW_Code);
				AssertNull(fixedPlaceOfSupply);
				AssertEquals("MB", origin.CountryStates.RW_Code);
			}

			using (AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				declaration.JE_RL_NKFinalDestination = "CAAAD";
				supporter = declaration.InvoicingSupporter;
				using (CACustomsDataRegistry.Instance.UseCasualProvinceForTaxOverrides.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					destination = supporter.Destination;
					fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
					origin = supporter.Origin;
					AssertEquals("MB", destination.CountryStates.RW_Code);
					AssertEquals("MB", fixedPlaceOfSupply.State.RW_Code);
					AssertNull(origin);
				}

				destination = supporter.Destination;
				fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
				origin = supporter.Origin;
				AssertEquals("MB", destination.CountryStates.RW_Code);
				AssertNull(fixedPlaceOfSupply);
				AssertEquals("MB", origin.CountryStates.RW_Code);
			}
		}

		public void TestFixedPlaceOfSupplyAndDestination_EXP()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();

			var supporter = declaration.InvoicingSupporter;
			AssertNotNull(supporter);

			var destination = supporter.Destination;
			AssertNull(destination);

			var fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
			AssertNull(fixedPlaceOfSupply);

			var origin = supporter.Origin;
			AssertNull(origin);

			using (AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				declaration.JE_RL_NKFinalDestination = "CAAAD";
				declaration.JE_RL_NKOrigin = "CAAAD";
				using (CACustomsDataRegistry.Instance.UseCasualProvinceForTaxOverrides.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					destination = supporter.Destination;
					fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
					origin = supporter.Origin;
					AssertEquals("MB", destination.CountryStates.RW_Code);
					AssertNull(fixedPlaceOfSupply);
					AssertNull(origin);
				}

				destination = supporter.Destination;
				fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
				origin = supporter.Origin;
				AssertEquals("MB", destination.CountryStates.RW_Code);
				AssertNull(fixedPlaceOfSupply);
				AssertEquals("MB", origin.CountryStates.RW_Code);
			}

			using (AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				declaration.JE_RL_NKFinalDestination = "CAAAD";
				supporter = declaration.InvoicingSupporter;
				using (CACustomsDataRegistry.Instance.UseCasualProvinceForTaxOverrides.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					destination = supporter.Destination;
					fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
					origin = supporter.Origin;
					AssertEquals("MB", destination.CountryStates.RW_Code);
					AssertEquals("MB", fixedPlaceOfSupply.State.RW_Code);
					AssertNull(origin);
				}

				destination = supporter.Destination;
				fixedPlaceOfSupply = supporter.FixedPlaceOfSupply;
				origin = supporter.Origin;
				AssertEquals("MB", destination.CountryStates.RW_Code);
				AssertNull(fixedPlaceOfSupply);
				AssertEquals("MB", origin.CountryStates.RW_Code);
			}
		}

		public void TestOverridenDepartment()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var expectedDepartmentPK = CargoWise.Application.ObjectFactory.Get<Integration.Accounting.IAccounting>().CustomsImportOther;

			CombineAssertions(() =>
			{
				foreach (var transportMode in new TransportTypeList().GetAllCodes())
				{
					declaration.JE_TransportMode = transportMode;
					switch (transportMode)
					{
						case TransportTypeList.Codes.Sea:
						case TransportTypeList.Codes.Air:
						case TransportTypeList.Codes.Rail:
						case TransportTypeList.Codes.Mail:
						case TransportTypeList.Codes.Road:
						case TransportTypeList.Codes.InlandWaterwayTransport:
							AssertEquals("Handled by Enterprise.Accounting.Business.JobInvoicing.DepartmentChooser",
								ZGuid.Empty, ((IJobInvoicingPlugIn)declaration).InvoicingSupporter.OverriddenDepartmentPK);
							break;
						default:
							AssertEquals(string.Format("TransportMode: {0}", transportMode),
								expectedDepartmentPK, ((IJobInvoicingPlugIn)declaration).InvoicingSupporter.OverriddenDepartmentPK);
							break;
					}
				}
			});

			expectedDepartmentPK = CargoWise.Application.ObjectFactory.Get<Integration.Accounting.IAccounting>().CustomsOther;
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			CombineAssertions(() =>
			{
				foreach (var transportMode in new TransportTypeList().GetAllCodes())
				{
					declaration.JE_TransportMode = transportMode;
					AssertEquals($"Default department from Customs Other registry for B2 declarations(TransportMode: {transportMode})", expectedDepartmentPK, ((IJobInvoicingPlugIn)declaration).InvoicingSupporter.OverriddenDepartmentPK);
				}
			});

			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			CombineAssertions(() =>
			{
				foreach (var transportMode in new TransportTypeList().GetAllCodes())
				{
					declaration.JE_TransportMode = transportMode;
					AssertEquals($"Default department from Customs Other registry for B3X declarations(TransportMode: {transportMode})", expectedDepartmentPK, ((IJobInvoicingPlugIn)declaration).InvoicingSupporter.OverriddenDepartmentPK);
				}
			});
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			JobDeclaration jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			return jobDeclaration;
		}

		protected override ZString TestingCountry
		{
			get { return Core.Constants.CountryCodes.Canada; }
		}
	}
}
