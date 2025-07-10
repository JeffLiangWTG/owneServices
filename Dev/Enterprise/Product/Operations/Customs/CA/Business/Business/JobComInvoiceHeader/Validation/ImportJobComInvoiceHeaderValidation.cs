using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	class ImportJobComInvoiceHeaderValidation : CommonImportJobComInvoiceHeaderValidation
	{
		public ImportJobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		CADeclarationValidator DeclarationValidator
		{
			get { return Parent.JobDeclaration?.DeclarationValidator; }
		}

		#region CheckJZ_InvoiceNumber (common)

		protected override void CheckJZ_InvoiceNumber()
		{
			if (!Parent.JZ_InvoiceNumber.IsEmpty && Parent.IsAttachedToPersistentLVSDeclaration)
			{
				ValidateLVSID();
			}
			else
			{
				base.CheckJZ_InvoiceNumber();
			}
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_InvoiceNumberInfo);
			var declaration = Parent.JobDeclaration;
			if (declaration != null && declaration.IsIID)
			{
				if (declaration.CargoControlNumbers.Count > 1 && !Parent.CargoControlNumbersList.Any())
				{
					Parent.JZ_InvoiceNumberInfo.AddMessageError(Res.GetString("72193266-8EE5-452F-A528-F5D240B9FC63", "Please enter at least one CCN in the Cargo Control Numbers grid when there are multiple CCNs entered on the Packing tab > Cargo Control Numbers"));
				}
			}
		}

		void ValidateLVSID()
		{
			var declaration = Parent.JobDeclaration;
			if (declaration != null)
			{
				var retriever = new Customs.Business.DuplicateInvoiceNumberRetriever(declaration.Factory, false);
				var duplicates = retriever.RetrieveDuplicateJobNumbers(declaration, Parent, new ZString[] { JobMessageTypeList.Codes.LowValueShipments, JobMessageTypeList.Codes.LVSForConsolidation });
				if (!duplicates.IsEmpty)
				{
					Parent.JZ_InvoiceNumberInfo.AddWarning(Res.GetString("A8566924-7405-4FB9-8BD9-8508E20047A1", "This LVS ID has already been used on job(s): {0}", duplicates));
				}
				else
				{
					var query = new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, Parent.JZ_InvoiceNumber);
					query.AddToFilter(JobComInvoiceHeaderSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
					if (declaration.Invoices.Find(query).Any())
					{
						Parent.JZ_InvoiceNumberInfo.AddMessageError(Res.GetString("449B18DF-CD6A-4CFC-9758-48AED34992F0", "This LVS ID has already been used on this job"));
					}
				}
			}
		}

		#endregion

		#region CheckJZ_OH_Buyer (validate if entered)

		protected override void CheckJZ_OH_Buyer()
		{
			base.CheckJZ_OH_Buyer();
			if (Parent.IsAttachedToPersistentLVXDeclaration)
			{
				var declaration = Parent.FirstAdditionalDeclaration;
				if (declaration != null && declaration.JE_MessageSubType == LowValueShipmentsTypes.Codes.ConsolidationByImporter
					&& declaration.JE_OH_Importer != Parent.JZ_OH_Buyer)
				{
					Parent.JZ_OH_BuyerInfo.AddMessageError(MSIImporterMatchWarning);
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_OH_BuyerInfo);
				}

				if (Parent.Buyer != null)
				{
					if (!Parent.Buyer.OH_Code.IsEmpty && Parent.Buyer.CompanyData.OB_IsDebtor)
					{
						Parent.Buyer.CreditChecker.ValidateIsCreditLimitExceeded(Parent.JZ_OH_BuyerInfo, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
					}
				}
			}
			else if (Parent.IsAttachedToPersistentLVSDeclaration)
			{
				var declaration = Parent.JobDeclaration;
				if (declaration.JE_MessageSubType == LowValueShipmentsTypes.Codes.TotalConsolidation)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_OH_BuyerInfo);
				}
			}

			if (Parent.IsAttachedToPersistentLVSDeclaration)
			{
				if (!Parent.JZ_OH_Buyer.IsEmpty && Parent.Buyer != null)
				{
					CAAddressValidator.ValidateMandatory(Parent.Buyer.MainAddress, Parent.JZ_OH_BuyerInfo, Res.GetString("24165825-6721-486e-8522-0AEEC4B88A5A", "Purchaser Main"));
				}
			}
		}

		static string MSIImporterMatchWarning
		{
			get { return Res.GetString("c34b6c9c-ad21-44a5-80b6-c2cb8fbe8837", "Importer specified on the shipment should match the header importer"); }
		}

		#endregion

		#region CheckJZ_RN_NKDefaultOrigin (common)

		protected override void CheckJZ_RN_NKDefaultOrigin()
		{
			base.CheckJZ_RN_NKDefaultOrigin();
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_RN_NKDefaultOriginInfo);
		}

		#endregion

		#region CheckJZ_InvoiceDate (ACROSS)

		protected override void CheckJZ_InvoiceDate()
		{
			base.CheckJZ_InvoiceDate();
			DeclarationValidator?.MessageErrorIfNotEntered(Parent.JZ_InvoiceDateInfo, string.Empty, ValidateForMessageType.ACROSS);

			if (Parent.JZ_InvoiceDate.IsValid)
			{
				if (Parent.JZ_ValuationDateOverride.IsValid && Parent.JZ_InvoiceDate > Parent.JZ_ValuationDateOverride)
				{
					Parent.JZ_InvoiceDateInfo.AddWarning(InvoiceDateCannotBeGreaterThanDirectShipmentDate);
				}
				Parent.Validation.ValidateJZ_ValuationDateOverride();

				ZDateTime timeInOttawa = EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("CAOTT", ZDateTime.UtcNow.ToDateTime());
				if (Parent.JZ_InvoiceDate.Date > timeInOttawa.Date)
				{
					Parent.JZ_InvoiceDateInfo.AddWarning(InvoiceDateIsInThefuture);
				}
			}
		}

		internal static string InvoiceDateCannotBeGreaterThanDirectShipmentDate
		{
			get { return Res.GetString("C22407E5-54DC-448B-8F64-065BA6CAD90B", "Invoice Date cannot be after Direct Shipment Date"); }
		}

		internal static string InvoiceDateIsInThefuture
		{
			get { return Res.GetString("CD6A0D4C-47F8-40DE-A1C1-529EC18EC746", "Invoice Date is in the future."); }
		}

		#endregion

		#region CheckJZ_Weight (ACROSS)

		protected override void CheckJZ_Weight()
		{
			base.CheckJZ_Weight();
			var declaration = Parent.JobDeclaration;
			if (declaration != null)
			{
				if (DeclarationValidator.IsValidationRequired(ValidateForMessageType.ACROSS))
				{
					MandatoryValidation.MessageErrorIfIsZero(Parent.JZ_WeightInfo);
					MandatoryValidation.MessageErrorIfIsNegative(Parent.JZ_WeightInfo);
				}
				if (DeclarationValidator.IsValidationRequired(ValidateForMessageType.B3CUSDEC) && !declaration.IsLVS)
				{
					var entryHeader = declaration.B3EntryHeader;
					if (entryHeader != null && entryHeader.GrossWeight.IsEmpty && IsGrossWeightRequired(entryHeader))
					{
						Parent.JZ_WeightInfo.AddMessageError(GrossWeightRequired);
					}
				}
			}
		}

		internal static string GrossWeightRequired
		{
			get { return Res.GetString("EADEA51A-4BC4-4917-B9E3-B7377A56310F", "A gross weight is required for this shipment. Please enter gross weight on Invoice Headers or on the Declaration tab."); }
		}

		bool IsGrossWeightRequired(CusEntryHeader entryHeader)
		{
			var declaration = Parent.JobDeclaration;
			return (declaration.IsAir || declaration.IsSea) &&
							entryHeader.CustomsValue >= JobComInvoiceHeader.VFDLimit &&
							declaration.Invoices.Cast<JobComInvoiceHeader>().Any(invoiceHeader => invoiceHeader.IsUSorTerritory);
		}

		#endregion

		#region CheckJZ_WeightUQ (ACROSS)

		protected override void CheckJZ_WeightUQ()
		{
			base.CheckJZ_WeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_WeightUQInfo, Parent.Lookups.JZ_WeightUQ_List);
			if (!Parent.JZ_Weight.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_WeightUQInfo);
			}
		}

		#endregion

		#region CheckJZ_NetWeight (common)

		protected override void CheckJZ_NetWeight()
		{
			base.CheckJZ_NetWeight();
			CompareValidation.CheckNumberNotNegative(Parent.JZ_NetWeightInfo);
		}

		protected override void CheckJZ_NetWeightUQ()
		{
			base.CheckJZ_NetWeightUQ();
			if (Parent.JZ_NetWeight > 0 && Parent.JZ_NetWeightUQ.IsEmpty)
			{
				Parent.JZ_NetWeightUQInfo.AddMessageError(Res.GetString("D5372636-49E3-4367-8FCB-B57B0F1AE42D", "Net weight unit must be entered."));
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JZ_NetWeightUQInfo, Parent.Lookups.JZ_WeightUQ_List);
			}
		}

		#endregion

		#region CheckJZ_ValuationDateOverride

		protected override void CheckJZ_ValuationDateOverride()
		{
			base.CheckJZ_ValuationDateOverride();
			var declaration = Parent.JobDeclaration;
			var isDateRequired = !Parent.CA_RL_NKLastPort.IsEmpty || Parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(c => c.GACPGAHeader != null);

			if (isDateRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_ValuationDateOverrideInfo);
			}
			else if (declaration != null && !declaration.JE_ExportDate.IsValid)
			{
				DeclarationValidator.MessageErrorIfNotEntered(Parent.JZ_ValuationDateOverrideInfo, DirectShipmentDateRequired, ValidateForMessageType.B3CUSDEC);

				if (declaration.IsAQ)
				{
					DeclarationValidator.MessageErrorIfNotEntered(Parent.JZ_ValuationDateOverrideInfo, DirectShipmentDateRequired, ValidateForMessageType.ACROSS);
				}
			}

			if (declaration != null
				&& !declaration.IsLVS && Parent.JZ_ValuationDateOverride > declaration.JE_EntryAuthorisationDate
				&& DeclarationValidator.IsValidationRequired(ValidateForMessageType.B3CUSDEC))
			{
				Parent.JZ_ValuationDateOverrideInfo.AddMessageError(DirectShipmentDateCannotBeGreaterThanRelaseDate);
			}

			if (Parent.JZ_InvoiceDate.IsValid && Parent.JZ_ValuationDateOverride.IsValid && Parent.JZ_InvoiceDate > Parent.JZ_ValuationDateOverride)
			{
				Parent.JZ_ValuationDateOverrideInfo.AddWarning(InvoiceDateCannotBeGreaterThanDirectShipmentDate);
			}

			Parent.Validation.ValidateJZ_InvoiceDate();
		}

		internal static string DirectShipmentDateCannotBeGreaterThanRelaseDate
		{
			get { return Res.GetString("D28E2771-6962-4A0B-B501-3DBAA2BECFC6", "Direct Shipment Date cannot be after Actual Release Date"); }
		}

		string DirectShipmentDateRequired
		{
			get
			{
				return Parent.IsAttachedToPersistentLVSDeclaration
					? Res.GetString("24ef45bc-9695-4451-96a9-f1e6ebf28ec3", "Direct Shipment Date, which is required for LVS calculations.")
					: Res.GetString("69BF3BA9-0517-4B59-8D08-26D35B0CD6AC", "Direct Shipment Date, which is required when no ATD is entered on the Declaration tab.");
			}
		}

		#endregion

		#region CheckJZ_InvoiceAmount

		protected override void CheckJZ_InvoiceAmount()
		{
			base.CheckJZ_InvoiceAmount();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_InvoiceAmountInfo);

			if ((Parent.IsSimplifiedLVSMode && !Parent.JZ_InvoiceAmount.IsEmpty)
				|| (!Parent.IsSimplifiedLVSMode && Parent.IsAttachedToPersistentLVXDeclaration))
			{
				if (!Parent.JobDeclaration.ApportionmentDirty && Parent.JZ_Calc_Balance.Round(2) != 0.00m)
				{
					Parent.JZ_InvoiceAmountInfo.AddMessageError(UnbalancedInvoiceMessage);
				}
			}
		}

		protected override void CheckJZ_Calc_BalanceCore()
		{
			if (Parent.IsSimplifiedLVSMode || Parent.IsAttachedToPersistentLVXDeclaration)
			{
				ValidateJZ_InvoiceAmount();
			}
			else
			{
				base.CheckJZ_Calc_BalanceCore();
			}
		}

		#endregion

		#region Suppresses base validation (common)

		protected override void CheckJZ_CU_RelatedHouseBill()
		{
			//not required for CA
		}

		protected override void CheckJZ_Calc_CIFAmount()
		{
			//no validation needed
		}

		#endregion

		#region CheckTotalValueForDuty

		protected override void CheckTotalValueForDuty()
		{
			base.CheckTotalValueForDuty();
			if (Parent.IsAttachedToPersistentLVSDeclaration)
			{
				var taxOrFee = new RefCusTaxOrFee.Loader(Parent.Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Canada, Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, Parent.EffectiveValuationDate.Date);

				var actualValue = Parent.TotalValueForDuty.Round(2);
				if (taxOrFee != null && actualValue > taxOrFee.ZZF_Value)
				{
					Parent.TotalValueForDutyInfo.AddError(Res.GetString("97274dcc-3fa8-4d74-8943-55ddcd7a4fc1", "Total value for duty of Low Value Shipment should not exceed {0} CAD, but it is {1} CAD. If the entered amount is wrong then correct it and then run apportionment from the brokerage menu (if available) or press the Calculate Duty button if using the wizard.",
						taxOrFee.ZZF_Value, actualValue));
				}
			}
		}

		#endregion

		#region CheckJZ_OA_ManufacturerAddress

		protected override void CheckJZ_OA_ManufacturerAddress()
		{
			base.CheckJZ_OA_ManufacturerAddress();
			var declaration = Parent.JobDeclaration;
			if (Parent.JZ_OA_ManufacturerAddress.IsEmpty)
			{
				if (declaration != null && declaration.IsOGD && declaration.CA_OGDTC)
				{
					Parent.JZ_OA_ManufacturerAddressInfo.AddMessageError(Res.GetString("C5287246-24CD-4e3c-981F-B0AF230B9B6A", "Manufacturer is required for Transport Canada (Tires) shipments"));
				}
			}
			else if (Parent.ManufacturerAddress != null)
			{
				CAAddressValidator.ValidateMandatory(Parent.ManufacturerAddress, Parent.JZ_OA_ManufacturerAddressInfo, Res.GetString("5F70E203-93A6-447b-BC99-8A31A61B164A", "Manufacturer"));
				if (declaration?.IsIID ?? false)
				{
					var requiredMessageAdded = false;
					var manufactureFallBackInvoices = Parent.InvoiceLines.OfType<JobComInvoiceLine>().Where(x => x.JI_OA_ManufacturerAddress == Parent.JZ_OA_ManufacturerAddress);
					if (Parent.HasInvoiceLinesWithWENIndOnECCCPGA || Parent.HasInvoiceLinesWithPESIndOnHCPGA)
					{
						if (manufactureFallBackInvoices.Any(x => new PGAInvoiceLineValidator(x).IsJI_OA_ManufacturerAddressPhoneAndEmailRequired))
						{
							OrganisationValidation.ValidateCAPContactOrAddressPhoneAndEmailRequired(Parent.JZ_OA_ManufacturerAddressInfo, Parent.ManufacturerAddress);
							requiredMessageAdded = true;
						}
					}

					if (!requiredMessageAdded && Parent.HasInvoiceLinesWithCPRIndOnHCPGA && manufactureFallBackInvoices.Any(x => new PGAInvoiceLineValidator(x).IsAddressPhoneOrEmailRecommended))
					{
						OrganisationValidation.ValidateCAPContactOrAddressPhoneAndEmailRecommened(Parent.JZ_OA_ManufacturerAddressInfo, Parent.ManufacturerAddress);
					}
				}
				OrganisationValidation.ValidateNamesAndAddressesSpecial(Parent.JZ_OA_ManufacturerAddressInfo, Parent.ManufacturerAddress);
			}
		}

		#endregion

		#region CheckJZ_NoOfPacks

		protected override void CheckJZ_NoOfPacks()
		{
			base.CheckJZ_NoOfPacks();

			var invoiceHeader = Parent;
			var declaration = invoiceHeader.JobDeclaration;

			if (invoiceHeader.JZ_NoOfPacks == 0
				&& declaration != null
				&& (declaration.SupportsChcPivotBetweenInvoiceLineAndPacking || declaration.SupportsChzPivotBetweenInvoiceHeaderAndPacking)
				&& declaration.Packages.Any())
			{
				var message = declaration.CA_RequiresMerge
					? Res.GetString("E6D5430C-BBE7-4A74-AEB6-45BF5E7666CF", "Merge has not occurred.\r\nPlease ensure this invoice or every line must have packages ticked and each amount must be greater than zero.\r\nThen save the job and try again.")
					: Res.GetString("F5C8626A-6D99-4B19-A130-F8D321AE57BB", "Please ensure this invoice or every line must have packages ticked and each amount must be greater than zero.");

				invoiceHeader.JZ_NoOfPacksInfo.AddMessageError(message);
			}
		}

		#endregion

		protected override ZBool ShouldCheckDuplicate
		{
			get
			{
				var declaration = Parent.JobDeclaration;
				return declaration != null && !declaration.IsIM2;
			}
		}
	}
}
