using System;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUAddInfoValidation : AutoAUAddInfoValidation
	{
		#region Constants

		const string AllowableAmberValuesList = "DVTQPOC";
		const string ZA_SCNFormatPattern = @"^[0-9][dcstbemxy][1-8][0-9]{4}$";

		#endregion

		public AUAddInfoValidation(AutoAUAddInfo parent)
			: base(parent)
		{
		}

		public new AUAddInfo Parent
		{
			get { return (AUAddInfo)base.Parent; }
		}

		#region Public methods for Calculated fields

		public void ValidateTCI_InstrumentType()
		{
			ValidateCalculatedProperty(AddInfo.TCI_InstrumentTypeInfo);
		}

		public void ValidateTCI_InstrumentNo()
		{
			ValidateCalculatedProperty(AddInfo.TCI_InstrumentNoInfo);
		}

		public void ValidatePRI_InstrumentType()
		{
			ValidateCalculatedProperty(AddInfo.PRI_InstrumentTypeInfo);
		}

		public void ValidatePRI_InstrumentNo()
		{
			ValidateCalculatedProperty(AddInfo.PRI_InstrumentNoInfo);
		}

		public void ValidateTI2_InstrumentType()
		{
			ValidateCalculatedProperty(AddInfo.TI2_InstrumentTypeInfo);
		}

		public void ValidateTI2_InstrumentNo()
		{
			ValidateCalculatedProperty(AddInfo.TI2_InstrumentNoInfo);
		}

		protected virtual void CheckTCI_InstrumentType()
		{
		}

		protected virtual void CheckTCI_InstrumentNo()
		{
		}

		protected virtual void CheckPRI_InstrumentType()
		{
		}

		protected virtual void CheckPRI_InstrumentNo()
		{
		}

		protected virtual void CheckTI2_InstrumentType()
		{
		}

		protected virtual void CheckTI2_InstrumentNo()
		{
		}

		#endregion

		#region Related Objects

		public AUAddInfo AddInfo
		{
			get { return Parent; }
		}

		public AUAddInfoLookups Lookups
		{
			get { return AddInfo.Lookups; }
		}

		protected IAggregatedAddInfo AggregateAddInfoParent
		{
			get { return AddInfo.Parent; }
		}

		protected BusinessObjectFactory Factory
		{
			get { return AddInfo.Factory; }
		}

		public JobDeclaration JobDeclaration
		{
			get
			{
				if (fJobDeclaration == null)
				{
					if (AggregateAddInfoParent is JobDeclaration)
					{
						fJobDeclaration = AggregateAddInfoParent as JobDeclaration;
					}
					else
					{
						if (AggregateAddInfoParent is JobComInvoiceGroupHeader invoiceGroupHeader)
						{
							fJobDeclaration = invoiceGroupHeader.JobDeclaration;
						}
						else if (AggregateAddInfoParent is JobComInvoiceHeader invoiceHeader)
						{
							fJobDeclaration = invoiceHeader.JobDeclaration;
						}
						else if (AggregateAddInfoParent is JobComInvoiceLine invoiceLine)
						{
							fJobDeclaration = invoiceLine.Declaration;
						}
						else if (AggregateAddInfoParent is CusContainer container)
						{
							fJobDeclaration = container.JobDeclaration;
						}
					}
				}
				return fJobDeclaration;
			}
		}

		protected JobComInvoiceLine InvoiceLine
		{
			get { return AggregateAddInfoParent as JobComInvoiceLine; }
		}

		#endregion

		#region Declaration Type

		protected bool IsExport
		{
			get { return JobDeclaration != null && JobDeclaration.IsExport; }
		}

		protected bool IsImport
		{
			get { return JobDeclaration != null && JobDeclaration.IsImport; }
		}

		protected bool IsExWarehouse
		{
			get { return JobDeclaration != null && JobDeclaration.IsExWarehouse; }
		}

		#endregion

		#region Add Info Line Validation

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateTCI_InstrumentType();
			ValidateTCI_InstrumentNo();
			ValidatePRI_InstrumentType();
			ValidatePRI_InstrumentNo();
			ValidateTI2_InstrumentType();
			ValidateTI2_InstrumentNo();

			ValidateAddInfoLine(); // must be called after all other validation
		}

		public void ValidateAddInfoLine()
		{
			ValidateCalculatedProperty(Parent.AddInfoLineInfo);
		}

		protected void CheckAddInfoLine()
		{
			foreach (ZPropertyInfo propertyInfo in Parent.ZPropertyInfoHash)
			{
				if (propertyInfo.HasSetter &&
					(propertyInfo.Name.Substring(0, 3) == AddInfo.TablePrefix ||
						propertyInfo.Name.Substring(0, 3) == "PRI" ||
						propertyInfo.Name.Substring(0, 3) == "TI2" ||
						propertyInfo.Name.Substring(0, 3) == "TCI") &&
					!propertyInfo.Name.EndsWith("Hidden", StringComparison.Ordinal))
				{
					if (propertyInfo.HasNotifications())
					{
						Parent.AddInfoLineInfo.AddAllNotificationsFrom(propertyInfo);
					}
				}
			}
		}

		#endregion

		#region Common Validation between Header/Lines for CMR

		protected override void CheckZA_WRN()
		{
			base.CheckZA_WRN();
			var cachedDeclaration = JobDeclaration;

			if (cachedDeclaration != null)
			{
				if (!cachedDeclaration.IsNature30 && !cachedDeclaration.IsWarehousedByExternalAgent && !AddInfo.ZA_WRN.IsEmpty)
				{
					AddInfo.ZA_WRNInfo.AddMessageError("Warehouse reference number is only required for a nature 30.");
				}
			}
		}

		protected override void CheckZA_PST()
		{
			base.CheckZA_PST();

			if (AddInfo.IsImportCMR && !AddInfo.ZA_PST.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_PSTInfo, AddInfo.Lookups.ZA_PST_List);
			}
			ValidateZA_POC();
			ValidateZA_PRT();
			ValidateAddInfoLine();
		}

		protected override void CheckZA_POC()
		{
			base.CheckZA_POC();

			if (AddInfo.IsImportCMR)
			{
				if (!AddInfo.ZA_POC.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_POCInfo, AddInfo.Lookups.ZA_POC_List);
				}
				else if (!AddInfo.IsGeneralRate && !AddInfo.ZA_PST.IsEmpty)
				{
					AddInfo.ZA_POCInfo.AddMessageError("Please select the Preference Origin.");
				}
			}

			ValidateAddInfoLine();
		}

		protected override void CheckZA_PRT()
		{
			base.CheckZA_PRT();

			if (AddInfo.IsImportCMR)
			{
				if (!AddInfo.ZA_PRT.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_PRTInfo, AddInfo.Lookups.ZA_PRT_List);
				}
				else if (!AddInfo.IsGeneralRate && !AddInfo.ZA_PST.IsEmpty)
				{
					AddInfo.ZA_PRTInfo.AddMessageError("Please select the Preference Rule Type.");
				}
			}

			ValidateAddInfoLine();
		}

		#region Quarantine (formerly known as AQIS)

		#region AQIS Commodity Code

		protected override void CheckZA_AQISCommCodes_Hidden()
		{
			base.CheckZA_AQISCommCodes_Hidden();

			if (AddInfo.Parent is IAQIS)
			{
				ValidateAQISField(((IAQIS)AddInfo.Parent).AQISCommodityCodes, AddInfo.ZA_AQISCommCodes_HiddenInfo, "Quarantine Commodity Code");
			}
		}

		#endregion

		#region AQIS Producer Code

		protected override void CheckZA_AQISProducerCodes_Hidden()
		{
			base.CheckZA_AQISProducerCodes_Hidden();

			if (AddInfo.Parent is IAQIS)
			{
				ValidateAQISField(((IAQIS)AddInfo.Parent).AQISProducerCodes, AddInfo.ZA_AQISProducerCodes_HiddenInfo, "Quarantine Producer Codes");
			}
		}

		#endregion

		#region AQIS Entity Id

		protected override void CheckZA_AQISEntityIds_Hidden()
		{
			base.CheckZA_AQISEntityIds_Hidden();

			if (AddInfo.Parent is IAQIS)
			{
				ValidateAQISField(((IAQIS)AddInfo.Parent).AQISEntityIds, AddInfo.ZA_AQISEntityIds_HiddenInfo, "Quarantine Entity Id");
			}
		}

		#endregion

		#region AQIS Permit Id

		protected override void CheckZA_AQISPermitIds_Hidden()
		{
			base.CheckZA_AQISPermitIds_Hidden();

			if (AddInfo.Parent is IAQIS)
			{
				ValidateAQISField(((IAQIS)AddInfo.Parent).AQISPermitIds, AddInfo.ZA_AQISPermitIds_HiddenInfo, "Quarantine Permit Ids");
			}
		}

		#endregion

		void ValidateAQISField(AQISSingleValueCollection collection, ZPropertyInfo property, ZString aQISFieldName)
		{
			if (collection.HasErrors())
			{
				property.AddError("At least " + aQISFieldName + " has an error. Please open the form and fix this error");
			}
			else if (collection.HasMessageErrors())
			{
				property.AddMessageError("At least " + aQISFieldName + " has a message error. Please open the form and fix this error");
			}
			else if (collection.HasWarnings())
			{
				property.AddWarning("At least " + aQISFieldName + " has a warning. Please open the form and fix this error");
			}
		}

		#endregion

		#endregion

		#region Unaccompanied Personal Effects

		bool IsUPEImporterEntered
		{
			get
			{
				return (!AddInfo.ZA_UPEImporterDOB_Hidden.IsEmpty && !AddInfo.ZA_UPEImporterSex_Hidden.IsEmpty &&
						!AddInfo.ZA_UPEImporterPassportCountry_Hidden.IsEmpty &&
						!AddInfo.ZA_UPEImporterPassportNumber_Hidden.IsEmpty);
			}
		}

		bool IsUPESpouseNotEntered
		{
			get
			{
				return (AddInfo.ZA_UPESpouseName_Hidden.IsEmpty && AddInfo.ZA_UPESpousePassportCountry_Hidden.IsEmpty &&
						AddInfo.ZA_UPESpousePassportNumber_Hidden.IsEmpty);
			}
		}

		protected override void CheckZA_UPEImporterDOB_Hidden()
		{
			base.CheckZA_UPEImporterDOB_Hidden();

			if (AddInfo.ZA_UPEIndicator_Hidden && !IsUPEImporterEntered)
			{
				MandatoryValidation.MessageErrorIfNotEntered(AddInfo.ZA_UPEImporterDOB_HiddenInfo, "DOB");
			}
		}

		protected override void CheckZA_UPEImporterDOB_HiddenIsValidZDateTimeRange()
		{
		}

		protected override void CheckZA_UPEImporterPassportCountry_Hidden()
		{
			base.CheckZA_UPEImporterPassportCountry_Hidden();

			if (AddInfo.ZA_UPEIndicator_Hidden)
			{
				if (!IsUPEImporterEntered)
				{
					MandatoryValidation.MessageErrorIfNotEntered(AddInfo.ZA_UPEImporterPassportCountry_HiddenInfo, "Country/Region");
					ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_UPEImporterPassportCountry_HiddenInfo, Parent.JobDeclaration.Lookups.CountryList);
				}
			}
		}

		protected override void CheckZA_UPEImporterPassportNumber_Hidden()
		{
			base.CheckZA_UPEImporterPassportNumber_Hidden();

			if (AddInfo.ZA_UPEIndicator_Hidden)
			{
				if (!IsUPEImporterEntered)
				{
					MandatoryValidation.MessageErrorIfNotEntered(AddInfo.ZA_UPEImporterPassportNumber_HiddenInfo, "Number");
				}
			}
		}

		protected override void CheckZA_UPEImporterSex_Hidden()
		{
			base.CheckZA_UPEImporterSex_Hidden();

			if (AddInfo.ZA_UPEIndicator_Hidden)
			{
				if (AddInfo.ZA_UPEImporterSex_Hidden.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(AddInfo.ZA_UPEImporterSex_HiddenInfo, "Sex");
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_UPEImporterSex_HiddenInfo, Parent.Lookups.Gender);
				}
			}
		}

		protected override void CheckZA_UPESpouseName_Hidden()
		{
			base.CheckZA_UPESpouseName_Hidden();

			if (AddInfo.ZA_UPEIndicator_Hidden && !IsUPESpouseNotEntered)
			{
				MandatoryValidation.MessageErrorIfNotEntered(AddInfo.ZA_UPESpouseName_HiddenInfo, "Spouse Name");
			}
		}

		protected override void CheckZA_UPESpousePassportCountry_Hidden()
		{
			base.CheckZA_UPESpousePassportCountry_Hidden();

			if (AddInfo.ZA_UPEIndicator_Hidden && !IsUPESpouseNotEntered)
			{
				MandatoryValidation.MessageErrorIfNotEntered(AddInfo.ZA_UPESpousePassportCountry_HiddenInfo, "Country/Region");
				ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_UPESpousePassportCountry_HiddenInfo, Parent.JobDeclaration.Lookups.CountryList);
			}
		}

		protected override void CheckZA_UPESpousePassportNumber_Hidden()
		{
			base.CheckZA_UPESpousePassportNumber_Hidden();

			if (AddInfo.ZA_UPEIndicator_Hidden && !IsUPESpouseNotEntered)
			{
				MandatoryValidation.MessageErrorIfNotEntered(AddInfo.ZA_UPESpousePassportNumber_HiddenInfo, "Number");
			}
		}

		#endregion

		#region Common Validation between Invoice Header/Lines in Edifice

		protected override void CheckZA_PRF()
		{
			base.CheckZA_PRF();

			if (!AddInfo.IsImportCMR)
			{
				if (AddInfo.Parent is JobComInvoiceLine && AddInfo.AggregatedZA_PRF.IsEmpty)
				{
					if (Lookups.ZA_PRFList.Count > 0)
					{
						new MessageValidation(AddInfo).CheckEntered(AddInfo.ZA_PRFInfo, "Enter a preference: If you enter an Origin on the line you are required also to enter a preference, an alternative is to enter an Origin and Preference on the header add info and leave the line without either unless they are different on that line");
					}
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_PRFInfo, Lookups.ZA_PRFList);
				}
				ValidateAddInfoLine();
			}
		}

		#endregion

		#region Common Validation between Invoice Header/Lines and CMR/Edifice

		protected override void CheckZA_AMB()
		{
			base.CheckZA_AMB();
			var allowableAmberValues = AllowableAmberValuesList;
			var errorMessage = ZString.Empty;

			foreach (var aCharacter in AddInfo.ZA_AMB.ToString())
			{
				var characterPosition = allowableAmberValues.IndexOf(aCharacter);

				if (characterPosition != -1)
				{
					allowableAmberValues = allowableAmberValues.Remove(characterPosition, 1);
				}
				else if (AllowableAmberValuesList.IndexOf(aCharacter) != -1)
				{
					errorMessage += ZString.Format("Duplicated character '{0}' is invalid for Amber Entry Processing\n", aCharacter);
				}
				else
				{
					errorMessage += ZString.Format("Character '{0}' is invalid from Amber Entry Processing\n", aCharacter);
				}
			}

			if (errorMessage.Length > 0)
			{
				AddInfo.ZA_AMBInfo.AddMessageError(errorMessage.TrimEnd('\n'));
			}
		}

		protected override void CheckZA_ORG()
		{
			base.CheckZA_ORG();
			var value = AddInfo.ZA_ORG;
			if (!value.IsEmpty && (!((ZInt)value.Length).IsInRange(2, 4) || !value.IsLettersOnlyOrEmpty))
			{
				AddInfo.ZA_ORGInfo.AddMessageError(value + " is not in the correct format.");
			}

			if (AddInfo.IsImportCMR)
			{
				if (!AddInfo.ZA_ORG.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_ORGInfo, Lookups.ZA_ORG_List);

					if (AddInfo.ZA_ORG.Length > 2)
					{
						AddInfo.ZA_ORGInfo.AddMessageError("Only 2 characters is allowed for Origin.");
					}
				}
			}
			else if (!AddInfo.ZA_ORG.IsEmpty && !AddInfo.ZA_ORGInfo.HasNotifications() && AddInfo.CountryOfOrigin == null)
			{
				AddInfo.ZA_ORGInfo.AddMessageError(AddInfo.ZA_ORG + " is not a valid country/region.");
			}

			if (IsImport && LineLevelAddInfo && AddInfo.AggregatedZA_ORG.IsEmpty)
			{
				AddInfo.ZA_ORGInfo.AddMessageError("You must enter the country/region of origin either at the header or line level.");
			}
		}

		protected override void CheckZA_GSTE()
		{
			base.CheckZA_GSTE();

			if (!Parent.ZA_GSTE.IsEmpty)
			{
				var code = new CMRCodeLists.Loader(Parent.Factory).Load(Parent.ZA_GSTE, CMRCodeLists.CodeTypes.GSTX, Parent.EffectiveDutyDate);
				if (code == null)
				{
					Parent.ZA_GSTEInfo.AddMessageError(EnterValidGSTECode + Parent.EffectiveDutyDate.ToShortDateString() + ".");
				}
			}
		}

		internal const string EnterValidGSTECode = "Please enter a GSTE code that is valid for ";

		protected override void CheckZA_SCN()
		{
			base.CheckZA_SCN();
			RegexCheck(AddInfo.ZA_SCNInfo, ZA_SCNFormatPattern, RegexOptions.IgnoreCase);
		}

		protected override void CheckZA_VALB_Hidden()
		{
			base.CheckZA_VALB_Hidden();

			if (AddInfo.IsImportCMR)
			{
				ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_VALB_HiddenInfo, Lookups.ValuationBasisListForCMR);
			}
		}

		protected override void CheckZA_VAN()
		{
			base.CheckZA_VAN();
			var value = AddInfo.ZA_VAN;
			if (!value.IsEmpty && (!((ZInt)value.Length).IsInRange(1, 4) || !value.IsNumbersOnlyOrEmpty))
			{
				AddInfo.ZA_VANInfo.AddMessageError(value + " is not in the correct format.");
			}
		}

		protected override void CheckZA_PermitNumbers_Hidden()
		{
			base.CheckZA_PermitNumbers_Hidden();
			if (AddInfo.IsExport)
			{
				new PermitNumberValidation().ValidatePermitNumbers(AddInfo.Permits, AddInfo.ZA_PermitNumbers_HiddenInfo);
				if (AddInfo.ZA_PermitNumbers_Hidden.Length > 0)
				{
					var permitAndEncryptions = AddInfo.ZA_PermitNumbers_Hidden.Split(',');
					var hasError = false;
					for (var i = 0; i < permitAndEncryptions.Length; i++)
					{
						var permit = permitAndEncryptions[i];
						if (permit.IsEmpty || permit.Length - permit.Replace(":", "").Length > 1)
						{
							hasError = true;
						}
					}
					if (hasError)
					{
						AddInfo.ZA_PermitNumbers_HiddenInfo.AddMessageError("The format of the permit number and encryption number is incorrect");
					}
				}
			}
		}

		protected override void CheckZA_ManifestClientIDOverride_Hidden()
		{
			base.CheckZA_ManifestClientIDOverride_Hidden();
			var cachedJobDeclaration = JobDeclaration;

			if (cachedJobDeclaration != null && cachedJobDeclaration.IsImport && !cachedJobDeclaration.IsImportCMR && AggregateAddInfoParent is JobDeclaration)
			{
				if (AddInfo.ZA_ManifestClientIDOverride_Hidden.IsEmpty)
				{
					if (cachedJobDeclaration.HasBreakBulk || cachedJobDeclaration.IsBulk || cachedJobDeclaration.IsLiquid)
					{
						AddInfo.ZA_ManifestClientIDOverride_HiddenInfo.AddMessageError("Manifest Client ID is required for Breakbulk, Bulk or Liquid shipments");
					}
				}
			}
		}

		#endregion

		#region General Validation

		protected void CheckValidEdificeRange(ZPropertyInfo propertyInfo)
		{
			var value = (ZDecimal)propertyInfo.Value;
			if (value < 0)
			{
				propertyInfo.AddMessageError("Value may not be negative.");
			}
			else
			{
				var trimmedValue = value.ToString().TrimEnd('0');
				trimmedValue = trimmedValue.TrimEnd('.');
				if (trimmedValue.Length > 10)
				{
					propertyInfo.AddMessageError("Value is too large to submit to EDIFICE");
				}
			}
		}

		protected void RegexCheck(ZPropertyInfo propertyInfo, string regularExpression)
		{
			RegexCheck(propertyInfo, regularExpression, RegexOptions.None);
		}

		protected void RegexCheck(ZPropertyInfo propertyInfo, string regularExpression, RegexOptions options)
		{
			var stringToMatch = (ZString)propertyInfo.Value;
			if (!stringToMatch.IsEmpty)
			{
				var r = new Regex(regularExpression, options);
				if (!r.IsMatch(stringToMatch))
				{
					propertyInfo.AddMessageError(stringToMatch + " is not in the correct format.");
				}
			}
		}

		protected string ValidateAmountAndGetErrorMessage(ZDecimal amount, bool allowNegative)
		{
			var result = "";
			if (amount != 0)
			{
				if (!allowNegative)
				{
					if (amount < 0)
					{
						result = "Negative amount is invalid.";
					}
				}
			}
			return result;
		}

		protected string ValidateCurrencyAndGetErrorMessage(string currency)
		{
			var result = "";
			if (string.IsNullOrEmpty(currency))
			{
				result = "Please enter a currency if not in percentage";
			}
			else
			{
				var curr = RefCurrency.LoadFromCurrencyCode(Factory, currency);
				if (curr == null)
				{
					result = currency + " is not a valid currency.";
				}
			}
			return result;
		}

		protected override void CheckZA_InstrumentCode_Hidden()
		{
			base.CheckZA_InstrumentCode_Hidden();
			if (!AddInfo.ZA_InstrumentCode_Hidden.IsEmpty)
			{
				if (!DoesInstrumentCodeExist)
				{
					AddInfo.ZA_InstrumentCode_HiddenInfo.AddMessageError("The selected instrument code does not exist.");
				}
				else
				{
					var cMRInstrument = CMRInstrument;
					if (cMRInstrument != null)
					{
						if (!cMRInstrument.IsValidForThisDate(Parent.Parent != null ? Parent.Parent.EffectiveDutyDate : ZDateTime.Today))
						{
							var messageText = ZString.Empty;
							if (!cMRInstrument.IN_EndDate.IsEmpty)
							{
								messageText = "The selected instrument code " + (cMRInstrument.IN_EndDate < ZDateTime.Now ? "has expired" : "will expire") + " on " + cMRInstrument.IN_EndDate.ToShortDateString();
							}
							if (!cMRInstrument.IN_RevocationDate.IsEmpty)
							{
								messageText += (messageText.IsEmpty) ? "The selected instrument code " : " and it ";
								messageText += (cMRInstrument.IN_RevocationDate < ZDateTime.Now ? "was revoked" : "will be revoked") + " on " + cMRInstrument.IN_RevocationDate.ToShortDateString();
							}
							if (!string.IsNullOrEmpty(messageText))
							{
								AddInfo.ZA_InstrumentCode_HiddenInfo.AddMessageError(messageText);
							}
						}
						else if (!cMRInstrument.IsValidForThisTariff(AddInfo.TariffAndStatNumber))
						{
							AddInfo.ZA_InstrumentCode_HiddenInfo.AddMessageError("The instrument is not valid for the tariff number. This instrument is valid only for " + cMRInstrument.TariffGroupRelevant);
						}
					}
				}
			}
		}

		protected CMRInstrument CMRInstrument
		{
			get { return CMRInstrument.Load(Factory, AddInfo.ZA_InstrumentCode_Hidden); }
		}

		protected virtual bool DoesInstrumentCodeExist
		{
			get { return true; }
		}

		#endregion

		protected bool LineLevelAddInfo
		{
			get { return AddInfo.LineLevelAddInfo; }
		}

		JobDeclaration fJobDeclaration;
	}
}
