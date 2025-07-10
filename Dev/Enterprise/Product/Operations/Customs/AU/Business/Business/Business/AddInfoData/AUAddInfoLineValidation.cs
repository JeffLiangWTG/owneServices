using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUAddInfoLineValidation : AUAddInfoValidation
	{
		public AUAddInfoLineValidation(AutoAUAddInfo parent)
			: base(parent)
		{
		}

		#region Overrides

		protected override void CheckZA_PRT()
		{
			base.CheckZA_PRT();
			if (!AddInfo.ZA_PRT.IsEmpty && InvoiceLine != null && InvoiceLine.IsNature20)
			{
				AddInfo.ZA_PRTInfo.AddWarning("Since this line is Nature 20 this Rule won't be used in the message.");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		protected override void CheckZA_RNO()
		{
			base.CheckZA_RNO();
			var invoiceLine = InvoiceLine;
			var addInfo = AddInfo;
			var targetInfo = addInfo.ZA_RNOInfo;
			if (invoiceLine.InheritingSchemeFromHeader)
			{
				if (invoiceLine.WillApplyGstScheme)
				{
					targetInfo.AddWarning(Res.GetString(
						"45B16CAD-3459-4E87-AFC6-912E80760222",
						"The system will IGNORE preference and origin data entered for this Invoice line, " +
						"or defaulted from the Invoice header preference and origin fields " +
						"as there are no preferential rates of duty applicable to the tariff item entered " +
						"and the general rate of duty will be applied."
					));
				}
				else
				{
					var headerAddInfo = invoiceLine.InvoiceHeader.AddInfo;
					if (headerAddInfo != null && !addInfo.Lookups.ZA_PST_List.ContainsCode(headerAddInfo.ZA_PST))
					{
						targetInfo.AddMessageError(
							Res.GetString(
								"A8F3116E-5DB4-4E11-AD21-7ACC9D26B2FF",
								"The system cannot default preference for this line because the Invoice header preference is not applicable, " +
								"but there are other preferential rates that may be applicable, or no rate (including the general rate) is applicable."
						));
					}
				}
			}
		}

		protected override void CheckZA_VID()
		{
			base.CheckZA_VID();

			var addInfo = AddInfo;
			if (!addInfo.ZA_VID.IsEmpty)
			{
				var part = InvoiceLine?.Part;
				if (part != null)
				{
					var cachedDeclaration = JobDeclaration;
					var importer = cachedDeclaration?.Importer;

					if (importer != null && cachedDeclaration.SupportsBondedWarehousing)
					{
						var partAttributeManager = importer.PartAttributeManager;

						if (partAttributeManager.VINPartAttribute == null)
						{
							addInfo.ZA_VIDInfo.AddError(VIDErrorMessages.SetupVINOnImporter);
						}
						else if (!partAttributeManager.HasVINForProduct(part))
						{
							addInfo.ZA_VIDInfo.AddError(VIDErrorMessages.SetupVINOnPart);
						}
						else
						{
							addInfo.ZA_VIDInfo.AddError(VIDErrorMessages.EnterVINOnLine);
						}
					}
				}
			}
		}

		public static class VIDErrorMessages
		{
			public const string SetupVINOnImporter = AutomationEnabled + "Please edit the Consignee and set up Part Attribute 1 or Part Attribute 2 as the VIN field.";
			public const string SetupVINOnPart = AutomationEnabled + "Please edit the Part on the Invoice Line and enable it for VINs.";
			public const string EnterVINOnLine = AutomationEnabled + "On each invoice line, enter a single VIN number in the VIN field. Leave the VIN field in additional info empty.";
			public const string AutomationEnabled = "You have Bonded Warehousing Automation enabled for this Consignee. ";
		}

		protected override void CheckZA_WRL()
		{
			base.CheckZA_WRL();
			if (JobDeclaration != null && InvoiceLine != null)
			{
				if (!JobDeclaration.IsNature30 && !AddInfo.ZA_WRL.IsEmpty && !JobDeclaration.IsWarehousedByExternalAgent)
				{
					AddInfo.ZA_WRLInfo.AddMessageError("Warehouse reference line is only required for a nature 30.");
				}
			}
		}

		protected override void CheckZA_EFD()
		{
			base.CheckZA_EFD();
			ValidateHeaderLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_EFDInfo);
		}

		protected override void CheckZA_DMP()
		{
			base.CheckZA_DMP();
			if (AddInfo.ZA_DMP < 0)
			{
				AddInfo.ZA_DMPInfo.AddMessageError("Amount may not be less than zero");
			}
		}

		protected override void CheckZA_DRE()
		{
			base.CheckZA_DRE();
			if (AddInfo.ZA_DRE < 0)
			{
				AddInfo.ZA_DREInfo.AddMessageError("Amount may not be less than zero");
			}
		}

		protected override void CheckZA_DXP()
		{
			var errorMessage = ZString.Empty;
			var warningMessage = ZString.Empty;
			base.CheckZA_DXP();

			if (!AddInfo.ZA_DXP.IsEmpty)
			{
				try
				{
					if (char.IsDigit(AddInfo.ZA_DXP[AddInfo.ZA_DXP.Length - 1]))
					{
						foreach (char aChar in AddInfo.ZA_DXP)
						{
							if (char.IsLetter(aChar))
							{
								errorMessage = "Incorrect format: Expected amount currency [nnnn.nn aaa]";
								break;
							}
						}
						warningMessage = "No currency code detected, defaulting to AUD";
					}
					else
					{
						string entry = AddInfo.ZA_DXP.Replace(" ", "");
						string amount = entry.Substring(0, entry.Length - 3);
						string currency = entry.Substring(entry.Length - 3); //the last three chars is currency

						try
						{
							var amountInDecimal = ZDecimal.Parse(amount);
							errorMessage = ValidateAmountAndGetErrorMessage(amountInDecimal, false);
							errorMessage += ValidateCurrencyAndGetErrorMessage(currency);
						}
						catch (FormatException)
						{
							if (amount == ZString.Empty)
							{
								errorMessage = DXPZeroAmountErrorMessage;
							}
							else
							{
								errorMessage = amount + DXPInvalidAmountErrorMessage;
							}
						}
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					errorMessage = "Incorrect Format.  Expected '<value> <currencycode>";
				}
			}

			if (!errorMessage.IsEmpty)
			{
				AddInfo.ZA_DXPInfo.AddMessageError(errorMessage);
			}
			else if (!warningMessage.IsEmpty)
			{
				AddInfo.ZA_DXPInfo.AddWarning(warningMessage);
			}
		}
		public const string DXPZeroAmountErrorMessage = "Amount must be entered";
		public const string DXPInvalidAmountErrorMessage = " is not a valid number";

		protected override void CheckZA_DTY()
		{
			base.CheckZA_DTY();
			if (AddInfo.ZA_DTY < 0)
			{
				AddInfo.ZA_DTYInfo.AddMessageError("Amount may not be less than zero");
			}
		}

		protected override void CheckZA_ADJ()
		{
			base.CheckZA_ADJ();
			ValidateADJInRightFormat();
			ValidateZA_ValuationBasis_Hidden();
		}

		protected override void CheckZA_ISS()
		{
			base.CheckZA_ISS();
			if (!AddInfo.ZA_ISSInfo.HasNotifications())
			{
				ValidateISSInRightFormat();
			}
		}

		protected override void CheckZA_LCTQ()
		{
			base.CheckZA_LCTQ();
			if (!AddInfo.ZA_LCTQ.IsEmpty && AddInfo.ZA_LCTQ != "Y")
			{
				AddInfo.ZA_LCTQInfo.AddMessageError(AddInfo.ZA_LCTQ + " is not valid. If you want a quotation, please enter 'Y' as a value.");
			}
		}

		protected override void CheckZA_LCTE()
		{
			base.CheckZA_LCTE();
			ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_LCTEInfo, Lookups.CMRLCTEList);
		}

		protected override void CheckZA_MLP()
		{
			base.CheckZA_MLP();
			var value = AddInfo.ZA_MLP;
			if (!value.IsEmpty && (value.Length != 4 || !value.Left(3).IsNumbersOnlyOrEmpty || !value.Right(1).IsLettersOnlyOrEmpty))
			{
				AddInfo.ZA_MLPInfo.AddMessageError(value + " is not in the correct format.");
			}
		}

		protected override void CheckZA_ODF()
		{
			base.CheckZA_ODF();
			if (AddInfo.ZA_ODF < 0)
			{
				AddInfo.ZA_ODFInfo.AddMessageError("Amount may not be less than zero");
			}
		}

		protected override void CheckZA_MD2()
		{
			base.CheckZA_MD2();
			var value = AddInfo.ZA_MD2;
			if (!value.IsEmpty && (value.Length != 6 || !value.IsNumbersOnlyOrEmpty))
			{
				AddInfo.ZA_MD2Info.AddMessageError(value + " is not in the correct format.");
			}
		}

		protected override void CheckZA_TC2()
		{
			base.CheckZA_TC2();
			var value = AddInfo.ZA_TC2;
			if (!value.IsEmpty && (!((ZInt)value.Length).IsInRange(1, 7) || !value.IsNumbersOnlyOrEmpty))
			{
				AddInfo.ZA_TC2Info.AddMessageError(value + " is not in the correct format.");
			}
		}

		protected override void CheckZA_QT2()
		{
			base.CheckZA_QT2();
			if (AddInfo.ZA_QT2 < 0)
			{
				AddInfo.ZA_QT2Info.AddMessageError("Amount may not be less than zero");
			}
			if (AddInfo.ZA_QT2 > 0 && AddInfo.ZA_UQ2.IsEmpty)
			{
				AddInfo.ZA_QT2Info.AddMessageError("Second Unit Quantity is empty. Either select a tariff that has a second unit of quantity or delete the second quantity.");
			}

			ValidateZA_UQ2();
		}

		protected override void CheckZA_UQ2()
		{
			base.CheckZA_UQ2();
			ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_UQ2Info, Lookups.QuantityUnits);

			JobComInvoiceLine invoiceLine = AddInfo.InvoiceLine;
			if (invoiceLine != null)
			{
				var secondUQ = ((JobComInvoiceLineValidation)invoiceLine.Validation).SecondUQ;
				if (secondUQ != AddInfo.ZA_UQ2)
				{
					AddInfo.ZA_UQ2Info.AddWarning(ZString.Format("The tariff requires a different second unit of quantity, {0}", secondUQ.IsEmpty ? "an empty UQ" : (string)secondUQ));
				}
			}

			ValidateZA_QT2();
		}

		protected override void CheckZA_STD()
		{
			base.CheckZA_STD();
			CheckValidEdificeRange(AddInfo.ZA_STDInfo);
		}

		protected override void CheckZA_TAN()
		{
			base.CheckZA_TAN();
			var value = AddInfo.ZA_TAN;
			if (!value.IsEmpty && (!((ZInt)value.Length).IsInRange(1, 10) || !value.IsNumbersOnlyOrEmpty))
			{
				AddInfo.ZA_TANInfo.AddMessageError(value + " is not in the correct format.");
			}
		}

		protected override void CheckZA_TFQ()
		{
			base.CheckZA_TFQ();
			ValidateZA_QSC();
		}

		protected override void CheckZA_WRQ()
		{
			base.CheckZA_WRQ();
			CheckValidEdificeRange(AddInfo.ZA_WRQInfo);
		}

		protected override void CheckZA_WRU()
		{
			base.CheckZA_WRU();
			ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_WRUInfo, Lookups.QuantityUnits);
			ValidateZA_WRQ();
		}

		protected override void CheckZA_WUV()
		{
			base.CheckZA_WUV();
			if (!AddInfo.ZA_WUV.IsEmpty)
			{
				if (AggregateAddInfoParent is JobComInvoiceLine && !(AddInfo.ZA_IsPackToBondForLine_Hidden == "Y" || JobDeclaration.IsNature30 || JobDeclaration.IsWarehousedByExternalAgent))
				{
					AddInfo.ZA_WUVInfo.AddMessageError("WUV is not allowed on nature 10");
				}
			}
		}

		protected override void CheckZA_WET()
		{
			base.CheckZA_WET();
			CheckValidEdificeRange(AddInfo.ZA_WETInfo);
		}

		protected override void CheckZA_WETQ()
		{
			base.CheckZA_WETQ();
			if (AddInfo.ZA_WETQ == "Y")
			{
				if (JobDeclaration != null)
				{
					if (JobDeclaration.Importer == null || JobDeclaration.Importer.LocalBusinessRegNo.IsEmpty || !JobDeclaration.Importer.IsValidABN)
					{
						AddInfo.ZA_WETQInfo.AddMessageError("The importer must have a valid ABN configured.");
					}
				}
			}
			else if (!AddInfo.ZA_WETQ.IsEmpty)
			{
				AddInfo.ZA_WETQInfo.AddMessageError("If you want to quote for WET, please enter 'Y' as a value");
			}
		}

		protected override void CheckZA_WETE()
		{
			base.CheckZA_WETE();
			ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_WETEInfo, Lookups.ZA_WETE_List);
		}

		protected override void CheckZA_TreatmentCode_Hidden()
		{
			base.CheckZA_TreatmentCode_Hidden();
			if (InvoiceLine != null && !InvoiceLine.IsExport)
			{
				ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_TreatmentCode_HiddenInfo, Lookups.TreatmentCodeList);
			}
			ValidateZA_RNO();
		}

		QuarantineExDocHeader ExDocHeader
		{
			get
			{
				return Parent.IsQuarantine && InvoiceLine != null && InvoiceLine.InvoiceHeader != null ?
					InvoiceLine.InvoiceHeader.QuarantineExDocHeader : null;
			}
		}

		protected override void CheckZA_AUState_Hidden()
		{
			base.CheckZA_AUState_Hidden();
			if (Parent.IsQuarantine)
			{
				var exDocHeader = ExDocHeader;
				if (exDocHeader != null)
				{
					if (exDocHeader.QH_ObtainExportCustomsPermit)
					{
						List<string> productsThatMustHaveState = new List<string> { EXDOCCommodityCodes.Codes.GrainsAndPlants, EXDOCCommodityCodes.Codes.Horticulture, EXDOCCommodityCodes.Codes.Meat, EXDOCCommodityCodes.Codes.SkinsAndHides, EXDOCCommodityCodes.Codes.InedibleMeat, EXDOCCommodityCodes.Codes.Wool };
						if (productsThatMustHaveState.Contains(exDocHeader.QH_ProduceType))
						{
							MandatoryValidation.MessageErrorIfNotEntered(AddInfo.ZA_AUState_HiddenInfo, "AU State");
						}
					}
					else
					{
						if (exDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Dairy)  // state is optional for Dairy when QH_ObtainExportCustomsPermit is false
						{
							MandatoryValidation.MessageErrorIfIsEntered(AddInfo.ZA_AUState_HiddenInfo, "AU State");
						}
					}
				}
			}
			else if (Parent.IsExport)
			{
				if (!Parent.ZA_AUState_Hidden.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.ZA_AUState_HiddenInfo, Parent.Lookups.ZA_AUStatesList);
				}
				if (Parent.InvoiceLine.IsAUOrigin && Parent.ZA_AUState_Hidden.IsEmpty && !Parent.ZA_AUState_HiddenInfo.HasMessageErrors())
				{
					Parent.ZA_AUState_HiddenInfo.AddMessageError("An AU State is required.");
				}
			}
		}

		protected override void CheckZA_AQISCustomsWt_Hidden()
		{
			base.CheckZA_AQISCustomsWt_Hidden();
			var exDocHeader = ExDocHeader;
			if (exDocHeader != null)
			{
				List<string> productsThatMayHaveLineWeight = new List<string> { EXDOCCommodityCodes.Codes.GrainsAndPlants, EXDOCCommodityCodes.Codes.SkinsAndHides, EXDOCCommodityCodes.Codes.Dairy, EXDOCCommodityCodes.Codes.Horticulture };
				if (!exDocHeader.QH_ObtainExportCustomsPermit || !productsThatMayHaveLineWeight.Contains(exDocHeader.QH_ProduceType))
				{
					MandatoryValidation.MessageErrorIfIsEntered(AddInfo.ZA_AQISCustomsWt_HiddenInfo, "Customs Weight");
				}
			}
		}

		protected override void CheckZA_AQISCustomsWtUQ_Hidden()
		{
			base.CheckZA_AQISCustomsWtUQ_Hidden();
			ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_AQISCustomsWtUQ_HiddenInfo, Lookups.AqisCustomsWeightUqList);
		}

		#endregion

		#region Implementation

		/// <summary>
		/// Validate the properties invalid for line level that have non-default values.
		/// </summary>
		protected void ValidateHeaderLevelPropertyDoesntHaveNonDefaultValue(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.Value.IsDefault)
			{
				if (AddInfo.LineLevelAddInfo && propertyInfo.Name == AUAddInfo.Schema.ZA_EFD)
				{
					propertyInfo.AddMessageError(propertyInfo.Name.Substring(3) + " is not a valid field for invoice lines.");
				}
			}
		}

		protected void ValidateISSInRightFormat()
		{
			string errorMessage = null;
			if (AddInfo.ZA_ISS >= 100m || AddInfo.ZA_ISS < 0)
			{
				errorMessage = "The strength should be between 0 and 100.";
			}
			else if (AddInfo.ZA_ISS.ToString().Length > 5)
			{
				errorMessage = "Maximum length of invoice spirit strength is 5";
			}
			if (errorMessage != null)
			{
				AddInfo.ZA_ISSInfo.AddMessageError(errorMessage);
			}
		}

		protected void ValidateADJInRightFormat()
		{
			var errorMessage = ZString.Empty;
			if (!AddInfo.ZA_ADJ.IsEmpty)
			{
				errorMessage = ValidateAmountAndGetErrorMessage(AddInfo.AdjustmentAmount_Hidden, true);
				if (AddInfo.AdjustmentDollarPercentage_Hidden != "%")
				{
					errorMessage += ValidateCurrencyAndGetErrorMessage(AddInfo.AdjustmentCurrency_Hidden);
				}
			}
			if (!errorMessage.IsEmpty)
			{
				AddInfo.ZA_ADJInfo.AddMessageError(errorMessage);
			}
		}

		#endregion

		protected const string LitresOfAlcohol = "LA";
	}
}
