namespace Enterprise.Customs.DE.Messaging
{
	public static class MessageSchema
	{
		public static class CommonMessageSchema
		{
			public const int EoriCodeMaxLength = 17;
			public const int EoriBranchCodeMaxLength = 4;
			public const int MRNMaxLength = 18;
			public const int CountryCodeMaxLength = 2;
			public const int CustomsOfficeReferenceNumberMaxLength = 8;
			public const int StreetAndNumberMaxLength = 70;
			public const int UNLocodeMaxLength = 17;
			public const int CusCodeMaxLength = 9;
		}

		public static class ATLASMessageSchema
		{
			public const int AddressCityMaxLength = 35;
			public const int AddressDistrictMaxLength = 35;
			public const int AddressLineMaxLength = 35;
			public const int AddressPostcodeMaxLength = 9;
			public const int AuthorisationNumberMaxLength = 25;
			public const int CommodityCodeMaxlength = 6;
			public const int ContactEmailAdressMaxLength = 256;
			public const int ContactFacsimileNumberMaxLength = 35;
			public const int ContactNameMaxLength = 35;
			public const int ContactPhoneNumberMaxLength = 35;
			public const int ContactPositionMaxLength = 35;
			public const int ContainerIdentificationNumberMaxLength = 17;
			public const int ContingentNumberMaxLength = 4;
			public const int CurrentProcedureMaxLength = 35;
			public const int DeliveryPlaceMaxLength = 4;
			public const int DepartureTransportMeansTypeMaxLength = 2;
			public const int DepartureTransportMeansIdentityMaxLength = 27;
			public const int DepartureTransportMeansNationalityMaxLength = 2;
			public const int DiscrepanciesDescriptionMaxLength = 140;
			public const int EndorsementAuthorityMaxLength = 35;
			public const int EndorsementCountryMaxLength = 2;
			public const int EndorsementPlaceMaxLength = 35;
			public const int EoriBranchCodeMaxLength = CommonMessageSchema.EoriBranchCodeMaxLength;
			public const int EoriCodeMaxLength = CommonMessageSchema.EoriCodeMaxLength;
			public const int LRNMaxLength = 22;
			public const int MRNMaxLength = CommonMessageSchema.MRNMaxLength;
			public const int EventCountryMaxLength = 2;
			public const int EventPlaceMaxLength = 35;
			public const int FirstEntryCustomsOfficeReferenceNumberMaxLength = 8;
			public const int GoodsDescription = 120;
			public const int GoodsDescriptionMaxLength = 280;
			public const int GrossWeightPrecision = 3;
			public const int IncidentInformationMaxLength = 350;
			public const int InterchangeRecipientReferenceNumberMaxLength = 8;
			public const int LocationOfGoodsMaxLength = 2;
			public const int PackageKindMaxLength = 3;
			public const int PackageMarksNumbersMaxLength = 42;
			public const int PartyNameMaxLength = 120;
			public const int ProcessingOwnerMaxLength = 35;
			public const int ReferenceNumberMaxLength = 18;
			public const int SealIdentityMaxLength = 20;
			public const int SupplementaryCodeMaxLength = 4;
			public const int UnionStatusMaxLegnth = 1;
			public const int UnloadingExplanationMaxLength = 350;
			public const int UnloadingPlaceMaxLength = 35;
			public const int WarehouseOwnerMaxLength = 35;
			public const int BorderTransportMeansInformationMaxLength = 17;
			public const int TransportReferenceNumberMaxLength = 70;
			public const int OtherThingsToReportMaxLength = 512;
			public const int CustomsOfficeOfDestinationActualIDMaxLength = 8;
			public const int UnloadingRemarkConformMaxLength = 1;
			public const int UnloadingRemarkStateOfSealsMaxLength = 1;
			public const int UnloadingRemarkMaxLength = 512;
			public const int HarmonizedSystemSubheadingCodeMaxLength = 6;
			public const int CombinedNomenclatureCodeMaxLength = 2;
			public const int AuthorisationTypeMaxLength = 4;
			public const int AuthorisationReferenceNumberMaxLength = 35;
			public const int QualifierOfIdentificationMaxLength = 1;
			public const int UNLocodeMaxLength = CommonMessageSchema.UNLocodeMaxLength;
			public const int CountryCodeMaxLength = CommonMessageSchema.CountryCodeMaxLength;
			public const int LatitudeMaxLength = 17;
			public const int LongitudeMaxLength = 17;
			public const int StreetAndNumberMaxLength = CommonMessageSchema.StreetAndNumberMaxLength;
			public const int CustomsOfficeReferenceNumberMaxLength = CommonMessageSchema.CustomsOfficeReferenceNumberMaxLength;
			public const int CusCodeMaxLength = CommonMessageSchema.CusCodeMaxLength;
		}
	}
}
