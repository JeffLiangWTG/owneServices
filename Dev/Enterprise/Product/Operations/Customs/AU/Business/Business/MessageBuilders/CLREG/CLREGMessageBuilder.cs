using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CLREGMessageBuilder : CMRCUSCARMessageBuilder
	{
		public CLREGMessageBuilder(ISupplyCLREGInfo queryInfo, BusinessObjectFactory factory)
		{
			this.queryInfo = queryInfo;
			MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			Messages = queryInfo.Messages;
			this.factory = factory;
		}
		readonly ISupplyCLREGInfo queryInfo;
		readonly BusinessObjectFactory factory;

		protected internal override ZString DocumentName => "CLREG";

		protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.RegistrationDocument;

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.CLREG;

		protected internal override Type TypeOfMessage => typeof(CMRCLREGMessage);

		protected internal override void GenerateMessageText()
		{
			if (CUSCAR == null)
			{
				CUSCAR = new CUSCARMessage();

				SetAdditionalEDIMessageDetails();

				PopulateUNH();
				PopulateBGMWithDocumentMessageNumber();

				PopulateGroup1();
				PopulateFTX();
				PopulateGroup4();
				PopulateGIS();
				PopulateGroup7();
				PopulateGroup8();
				PopulateAddressesFTX();
				PopulateContactDetailsFTX();
				PopulateRollsMEA();

				PopulateGroup17();
				PopulateUNT();
			}
		}

		void SetAdditionalEDIMessageDetails()
		{
			var addOn = factory.New<GenAddOnColumn>();
			addOn.XA_ParentID = queryInfo.OrganizationPK;
			addOn.XA_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			addOn.XA_Name = DocumentMessageNumber;
			addOn.XA_Type = AddOnColumnDataType.Codes.String;
			addOn.XA_Data = DocumentMessageNumber;
		}

		ZString DocumentMessageNumber
		{
			get
			{
				if (documentMessageNumber.IsEmpty)
				{
					documentMessageNumber = Guid.NewGuid().ToString("N");
				}
				return documentMessageNumber;
			}
		}
		ZString documentMessageNumber;

		void PopulateBGMWithDocumentMessageNumber()
		{
			BGM.DocumentMessageName.DocumentNameCode = DocumentNameCode;
			BGM.DocumentMessageName.DocumentName = DocumentName;
			BGM.DocumentMessageIdentification.DocumentMessageNumber = DocumentMessageNumber;
			BGM.DocumentMessageIdentification.Version = Version.ToString();
			BGM.MessageFunctionCode = MessageFunctionCode;
		}

		void PopulateGroup1()
		{
			RFFSegment rFF;
			if (!queryInfo.ABN.IsEmpty)
			{
				rFF = CUSCAR.Group1.InstantiateAChildAndAddItToChildrenCollection().RFF[0];
				rFF.Reference.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.SellersCatalogueNumber;
				rFF.Reference.ReferenceIdentifier = queryInfo.ABN;
				rFF.Reference.LineNumber = queryInfo.CAC;
				rFF.Reference.ReferenceVersionIdentifier = queryInfo.CACType;
			}
			else
			{
				rFF = CUSCAR.Group1.InstantiateAChildAndAddItToChildrenCollection().RFF[0];
				rFF.Reference.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.RegisteredContractorActivityType;
				rFF.Reference.ReferenceIdentifier = (queryInfo.IsIndividual ? "IND" : "ORG");
			}
		}

		void PopulateFTX()
		{
			FTXSegment fTX;

			if (queryInfo.IsIndividual && !queryInfo.FamilyName.IsEmpty)
			{
				fTX = CUSCAR.FTX.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.Title;
				fTX.TextLiteral.FreeTextValue1 = queryInfo.Title;
				fTX.TextLiteral.FreeTextValue2 = queryInfo.FirstName;
				fTX.TextLiteral.FreeTextValue3 = queryInfo.SecondName;
				fTX.TextLiteral.FreeTextValue4 = queryInfo.FamilyName;
				fTX.TextLiteral.FreeTextValue5 = queryInfo.Suffix;
			}

			if (!queryInfo.ContactName.IsEmpty)
			{
				fTX = CUSCAR.FTX.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.GetFromString("CNP");
				fTX.TextLiteral.FreeTextValue1 = queryInfo.ContactName;
				fTX.TextLiteral.FreeTextValue2 = queryInfo.ContactPurpose;
			}
		}

		void PopulateGroup4()
		{
			if (queryInfo.IsOrganisation && !queryInfo.BusinessName.IsEmpty)
			{
				TDTSegment tDT;
				tDT = CUSCAR.Group4.InstantiateAChildAndAddItToChildrenCollection().TDT[0];
				tDT.TransportStageCodeQualifier = TransportStageCodeQualifierList.InlandTransport;

				LOCSegment lOC = CUSCAR.Group4.InstantiateAChildAndAddItToChildrenCollection().LOC[0];
				lOC.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.MutuallyDefined;
				lOC.LocationIdentification.LocationName = queryInfo.BusinessName;
			}
		}

		void PopulateGIS()
		{
			if (queryInfo.IsEvidenceOfID)
			{
				GISSegment gIS = CUSCAR.GIS.InstantiateAChildAndAddItToChildrenCollection();
				gIS.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString("EVI");
				gIS.ProcessingIndicator_X.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
			}

			if (queryInfo.IsExDocsUser)
			{
				GISSegment gIS = CUSCAR.GIS.InstantiateAChildAndAddItToChildrenCollection();
				gIS.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString("EXD");
				gIS.ProcessingIndicator_X.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
			}
		}

		void PopulateGroup7()
		{
			CNISegment cNI = CUSCAR.Group7.InstantiateAChildAndAddItToChildrenCollection().CNI[0];
			cNI.ConsolidationItemNumber = "1";
		}

		void PopulateGroup8()
		{
			if (queryInfo.IsIndividual)
			{
				if (queryInfo.TravelDocuments.Count > 0)
				{
					foreach (var travelDocument in queryInfo.TravelDocuments)
					{
						PopulateRFFTrigger();

						LOCSegment lOC = CUSCAR.Group7.InstantiateAChildAndAddItToChildrenCollection().Group8.InstantiateAChildAndAddItToChildrenCollection().LOC[0];
						lOC.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.GetFromString("DN");
						lOC.LocationIdentification.LocationNameCode = travelDocument.ZA_DocumentNo;

						lOC = CUSCAR.Group7.InstantiateAChildAndAddItToChildrenCollection().Group8.InstantiateAChildAndAddItToChildrenCollection().LOC[0];
						lOC.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.GetFromString("ISS");
						lOC.LocationIdentification.LocationNameCode = travelDocument.ZA_Country;
					}
				}
				else
				{
					PopulateRFFTrigger();
				}
			}
			else
			{
				PopulateRFFTrigger();
			}
		}

		void PopulateRFFTrigger()
		{
			RFFSegment rFF = CUSCAR.Group7.InstantiateAChildAndAddItToChildrenCollection().Group8.InstantiateAChildAndAddItToChildrenCollection().RFF[0];
			rFF.Reference.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.GetFromString("1");
		}

		void PopulateRollsMEA()
		{
			foreach (ZString roll in queryInfo.Rolls)
			{
				MEASegment mEA = CUSCAR.Group7.InstantiateAChildAndAddItToChildrenCollection().Group8.InstantiateAChildAndAddItToChildrenCollection().Group14.InstantiateAChildAndAddItToChildrenCollection().MEA[0];
				mEA.MeasurementAttributeCode = MeasurementAttributeCodeList.GetFromString("RN");
				mEA.MeasurementDetails.NonDiscreteMeasurementName = roll;
				mEA.ValueRange.MeasurementUnitCode = "";
			}
		}

		void PopulateAddressesFTX()
		{
			GIDSegment gID = CUSCAR.Group7.InstantiateAChildAndAddItToChildrenCollection().Group8.InstantiateAChildAndAddItToChildrenCollection().Group14.InstantiateAChildAndAddItToChildrenCollection().GID[0];
			gID.GoodsItemNumber = "1";

			if (!queryInfo.BusinessAddress1.IsEmpty)
			{
				PopulateAddress("ATY", "BA", queryInfo.BusinessAddress1, queryInfo.BusinessAddress2, queryInfo.BusinessAddressCity, queryInfo.BusinessAddressPostCode, queryInfo.BusinessAddressState, queryInfo.BusinessAddressCountry);
			}

			if (!queryInfo.PostalAddress1.IsEmpty || !queryInfo.PostalAddress2.IsEmpty)
			{
				PopulateAddress("ATY", "PA", queryInfo.PostalAddress1, queryInfo.PostalAddress2, queryInfo.PostalAddressCity, queryInfo.PostalAddressPostCode, queryInfo.PostalAddressState, queryInfo.PostalAddressCountry);
			}

			if (!queryInfo.ContactAddress1.IsEmpty || !queryInfo.ContactAddress2.IsEmpty)
			{
				PopulateAddress("CAT", "BA", queryInfo.ContactAddress1, queryInfo.ContactAddress2, queryInfo.ContactAddressCity, queryInfo.ContactAddressPostCode, queryInfo.ContactAddressState, queryInfo.ContactAddressCountry);
			}

			if (!queryInfo.ContactPostalAddress1.IsEmpty || !queryInfo.ContactPostalAddress2.IsEmpty)
			{
				PopulateAddress("CAT", "PA", queryInfo.ContactPostalAddress1, queryInfo.ContactPostalAddress2, queryInfo.ContactPostalAddressCity, queryInfo.ContactPostalAddressPostCode, queryInfo.ContactPostalAddressState, queryInfo.ContactPostalAddressCountry);
			}
		}

		void PopulateAddress(string addressDetailsCode, string addressType, string address1, string address2, string city, string postCode, string state, string countryCode)
		{
			FTXSegment fTX = CUSCAR.Group7.InstantiateAChildAndAddItToChildrenCollection().Group8.InstantiateAChildAndAddItToChildrenCollection().Group14.InstantiateAChildAndAddItToChildrenCollection().FTX[0];
			fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.GetFromString(addressDetailsCode);
			fTX.TextFunctionCoded = TextFunctionCodedList.GetFromString(addressType);
			fTX.TextLiteral.FreeTextValue1 = address1;
			fTX.TextLiteral.FreeTextValue2 = address2;
			fTX.TextLiteral.FreeTextValue3 = city;
			fTX.TextLiteral.FreeTextValue4 = postCode;
			fTX.TextLiteral.FreeTextValue5 = state;
			fTX.LanguageNameCode = countryCode;
		}

		void PopulateContactDetailsFTX()
		{
			if (!queryInfo.ContactPh.IsEmpty)
			{
				PopulateContactDetails("BP", queryInfo.ContactPhPrefix, queryInfo.ContactPh, queryInfo.ContactPhComment);
			}

			if (!queryInfo.ContactFax.IsEmpty)
			{
				PopulateContactDetails("FA", queryInfo.ContactFaxPrefix, queryInfo.ContactFax, queryInfo.ContactFaxComment);
			}

			if (!queryInfo.ContactAH.IsEmpty)
			{
				PopulateContactDetails("AP", queryInfo.ContactAHPrefix, queryInfo.ContactAH, queryInfo.ContactAHComment);
			}

			if (!queryInfo.ContactMobile.IsEmpty)
			{
				FTXSegment fTX = CUSCAR.Group7.InstantiateAChildAndAddItToChildrenCollection().Group8.InstantiateAChildAndAddItToChildrenCollection().Group14.InstantiateAChildAndAddItToChildrenCollection().FTX[0];
				fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.GetFromString("CAT");
				fTX.TextFunctionCoded = TextFunctionCodedList.GetFromString("MO");
				fTX.TextLiteral.FreeTextValue1 = queryInfo.ContactMobile;
				fTX.TextLiteral.FreeTextValue2 = queryInfo.ContactMobileComment;
			}

			if (!queryInfo.ContactEmail.IsEmpty)
			{
				FTXSegment fTX = CUSCAR.Group7.InstantiateAChildAndAddItToChildrenCollection().Group8.InstantiateAChildAndAddItToChildrenCollection().Group14.InstantiateAChildAndAddItToChildrenCollection().FTX[0];
				fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.GetFromString("CAT");
				fTX.TextFunctionCoded = TextFunctionCodedList.GetFromString("EA");
				fTX.TextLiteral.FreeTextValue1 = queryInfo.ContactEmail;
			}
		}

		void PopulateContactDetails(string addressType, string phPrefix, string phone, string comment)
		{
			FTXSegment fTX = CUSCAR.Group7.InstantiateAChildAndAddItToChildrenCollection().Group8.InstantiateAChildAndAddItToChildrenCollection().Group14.InstantiateAChildAndAddItToChildrenCollection().FTX[0];
			fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.GetFromString("CAT");
			fTX.TextFunctionCoded = TextFunctionCodedList.GetFromString(addressType);
			fTX.TextReference.FreeTextValueCode = phPrefix;
			fTX.TextLiteral.FreeTextValue1 = phone;
			fTX.TextLiteral.FreeTextValue2 = comment;
		}

		void PopulateGroup17()
		{
			bool canPopulateBlock = queryInfo.IsIndividual || !queryInfo.ABN.IsEmpty;

			if (canPopulateBlock && !queryInfo.Gender.IsEmpty)
			{
				AUTSegment aUT = CUSCAR.Group17.InstantiateAChildAndAddItToChildrenCollection().AUT[0];
				aUT.ValidationResult = "GE";
				aUT.ValidationKeyIdentification = queryInfo.Gender;
			}

			if (canPopulateBlock && !queryInfo.DateofBirth.IsEmpty)
			{
				DTMSegment dTM = CUSCAR.Group17.InstantiateAChildAndAddItToChildrenCollection().DTM[0];
				dTM.DateTimePeriod.DateTimePeriodFunctionCodeQualifier = DateTimePeriodFunctionCodeQualifierList.BirthDateTime;
				dTM.DateTimePeriod.DateTimePeriodValue = queryInfo.DateofBirth.ToString("yyyyMMdd");
				dTM.DateTimePeriod.DateTimePeriodFormatCode = DateTimePeriodFormatCodeList.Ccyymmdd;
			}
		}
	}
}
