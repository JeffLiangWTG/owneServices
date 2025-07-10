using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;
using ApplicationCodeTypeList = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	public abstract class BaseManifestFormPerformanceTest<THeader, TContainer, TBill, TPack, TPackedItem, TABCEntryNum, TAsycudaPackedItemEntryNum> : TestCaseWithFactory
			where THeader : AsycudaManifestHeader
			where TContainer : AsycudaContainer
			where TBill : AsycudaBill
			where TPack : AsycudaPack
			where TPackedItem : AsycudaPackedItem
			where TABCEntryNum : ABLEntryNum
			where TAsycudaPackedItemEntryNum : AsycudaPackedItemEntryNum
	{
		protected BusinessObject CreateFullyPopulatedObject(BusinessObjectFactory factory) => CreateFullyPopulatedObject(new TestData(CountryCode, factory), factory, 30, 10, 10, true);

		protected abstract string CountryCode { get; }

		protected const string DateFormat = "yyyyMMddHHmm";

		protected THeader CreateFullyPopulatedObject(TestData testData, BusinessObjectFactory factory, int numberOfBills, int numberOfPacks, int numberOfContainers, bool createContact)
		{
			var manifestHeader = (THeader)AsycudaManifestHeaderHelper.CreateNew(factory, testData.CountryCode, testData.ManifestType);
			using (manifestHeader.GetValidationSuspender())
			{
				var testBusinessObjectKind = TestBusinessObjectKind.PopulateDates | TestBusinessObjectKind.PopulateNumbers | TestBusinessObjectKind.PopulateStrings;
				manifestHeader.FillWithValidTestData(testBusinessObjectKind, Array.Empty<PropertyDescriptor>());
				var clusterKey = manifestHeader.AMA_ClusterKey;
				manifestHeader.AMA_JobReference = ZString.Empty;
				DecorateManifestHeader(testData, manifestHeader);

				if (createContact)
				{
					foreach (var contact in testData.contacts)
					{
						var glbPerson = GlbPerson.CreateFromContact(factory, contact);
						var person = manifestHeader.Persons.AddNew();
						person.CPN_PER_Person = glbPerson.PK;
						person.FillWithValidTestData(testBusinessObjectKind, Array.Empty<PropertyDescriptor>());
						person.CPN_ParentID = manifestHeader.PK;
						person.CPN_ParentTableCode = manifestHeader.TablePrefix;
						var personCountry = person.Countries.OfType<CusPersonCountry>().FirstOrDefault(x => x.CPC_RN_NKCountry == testData.CountryCode) ?? person.Countries.AddNew();
						personCountry.FillWithValidTestData(testBusinessObjectKind, Array.Empty<PropertyDescriptor>());
						personCountry.CPC_CPN_Person = person.PK;
						personCountry.CPC_RN_NKCountry = testData.CountryCode;
						personCountry.CPC_Type = testData.CountryCode + "1";
						personCountry.CPC_Value = testData.CountryCode + "123";
					}
				}

				for (int containerLoop = 1; containerLoop <= numberOfContainers; containerLoop++)
				{
					var container = (TContainer)manifestHeader.Containers.AddNew();
					container.FillWithValidTestData(testBusinessObjectKind, Array.Empty<PropertyDescriptor>());
					container.ACN_AMA_Manifest = manifestHeader.PK;
					container.ACN_ClusterKey = clusterKey;
					container.ACN_SealType1 = ZString.Empty;
					container.ACN_SealType2 = ZString.Empty;
					container.ACN_SealType3 = ZString.Empty;
					container.ACN_SetPointTemperature = ZDecimal.Zero;
					container.ACN_SetPointTemperatureUnit = ZString.Empty;
					container.ACN_Seal1UnloadingState = ZString.Empty;
					container.ACN_Seal2UnloadingState = ZString.Empty;
					container.ACN_Seal3UnloadingState = ZString.Empty;
					DecorateContainer(testData, container, containerLoop);
				}

				for (int billLoop = 1; billLoop <= numberOfBills; billLoop++)
				{
					var bill = (TBill)manifestHeader.Bills.AddNew();
					bill.FillWithValidTestData(testBusinessObjectKind, Array.Empty<PropertyDescriptor>());
					bill.ABL_AMA = manifestHeader.PK;
					bill.ABL_ClusterKey = clusterKey;
					DecorateBill(testData, bill, billLoop);

					for (int packLoop = 0; packLoop < numberOfPacks; packLoop++)
					{
						var pack = (TPack)bill.Packs.AddNew();
						pack.FillWithValidTestData(testBusinessObjectKind, Array.Empty<PropertyDescriptor>());
						pack.APA_ABL_Bill = bill.PK;
						pack.APA_ClusterKey = clusterKey;
						pack.APA_WeightUQ = GetData(testData.weight, packLoop);
						pack.APA_VolumeUQ = GetData(testData.volume, packLoop);
						var packItemCountry = (TPackedItem)pack.PackedItemForTesting();
						packItemCountry.FillWithValidTestData(testBusinessObjectKind, Array.Empty<PropertyDescriptor>());
						// API_ABL_Bill is cleared and set again to recalculate API_LineNo after it was incorrectly set by FillWithValidTestData
						packItemCountry.API_ABL_Bill = ZGuid.Empty;
						packItemCountry.API_ABL_Bill = bill.PK;
						packItemCountry.API_ClusterKey = clusterKey;
						DecoratePackedItem(testData, packItemCountry, billLoop, packLoop);
					}
				}
			}
			return manifestHeader;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var company = GlbCompany.CurrentCompany;
			var currentCountry = company.GC_RN_NKCountryCode;
			company.Reload();
			company.GC_RN_NKCountryCode = currentCountry;
			company.SetCountry(currentCountry);
			company.Factory.Save();
		}

		protected virtual void DecorateContainer(TestData testData, TContainer container, int containerLoop)
		{
			container.ACN_ContainerNumber = "CONT" + containerLoop.ToString();
			container.ACN_Seal1 = "SEAL" + containerLoop.ToString();
			container.ACN_GoodsWeightUQ = GetData(testData.weight, containerLoop);
		}

		protected virtual void DecoratePack(TestData countryData, TPack pack, int billLoop, int packLoop)
		{
		}

		protected virtual void DecoratePackedItem(TestData testData, TPackedItem packedItem, int billLoop, int packLoop)
		{
			packedItem.API_PackStatus = new ZString(packLoop.ToString() + testData.CountryCode).Left(3);
			packedItem.API_MessageStatus = new ZString((packLoop + billLoop).ToString() + testData.CountryCode).Left(3);
			packedItem.API_Tariff = ((billLoop * 10101) + packLoop).ToString();
			var cusEntryNumber = packedItem.CustomsEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryNum = testData.CountryCode + "PCS" + packLoop.ToString();
			cusEntryNumber.CE_EntryType = testData.CountryCode + "P";
			AsycudaEntryNumberCreator.CreateOrUpdateRegistrationNumber<TAsycudaPackedItemEntryNum>(packedItem, testData.CountryCode + "PRG" + packLoop.ToString(), packedItem.CountryCode);
		}

		protected virtual void DecorateBill(TestData testData, TBill bill, int billLoop)
		{
			bill.ABL_BillNumber = "HB" + ZDateTime.Now.ToString(DateFormat);
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			bill.ABL_BillIssueDate = ZDate.Today;
			var index = billLoop;
			bill.ABL_RL_NKOrigin = GetData(testData.Ports, index++);
			bill.ABL_RL_NKFinalDestination = GetData(testData.Ports, index);
			bill.ABL_GrossWeightUQ = GetData(testData.weight, billLoop);
			bill.ABL_VolumeUQ = GetData(testData.volume, billLoop);
			if (billLoop % 1 == 0)
			{
				bill.ABL_OA_Shipper = testData.Shipper.MainAddress.PK;
				SetupOrg(testData.Consignee.MainAddress, bill.ABL_OA_ConsigneeInfo, bill.ABL_ConsigneeNameInfo, bill.ABL_ConsigneeStreet1Info, bill.ABL_ConsigneeStreet2Info, bill.ABL_ConsigneeCityInfo, bill.ABL_ConsigneeStateInfo, bill.ABL_ConsigneePostcodeInfo, bill.ABL_RN_NKConsigneeCountryInfo, bill.ABL_ConsigneePhoneInfo);
				SetupOrg(testData.NotifyParty.MainAddress, bill.ABL_OA_NotifyPartyInfo, bill.ABL_NotifyPartyNameInfo, bill.ABL_NotifyPartyStreet1Info, bill.ABL_NotifyPartyStreet2Info, bill.ABL_NotifyPartyCityInfo, bill.ABL_NotifyPartyStateInfo, bill.ABL_NotifyPartyPostcodeInfo, bill.ABL_RN_NKNotifyPartyCountryInfo, bill.ABL_NotifyPartyPhoneInfo);
			}
			else
			{
				SetupOrg(testData.Shipper.MainAddress, bill.ABL_OA_ShipperInfo, bill.ABL_ShipperNameInfo, bill.ABL_ShipperStreet1Info, bill.ABL_ShipperStreet2Info, bill.ABL_ShipperCityInfo, bill.ABL_ShipperStateInfo, bill.ABL_ShipperPostcodeInfo, bill.ABL_RN_NKShipperCountryInfo);
				bill.ABL_OA_Consignee = testData.Consignee.MainAddress.PK;
				bill.ABL_OA_NotifyParty = testData.NotifyParty.MainAddress.PK;
			}
			bill.ABL_RX_NKCustomsValueCurrency = GetData(testData.currencies, billLoop);
			bill.ABL_RX_NKFreightValueCurrency = GetData(testData.currencies, billLoop + 1);
			bill.ABL_RX_NKInsuranceValueCurrency = GetData(testData.currencies, billLoop + 2);
			bill.ABL_RX_NKTransportValueCurrency = GetData(testData.currencies, billLoop + 3);

			bill.CustomsEntryNumber = testData.CountryCode + "CUS" + billLoop.ToString();
			bill.CustomsEntryNumberType = testData.CountryCode + "T";
			bill.RegistrationDate = ZDateTime.Today.AddMinutes(billLoop);
			AsycudaEntryNumberCreator.CreateOrUpdateRegistrationNumber<TABCEntryNum>(bill, testData.CountryCode + "REG" + billLoop.ToString(), bill.CountryCode);
			bill.ABL_BillStatus = new ZString(billLoop.ToString() + testData.CountryCode).Left(3);
			bill.ABL_MessageStatus = new ZString((billLoop + 1).ToString() + testData.CountryCode).Left(3);
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			bill.DutyAmount = billLoop * 110m;
			bill.TaxAmount = billLoop * 350m;
		}

		protected virtual void DecorateManifestHeader(TestData testData, THeader header)
		{
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header.AMA_MasterBill = "MB" + ZDateTime.Now.ToString(DateFormat);
			header.AMA_OA_Carrier = testData.carrier.MainAddress.PK;
			header.AMA_OA_DeconsolidateAddress = testData.deconsolidate.MainAddress.PK;
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			header.AMA_OA_ShippingAgent = testData.ShippingAgent.MainAddress.PK;
			header.AMA_RL_NKPortOfFirstArrival = testData.Ports[0];
			header.RegistrationNumber = testData.CountryCode + "REGO";
			header.RegistrationStatus = testData.CountryCode + "S";
			header.RegistrationDate = ZDateTime.Today;
		}

		protected virtual ZForm GetForm(BusinessObject bizO) => new ManifestForm((THeader)bizO);

		internal static TBizObj GetData<TBizObj>(TBizObj[] dataList, int index) => dataList[index % dataList.Length];

		void SetupOrg(IAddressDetails addressDetails, ZPropertyInfo partyPKInfo, ZPropertyInfo partyNameInfo, ZPropertyInfo partyStreet1Info, ZPropertyInfo partyStreet2Info, ZPropertyInfo partyCityInfo, ZPropertyInfo partyStateInfo, ZPropertyInfo partyPostcodeInfo, ZPropertyInfo partyCountryInfo, ZPropertyInfo partyPhoneInfo = null)
		{
			partyPKInfo.Value = partyPKInfo.DefaultValue;
			partyNameInfo.Value = addressDetails.CompanyName;
			partyStreet1Info.Value = addressDetails.AddressLine1;
			partyStreet2Info.Value = addressDetails.AddressLine2;
			partyCityInfo.Value = addressDetails.City;
			partyStateInfo.Value = addressDetails.State;
			partyPostcodeInfo.Value = addressDetails.PostCode;
			partyCountryInfo.Value = addressDetails.Country;
			if (partyPhoneInfo != null)
			{
				partyPhoneInfo.Value = addressDetails.Phone;
			}
		}

		protected sealed class TestData
		{
			public TestData(ZString countryCode, BusinessObjectFactory factory)
			{
				SetupCountriesData(factory, countryCode);
				contacts = CreateContants(factory);
				carrier = CreateOrg(factory, "CARRIER");
				deconsolidate = CreateOrg(factory, "DECONSOL");
				currencies = new[]
				{
					Core.Constants.CurrencyCodes.Australia,
					Core.Constants.CurrencyCodes.Singapore,
					Core.Constants.CurrencyCodes.UnitedStates,
					Core.Constants.CurrencyCodes.NewZealand,
					Core.Constants.CurrencyCodes.SouthAfrica,
					Core.Constants.CurrencyCodes.Fiji
				};
				weight = new[]
				{
					Core.Constants.Weight.Kilograms,
					Core.Constants.Weight.Grams,
					Core.Constants.Weight.Tonnes,
					Core.Constants.Weight.Pounds,
					Core.Constants.Weight.Ounces,
					Core.Constants.Weight.Milligrams
				};
				volume = new[]
				{
					Core.Constants.Volume.CubicMetres,
					Core.Constants.Volume.CubicCentimeters,
					Core.Constants.Volume.CubicDecimetres,
					Core.Constants.Volume.CubicFeet,
					Core.Constants.Volume.CubicInches,
					Core.Constants.Volume.CubicYards
				};
				SetupZZData(factory);
				factory.Save();
			}

			internal readonly OrgHeader carrier;
			internal readonly OrgHeader deconsolidate;
			public ZString CountryCode;
			public ZString ManifestType;
			public string[] Ports;
			public OrgHeader ShippingAgent;
			public OrgHeader Shipper;
			public OrgHeader Consignee;
			public OrgHeader NotifyParty;
			internal readonly OrgContact[] contacts;
			internal readonly string[] currencies;
			internal readonly string[] weight;
			internal readonly string[] volume;

			void SetupCountriesData(BusinessObjectFactory factory, ZString countryCode)
			{
				switch (countryCode)
				{
					case Core.Constants.CountryCodes.SouthAfrica:
						CreateCountryData(factory, Core.Constants.CountryCodes.SouthAfrica, "HAB", SouthAfricaPorts);
						break;
					case Core.Constants.CountryCodes.Singapore:
						CreateCountryData(factory, Core.Constants.CountryCodes.Singapore, "MGI", SingaporePorts);
						break;
					case Core.Constants.CountryCodes.Vanuatu:
						CreateCountryData(factory, Core.Constants.CountryCodes.Vanuatu, "ASY", VanuatuPorts);
						break;
					case Core.Constants.CountryCodes.Fiji:
						CreateCountryData(factory, Core.Constants.CountryCodes.Fiji, "ASY", FijiPorts);
						break;
					case Core.Constants.CountryCodes.Bangladesh:
						CreateCountryData(factory, Core.Constants.CountryCodes.Bangladesh, "ASY", BangladeshPorts);
						break;
					case Core.Constants.CountryCodes.PapuaNewGuinea:
						CreateCountryData(factory, Core.Constants.CountryCodes.PapuaNewGuinea, "ASY", PapuaNewGuineaPorts);
						break;
					case Core.Constants.CountryCodes.SolomonIslands:
						CreateCountryData(factory, Core.Constants.CountryCodes.SolomonIslands, "ASY", SolomonIslandsPorts);
						break;
					case Core.Constants.CountryCodes.SriLanka:
						CreateCountryData(factory, Core.Constants.CountryCodes.SriLanka, "ASY", SriLankaPorts);
						break;
					case Core.Constants.CountryCodes.UnitedStates:
						CreateCountryData(factory, Core.Constants.CountryCodes.UnitedStates, "IAM", UnitedStatesPorts);
						break;
					default:
						CreateCountryData(factory, Core.Constants.CountryCodes.Eritrea, "ASY", EritreaPorts);
						break;
				}
			}

			void CreateCountryData(BusinessObjectFactory factory, string countryCode, string manifestType, string[] ports)
			{
				CountryCode = countryCode;
				ManifestType = manifestType;
				Ports = ports;
				ShippingAgent = CreateOrg(factory, countryCode + "SHIPAGT");
				Shipper = CreateOrg(factory, countryCode + "SHIPPER");
				Consignee = CreateOrg(factory, countryCode + "CONSIGNEE");
				NotifyParty = CreateOrg(factory, countryCode + "NOTIFY");
			}

			internal readonly ZString[] countries = new ZString[]
			{
				Core.Constants.CountryCodes.SouthAfrica,
				Core.Constants.CountryCodes.Singapore,
				Core.Constants.CountryCodes.Vanuatu,
				Core.Constants.CountryCodes.Fiji,
				Core.Constants.CountryCodes.Bangladesh,
				Core.Constants.CountryCodes.PapuaNewGuinea,
				Core.Constants.CountryCodes.SolomonIslands,
				Core.Constants.CountryCodes.SriLanka,
				Core.Constants.CountryCodes.Eritrea
			};

			internal readonly string[] FijiPorts = new[]
			{
				"FJBFJ",
				"FJCST",
				"FJELL",
				"FJICI",
				"FJLTK",
				"FJMAL"
			};

			internal readonly string[] EritreaPorts = new[]
			{
				"ERASA",
				"ERASM",
				"ERMSW",
				"ERTES"
			};

			internal readonly string[] SolomonIslandsPorts = new[]
			{
				"SBAFT",
				"SBALB",
				"SBAOB",
				"SBATD",
				"SBBAS",
				"SBBPF"
			};

			internal readonly string[] SriLankaPorts = new[]
			{
				"LKADP",
				"LKBJT",
				"LKBRW",
				"LKBTC",
				"LKGAL",
				"LKGOY"
			};

			internal readonly string[] SouthAfricaPorts = new[]
			{
				"ZAALJ",
				"ZABFN",
				"ZACDO",
				"ZACTP",
				"ZADUK",
				"ZADUR"
			};

			internal readonly string[] SingaporePorts = new[]
			{
				"SGPPT",
				"SGAYC",
				"SGJUR",
				"SGPAP",
				"SGQPG",
				"SGSCT"
			};

			internal readonly string[] VanuatuPorts = new[]
			{
				"VUAUY",
				"VUCCV",
				"VUEAE",
				"VUFTA",
				"VUIPA",
				"VULNB"
			};

			internal readonly string[] PapuaNewGuineaPorts = new[]
			{
				"PGAUP",
				"PGAWB",
				"PGAYU",
				"PGBAA",
				"PGBAP",
				"PGBCP",
				"PGBDZ"
			};

			internal readonly string[] BangladeshPorts = new[]
			{
				"BDMUN",
				"BDASJ",
				"BDBZL",
				"BDCHL",
				"BDCXB",
				"BDIRD"
			};

			internal readonly string[] UnitedStatesPorts = new[]
			{
				"USCHI",
				"USLAX",
				"USNYC",
				"USPHL",
				"USSAN",
				"USMKC"
			};

			internal readonly string[] StaffCertificateTypes = new[]
			{
				StaffCertificateType.PAS,
				StaffCertificateType.IATA,
				StaffCertificateType.DG,
				StaffCertificateType.NID
			};

			OrgHeader CreateOrg(BusinessObjectFactory factory, ZString code)
			{
				var org = factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = code;
				org.OH_IsShippingLine = true;
				org.OH_IsConsignee = true;
				org.OH_IsConsignor = true;
				org.MainAddress.OA_Address1 = code + " ADD1";
				return org;
			}

			void SetupZZData(BusinessObjectFactory factory)
			{
				ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var helper = new UniversalReferenceTestDataHelper(factory);
				helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.AdditionalInformation, "AdditionalInformation", Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
				helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice", Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
				helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsStatus, "CustomsStatus", Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
				helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.GoodsType, "GoodsType", Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
				helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Facilities, "Facilities", Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
				helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.NVC, "NVC", Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
				helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ZADocumentType, "ZADocumentType", Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
				helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry", Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
				helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestMessageStatus, "ManifestMessageStatus", Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
				helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule", Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);

				if (CountryCode != CountryCodes.Eritrea)
				{
					var country = CountryCode;
					var ports = Ports;
					helper.CreateNewOrGetExistingCusCodeList(Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, RefCusCodeListTypes.Codes.ManifestCountry, country, country + " Country", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
					var customOffice = helper.CreateNewOrGetExistingCusCodeList(country, RefCusCodeListTypes.Codes.CustomsOffice, country + "COF", country + " Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
					helper.CreateNewOrGetExistingCusCodeListAttribute(customOffice.PK, "SEA", ports[0]);
					switch (country)
					{
						case Core.Constants.CountryCodes.Singapore:
							SetupSGRefData(factory, helper, country);
							break;
						case Core.Constants.CountryCodes.UnitedStates:
							SetupUSRefData(factory, helper, country);
							break;
						default:
							break;
					}
				}
			}

			static void SetupUSRefData(BusinessObjectFactory factory, UniversalReferenceTestDataHelper helper, ZString country)
			{
				var us = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.UnitedStates, "United States", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(us.PK, RefCusCodeListTypes.Codes.NVC, "17.3.29.1");

				var usPortOfFirstArrival = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.PortOfFirstArrival, "A Port Of First Arrival is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateTransportModeForCusCodeList(usPortOfFirstArrival.PK, Core.Constants.TransportModes.Air);

				var usETA = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.EstimatedTimeOfArrivalAtBorder, "An Estimated Time of Arrival at Border is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateTransportModeForCusCodeList(usETA.PK, Core.Constants.TransportModes.Air);

				var usGoodsDescription = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.GoodsDescription, "A Goods Description is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(usGoodsDescription.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);

				var usCarrierCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.CarrierCode, "A Carrier Code is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateTransportModeForCusCodeList(usCarrierCode.PK, Core.Constants.TransportModes.Air);

				var usConsignee = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.Consignee, "Consignee''s detail is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(usConsignee.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);

				var usConsigneeState = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.ConsigneeState, "Consignee''s state is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(usConsigneeState.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);

				var usConsigneePhone = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.ConsigneePhone, "Consignee''s phone is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(usConsigneePhone.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);

				var usConsignor = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.Consignor, "Shipper''s detail is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(usConsignor.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);

				var usConsignorState = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.ConsignorState, "Shipper''s state is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(usConsignorState.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);

				var usIATAPortOfOrigin = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.IATAPortOfOrigin, "IATA Code is required for Port of Origin", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(usIATAPortOfOrigin.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);

				var usIATAPortOfFirstArrival = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.IATAPortOfFirstArrival, "IATA Code is required for Port of First Arrival", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(usIATAPortOfFirstArrival.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);

				var usIATAPortOfDischarge = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.IATAPortOfDischarge, "IATA Code is required for Port of Discharge", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(usIATAPortOfDischarge.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);

				var usEstimatedDepartureTime = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.EstimatedDepartureTime, "(Optional)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(usEstimatedDepartureTime.PK, ManifestValidationRuleCodes.Optional, ZString.Empty);

				var usFinalDestination = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.FinalDestination, "(Optional)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(usFinalDestination.PK, ManifestValidationRuleCodes.Optional, ZString.Empty);

				var usOfficeCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.OfficeCode, "(Optional)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(usOfficeCode.PK, ManifestValidationRuleCodes.Optional, ZString.Empty);

				var usNature = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.Nature, "(Optional)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(usNature.PK, ManifestValidationRuleCodes.Optional, ZString.Empty);

				var usShipmentType = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.ShipmentType, "(Optional)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(usShipmentType.PK, ManifestValidationRuleCodes.Optional, ZString.Empty);
			}

			static void SetupSGRefData(BusinessObjectFactory factory, UniversalReferenceTestDataHelper helper, ZString country)
			{
				var officeCodeValidationRule = helper.CreateNewOrGetExistingCusCodeList(country, RefCusCodeListTypes.Codes.ManifestValidationRule, "OfficeCode", "(Optional)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(officeCodeValidationRule.PK, "OPTIONAL", "");

				var orgProxyValidationRule = helper.CreateNewOrGetExistingCusCodeList(country, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.OrgProxy, "OrgProxy", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(orgProxyValidationRule.PK, "MANDATORYCUSCODE", "UEN");
				factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy).CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "SGUEN00001", country);
			}

			OrgContact[] CreateContants(BusinessObjectFactory factory)
			{
				return new[]
				{
					CreateContant(factory, "BOB", 342234),
					CreateContant(factory, "JOE", 740234),
					CreateContant(factory, "MARY", 943523),
					CreateContant(factory, "JOHN", 583455),
					CreateContant(factory, "BRETT", 408324),
					CreateContant(factory, "JACK", 843435)
				};
			}

			OrgContact CreateContant(BusinessObjectFactory factory, ZString name, int passportNumber)
			{
				var contact = factory.NewWithValidTestData<OrgContact>();
				contact.OC_ContactName = name;
				var pp = contact.Certificates.AddNew();
				pp.XZ_Type = GetData(StaffCertificateTypes, passportNumber);
				pp.XZ_RefNumber = passportNumber.ToString();
				pp.XZ_RN_NKCountryOfIssuance = GetData(countries, passportNumber);
				return contact;
			}
		}
	}
}
