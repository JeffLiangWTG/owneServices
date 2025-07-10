using System;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using OrgCodeGenerator = Enterprise.MasterFiles.Business.OrgCodeGenerator;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class OrganisationValueObjectDataAdapter : ValueObjectDataAdapter<OrgHeader, Xsd.Organisation>
	{
		public OrganisationValueObjectDataAdapter()
			: this(OrganisationTypes.None, null)
		{
		}

		public OrganisationValueObjectDataAdapter(OrganisationTypes orgTypes)
			: this(orgTypes, null)
		{
		}

		public OrganisationValueObjectDataAdapter(IDocAddresses parent)
			: this(OrganisationTypes.None, parent)
		{
		}

		public OrganisationValueObjectDataAdapter(OrgAddressesExporter orgAddressExportOverride)
			: this()
		{
			this.orgAddressExportOverride = orgAddressExportOverride;
		}

		public OrganisationValueObjectDataAdapter(OrganisationTypes orgTypes, IDocAddresses parent)
		{
			DocAddressesParent = parent;
			this.OrgTypes = orgTypes;
		}

		public IDocAddresses DocAddressesParent { get; private set; }
		public readonly OrganisationTypes OrgTypes;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is the name of an XML element")]
		public override string RootCollectionElementName
		{
			get { return "Organisations"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is the name of an XML element")]
		public override string RootElementName
		{
			get { return "Organisation"; }
		}

		public override XmlSchema Schema
		{
			get { return XmlSchemaDefinitions.Instance.SingleOrganisationSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return null; }
		}

		OrgAddressesExporter GetOrgAddressExporter(OrgHeader organisation)
		{
			if (orgAddressExportOverride != null)
			{
				if (orgAddressExportOverride.Organisation != organisation)
				{
					throw new InvalidOperationException("Trying to Export an Organisation that doesn't match on the OrgAddressesExporterOverride");
				}
				return orgAddressExportOverride;
			}
			else
			{
				return new OrgAddressesExporter(organisation, DocAddressesParent);
			}
		}
		readonly OrgAddressesExporter orgAddressExportOverride;

		protected override OrgHeader FindBusinessObject(Xsd.Organisation value, IValueObjectImportContext context)
		{
			return context.FindOrganisation(value, null, OrganisationTypes.None);
		}

		#region Import

		protected override void ImportFromValueObjectCore(OrgHeader bizObj, Xsd.Organisation valueObj, IValueObjectImportContext context)
		{
			Xsd.Organisation value = valueObj;
			var organisation = bizObj;

			if (OrgTypes != OrganisationTypes.None)
			{
				organisation.OrganisationTypes = OrgTypes;
			}

			context.SetPropertyInfoValue(organisation.OH_FullNameInfo, value.OrganisationDetails.Name, value.OrganisationDetails.NameSpecified);
			context.SetPropertyInfoValue(organisation.OH_LanguageInfo, value.OrganisationDetails.Language);
			if (value.OrganisationDetails.Location.IsSpecified)
			{
				context.SetPropertyInfoValue(organisation.OH_RL_NKClosestPortInfo, value.OrganisationDetails.Location.Value, ForeignKeyType.PortNK);
			}
			string errorContext = Res.GetString("c6c85ad7-7fc2-41c8-a66e-3f2455c300d6", "Organization with code '{0}'", organisation.OH_Code);

			ImportOrganisationDetails(organisation, value.OrganisationDetails, context, errorContext);
			ImportNotes(organisation, value, context);
			ImportOrGenerateEDICode(organisation, (Xsd.XmlInterchange)context.Interchange, value);
			AddImportEvent(bizObj);

			CusBondDetailValueObjectDataAdapter.ImportAllBondDetails(organisation, valueObj.OrganisationDetails.BondDetails, bizObj.Factory, context);
			OrganisationCountrySpecificDataTransferTool.ImportData(organisation, valueObj.OrganisationDetails, context);
		}

		void ImportNotes(OrgHeader organisation, Xsd.Organisation value, IValueObjectImportContext context)
		{
			new NoteValueObjectDataAdapter().ImportNotesAndAttachToBusinessObjectNotes(organisation.Notes, value.Notes, context);
		}

		void ImportOrganisationDetails(OrgHeader organisation, Xsd.OrganisationDetail organisationDetailsValue, IValueObjectImportContext context, string errorContext)
		{
			if (!organisationDetailsValue.IsSpecified)
			{
				return;
			}

			ImportAddresses(organisationDetailsValue.Addresses, organisation, context, errorContext);
			ImportRegistrationNumbers(organisation, organisationDetailsValue.RegistrationNumbers, context);
			ImportEDICodeMapping(organisation, organisationDetailsValue, context, errorContext);

			ImportAccountRecievable(organisation, organisationDetailsValue, context, errorContext);
			ImportAccountPayable(organisation, organisationDetailsValue, context, errorContext);

			ImportContacts(organisationDetailsValue.Contacts, organisation, context, errorContext);
			if (organisationDetailsValue.WebAddressSpecified)
			{
				ImportMainWebURL(organisation, context, errorContext, organisationDetailsValue.WebAddress);
			}
			else
			{
				ImportWebURLs(organisationDetailsValue.OrgWebURLs, organisation, context, errorContext);
			}

			ImportOrganisationTypes(organisation, organisationDetailsValue, context, errorContext);
		}

		protected virtual void ImportAddresses(Xsd.OrgAddressCollection address, OrgHeader organisation, IValueObjectImportContext context, ZString errorContext)
		{
			var addressHelper = new AddressValueObjectHelper(errorContext);
			addressHelper.ImportFromValueObjectCollection(address, organisation, context);
		}

		protected virtual void ImportContacts(Xsd.OrgContactCollection contacts, OrgHeader organisation, IValueObjectImportContext context, ZString errorContext)
		{
			var contactHelper = new ContactValueObjectHelper(errorContext);
			contactHelper.ImportFromValueObjectCollection(contacts, organisation, context);
		}

		protected virtual void ImportWebURLs(Xsd.OrgWebURLCollection webUrls, OrgHeader organisation, IValueObjectImportContext context, ZString errorContext)
		{
			var webUrlHelper = new WebURLValueObjectHelper(errorContext);
			webUrlHelper.ImportFromValueObjectCollection(webUrls, organisation, context);
		}

		protected virtual void ImportMainWebURL(OrgHeader organisation, IValueObjectImportContext context, ZString errorContext, ZString urlValue)
		{
			var webHelper = new WebURLValueObjectHelper(errorContext);
			webHelper.ImportMainWebURL(organisation, context, urlValue);
		}

		void ImportRegistrationNumbers(OrgHeader organisation, Xsd.RegistrationNumberCollection registrationNumbers, IValueObjectImportContext context)
		{
			if (!registrationNumbers.IsSpecified)
			{
				return;
			}

			foreach (Xsd.RegistrationNumber number in registrationNumbers)
			{
				if (OrganisationValueObjectDataAdapterHelper.NumberIsAUGSTAndABNIsSpecified(number, registrationNumbers))
				{
					continue;
				}

				if (OrganisationValueObjectDataAdapterHelper.IsUSDeprecatedSAN(number, organisation.OH_Code, context))
				{
					continue;
				}

				var numberType = OrganisationValueObjectDataAdapterHelper.GetNumberType(number, organisation.OH_Code, context);
				var orgCusCode = organisation.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(numberType, RefCountry.LoadFromCountryCode(organisation.Factory, number.CountryOfRegistration));
				if (orgCusCode == null)
				{
					if (number.NumberSpecified)
					{
						orgCusCode = organisation.CustomsCodes.AddNew();
						context.SetPropertyInfoValue(orgCusCode.OK_RN_NKCodeCountryInfo, number.CountryOfRegistration, ForeignKeyType.CountryNK);
						context.SetPropertyInfoValue(orgCusCode.OK_CodeTypeInfo, numberType);
						context.SetPropertyInfoValue(orgCusCode.OK_CustomsRegNoInfo, number.Number);

						if (number.AddressCodeSpecified && orgCusCode.PremisesAddressIsAllowed)
						{
							ZQuery query = new ZQuery(OrgAddressSchema.OA_Code, number.AddressCode);
							OrgAddress[] addresses = (OrgAddress[])organisation.Addresses.Find(query);

							if (addresses.Length > 0)
							{
								orgCusCode.OK_OA_PremisesAddress = addresses[0].PK;
							}
						}
					}
				}
				else
				{
					if (number.NumberSpecified)
					{
						context.SetPropertyInfoValue(orgCusCode.OK_CustomsRegNoInfo, number.Number);
					}
					else
					{
						orgCusCode.Delete();
					}
				}
			}
		}

		protected virtual void ImportAccountRecievable(OrgHeader organisation, Xsd.OrganisationDetail organisationDetail, IValueObjectImportContext context, string errorContext)
		{
		}

		protected virtual void ImportAccountPayable(OrgHeader organisation, Xsd.OrganisationDetail organisationDetail, IValueObjectImportContext context, string errorContext)
		{
		}

		protected virtual void ImportOrganisationTypes(OrgHeader organisation, Xsd.OrganisationDetail organisationDetail, IValueObjectImportContext context, string errorContext)
		{
		}

		void ImportEDICodeMapping(OrgHeader organisation, Xsd.OrganisationDetail organisationDetail, IValueObjectImportContext context, string errorContext)
		{
			foreach (Xsd.EDICodeMapping codeMapping in organisationDetail.EDICodeMappings)
			{
				bool isCode = false;

				ZGuid localGuid = ZGuid.Empty;
				switch (codeMapping.Relationship)
				{
					case Constants.OrgPatternMatchOverrideRelationships.Organisation:
						OrgHeader loadedOrg = context.Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, codeMapping.EDICode);
						if (loadedOrg != null)
						{
							localGuid = loadedOrg.PK;
						}
						break;
					case Constants.OrgPatternMatchOverrideRelationships.Port:
						RefUNLOCO loadedPort = context.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, codeMapping.EDICode);
						if (loadedPort != null)
						{
							localGuid = loadedPort.PK;
						}
						break;
					case Constants.OrgPatternMatchOverrideRelationships.Currency:
						RefCurrency loadedCurrency = RefCurrency.LoadFromCurrencyCode(context.Factory, codeMapping.EDICode);
						if (loadedCurrency != null)
						{
							localGuid = loadedCurrency.PK;
						}
						break;
					case Constants.OrgPatternMatchOverrideRelationships.Country:
						RefCountry loadedCountry = RefCountry.LoadFromCountryCode(context.Factory, codeMapping.EDICode);
						if (loadedCountry != null)
						{
							localGuid = loadedCountry.PK;
						}
						break;
					case Constants.OrgPatternMatchOverrideRelationships.ContainerType:
						RefContainer loadedContainer = new RefContainer.Loader(context.Factory).LoadFromCode(codeMapping.EDICode);
						if (loadedContainer != null)
						{
							localGuid = loadedContainer.PK;
						}
						break;
					case Constants.OrgPatternMatchOverrideRelationships.IncoTerm:
						isCode = true;
						break;
				}

				if (!isCode && (localGuid.IsEmpty))
				{
					continue;
				}

				var patternMatch = organisation.Factory.New<OrgPatternMatchOverride>();
				using (patternMatch.SuspendSettingHasChanges())
				using (patternMatch.GetValidationSuspender())
				{
					patternMatch.OO_OH = organisation.PK;
					patternMatch.OO_Relationship = codeMapping.Relationship;
					if (isCode)
					{
						patternMatch.OO_LocalCode = codeMapping.ForeignCode;
					}
					else
					{
						patternMatch.OO_LocalGuid = localGuid;
					}
					patternMatch.OO_ForeignCode = codeMapping.ForeignCode;
				}
			}
		}

		void ImportOrGenerateEDICode(OrgHeader organisation, Xsd.XmlInterchange interchange, Xsd.Organisation orgValue)
		{
			if (interchange.ImportEDICode)
			{
				organisation.OH_Code = orgValue.EDICode.Left(organisation.OH_CodeInfo.MaxLength);
			}
			else
			{
				organisation.GenerateProposedCode();
				EnsureCodeExistsAndIsUnique(organisation);
			}
		}

		#region EnsureCodeExistsAndIsUnique

		void EnsureCodeExistsAndIsUnique(OrgHeader organisation)
		{
			if (organisation.OH_Code.IsEmpty)
			{
				organisation.OH_Code = (organisation.UNLOCO != null && !organisation.UNLOCO.HasErrors)
					? new OrgCodeGenerator().GenerateCode(organisation).GetProposedCode()
					: new AbbreviatedNameCodeGenerator(OrgPatternLanguageSetting.FactoryInstance).GenerateCode(organisation.OH_FullNameTruncated, organisation.PortName, organisation.CountryName, organisation.OH_Language);
			}
			if (!organisation.OH_Code.IsEmpty)
			{
				EnsureOrgCodeIsUnique(organisation);
			}
		}

		void EnsureOrgCodeIsUnique(OrgHeader org)
		{
			OrgCodeGenerator generator = new OrgCodeGenerator();
			OrgCodeAlgorithm alg = generator.GetAlgorithm(org);
			bool hasNumberElement = false;
			foreach (OrgCodeElement element in alg.Elements)
			{
				if (!element.Empty && (element.Description == OrgCodeElementDescription.GloballyUniqueNumber || element.Description == OrgCodeElementDescription.CodeSpecificUniqueNumber))
				{
					hasNumberElement = true;
					break;
				}
			}

			if (!hasNumberElement || (!org.OH_Code.IsEmpty && !org.OH_Code.Substring(org.OH_Code.Length - 1, 1).IsNumbersOnlyOrEmpty))
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.Equal, org.OH_Code);
				filter.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, org.PK);
				bool alreadyExists = org.Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(OrgHeader)), filter);

				string codeWithoutTail = org.OH_Code.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');

				if (alreadyExists)
				{
					DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(org.Factory);
					ZSqlParameter codeParam = ZSqlParameter.New("@Code", codeWithoutTail + "%", OrgHeaderSchema.OH_Code);

					int numericStartIndex = codeWithoutTail.Length + 1;
					int length = OrgHeaderSchema.OH_Code.MaxLength - numericStartIndex;

					string query = String.Format(@"
										select top 1 SUBSTRING(oh_code, {0}, {1}) HighestCode
										from dbo.OrgHeader 
										where oh_code like @Code
										and isnumeric(SUBSTRING(oh_code, {0}, {1})) = 1
										order by CONVERT(int, SUBSTRING(oh_code, {0}, {1})) desc", numericStartIndex, length);

					collection.Load(query, new ZSqlParameter[] { codeParam });

					int nextTail = 1;

					if (collection.Count > 0)
					{
						ZString highestCode = collection[0]["HighestCode"].ToString();
						if (highestCode.IsNumbersOnlyOrEmpty)
						{
							nextTail = Convert.ToInt32(highestCode) + 1;
						}
					}

					org.OH_Code = codeWithoutTail + nextTail.ToString();
				}

				bool foundDuplicate = true;
				bool codeWasChanged = false;
				while (foundDuplicate)
				{
					ZQuery localFilter = new ZQuery();
					localFilter.FetchOnlyFromLocalCache = true;

					localFilter.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.Equal, org.OH_Code);
					localFilter.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, org.PK);
					OrgHeader[] existingOrgs = (OrgHeader[])org.Factory.Load(typeof(OrgHeader), localFilter);

					foundDuplicate = false;
					foreach (OrgHeader existingOrg in existingOrgs)
					{
						if (existingOrg.OH_Code == org.OH_Code)
						{
							foundDuplicate = true;

							ZString tailString = org.OH_Code.Replace(codeWithoutTail, "");
							int currentTail = 1;
							if (!tailString.IsEmpty && tailString.IsNumbersOnlyOrEmpty)
							{
								currentTail = Convert.ToInt32(tailString) + 1;
							}

							org.OH_Code = codeWithoutTail + currentTail.ToString();
							codeWasChanged = true;

							break;
						}
					}
				}

				if (codeWasChanged)
				{
					EnsureOrgCodeIsUnique(org);
				}
			}
		}

		#endregion

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(OrgHeader bizObj, Xsd.Organisation constructedValueObject, IValueObjectExportContext context)
		{
			var result = constructedValueObject;
			var organisation = bizObj;
			var errorContext = Res.GetString("c6c85ad7-7fc2-41c8-a66e-3f2455c300d6", "Organization with code '{0}'", organisation.OH_Code);

			result.EDICode = organisation.OH_Code;
			result.OwnerCode = organisation.OH_Code;

			ExportOrganisationDetails(organisation, result);

			GetOrgAddressExporter(organisation).Export(result, errorContext, context);
			ExportContacts(organisation, result, errorContext, context);
			ExportWebURLs(organisation, result, errorContext, context);
			ExportRegistrationNumbers(organisation, result, context);
			ExportEDITransmissions(organisation, result, context);
			ExportNotes(organisation, result, context);
			ExportBrandAndCompanyNames(organisation, result.OrganisationDetails);

			CusBondDetailValueObjectDataAdapter.ExportAllBondDetails(organisation.PK, constructedValueObject.OrganisationDetails.BondDetails, organisation.Factory, context);
			OrganisationCountrySpecificDataTransferTool.ExportData(organisation, constructedValueObject.OrganisationDetails, context);
		}

		#region Export Organisation Details

		void ExportOrganisationDetails(OrgHeader organisation, Xsd.Organisation result)
		{
			var organisationDetail = new Xsd.OrganisationDetail();
			ExportName(organisation, organisationDetail);
			ExportLanguage(organisation, organisationDetail);
			ExportLocation(organisation, organisationDetail);
			result.OrganisationDetails = organisationDetail;
		}

		void ExportName(OrgHeader organisation, Xsd.OrganisationDetail result)
		{
			if (!organisation.OH_FullName.IsEmpty)
			{
				result.Name = organisation.OH_FullNameTruncated;
			}
		}

		void ExportLanguage(OrgHeader organisation, Xsd.OrganisationDetail result)
		{
			if (organisation.OH_Language != Constants.Languages.English)
			{
				result.Language = organisation.OH_Language;
			}
		}

		void ExportLocation(OrgHeader organisation, Xsd.OrganisationDetail result)
		{
			if (organisation.ClosestPort != null)
			{
				result.Location = Xsd.UNLOCO.FromPort(organisation.ClosestPort);
			}
			else
			{
				result.Location = Xsd.UNLOCO.FromPortCode(organisation.Factory, organisation.OH_RL_NKClosestPort);
			}
		}

		#endregion

		#region Export Relative Business Object

		void ExportContacts(OrgHeader organisation, Xsd.Organisation result, string errorContext, IValueObjectExportContext context)
		{
			var contactHelper = GetNewContactValueObjectHelper(errorContext, DocAddressesParent);
			var contacts = organisation.Contacts;
			var contactResults = result.OrganisationDetails.Contacts;
			contactHelper.ExportToValueObjectCollection(contacts, contactResults, context);
		}

		protected virtual ContactValueObjectHelper GetNewContactValueObjectHelper(string errorContext, IDocAddresses docAddressesParent)
		{
			return new ContactValueObjectHelper(errorContext, docAddressesParent);
		}

		void ExportWebURLs(OrgHeader organisation, Xsd.Organisation result, string errorContext, IValueObjectExportContext context)
		{
			var webUrlHelper = new WebURLValueObjectHelper(errorContext);
			webUrlHelper.ExportToValueObjectCollection(organisation.OrgWebURLs, result.OrganisationDetails.OrgWebURLs, context);
		}

		void ExportRegistrationNumbers(OrgHeader organisation, Xsd.Organisation result, INotifications notifications)
		{
			for (var i = 0; i < organisation.CustomsCodes.Count; i++)
			{
				var code = organisation.CustomsCodes[i];
				if (code.OK_CustomsRegNo.Trim().IsEmpty)
				{
					continue;
				}

				var regNumber = result.OrganisationDetails.RegistrationNumbers.AddNew();
				var countryOfRegistration = code.CodeCountry;
				regNumber.CountryOfRegistration = (countryOfRegistration == null) ? null : countryOfRegistration.Code;
				regNumber.Number = code.OK_CustomsRegNo;
				regNumber.NumberType = OrgCusCodeXmlMappings.Instance.GetExternalCode(
					code.OK_CodeType,
					Res.GetString("57887459-1c0c-4137-8106-be8048311bb3", "Organization {0}", organisation.OH_Code),
					notifications);

				if (!code.OK_OA_PremisesAddress.IsEmpty)
				{
					var loadedAddress = (OrgAddress)organisation.Addresses.FindByPK(code.OK_OA_PremisesAddress);
					if (loadedAddress != null && !loadedAddress.OA_Code.IsEmpty)
					{
						regNumber.AddressCode = loadedAddress.OA_Code;
						regNumber.AddressCodeSpecified = true;
					}
				}
			}
			if (result.OrganisationDetails.RegistrationNumbers.Count > 0)
			{
				DoRegistrationNumberBackWardsCompatibility(result);
			}
		}

		void DoRegistrationNumberBackWardsCompatibility(Xsd.Organisation result)
		{
			var existingABN = result.OrganisationDetails.RegistrationNumbers.FindRegistrationNumber(Xsd.RegistrationNumberTypes.ABN, Constants.CountryCodes.Australia);
			if (existingABN != null)
			{
				var gSTForBackwardsCompatibility = result.OrganisationDetails.RegistrationNumbers.AddNew();
				gSTForBackwardsCompatibility.NumberType = Xsd.RegistrationNumberTypes.GST;
				gSTForBackwardsCompatibility.Number = existingABN.Number;
				gSTForBackwardsCompatibility.CountryOfRegistration = existingABN.CountryOfRegistration;
			}
		}

		void ExportNotes(OrgHeader organisation, Xsd.Organisation value, IValueObjectExportContext context)
		{
			value.Notes = new NoteValueObjectDataAdapter().ExportToXmlValueObjectCollection(organisation.Notes, context);
		}

		void ExportBrandAndCompanyNames(OrgHeader organisation, Xsd.OrganisationDetail xsdOrganisationDetail)
		{
			var xsdBrandNames = new Xsd.OrganisationDetailBrandNameCollection();
			foreach (OrgBrandOrRelatedName brandName in organisation.BrandsOrRelatedNames)
			{
				xsdBrandNames.AddNew().Value = brandName.P1_RelatedName;
			}
			xsdOrganisationDetail.BrandNames = xsdBrandNames;
		}

		void ExportEDITransmissions(OrgHeader organisation, Xsd.Organisation value, IValueObjectExportContext context)
		{
			var miscServ = organisation.MiscServ;
			var eDITransmissionDetails = value.OrganisationDetails.EDITransmissionDetails;

			if (miscServ.Header.EDICommunicationsModes.ClientSpecificDestination == "" || miscServ.Header.EDICommunicationsModes.ClientSpecificCommunicationTransport == "")
			{
				return;
			}

			eDITransmissionDetails.Type = OrgEDITransmissionConfigurationXmlMappings.Instance.GetExternalCode(
				miscServ.Header.EDICommunicationsModes.ClientSpecificCommunicationTransport,
				Res.GetString("f81a0ddd-b139-41e4-abfb-ad8fc0bc88d7", "Address Type"),
				context);
			eDITransmissionDetails.Address = miscServ.Header.EDICommunicationsModes.ClientSpecificDestination;
		}
		#endregion

		#endregion

		#region Create Adapters

		CusBondDetailValueObjectDataAdapter CusBondDetailValueObjectDataAdapter
		{
			get { return cusBondDetailValueObjectDataAdapter ?? (cusBondDetailValueObjectDataAdapter = new CusBondDetailValueObjectDataAdapter()); }
		}
		CusBondDetailValueObjectDataAdapter cusBondDetailValueObjectDataAdapter;

		OrganisationCountrySpecificDataTransferTool OrganisationCountrySpecificDataTransferTool
		{
			get { return orgCountrySpecificDataTransferTool ?? (orgCountrySpecificDataTransferTool = OrganisationCountrySpecificDataTransferTool.New()); }
		}
		OrganisationCountrySpecificDataTransferTool orgCountrySpecificDataTransferTool;

		#endregion
	}
}
