using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.ES.Business.Testing
{
	public abstract class WrapperHelperTest<T> : Customs.Business.Testing.DataProviderTestCase<T>
		where T : class
	{
		public WrapperHelperTest()
		{
		}

		public WrapperHelperTest(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		protected override BusinessObjectFactory NewFactory()
		{
			return factory ?? base.NewFactory();
		}

		public struct OrgHeaderData
		{
			public const string Id = "ORG22222222";
			public const string IdType = OrgCusCode.SpainCodeTypes.NIF;
			public const string Code = "ORGTEST";
			public const string Name = "OrgHeader Test";
			public const string Address = "1234 Test Street";
			public const string City = "Barcelona";
			public const string PostCode = "98765";
			public const string Country = "ES";
			public const string DeclarantType = ESRepresentationTypeList.Codes._4DirectATC;
			public const string DeclarantTypeToMap1 = EU.Business.RepresentationTypeList.Codes._1Self;
			public const string DeclarantTypeMapped1 = ESRepresentationTypeList.Codes._1Auto;
			public const string DeclarantTypeToMap2 = EU.Business.RepresentationTypeList.Codes._2Direct;
			public const string DeclarantTypeMapped2 = ESRepresentationTypeList.Codes._2Direct;
			public const string DeclarantTypeToMap3 = EU.Business.RepresentationTypeList.Codes._3Indirect;
			public const string DeclarantTypeMapped3 = ESRepresentationTypeList.Codes._3Indirect;
			public const string Email = "mail.mail@mail.com";
		}

		[CodeAlive("This struct is used in the wrapper tests")]
		public struct SupportingDocumentData
		{
			public const string Code = "X001";
			public const string Reference = "ES3600000001";
			public const decimal Quantity = 1234.56M;
			public const string QtyUnitCW1 = "KGM";
			public const string QtyUnitCustoms = "KN";
			public const string QtyUnitNotMapped = "DTN";
			public const string DateOfExpiry = "20190820";
			public const string DateOfIssue = "20190810";
		}

		public static List<(string Code, string Desc)> AddInfos = new List<(string Code, string Desc)>
		{
			("9001", "9001 Desc"),
			("9002", "9002 Desc"),
			("9003", "9003 Desc"),
			("9004", "9004 Desc"),
			("9005", "9005 Desc"),
			("9006", "9006 Desc"),
			("9007", "9007 Desc")
		};

		public static ZString[] SpecialInstructionsCodes = new ZString[]
		{
			"9001", "9002", "9003"
		};

		public static ZString[] CountriesOfRouting = new ZString[]
		{
			"AU", "IN", "TR", "DE", "GB"
		};

		public static ZString[] ContainerSeals = new ZString[]
		{
			"SEAL1", "SEAL2", "SEAL3", "SEAL4", "SEAL5", "SEAL6", "SEAL7", "SEAL8", "SEAL9", "SEAL10"
		};

		public static ZString[] SecondContainerSeals = new ZString[]
		{
			"SEAL1-2", "SEAL2-2", "SEAL3-2", "SEAL4-2", "SEAL5-2", "SEAL6-2", "SEAL7-2", "SEAL8-2", "SEAL9-2", "SEAL10-2"
		};

		public static List<ZString> ContainerSeals1 = new List<ZString>
		{
			"SEAL1", "SEAL2", "SEAL3", "SEAL4", "SEAL5"
		};

		public static List<ZString> ContainerSeals2 = new List<ZString>
		{
			"SEAL6", "SEAL7", "SEAL8", "SEAL9", "SEAL10"
		};

		public static ZString[] ContainerTagsWithEmpty = new ZString[]
		{
			"CONT1", ZString.Empty, "CONT2", ZString.Empty, "CONT3", ZString.Empty, "CONT4", ZString.Empty, "CONT5", ZString.Empty, "CONT6", ZString.Empty, "CONT7", ZString.Empty, "CONT8", ZString.Empty, "CONT9", ZString.Empty, "CONT10"
		};

		public static ZString[] ContainerTags = new ZString[]
		{
			"CONT1", "CONT2", "CONT3", "CONT4", "CONT5", "CONT6", "CONT7", "CONT8", "CONT9", "CONT10"
		};

		public static ZString[] GRNGuaranteesCodes = new ZString[]
		{
			"1234A1", "1234A2", "1234A3", "1234A4", "1234A5", "1234A6", "1234A7", "1234A8", "1234A9", "1234A10"
		};

		public static ZString[] GRNGuaranteesCanCodes = new ZString[]
		{
			"1234C1", "1234C2", "1234C3", "1234C4", "1234C5", "1234C6", "1234C7", "1234C8", "1234C9", "1234C10"
		};

		public struct InternalPackage1
		{
			public const string Type = "CT";
			public const string Marks = "marks";
			public const int NumberOfElements = 9;
			public const int NumberOfPackages = 9;
			public const int NumberOfPieces = 0;
		}

		public struct InternalPackage2
		{
			public const string Type = "NE";
			public const string Marks = "marks2";
			public const int NumberOfElements = 2;
			public const int NumberOfPackages = 0;
			public const int NumberOfPieces = 2;
		}

		public struct InternalPackage3
		{
			public const string Type = "FR";
			public const string Marks = "BASTIDORES";
			public const int NumberOfElements = 2;
			public const string Vin = "VINCODE";
			public const string Brand = "Brand";
			public const string Model = "Model";
		}

		public struct VehiclePackage
		{
			public const string Vin = "VINCODE";
			public const string Brand = "Brand";
			public const string Model = "Model";
		}

		public struct BorderTransportAir
		{
			public const string Mode = Core.Constants.TransportModes.Air;
			public const string ModeCoded = "4";
			public const string Name = "FlightNo";
			public const string Nationality = "ES";
		}
		public struct BorderTransportRoad
		{
			public const string Mode = Core.Constants.TransportModes.Road;
			public const string ModeCoded = "3";
			public const string Name = "TruckNo";
			public const string Nationality = "FR";
		}

		public struct NctsTransportData1
		{
			public const string Mode = "10";
			public const string Id = "Medium";
			public const string Medium = "Medium";
			public const string Name = "Medium";
			public const string Nationality = "ES";
		}
		public struct NctsTransportData2
		{
			public const string Mode = "10";
			public const string Id = "Medium Name";
			public const string Medium = "Medium";
			public const string Name = "Name";
			public const string Nationality = "FR";
		}

		[CodeAlive("This struct is used in the wrapper tests")]
		public struct HeaderData
		{
			public const string ReferenceNumber = "Reference";
			public const string MessageType = "EXP";
			public const string MessageSubType = "EX";
			public const string MessageSubTypePDI = "IM";
			public const string MessageSubTypeCO = "CO";
			public const string EntryInstructionSubStyle = "A";
			public const string EntryInstructionSubStyle2 = "B";
			public const string CTStatus = "";
			public const string ValuationCode = "7";
			public const string CustomsOffice = "ES009999";
			public const string CustomsOfficeCountry = "ES";
			public const string CustomsOfficeCode = "9999";
			public const string CustomsOfficeCodeComplete = "009999";
			public const string CountryOfExport = "ES";
			public const string CountryOfOrigin = "ES";
			public const string CountryOfDestination = "FR";
			public const string StateOfDestination = "08";
			public const string CustomsOfficeOfExit = "ES009998";
			public const string CustomsOfficeOfExitCountry = "ES";
			public const string CustomsOfficeOfExitCode = "009998";
			public const string CustomsOfficeOfExport = "FR008889";
			public const string CustomsOfficeOfExportCountry = "FR";
			public const string CustomsOfficeOfExportCode = "008889";
			public const string CustomsOfficeOfEntry = "ES009998";
			public const string LocationOfGoods = "9999000002";
			public const string LocationOfGoodsFirstFour = "9999";
			public const string LocationOfGoodsCustomsOffice = "9999";
			public const string LocationOfGoodsCode = "000002";
			public const string Warehouse = ""; // Not working with value ("ESX456789ABC")
			public const string ToWarehouseId = ""; // Not working with value ("DBNSOS78901")
			public const string FromWarehouseId = ""; // Not working with value ("DBNSOS78902")
			public const string DateOfRecap = "20190817";
			public const string ContainerMode = Core.Constants.ContainerModes.Groupage;
			public const bool ContainerIndicator = true;
			public const bool RMTIndicator = true;
			public const string TransportCharges = "A";
			public const string OwnerRef = "AAAA";
			public const string SpecificCircumstancesInd = "E";
			public const string InternalTransportMode = Core.Constants.TransportModes.Air;
			public const string InternalTransportModeCode = "4";
			public const string TransportModeId = "4456BGT";
			public const string ExporterCode = "EXPORTER";
			public const string ExporterCode2 = "EXPORTER2";
			public const string ReceiverCode = "RECEIVER";
			public const string ImporterCode = "IMPORTER";
			public const string DeclarantCode = "DECLARANT";
			public const string SenderCode = "SENDER";
			public const string ConsigneeCode = "CONSIGNEE";
			public const string TermsOfDeliveryDeclarationCode = "CIF";
			public const string DeliveryLocationDeclaration = "BARCELONA";
			public const string TermsOfDeliveryCode = "FOB";
			public const string DeliveryLocation = "MADRID";
			public const string LocationId = "1";
			public const decimal TotalAmount = 2000;
			public const string TotalAmountCurrency = "EUR";
			public const string MovementReferenceNumber = "TestMRN";
			public const string ExpeditionCustomsOffice = "ES009999";
			public const string ExpeditionCustomsOfficeCode = "009999";
			public const string ExpeditionOrigin = "ESMAD";
			public const string ExpeditionOriginCountry = "ES";
			public const string ExpeditionDestination = "FRPAR";
			public const string ExpeditionDestinationCountry = "FR";
			public const string DeclarationEmail = "mail.mail@mail.com";
			public const string OtherEmail = "other.mail@mail.com";
			public const decimal TotalTributesAmount = 1000.200M;
			public const string PaymentMode = MethodOfPaymentList.Codes.A;
			public const string GuaranteeReferenceClearance = "Clearance";
			public const string GuaranteeReferencePendencies = "Pendencies";
			public const string PaymentModeCan = MethodOfPaymentList.Codes.J;
			public const string GuaranteeReferenceClearanceCan = "ClearanceCan";
			public const string GuaranteeReferencePendenciesCan = "PendenciesCan";
		}

		[CodeAlive("This struct is used in the wrapper tests")]
		public struct EntryLineData
		{
			public const int ItemNumber = 1;
			public const string Tariff = "2203001010";
			public const string TariffShort = "22030010";
			public const string SuppCode1 = "First";
			public const string SuppCode2 = "Second";
			public const string ProcedurePart1 = "12";
			public const string ProcedurePart2 = "34";
			public const string ProcedureConcessionPart = "001";
			public const string Procedure = "1234001";
			public const string Procedure9VA = "12349VA";
			public const string CPCCode = "12.34";
			public const string AddProcedure1 = "123002";
			public const string Procedure1ConcessionPart = "002";
			public const string AddProcedure2 = "123003";
			public const string Procedure2ConcessionPart = "003";
			public const string ConcessionCodes = "001002003";
			public const string AddSupplement1 = "AA";
			public const string AddSupplement2 = "BB";
			public const string AddSupplementsCode = "AABB";
			public const string GoodsDescription = "Description1";
			public const string CountryOfOrigin = "ES";
			public const string StateOfOrigin = "01";
			public const decimal NetWeight = 0.1234M;
			public const decimal NetWeightRound = 0.123M;
			public const decimal SupplQuantity = 100.1M;
			public const string SupplQtyUnitCW1 = "KGM";
			public const string SupplQtyUnitCustoms = "KN";
			public const string SupplQtyUnitNotMapped = "DTN";
			public const decimal ThirdQuantity = 300;
			public const string ThirdQtyUnitCW1 = "KGM";
			public const string ThirdQtyUnitCustoms = "KN";
			public const string ThirdQtyUnitNotMapped = "LTR";
			public const string DangerousGoodsCode = "1001";
			public const string Vin = "VINCODE";
			public const decimal LinePrice = 1000;
			public const decimal TotalGoodValue = 1000;
			public const string PrevDocCode = "Code";
			public const string PrevDocRefNum = "Reference";
			public const string PrevDocCodeRef = "CodeReference";
			public const string PrevDocSubType = "Z";
			public const string ExciseCode = "0A0";
			public const string ExciseExemption = "0";
			public const string ReaCode = "T001";
			public const string PrimaryPreferenceCode = "123";
			public const string PrimaryPreferenceCodePref = "1";
			public const string PrimaryPreferenceCodeRed = "23";
			public const string ContingencyCode = "AAJ";
			public const decimal PosAdjustment = 33M;
			public const decimal NegAdjustment = -22M;
			public const decimal FeesTotalChargeAmount = 20.40M;
		}

		public const string MovementReferenceNumber = "20ES00999910000035";
		public const string EntryReferenceNumber = "ReferenceNum";

		public CertificateProviderTestClass Certificate
		{
			get
			{
				return certificate ?? (certificate = new StaffWithCertificateTestHelper(Factory).Certificate);
			}
		}
		CertificateProviderTestClass certificate;

		public struct SupplierData
		{
			public const string Id = "SUP22222222";
			public const string IdType = OrgCusCode.SpainCodeTypes.NIF;
			public const string Code = "SUPPLIERTEST";
			public const string Name = "Supplier Test Org";
			public const string Address = "1234 Test Street";
			public const string City = "Barcelona";
			public const string PostCode = "98765";
			public const string Country = "ES";
		}
		public struct ImporterData
		{
			public const string Id = "IMP11111111";
			public const string IdType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			public const string Code = "IMPORTERTEST";
			public const string Name = "Importer Test Org";
			public const string Address = "Test Street 1234";
			public const string City = "Madrid";
			public const string PostCode = "12345";
			public const string Country = "ES";
		}
		public struct DeclarantData
		{
			public const string Id = "DEC33333333";
			public const string IdType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			public const string Code = "DECTEST";
			public const string Name = "Declarant Test Org";
			public const string Address = "Declarant Test Street";
			public const string City = "Valencia";
			public const string PostCode = "45612";
			public const string Country = "ES";

			public const string DeclarantType = ESRepresentationTypeList.Codes._2Direct;
			public const string Email = "mail.mail@mail.com";
			public const string OtherEmail = "other.mail@mail.com";
		}

		public struct BorderTransport
		{
			public const string Mode = Core.Constants.TransportModes.Air;
			public const string ModeCoded = "4";
			public const string Name = "FlightNo";
			public const string Nationality = "ES";
		}
		[CodeAlive("This struct is used in the wrapper tests")]
		public struct SupportingDocument1
		{
			public const string Code = "X001";
			public const string Reference = "ES3600000001";
			public const decimal Quantity = 1234.56M;
			public const string QtyUnit = "KN";
			public const string DateOfExpiry = "20190816";
		}

		[CodeAlive("This struct is used in the wrapper tests")]
		public struct SupportingDocument2
		{
			public const string Code = "X002";
			public const string Reference = "ES3600000002";
			public const decimal Quantity = 76.54M;
			public const string QtyUnit = "KN";
			public const string DateOfIssue = "20190816";
		}

		[CodeAlive("This struct is used in the wrapper tests")]
		public struct EntryLine1
		{
			public const int LineNum = 1;
			public const string Tariff = "2203001010";
			public const string SuppCode1 = "First";
			public const string SuppCode2 = "Second";
			public const string Procedure = "1234001";
			public const string ProcedurePart1 = "12";
			public const string ProcedurePart2 = "34";
			public const string ProcedureConcessionPart = "001";
			public const string CPCCode = "12.34";
			public const string AddProcedure1 = "1234002";
			public const string AddProcedure2 = "1234003";
			public const string ConcessionCodes = "001002003";
			public const string AddSupplement1 = "AA";
			public const string AddSupplement2 = "BB";
			public const string AddSupplementsCode = "AABB";
			public const string GoodsDescription = "Description1";
			public const string CountryOfOrigin = "ES";
			public const string StateOfOrigin = "01";
			public const decimal GrossWeight = 200.555M;
			public const decimal GrossWeightRound = 201;
			public const decimal NetWeight = 0.1234M;
			public const decimal NetWeightRound = 0.123M;
			public const decimal SupplQuantity = 100.1M;
			public const string SupplQyUnit = "DTN";
			public const decimal ThirdQuantity = 300;
			public const string ThirdQyUnit = "LTR";
			public const string DangerousGoodsCode = "1001";
			public const decimal LinePrice = 1000;
			public const decimal TotalGoodValue = 1000;
			public const string PrevDocCode = "Code";
			public const string PrevDocRefNum = "Reference";
			public const string PrevDocCodeRef = "CodeReference";
			public const string PrevDocSubType = "Z";
		}

		[CodeAlive("This struct is used in the wrapper tests")]
		public struct EntryLine2
		{
			public const int LineNum = 2;
			public const string Tariff = "2203001011";
			public const string SuppCode1 = "First2";
			public const string SuppCode2 = "Second2";
			public const string Procedure = "98";
			public const string ProcedurePart1 = "98";
			public const string ProcedurePart2 = "76";
			public const string ProcedureConcessionPart = "004";
			public const string CPCCode = "98";
			public const string AddProcedure1 = "9876005";
			public const string AddProcedure2 = "9876006";
			public const string ConcessionCodes = "005006";
			public const string AddSupplement1 = "CC";
			public const string AddSupplement2 = "DD";
			public const string AddSupplementsCode = "CCDD";
			public const string GoodsDescription = "Description2";
			public const string CountryOfOrigin = "FR";
			public const string StateOfOrigin = "60";
			public const decimal GrossWeight = 0.9886M;
			public const decimal GrossWeightRound = 0.989M;
			public const decimal NetWeight = 0.9876M;
			public const decimal NetWeightRound = 0.988M;
			public const decimal SupplQuantity = 200.2M;
			public const string SupplQyUnit = "DTN";
			public const decimal ThirdQuantity = 200;
			public const string ThirdQyUnit = "AAA";
			public const string DangerousGoodsCode = "2002";
			public const decimal LinePrice = 1000;
			public const decimal TotalGoodValue = 1100;
		}

		[CodeAlive("This struct is used in the wrapper tests")]
		public struct Header
		{
			public const string ReferenceNumber = "Reference";
			public const string MessageType = "EXP";
			public const string MessageSubType = "EX";
			public const string EntryInstructionSubStyle = "A";
			public const string CTStatus = "";
			public const string ValuationCode = "7";
			public const string CustomsOffice = "ES009999";
			public const string CustomsOfficeCode = "9999";
			public const string CountryOfExport = "ES"; //Taken from Routing region
			public const string CountryOfDestination = "FR"; //Taken from Routing region
			public const string CustomsOfficeOfExit = "ES009998";
			public const string CustomsOfficeOfExitCountry = "ES";
			public const string CustomsOfficeOfExitCode = "009998";
			public const string LocationOfGoods = "9999000002";
			public const string LocationOfGoodsCustomsOffice = "9999";
			public const string LocationOfGoodsCode = "000002";
			public const string Warehouse = ""; // Not working with value ("ESX456789ABC")
			public const string DateOfRecap = "20190817";
			public const string ContainerMode = Core.Constants.ContainerModes.Groupage;
			public const bool ContainerIndicator = true;
			public const bool RMTIndicator = true;
			public const string TransportCharges = "A";
			public const string OwnerRef = "AAAA";
			public const string SpecificCircumstancesInd = "E";
			public const string InternalTransportMode = "AIR";
			public const string TransportModeId = " 4456BGT";
			public const string TermsOfDeliveryCode = "FOB";
			public const string DeliveryLocation = "MADRID";
			public const string LocationId = "1";
			public const decimal TotalAmount = 2000;
			public const string TotalAmountCurrency = "EUR";
			public const int TotalNumberOfGoods = 2;
			public const int TotalNumberOfPackageElements = 10;
			public const bool IsTest = false;
		}

		public struct HeaderDataNCTS
		{
			public const string LocalReferenceNumber = "NCTS00000001";
			public const string DepartureCustomsOffice = "ES009998";
			public const string DepartureCustomsOfficeCode = "9998";
			public const string DestinationCountry = "ES";
			public const string AgreedLocationOfGoodsCode = "9999AAAAAA";
			public const string LocationOfGoodsExamCustomsOffice = "9999";
			public const string LocationOfGoodsExam = "AAAAAA";
			public const string LoadingPlaceCode = "LOAD";
			public const string UnloadingPlaceCode = "ULOAD";
			public const string PaymentMethod = "A";
			public const string ConveyanceReferenceNumber = "NCTS00000002";
			public const string ReferenceNumber = "NCTS00000003";
			public const string SpecificCircumstancesIndicator = "E";
			public const string PrincipalCode = "PRINCIPAL";
			public const string ConsignorCode = "CONSIGNOR";
			public const string ConsigneeCode = "CONSIGNEE";
			public const string DeclarantCode = "DECLARANT";
			public const string SecurityCarrierCode = "SECCARRIER";
			public const string SecurityConsignorCode = "SECCONSIGNOR";
			public const string SecurityConsigneeCode = "SECCONSIGNEE";
			public const string EntryReferenceNumber = "NCTS00000001";
			public const string OriginCountry = "ES";
			public const string DestinationCustomsOffice = "ES009999";
			public const string TIRNumber = "DX00000000";
			public const string HolderCode = "HOLDER";
			public const string DeclarationType = "T2";
			public const string DepartureCountry = "ES";
			public const string NationalSimplificatorInd = "A";
		}

		[CodeAlive("This struct is used in the wrapper tests")]
		public struct GoodsItemDataNCTS
		{
			public const int ItemNumber = 1;
			public const string CommodityCode = "commodityCode";
			public const string GoodsDescription = "description";
			public const decimal GrossMassKg = 10.2M;
			public const decimal GrossMassKgRounded = 10M;
			public const decimal NetMassKg = 10.2M;
			public const decimal ThirdQuantity = 13.1M;
			public const string ThirdQtyUnit = "KG";
			public const decimal FiscalUnits = 13.1M;
			public const string FiscalUnitsUQ = "KG";
			public const string DangerousGoods = "0004a";
			public const string DangerousGoodsCode = "0004";
			public const short DocLineNo = 2;
			public const string DocLineNoString = "2";
			public const string DocClass = "AAA";
			public const string DepartureCountry = "ES";
			public const string DestinationCountry = "FR";
			public const string DeclarationType = "T2";
			public const decimal CustomsValue = 200.2M;
			public const string MethodOfPayment = "A";
			public const string GoodsCountryCode = "ES";
			public const string SecondUnitQtyNotMapped = "DTN";
			public const string SecondUnitQtyCW1 = "KGM";
			public const string SecondUnitQtyCustoms = "KN";
		}

		[CodeAlive("This struct is used in the wrapper tests")]
		public struct EntryLineFeeData
		{
			public const string ChargeType = "B00";
			public const string ChargeTypeCanary = "3IG";
			public const decimal BaseValue = 42.560M;
			public const decimal Rate = 2.700000M;
			public const string MaxMin = "MA";
			public const string MethodOfCalculationPercent = "%";
			public const string MethodOfCalculationCW1 = "KGM";
			public const string MethodOfCalculationCustoms = "KN";
			public const string MethodOfCalculationNotMapped = "AAA";
			public const string MethodOfCalculationPVP = "PVP";
			public const decimal ChargeAmount = 1.15M;
		}

		[CodeAlive("This struct is used in the wrapper tests")]
		public struct ExitSummaryData
		{
			public const string MRN = "11ES00113112683757";
			public const string ReferenceNumber = "11113112683757";
			public const string CustomsOfficeOfExit = "ES009998";
			public const string CustomsOfficeOfExitCode = "009998";
			public const string CustomsOfficeOfExitCountry = "ES";
			public const string LocationOfGoods = "ES002801000001";
			public const string LocationOfGoodsShort = "2801000001";
			public const string LocationOfGoodsCustomsOffice = "2801";
			public const string LocationOfGoodsCode = "000001";
			public const string ArrivalNotifDate = "20190817";
			public const string AgentCode = "AGENT";
			public const string CarrierCode = "CARRIER";
			public const string BrokerCode = "BRK";
			public const string BrokerMainEmail = "main@wtg.com";
			public const string BrokerExtraEmail = "extra@wtg.com";
		}

		protected void SetSupportingDocumentsRefData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "N380", "N380 DESC", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "N325", "N325 DESC", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "D005", "D005 DESC", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "D008", "D008 DESC", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "N935", "N935 DESC", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "1001", "1001 DESC", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "1003", "1003 DESC", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "1004", "1004 DESC", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "AAA", "AAA DESC", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}

		protected SupportingDocument GetSupportingDoc(ZString code, ZString refNumber)
		{
			var supDoc = Factory.CreateSupportingDocument(code, refNumber);
			supDoc.CSI_SubType = "A";
			supDoc.CSI_Quantity = 10;

			return supDoc;
		}

		protected IDisposable TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(JobDeclaration declaration, bool configurationValue)
		{
			ConfigurationTestHelper.ClearDeclarationConfiguration(declaration);
			return ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(declaration.Factory, "IsTransitionPeriodAES30Core", configurationValue, declaration.GetDefaultDataGroupingCode());
		}

		protected static void AddAdditionalInfos(AdditionalInfoCollection collection, params (string Code, string SubType)[] infos)
		{
			foreach (var (code, subType) in infos)
			{
				var item = collection.AddNew();
				item.CSI_Code = code;
				item.CSI_SubType = subType;
			}
		}

		protected static void AddPreviousDocuments(PreviousDocumentCollection collection, params string[] codes)
		{
			foreach (var code in codes)
			{
				var item = collection.AddNew();
				item.CSI_Code = code;
			}
		}

		protected static void AddSupportingDocuments(SupportingDocumentCollection collection, params string[] codes)
		{
			foreach (var code in codes)
			{
				var item = collection.AddNew();
				item.CSI_Code = code;
			}
		}
	}
}
