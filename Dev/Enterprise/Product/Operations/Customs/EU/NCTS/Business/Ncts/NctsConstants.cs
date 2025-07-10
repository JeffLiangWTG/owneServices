namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class NctsConstants
	{
		public static class NctsTypeOfDeclaration
		{
			public static class Codes
			{
				public const string GoodsMovingUnderExternalCommunityTransitProcedure = "T1";
				public const string GoodsMovingUnderInternalCommunityTransitProcedure = "T2";
				public const string GoodsMovingUnderInternalCommunityTransitProcedureBetweenDifferentFiscalTerritories = "T2F";
				public const string MixedConsignmentOfT1AndT2GoodsPhase4 = "T-";
				public const string MixedConsignmentOfT1AndT2GoodsPhase5 = "T";
				public const string T2PlusSanMarino = "T2SM";
				public const string TirDeclaration = "TIR";
				public const string SGI = "SGI";
				public const string NationalTransitSwitzerland = "T-CH";
			}
		}

		public static class ModeOfRepresentation
		{
			public static class Codes
			{
				public const string Blank = "";
				public const string One = "1";
				public const string Two = "2";
			}
		}

		public static class CusCodeDataTypes
		{
			public const string TransportInland = "TPI";
		}

		public static class NctsTypeOfPreviousDocument
		{
			public static class Codes
			{
				public const string C651 = "C651";
				public const string C658 = "C658";
				public const string N820 = "N820";
				public const string N821 = "N821";
				public const string N822 = "N822";
				public const string N830 = "N830";
				public const string N952 = "N952";
				public const string NMRN = "NMRN";
			}
		}
		public static class NctsTypeOfSupportingDocument
		{
			public static class Codes
			{
				public const string _7P17 = "7P17";
				public const string _67YY = "67YY";
			}
		}

		public static class AdditionalInfoCodes
		{
			public const string InEXSWhereNegotiableBillsOfLadingAndUnknownConsigneeParticulars = "30600";
			public const string HouseBillOfLading = "N714";
			public const string ConsignorAEOCertificateNumber = "Y022";
			public const string ConsigneeAEOCertificateNumber = "Y023";
			public const string RepresentativeAEOCertificateNumber = "Y025";
			public const string PrincipalAEOCertificateNumber = "Y026";
			public const string CarrierAEOCertificateNumber = "Y028";
		}

		public static class ValidationRuleMessagePrefixes
		{
			public const string B1838 = "[B1838] ";
			public const string C0191 = "[C0191] ";
			public const string C0403 = "[C0403] ";
			public const string C0505 = "[C0505] ";
			public const string C0806 = "[C0806] ";
			public const string C0839 = "[C0839] ";
			public const string E1117 = "[E1117] ";
			public const string TR0005 = "[TR0005] ";
			public const string TR0017 = "[TR0017] ";
			public const string TR0057 = "[TR0057] ";
			public const string TR0058 = "[TR0058] ";
			public const string R0473 = "[R0473]";
			public const string R0473_1 = "[R0473-1]";
		}

		public static class DischargeTypes
		{
			public const string FullDischarge = "FD";
			public const string PartialDischarge = "PD";
		}

		public static class ValidationMessages
		{
			public static string YouHaveNotEnteredACustomsOfficeOfTransitDeclared => Res.GetString("0E080E12-5E48-4B3E-B550-96FFF3C420D7", "[C0030] You have not entered a Customs Office Of Transit Declared (TRA).");
			public static string PreviousDocumentOfTypeCL178IsRequiredForCustomsOfficeOfDepartureInCL112AndDeclarationTypeT2OrT2F => Res.GetString("3B65E0E5-86F4-4E4C-8B0D-ED14342D57DD", "[R0020] Previous Document of Type in CL178 is required either at Consignment level or for all Goods Item when Country of Customs Office of Departure is from the CL112 (Country Codes CTC) list and the Declaration Type is T2/T2F.");
			public static string CountryCodeDepartureOfficeShouldBeSameOfNCTSCountryCode => Res.GetString("29584EE6-4324-42B9-88F6-FE3492361D6E", "The country code of the NCTS departure office should be the same as the job's NCTS country code (login country) or a customs region/territory thereof.");
		}

		public static class CustomsFieldMaxLength
		{
			public static class TransitionPeriod
			{
				public const int DescriptionInGoodsItem = 280;
				public static class Trader
				{
					public const int Name = 35;
					public const int Address = 35;
					public const int PostCode = 9;
					public const int City = 35;
				}

				public static class AdditionalDocument
				{
					public const int ReferenceNumber = 35;
				}
			}

			public static class Trader
			{
				public const int Name = 70;
				public const int Address = 70;
			}
		}

		public static class Traders
		{
			public static string PrincipalCaption => Res.GetString("20FF655A-99E9-406E-90A0-795ED0D65D8C", "Principal");
			public static string ConsignorCaption => Res.GetString("F54C94AE-5777-448B-BF31-5197B7EB5AC6", "Consignor");
			public static string ConsigneeCaption => Res.GetString("095AF222-E8E6-4AFB-950E-1717328CA360", "Consignee");
		}
	}
}
