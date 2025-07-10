using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.EU;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public partial class JobDeclarationValidation : AutoEUJobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

		protected override void CheckJE_RL_NKPortOfLoading()
		{
			base.CheckJE_RL_NKPortOfLoading();
			var declaration = Parent;
			if (declaration.IsImport
				&& declaration.JE_EntryStyle == EntryStyleListImport.Codes.ImportNormal
				&& declaration.ZG_CTStatusID != ImportCommunityTransitStatusList.Codes.T1
				&& (declaration.PortOfLoading?.IsInEU ?? false))
			{
				Parent.JE_RL_NKPortOfLoadingInfo.AddWarning(Res.GetString("07348F52-B34B-44D7-89F0-AAB0EF823BF4", "This port is in the EU. Please, check that is correct."));
			}
		}

		protected override void CheckJE_LocationOfGoods()
		{
			base.CheckJE_LocationOfGoods();
			if (Parent.IsExport)
			{
				if (EUCustomsDataRegistry.Instance.ExportAuthorizedLocationCheck.Value)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.JE_LocationOfGoodsInfo, GoodsLocationNotAuthorizedLocationMessage);
				}
				else
				{
					ListValidation.WarnIfInvalidCode(Parent.JE_LocationOfGoodsInfo, GoodsLocationNotAuthorizedLocationMessage);
				}
			}
		}

		public static MultilingualString GoodsLocationNotAuthorizedLocationMessage => ResString.GetMultilingualString("000FDB84-0701-4A91-B715-89D4CAD8B2AD", "The Goods location is not an Authorized location.");

		protected virtual void CheckLocation()
		{
		}

		public virtual void ValidateImporterDocumentaryAddress(JobDocAddressValidation validation)
		{ }

		public virtual void ValidateSupplierDocumentaryAddress(JobDocAddressValidation validation)
		{ }

		public virtual void ValidateWarehouseDocAddressForeignKey()
		{
			var parent = Parent;
			var warehouseDocAddress = parent.WarehouseDocAddress;
			if (warehouseDocAddress.E2_OA_Address.IsEmpty && parent.BondedWarehouseEditable && parent.IsWarehouseNeeded)
			{
				warehouseDocAddress.E2_OA_AddressInfo.AddMessageError(ValidationMessageForWhenCpcDemandWarehouse);
			}
		}

		#region JE_EntryStyle
		public void ValidateJE_EntryStyle()
		{
			ValidateCalculatedProperty(Parent.JE_EntryStyleInfo);
		}

		protected virtual void CheckJE_EntryStyle()
		{
			if (Parent.IsImport || Parent.IsExport)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_EntryStyleInfo);
			}
		}
		#endregion

		protected override void CheckJE_RN_NKTransportNationality()
		{
			base.CheckJE_RN_NKTransportNationality();

			ListValidation.MessageErrorIfInvalidCode(Parent.JE_RN_NKTransportNationalityInfo);
		}

		protected override void CheckJE_RN_NKTransportNationalityInland()
		{
			base.CheckJE_RN_NKTransportNationalityInland();

			ListValidation.MessageErrorIfInvalidCode(Parent.JE_RN_NKTransportNationalityInlandInfo);
		}

		protected override void CheckJE_MessageType()
		{
			base.CheckJE_MessageType();
			ValidateJE_EntryStyle();
		}

		protected override void CheckJE_MessageSubType()
		{
			ValidateJE_EntryStyle();
		}

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();
			ValidateJE_RL_NKPortOfFirstArrival();
		}

		protected override void CheckJE_TransportModeInland()
		{
			base.CheckJE_TransportModeInland();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_TransportModeInlandInfo);
			CheckRuleC0841();
			CheckRuleC0843();
		}

		protected override void CheckJE_UCR()
		{
			base.CheckJE_UCR();
			var declaration = Parent;

			if (declaration.IsInDatabase)
			{
				MandatoryValidation.MessageErrorIfNotEntered(declaration.JE_UCRInfo);
			}
			var ducr = declaration.JE_UCR;
			if (!ducr.IsEmpty && ThisDucrExistsOnAnotherDeclaration())
			{
				declaration.JE_UCRInfo.AddMessageError(ErrorMessageTextForDuplicateUCR);
			}

			bool ThisDucrExistsOnAnotherDeclaration()
			{
				var query = new ZDBOnlyQuery(typeof(JobDeclaration));
				query.AddToFilter(JobDeclarationSchema.PK, SQLComparisonOperator.NotEqual, declaration.PK);
				query.AddToFilter(JobDeclarationSchema.JE_UCR, SQLComparisonOperator.Equal, ducr);
				query.AddToFilter(JobDeclarationSchema.JE_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo,
					new ZDateTime(ZDateTime.Now.AddYears(-10)));

				var subQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbCompanySchema.PK);
				subQuery.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, SQLComparisonOperator.Equal, declaration.Country.Code);
				query.AddSubQuery(JobDeclarationSchema.JE_GC, subQuery, JoinCondition.And);

				var declarationsWithSameDucr = declaration.Factory.Load<BaseJobDeclaration>(query);
				return declarationsWithSameDucr.Any(x => !HaveSUPRelationship(declaration, x) && !HaveSUPRelationship(x, declaration));
			}

			bool HaveSUPRelationship(BaseJobDeclaration masterDeclaration, BaseJobDeclaration supplementaryDeclaration)
			{
				var relationship = (ManyToManyRelationship)masterDeclaration.RelatedDeclarations.Relationship;
				return ((relationship?.GetPivotObject(supplementaryDeclaration) as GenPivot)?.XX_RelationType ?? ZString.Empty) == EUCommonConstants.DeclarationRelationshipType.SupplementaryDeclaration;
			}
		}

		public virtual ZString ErrorMessageTextForDuplicateUCR => ResString.GetMultilingualString("JobDeclarationValidation | ErrorMessageTextForDuplicateUCR", "This Unique Consignment Reference is already in use on another declaration in {0}", Parent.Country.Description);

		protected override void CheckJE_RL_NKPortOfFirstArrival()
		{
			if (Parent.JE_MessageType == MessageTypeList.Codes.Import)
			{
				base.CheckJE_RL_NKPortOfFirstArrival();
			}
		}

		protected override void CheckJE_DateOfFirstArrival()
		{
			if (Parent.JE_MessageType == MessageTypeList.Codes.Import)
			{
				base.CheckJE_DateOfFirstArrival();
			}
		}

		protected override void CheckJE_DateOfFirstArrivalIsValidZDateTime()
		{
			TypeValidation.CheckValidSmallDateTime(Parent.JE_DateOfFirstArrivalInfo);
		}

		protected override void CheckJE_DateOfFirstArrivalIsValidZDateTimeRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.JE_DateOfFirstArrivalInfo);
		}

		protected internal virtual void BarrierPortMightHaveChanged()
		{
		}

		protected override void CheckJE_DeclarantType()
		{
			base.CheckJE_DeclarantType();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_DeclarantTypeInfo);
			AddMessageErrorIfDeclarantTypeNotEntered();
			ValidatePowerOfAttorney();
			CheckLegalForDIR();
		}

		protected virtual void AddMessageErrorIfDeclarantTypeNotEntered()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_DeclarantTypeInfo);
		}

		protected virtual void ValidatePowerOfAttorney()
		{
			if (Parent.IsDeclarantAddressRequired)
			{
				var party = Parent.IsImport ? Parent.Importer : Parent.Supplier;
				new AuthorityToActValidator().Validate(Parent, party, Parent.JE_DeclarantTypeInfo, Core.Constants.RefDocTypes.PowerOfAttorney, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms);
			}
		}

		public void CheckLegalForDIR()
		{
			var parent = Parent;

			if (parent.ShouldCheckLegalByDeclarantType)
			{
				if (parent.JE_DeclarantType == RepresentationTypeList.Codes._2Direct)
				{
					var euProvider = new EuropeanUnionCustomsMembersProvider();

					if (parent.IsExport)
					{
						if (!parent.SupplierDocumentaryAddress.E2_RN_NKCountryCode.IsEmpty && !euProvider.IsMemberOfEU(Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(parent.SupplierDocumentaryAddress.E2_RN_NKCountryCode)))
						{
							parent.JE_DeclarantTypeInfo.AddMessageError(Res.GetString("994245EB-4A25-421F-BE87-9A9DF9C9BAA7", "For non-EU office of Supplier it is legally forbidden to use Direct Representation."));
						}
					}
					else if (parent.IsImport)
					{
						if (!parent.ImporterDocumentaryAddress.E2_RN_NKCountryCode.IsEmpty && !euProvider.IsMemberOfEU(Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(parent.ImporterDocumentaryAddress.E2_RN_NKCountryCode)))
						{
							parent.JE_DeclarantTypeInfo.AddMessageError(Res.GetString("5B70AD80-71F8-46B3-9065-420F6CBC6C28", "For non-EU office of Importer it is legally forbidden to use Direct Representation."));
						}
					}
				}
			}
		}

		protected override void CheckJE_OA_DeclarantAddress()
		{
			base.CheckJE_OA_DeclarantAddress();
			if (Parent.IsDeclarantAddressRequired)
			{
				CheckJE_OA_DeclarantAddressIsNotEmpty();
			}
		}

		protected virtual void CheckJE_OA_DeclarantAddressIsNotEmpty()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OA_DeclarantAddressInfo);
		}

		protected override void CheckJE_PaymentMethod()
		{
			base.CheckJE_PaymentMethod();
			CheckJE_PaymentMethodLogicForEU();
			ValidateGuarantees();
		}

		protected virtual void CheckJE_PaymentMethodLogicForEU()
		{
			if ((Parent.JE_PaymentMethod == DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14) // A
				&& ((Parent.Declarant?.Header.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber) ?? ZString.Empty) == ZString.Empty))
			{
				Parent.JE_PaymentMethodInfo.AddMessageError(Res.GetString("7EE1CF89-D962-4F06-BDDD-95FC49A87ECD", "The Declarant requires a Deferment Approval Number (DAN) for the relevant country/region, (Edit Organization > Details > Details > Registration Numbers / Codes)"));
			}

			else if ((Parent.JE_PaymentMethod == DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority // B
				|| Parent.JE_PaymentMethod == DefermentMethodList.Codes.ConsigneesAccountStandingAuthority // C
				|| Parent.JE_PaymentMethod == DefermentMethodList.Codes.ConsigneesAccountConsigneeCompletingTheDeclaration)) // D
			{
				if ((Parent.Importer?.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber) ?? ZString.Empty) == ZString.Empty)
				{
					Parent.JE_PaymentMethodInfo.AddMessageError(Res.GetString("C05D92E9-B06F-4263-A76E-E41D31081681", "The Importer/Consignee requires a Deferment Approval Number (DAN) for the relevant country/region, (Edit Organization > Details > Details > Registration Numbers / Codes)"));
				}
			}
		}

		void ValidateGuarantees()
		{
			var parent = Parent;
			if (parent.CustomsGuarantee != null && parent.JE_MessageType != MessageTypeList.Codes.Export)
			{
				var localCurrency = parent.LocalCurrency;
				var currencyConverter = new CurrencyConverterWithDataProvider(parent.Factory, parent);
				var amountToGuarantee = currencyConverter.ConvertExact(parent.AmountToGuarantee, localCurrency);
				var customsGuaranteeAmount = currencyConverter.ConvertExact(parent.RemainingGuaranteeBalance, localCurrency);

				if (customsGuaranteeAmount.Amount < amountToGuarantee.Amount)
				{
					parent.JE_PaymentMethodInfo.AddMessageError(Res.GetString("86104822-0820-478A-8E78-246B02A44247", "The remaining balance of this guarantee is {0} but the open entries on this declaration require {1}", customsGuaranteeAmount.ToString(), amountToGuarantee.ToString()));
				}
			}
		}

		public static string ValidationMessageForWhenCpcDemandWarehouse => Res.GetString("81E5F17B-69A8-4307-8CF3-82546BC04D85", "At least one invoice line uses a CPC that requires that [49] Customs Warehouse is set. Please select a value.");

		protected override void CheckJE_IATALoadPort()
		{
			base.CheckJE_IATALoadPort();
			if (Parent.IsAir && Parent.IsPersistent)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_IATALoadPortInfo);
			}
		}

		protected override void CheckJE_CustomsOffice()
		{
			base.CheckJE_CustomsOffice();

			var errors = Parent.CustomsOfficeRequirementHelper.Validate();
			errors.ForEach(e => Parent.JE_CustomsOfficeInfo.AddMessageError(e));
		}

		protected override void CheckJE_GoodsOrigin()
		{
			base.CheckJE_GoodsOrigin();
			ValidateIfListIsNotEmpty(Parent.JE_GoodsOriginInfo, (ICodeDescriptionPairList)Parent.Lookups.GoodsOrigin);
		}

		protected override void CheckJE_GoodsDestination()
		{
			base.CheckJE_GoodsDestination();
			ValidateIfListIsNotEmpty(Parent.JE_GoodsDestinationInfo, (ICodeDescriptionPairList)Parent.Lookups.GoodsDestination);

			CheckRuleC0002();
		}

		void ValidateIfListIsNotEmpty(ZPropertyInfo propertyInfo, ICodeDescriptionPairList list)
		{
			if (list.Count > 0)
			{
				ListValidation.MessageErrorIfInvalidCode(propertyInfo, list);
			}
		}

		protected override void CheckJE_ShipmentIncoTerm()
		{
			base.CheckJE_ShipmentIncoTerm();

			var parent = Parent;
			var incoTerm = parent.JE_ShipmentIncoTerm;
			var incotermInfo = parent.JE_ShipmentIncoTermInfo;

			if (IsIncotermRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(incotermInfo);
			}

			if (parent.Configuration.IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice)
			{
				if (parent.Invoices.Cast<JobComInvoiceHeader>().Any(x => x.JZ_IncoTerm != incoTerm))
				{
					parent.JE_ShipmentIncoTermInfo.AddMessageError(Res.GetString("0088EED0-67DB-4D75-A1A4-0613951A56B8", "Incoterm values do not match between Declaration and Invoice Header."));
				}
			}

			IncoTermValidationHelper.ValidateWaterIncoTermAndTransportMode(parent.JE_ShipmentIncoTermInfo, parent.JE_TransportMode);
		}

		protected virtual bool IsIncotermRequired
		{
			get
			{
				return IsIncotermRequiredAsPerRuleC0729(Parent) || IsIncotermRequiredAsPerRuleC0738(Parent);
			}
		}

		bool IsIncotermRequiredAsPerRuleC0729(JobDeclaration jobDeclaration)
		{
			return (ValidationDecider?.IsRuleC0729Active ?? false) && (jobDeclaration.CustomsEntryInstructions.IsNullOrEmpty() || jobDeclaration.CustomsEntryInstructions.Any(x => !subStylesForRuleC0729.Contains(x.CEI_SubStyle) || !proceduresForRuleC0729.Contains(x.CEI_Procedure)));
		}

		readonly List<string> subStylesForRuleC0729 = new List<string>() { EntrySubStyleList.Codes.NormalDeclaration, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA, EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF };
		readonly List<string> proceduresForRuleC0729 = new List<string>() { Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration, Core.Constants.Customs.Universal.RefCusProcedure.Codes._53 };

		bool IsIncotermRequiredAsPerRuleC0738(JobDeclaration jobDeclaration)
		{
			return (ValidationDecider?.IsRuleC0738Active ?? false) && (jobDeclaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.JI_ValuationCode.Equals(ValuationMethodList.Codes._1)));
		}

		protected override void CheckJE_ShipmentIncoTermPlace()
		{
			if (!Parent.JE_ShipmentIncoTermPlace_ReadOnly)
			{
				base.CheckJE_ShipmentIncoTermPlace();

				if (RequireJE_ShipmentIncoTermPlaceMandatory)
				{
					var incoTermPlaceInfo = Parent.JE_ShipmentIncoTermPlaceInfo;
					MandatoryValidation.MessageErrorIfNotEntered(incoTermPlaceInfo);
				}
			}
		}

		protected virtual ZBool RequireJE_ShipmentIncoTermPlaceMandatory => (Parent.AgreedPlaceCodeSupportAndVisible && Parent.EUD_AgreedPlaceCode.Length == 2) || (Parent.AgreedPlaceCodeSupportAndVisible && Parent.ZG_AgreedPlaceCode.Length == 2) || (Parent.AgreedPlaceCodeSupport && Parent.JE_ShipmentIncoTerm == Core.Constants.IncoTerms.Other);

		protected override string MessageErrorOrWarningTotalWeightMustBeGreaterThanTotalOfCustomsQuantitiesCore
		{
			get { return Res.GetString("6D101674-AA80-4D7A-BFAB-BE40655FE378", "Total Gross weight on declaration should be equal to the sum of Gross Weight of individual lines ({0} kg).", Parent.TotalInvoiceLineGrossWeightInKG); }
		}

		protected override bool CompareDeclarationWeightAndTotalInvoiceLineWeight()
		{
			return Parent.TotalInvoiceLineGrossWeightInKG != 0 && Parent.GrossWeight.InKilogramsSafe != Parent.TotalInvoiceLineGrossWeightInKG;
		}

		protected override void CheckJE_TransportIDInland()
		{
			base.CheckJE_TransportIDInland();
			if (Parent.IsSeaInland && Parent.IsTransportMeansImoShipIdentificationNumber)
			{
				var lloydsValidation = new LloydsNumberValidation();
				string humanReadableName = Res.GetString("E0EBB6B0-8A1B-4B17-ABF9-09883323AD9E", "IMO number");
				lloydsValidation.Validate(Parent.JE_TransportIDInland, humanReadableName);
				if (!lloydsValidation.IsValid)
				{
					Parent.JE_TransportIDInlandInfo.AddWarning(lloydsValidation.ErrorText);
				}
			}
		}

		protected override void CheckJE_RN_NKTrailer1Nationality()
		{
			base.CheckJE_RN_NKTrailer1Nationality();
			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.JE_RN_NKTrailer1NationalityInfo);
		}

		protected override void CheckJE_RN_NKTrailer2Nationality()
		{
			base.CheckJE_RN_NKTrailer2Nationality();
			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.JE_RN_NKTrailer2NationalityInfo);
		}

		protected override void CheckJE_TransportMeans()
		{
			base.CheckJE_TransportMeans();
			var parent = Parent;
			if (!parent.JE_TransportMeans.IsEmpty)
			{
				CheckRuleC0623(parent.JE_TransportMeansInfo);
				CheckRuleC0646(parent.JE_TransportMeansInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_TransportMeansInfo);
		}

		protected internal IDeclarationValidationDecider ValidationDecider => Parent.Factory.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedProperty<IDeclarationValidationDecider> validationDeciderCached;

		IDeclarationValidationDecider GetValidationDecider() => Parent?.Configuration.GetValidationDecider(Parent);

		void CheckRuleC0623(ZPropertyInfo info)
		{
			var message = Res.GetString("CD61F89C-2F04-47D7-B2A9-2583E5E108BC", "[C0623] Arrival Transport means must not be entered for this declaration type or requested procedure.");
			var parent = Parent;
			var entryInstructions = parent.CustomsEntryInstructions;

			if ((ValidationDecider?.IsRuleC0623Active ?? false)
				&& entryInstructions
					.Any(x =>
						x.CEI_SubStyle == EntrySubStyleList.Codes.SimplifiedDeclaration
						|| x.CEI_SubStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC
						|| x.CEI_Procedure == Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration))
			{
				if (entryInstructions.Count == 1)
				{
					info.AddMessageError(message);
				}
				else
				{
					info.AddWarning(message);
				}
			}
		}

		void CheckRuleC0646(ZPropertyInfo info)
		{
			var parent = Parent;

			if ((ValidationDecider?.IsRuleC0646Active ?? false) && (parent.JE_TransportMode == Core.Constants.TransportModes.Mail || parent.JE_TransportMode == Core.Constants.TransportModes.Road))
			{
				info.AddMessageError(Res.GetString("30104D2A-43BE-4DF9-BF6C-34ED1EDFA238", "[C0646] Transport ID & Code shouldn't be entered for this transport mode."));
			}
		}

		void CheckRuleC0002()
		{
			var parent = Parent;

			if ((ValidationDecider?.IsRuleC0002Active ?? false)
				&& !parent.JE_GoodsDestination.IsEmpty
				&& parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => !x.ZG_CountryOfDestination.IsEmpty))
			{
				parent.JE_GoodsDestinationInfo.AddMessageError(Res.GetString("256D0357-AF9D-46BB-8FED-735388C42A23", "[C0002] Value can't be entered in both Declaration and Invoice lines."));
			}
		}

		void CheckRuleC0841()
		{
			var subStylesC0841 = new ZString[]
			{
				EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA,
				EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB,
				EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC,
			};
			if ((ValidationDecider?.IsRuleC0841Active ?? false)
				&& Parent.JE_TransportModeInland.IsEmpty
				&& !Parent.JE_CustomsOffice.IsEmpty
				&& Parent.CustomsOffices.Where(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfExit).Any(x => x.CY_Data != Parent.JE_CustomsOffice)
				&& !Parent.CustomsEntryInstructions.Any(x => x.CEI_SubStyle.In(subStylesC0841))
				&& Parent.CustomsEntryHeaders.Any(x => x.CH_EntryStatus == AESEntryStatusList.Codes.Prelodged))
			{
				Parent.JE_TransportModeInlandInfo.AddMessageError(Res.GetString("9C9B6F02-CA13-44FB-AB3D-C7B493D17911", "[C0841] Only for declarations with Sub Style D, E or F the Departure Mode of Transport is optional, in the initial 515 message ELSE required."));
			}
		}

		void CheckRuleC0843()
		{
			var parent = Parent;

			if ((ValidationDecider?.IsRuleC0843Active ?? false)
				&& parent.CustomsOffices.Cast<EuOfficeCode>().Any(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfExit && x.CY_Data != parent.JE_CustomsOffice) && parent.JE_TransportModeInland.IsEmpty
				&& parent.CustomsEntryInstructions.Any(x => !x.CEI_SubStyle.In( new ZString[] { EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC })))
			{
				parent.JE_TransportModeInlandInfo.AddMessageError(Res.GetString("256D0357-AF9D-46BB-8FED-735388C42A24", "[C0843] If declaration office is not equal to exit office AND declaration Sub Style is not equal to D, E or F then the Inland Mode of Transport is required."));
			}
		}
	}
}
