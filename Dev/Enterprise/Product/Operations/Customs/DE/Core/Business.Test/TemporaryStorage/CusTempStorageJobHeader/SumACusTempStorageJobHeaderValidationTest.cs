using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class SumACusTempStorageJobHeaderValidationTest : CusTempStorageJobHeaderValidationAbstractTest<CusTempStorageJobHeader>
	{
		public void TestCheckSJH_TransportMeansDescription()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			var humanReadableName = header.SJH_TransportMeansDescriptionInfo.GetHumanReadableName();
			header.Validation.ValidateSJH_TransportMeansDescription();
			AssertNoMessageErrors(header.SJH_TransportMeansDescriptionInfo);

			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Other;
			header.Validation.ValidateSJH_TransportMeansDescription();
			AssertHasMessageErrorContaining(header.SJH_TransportMeansDescriptionInfo, MandatoryValidation.YouHaveNotEnteredMessage(humanReadableName));
			header.SJH_TransportMeansDescription = "DESCRIPTION";
			header.Validation.ValidateSJH_TransportMeansDescription();
			AssertNoMessageErrorContaining(header.SJH_TransportMeansDescriptionInfo, MandatoryValidation.YouHaveNotEnteredMessage(humanReadableName));
		}

		public void TestCheckSJH_ContainerCount()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			var msg = "Container count should be between 0 and 9999.";

			header.SJH_ContainerCount = 9999;
			header.Validation.ValidateSJH_ContainerCount();
			AssertNoMessageErrorContaining(header.SJH_ContainerCountInfo, msg);

			header.SJH_ContainerCount = 10000;
			header.Validation.ValidateSJH_ContainerCount();
			AssertHasMessageErrorContaining(header.SJH_ContainerCountInfo, msg);
		}

		public void TestCheckSJH_CustomsOfficeOfEntryIntoEU()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			var humandReadableName = header.SJH_CustomsOfficeOfEntryIntoEUInfo.GetHumanReadableName();
			SetupCustomsOffices();
			header.Validation.ValidateSJH_CustomsOfficeOfEntryIntoEU();
			AssertNoMessageError(header.SJH_CustomsOfficeOfEntryIntoEUInfo, MandatoryValidation.YouHaveNotEnteredMessage(humandReadableName));

			header.SJH_PreviousReferenceType = PreviousReferenceType.Codes._ESUMA;
			header.Validation.ValidateSJH_CustomsOfficeOfEntryIntoEU();
			AssertHasMessageError(header.SJH_CustomsOfficeOfEntryIntoEUInfo, MandatoryValidation.YouHaveNotEnteredMessage(humandReadableName));

			header.SJH_PreviousReferenceType = PreviousReferenceType.Codes._ENST2L;
			header.Validation.ValidateSJH_CustomsOfficeOfEntryIntoEU();
			AssertHasMessageError(header.SJH_CustomsOfficeOfEntryIntoEUInfo, MandatoryValidation.YouHaveNotEnteredMessage(humandReadableName));

			header.SJH_CustomsOfficeOfEntryIntoEU = "GB1";
			AssertNoMessageError(header.SJH_CustomsOfficeOfEntryIntoEUInfo, ListValidation.InvalidCodeMessageError);
			header.SJH_CustomsOfficeOfEntryIntoEU = "DE";
			AssertHasMessageError(header.SJH_CustomsOfficeOfEntryIntoEUInfo, ListValidation.InvalidCodeMessageError);
			header.SJH_CustomsOfficeOfEntryIntoEU = "DE1";
			AssertNoMessageErrors(header.SJH_CustomsOfficeOfEntryIntoEUInfo);
		}

		public void TestCheckSJH_PreviousReferenceType()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			header.SJH_PreviousReferenceType = ZString.Empty;
			AssertHasMessageError(header.SJH_PreviousReferenceTypeInfo, MandatoryValidation.YouHaveNotEnteredMessage(header.SJH_PreviousReferenceTypeInfo.GetHumanReadableName()));
			header.SJH_PreviousReferenceType = "BLAH";
			header.Validation.ValidateSJH_PreviousReferenceType();
			AssertHasMessageErrorContaining(header.SJH_PreviousReferenceTypeInfo, ListValidation.InvalidCodeMessageError);
			header.SJH_PreviousReferenceType = PreviousReferenceType.Codes._444T1;
			header.Validation.ValidateSJH_PreviousReferenceType();
			AssertNoMessageErrorContaining(header.SJH_PreviousReferenceTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckSJH_PreviousReferenceNumber_BlankAllowed()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			var previousReferenceTypesAllowingBlankReferenceNumber = new string[] {
				PreviousReferenceType.Codes._OHNE
				, PreviousReferenceType.Codes._ESUMA
				, PreviousReferenceType.Codes._ENST2L
				, PreviousReferenceType.Codes._FREIZ
				, PreviousReferenceType.Codes._199
				, PreviousReferenceType.Codes._200
				, PreviousReferenceType.Codes._444T1
				, PreviousReferenceType.Codes._444T2
				, PreviousReferenceType.Codes._444TF
				, PreviousReferenceType.Codes._447T1
				, PreviousReferenceType.Codes._447T2
				, PreviousReferenceType.Codes._447TF
				, PreviousReferenceType.Codes._POUS
				, PreviousReferenceType.Codes._N355
			};

			CombineAssertions(() =>
			{
				foreach (var previousRefType in previousReferenceTypesAllowingBlankReferenceNumber)
				{
					header.SJH_PreviousReferenceType = previousRefType;
					ValidationTestHelper.AssertFieldIsNotMandatory(header.SJH_PreviousReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered, $"SJH_PreviousReferenceType = {previousRefType}");
				}
			});
		}

		public void TestCheckSJH_PreviousReferenceNumber_MandatoryBlank()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			var previousReferenceTypesRequiringBlank = new string[]
			{
				PreviousReferenceType.Codes._OHNE
				, PreviousReferenceType.Codes._ENST2L
				, PreviousReferenceType.Codes._FREIZ
				, PreviousReferenceType.Codes._199
				, PreviousReferenceType.Codes._200
				, PreviousReferenceType.Codes._PVV
			};

			CombineAssertions(() =>
			{
				foreach (var previousRefType in previousReferenceTypesRequiringBlank)
				{
					header.SJH_PreviousReferenceType = previousRefType;
					ValidationTestHelper.AssertIfIsEnteredMessageError(header.SJH_PreviousReferenceNumberInfo, $"Previous Ref. Number must be blank for Previous Reference Type '{previousRefType}'.", $"SJH_PreviousReferenceType = {previousRefType}");
				}
			});
		}

		public void TestCheckSJH_PreviousReferenceNumber_Mandatory()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			var previousReferenceTypesRequiringReferenceNumber = new string[]
			{
				PreviousReferenceType.Codes._A
				, PreviousReferenceType.Codes._AE
				, PreviousReferenceType.Codes._ATA
				, PreviousReferenceType.Codes._AV
				, PreviousReferenceType.Codes._FV
				, PreviousReferenceType.Codes._MAN
				, PreviousReferenceType.Codes._MO
				, PreviousReferenceType.Codes._OESUMA
				, PreviousReferenceType.Codes._T
				, PreviousReferenceType.Codes._T1
				, PreviousReferenceType.Codes._T1CF
				, PreviousReferenceType.Codes._T1DF
				, PreviousReferenceType.Codes._T1IC
				, PreviousReferenceType.Codes._T1IE
				, PreviousReferenceType.Codes._T1IF
				, PreviousReferenceType.Codes._T2
				, PreviousReferenceType.Codes._T2AN
				, PreviousReferenceType.Codes._T2CF
				, PreviousReferenceType.Codes._T2DF
				, PreviousReferenceType.Codes._T2F
				, PreviousReferenceType.Codes._T2IC
				, PreviousReferenceType.Codes._T2IE
				, PreviousReferenceType.Codes._T2IF
				, PreviousReferenceType.Codes._T2L
				, PreviousReferenceType.Codes._T2LF
				, PreviousReferenceType.Codes._T2M
				, PreviousReferenceType.Codes._T2SM
				, PreviousReferenceType.Codes._T5
				, PreviousReferenceType.Codes._TIR
				, PreviousReferenceType.Codes._TRPPVW
				, PreviousReferenceType.Codes._V
				, PreviousReferenceType.Codes._VV
				, PreviousReferenceType.Codes._Z
				, PreviousReferenceType.Codes._ZL
			};

			CombineAssertions(() =>
			{
				foreach (var previousRefType in previousReferenceTypesRequiringReferenceNumber)
				{
					header.SJH_PreviousReferenceType = previousRefType;
					ValidationTestHelper.AssertYouHaveNotEnteredMessageError(header.SJH_PreviousReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered, $"SJH_PreviousReferenceType = {previousRefType}");
				}
			});
		}

		public void TestCheckSJH_PreviousReferenceNumber_PreviousReferenceTypeRequiresMRNFormat()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			var previousReferenceTypesRequiringMRNFormat = new string[]
			{
				PreviousReferenceType.Codes._ESUMA
				, PreviousReferenceType.Codes._N355
				, PreviousReferenceType.Codes._POUS
			};

			CombineAssertions(() =>
			{
				foreach (var previousRefType in previousReferenceTypesRequiringMRNFormat)
				{
					header.SJH_PreviousReferenceType = previousRefType;
					header.SJH_PreviousReferenceNumber = "123456";
					AssertHasMessageErrorContaining("Not MRN Format. Previous Reference Type: " + previousRefType, header.SJH_PreviousReferenceNumberInfo, RequiresMRNFormatMessageError);
					header.SJH_PreviousReferenceNumber = "11DE11111111111115";
					AssertNoMessageErrorContaining("MRN Format. Previous Reference Type: " + previousRefType, header.SJH_PreviousReferenceNumberInfo, RequiresMRNFormatMessageError);
				}
			});
		}

		public void TestCheckSJH_PreviousReferenceNumber_NCTSFlagRequiresMRNFormat()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			CombineAssertions(() =>
			{
				header.SJH_PreviousReferenceType = PreviousReferenceType.Codes._AV;
				header.SJH_PreviousReferenceNumber = "123456";
				AssertNoMessageErrorContaining("NCTS Flag false, not MRN Format", header.SJH_PreviousReferenceNumberInfo, RequiresMRNFormatMessageError);

				header.SJH_NCTSFlag = true;
				header.Validation.ValidateSJH_PreviousReferenceNumber();
				AssertHasMessageErrorContaining("NCTS Flag true, not MRN Format", header.SJH_PreviousReferenceNumberInfo, RequiresMRNFormatMessageError);

				header.SJH_PreviousReferenceNumber = "11DE11111111111115";
				AssertNoMessageErrorContaining("NCTS Flag true, MRN Format", header.SJH_PreviousReferenceNumberInfo, RequiresMRNFormatMessageError);
			});
		}

		public void TestCheckSJH_PreviousReferenceNumber_CountryCodeShouldMatchEntryCustomsOfficeCountryCode()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			header.SJH_NCTSFlag = false;
			header.SJH_PreviousReferenceType = PreviousReferenceType.Codes._ESUMA;

			CombineAssertions(() =>
			{
				var messageError = "When Previous Reference Type is ESUMA and Entry Customs Office does not start with AT, BG, CY, CZ, DK, LV, PT, RO or SK, then the Previous Reference Number's country/region code (US) should match the Entry Customs Office country/region code (A).";
				header.SJH_CustomsOfficeOfEntryIntoEU = "A";
				header.SJH_PreviousReferenceNumber = "11US11111111111110";
				AssertHasMessageError("CusOffCountryCode A, RefNumCountryCode US", header.SJH_PreviousReferenceNumberInfo, messageError);

				header.SJH_CustomsOfficeOfEntryIntoEU = "AT1";
				header.SJH_PreviousReferenceNumber = "11DE11111111111115";
				AssertNoMessageErrors("CusOffCountryCode AT1, RefNumCountryCode DE", header.SJH_PreviousReferenceNumberInfo);

				messageError = "When Previous Reference Type is ESUMA and Entry Customs Office does not start with AT, BG, CY, CZ, DK, LV, PT, RO or SK, then the Previous Reference Number's country/region code (AU) should match the Entry Customs Office country/region code (IT).";
				header.SJH_CustomsOfficeOfEntryIntoEU = "IT1";
				header.SJH_PreviousReferenceNumber = "11AU11111111111114";
				AssertHasMessageError("CusOffCountryCode IT1, RefNumCountryCode AU", header.SJH_PreviousReferenceNumberInfo, messageError);

				header.SJH_PreviousReferenceNumber = "11IT11111111111110";
				AssertNoMessageErrors("CusOffCountryCode IT1, RefNumCiuntryCode IT", header.SJH_PreviousReferenceNumberInfo);
			});
		}

		public void TestCheckSJH_ArrivalDate()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			var humandReadableName = header.SJH_ArrivalDateInfo.HumanReadableName;
			header.SJH_TransportMeansCode = ZString.Empty;
			header.Validation.ValidateSJH_ArrivalDate();
			AssertNoMessageError(header.SJH_ArrivalDateInfo, MandatoryValidation.YouHaveNotEnteredMessage(humandReadableName));
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Vessel;
			header.Validation.ValidateSJH_ArrivalDate();
			AssertHasMessageError(header.SJH_ArrivalDateInfo, MandatoryValidation.YouHaveNotEnteredMessage(humandReadableName));
			header.SJH_ArrivalDate = ZDate.Today;
			AssertNoMessageError(header.SJH_ArrivalDateInfo, MandatoryValidation.YouHaveNotEnteredMessage(humandReadableName));
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Aircraft;
			header.SJH_ArrivalDate = ZDate.Empty;
			AssertHasMessageError(header.SJH_ArrivalDateInfo, MandatoryValidation.YouHaveNotEnteredMessage(humandReadableName));
			header.SJH_ArrivalDate = ZDate.Today;
			AssertNoMessageError(header.SJH_ArrivalDateInfo, MandatoryValidation.YouHaveNotEnteredMessage(humandReadableName));
			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Other;
			header.SJH_ArrivalDate = ZDate.Empty;
			AssertNoMessageError(header.SJH_ArrivalDateInfo, MandatoryValidation.YouHaveNotEnteredMessage(humandReadableName));
		}

		public void TestCheckSJH_PresentationDate()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			var humandReadableName = header.SJH_PresentationDateInfo.HumanReadableName;
			header.SJH_PresentationDate = ZDate.Empty;
			AssertHasMessageError(header.SJH_PresentationDateInfo, MandatoryValidation.YouHaveNotEnteredMessage(humandReadableName));
			header.SJH_PresentationDate = ZDate.Today;
			AssertNoMessageError(header.SJH_PresentationDateInfo, MandatoryValidation.YouHaveNotEnteredMessage(humandReadableName));
		}

		public void TestCheckSJH_DepartureDate()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			header.SJH_DepartureDate = ZDate.Today;
			AssertNoMessageErrors(header.SJH_DepartureDateInfo);
			header.SJH_DepartureDate = ZDate.Empty;
			AssertNoMessageErrors(header.SJH_DepartureDateInfo);
		}

		public void TestCheckSJH_OA_Presenter_EORINumberIsMandatoryIfPrevReferenceTypeIsN355()
		{
			const string messageError = "Presenter must have a Registration Number / Code of Type 'EOR'.";
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			var header = GetCusTempStorageJobHeaderToTest();
			var presenter = Factory.GetOrgHeaderWithEori("SUMATEST", "12345", "GR");
			header.SJH_OA_Presenter = presenter.MainAddress.PK;

			CombineAssertions(() =>
			{
				var info = header.SJH_OA_PresenterInfo;
				header.SJH_PreviousReferenceType = "N355";
				header.Validation.ValidateSJH_OA_Presenter();
				AssertNoMessageError("Has EORI, is N355", info, messageError);

				presenter.DeleteSingleEORINumber();
				header.Validation.ValidateSJH_OA_Presenter();
				AssertHasMessageError("No EORI, is N355", info, messageError);

				header.SJH_PreviousReferenceType = "ESUMA";
				header.Validation.ValidateSJH_OA_Presenter();
				AssertNoMessageError("No EORI, not N355", info, messageError);
			});
		}

		public void TestCheckSJH_TransportMeansCode_Mandatory()
		{
			CombineAssertions(() =>
			{
				var header = GetCusTempStorageJobHeaderToTest();
				ValidationTestHelper.AssertFieldIsNotMandatory(header.SJH_TransportMeansCodeInfo);

				CUSPRLCusTempStorageDec.New(header);
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(header.SJH_TransportMeansCodeInfo);
			});
		}

		protected override CusTempStorageJobHeader GetCusTempStorageJobHeaderToTest()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.SJH_AppCode = TemporaryStorageApplicationCodeList.Codes.SumA;
			return header;
		}

		const string RequiresMRNFormatMessageError = "When NCTS-Flag is ticked or previous reference type is 'ESUMA', 'N355' or 'POUS'";
	}
}
