using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.CA;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	class ImportAddInfoJobComInvoiceHeaderValidation : CommonImportAddInfoJobComInvoiceHeaderValidation
	{
		public ImportAddInfoJobComInvoiceHeaderValidation(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		CADeclarationValidator DeclarationValidator
		{
			get { return InvoiceHeader.JobDeclaration?.DeclarationValidator; }
		}

		#region CheckCA_PortOfClearance (LVS)

		protected override void CheckCA_PortOfClearance()
		{
			base.CheckCA_PortOfClearance();
			if (InvoiceHeader.IsAttachedToPersistentLVXDeclaration)
			{
				MandatoryValidation.MessageErrorIfNotEntered(InvoiceHeader.CA_PortOfClearanceInfo);
			}
			if (InvoiceHeader.IsAttachedToPersistentLVSDeclaration)
			{
				ListValidation.MessageErrorIfInvalidCode(InvoiceHeader.CA_PortOfClearanceInfo);
			}
		}

		#endregion

		#region CheckCA_RN_NKExport (common)

		protected override void CheckCA_RN_NKExport()
		{
			base.CheckCA_RN_NKExport();
			if (InvoiceHeader.IsAttachedToPersistentLVSDeclaration)
			{
				ListValidation.MessageErrorIfInvalidCode(InvoiceHeader.CA_RN_NKExportInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(InvoiceHeader.CA_RN_NKExportInfo);
			}

			var declaration = Parent.Declaration;
			if (declaration != null
				&& declaration.JE_MessageType != JobMessageTypeList.Codes.LowValueShipments
				&& !declaration.IsOtherWarehouseEntry
				&& declaration.CalculatedFreightAmount.IsEmpty
				&& DeclarationValidator.IsValidationRequired(ValidateForMessageType.B3CUSDEC)
				&& InvoiceHeader.IsUSCountryOfExport
				&& InvoiceHeader.OverseasFreight.IsEmpty)
			{
				InvoiceHeader.CA_RN_NKExportInfo.AddMessageError(FreightChargeIsRequired);
			}
		}

		internal static string FreightChargeIsRequired
		{
			get { return Res.GetString("C13AFCC6-9EB5-4ADF-8609-568F12B4EC2E", "Freight Amount Required."); }
		}

		#endregion

		#region CheckCA_RL_NKLastPort and Date (ACROSS)

		protected override void CheckCA_RL_NKLastPort()
		{
			base.CheckCA_RL_NKLastPort();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_RL_NKLastPortInfo, Lookups.LastPorts);
			var declaration = Parent.Declaration;
			if (declaration != null)
			{
				if (declaration.JE_RL_NKPortOfLoading.IsEmpty && declaration.IsAQ)
				{
					DeclarationValidator.MessageErrorIfNotEntered(Parent.CA_RL_NKLastPortInfo, LastPortRequired, ValidateForMessageType.ACROSS);
				}
				if (declaration.IsIID)
				{
					if (Parent.CA_RL_NKLastPort.IsEmpty && declaration.HasInvoiceLinesWithGACPGA)
					{
						Parent.CA_RL_NKLastPortInfo.AddMessageError(LastPortRequiredIIDWithGAC);
					}
					else if (Parent.LastPort != null && !Parent.LastPort.RL_IsSystem)
					{
						Parent.CA_RL_NKLastPortInfo.AddMessageError(SystemDefinedUNLOCORequired);
					}
				}
			}
		}

		internal static string LastPortRequired
		{
			get { return Res.GetString("97629FF7-A277-412B-B075-A3C2BF90DAA6", "The Place of Direct Shipment is required for Release message, when no Port of Loading is entered on the Declaration tab."); }
		}

		internal static string LastPortRequiredIIDWithGAC
		{
			get { return Res.GetString("491d6572-8fd3-43f8-bd77-a22cdeb366b0", "Place of Direct Shipment is required"); }
		}

		internal static string SystemDefinedUNLOCORequired
		{
			get { return Res.GetString("5ab13ab8-31a2-41ff-801a-ae0357577466", "UNLOCO must be a system defined UNLOCO.  Please enter a different UNLOCO."); }
		}

		#endregion

		#region CheckCA_RN_NKTranshipment (validate if entered)

		protected override void CheckCA_RN_NKTranshipment()
		{
			base.CheckCA_RN_NKTranshipment();
			ListValidation.MessageErrorIfInvalidCode(InvoiceHeader.CA_RN_NKTranshipmentInfo);
		}

		#endregion

		#region CheckCA_TradeZone (validate if entered)

		protected override void CheckCA_TradeZone()
		{
			base.CheckCA_TradeZone();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_TradeZoneInfo, Lookups.TradeZones);
		}

		#endregion

		#region CheckCA_USPortOfExit (B3)

		protected override void CheckCA_USPortOfExit()
		{
			base.CheckCA_USPortOfExit();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_USPortOfExitInfo, Lookups.USPortOfExitList);

			var declaration = Parent.Declaration;
			if (!InvoiceHeader.IsAttachedToPersistentLVSDeclaration
				&& (InvoiceHeader.CA_RN_NKExport == Constants.CountryCodes.UnitedStates || InvoiceHeader.IsUSPlaceOfExport)
				&& declaration != null
				&& !declaration.IsOtherWarehouseEntry)
			{
				DeclarationValidator.MessageErrorIfNotEntered(InvoiceHeader.CA_USPortOfExitInfo, string.Empty, ValidateForMessageType.B3CUSDEC);
			}
		}

		#endregion

		#region CheckCA_TreatmentCode (B3)

		protected override bool IsTreatmentCodeRequired
		{
			get { return DeclarationValidator?.IsValidationRequired(ValidateForMessageType.B3CUSDEC) ?? false; }
		}

		#endregion

		#region CheckCA_TimeLimit (validate if entered)

		protected override void CheckCA_TimeLimit()
		{
			base.CheckCA_TimeLimit();

			if (!Parent.CA_TimeLimitInfo.HasNotifications()
				&& Parent.Parent.InvoiceLines.Any(line => ((JobComInvoiceLine)line).CA_CalculationMethod == CalculationMethods.Codes.OneSixtiethRemission)
				&& (Parent.CA_TimeLimitCode != TimeLimitUnitCodes.Codes.Month || Parent.CA_TimeLimit.IsEmpty))
			{
				Parent.CA_TimeLimitInfo.AddMessageError(Res.GetString("8e0f3a2b-f80a-4f21-83fc-aaae3e991cd9", "A monthly time limit must be specified with 1/60 Remission calculation method."));
			}

			if (!Parent.CA_TimeLimitInfo.HasNotifications()
				&& Parent.Parent.InvoiceLines.Any(line => ((JobComInvoiceLine)line).CA_CalculationMethod == CalculationMethods.Codes.OneOneTwentiethRemission)
				&& (Parent.CA_TimeLimitCode != TimeLimitUnitCodes.Codes.Month || Parent.CA_TimeLimit.IsEmpty))
			{
				Parent.CA_TimeLimitInfo.AddMessageError(Res.GetString("6b8fc92d-549e-4b40-aef9-a59eb4a5e294", "A monthly time limit must be specified with 1/120 Remission calculation method."));
			}

			var declaration = InvoiceHeader.JobDeclaration;
			if (declaration != null && Parent.CA_TimeLimit.IsEmpty)
			{
				if (declaration.IsCADEnabled
					? declaration.JE_MessageSubType == CADEntryTypeList.Codes.Warehouse101 || declaration.JE_MessageSubType == CADEntryTypeList.Codes.Warehouse102
					: declaration.JE_MessageSubType == B3EntryTypeList.Codes.Warehouse10)
				{
					Parent.CA_TimeLimitInfo.AddMessageError(Res.GetString("8027dcb4-a48e-469e-ac80-1302e8778527", "Time Limits are mandatory on Warehouse Type 10 Jobs."));
				}
				else if (declaration.JE_MessageSubType == B3EntryTypeList.Codes.CashD)
				{
					Parent.CA_TimeLimitInfo.AddMessageError(Res.GetString("1bf907c7-02ec-48f3-a932-fadfe79d3186", "Time Limits are mandatory on Sight Type D Jobs."));
				}
				else if (declaration.JE_MessageSubType == B3EntryTypeList.Codes.ConfirmingSight)
				{
					Parent.CA_TimeLimitInfo.AddMessageError(Res.GetString("f2008355-e5d6-4dab-bd90-5ce1bd4f812e", "Time Limits are mandatory on Confirming Sight Type AD Jobs."));
				}
				else if (declaration.JE_MessageSubType == B3EntryTypeList.Codes.Confirming
					&& InvoiceHeader.JobComInvoiceLines.Cast<JobComInvoiceLine>().Any(x => CalculationMethods.IsTemporaryImport(x.CA_CalculationMethod)))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_TimeLimitInfo);
				}
			}
		}

		#endregion

		#region CheckCA_ValueForDutyCode (B3)

		protected override void CheckCA_ValueForDutyCode()
		{
			base.CheckCA_ValueForDutyCode();
			DeclarationValidator?.MessageErrorIfInvalidCode(Parent.CA_ValueForDutyCodeInfo, Lookups.ValueForDutyCodes, ValidateForMessageType.B3CUSDEC);
		}

		#endregion

		#region CheckCA_CasualImportCommodity

		protected override void CheckCA_CasualImportCommodity()
		{
			base.CheckCA_CasualImportCommodity();
			if (!InvoiceHeader.CA_CasualImportCommodity.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(InvoiceHeader.CA_CasualImportCommodityInfo);
			}
		}

		#endregion

		#region CheckCA_CasualImportDestinationProvince

		protected override void CheckCA_CasualImportDestinationProvince()
		{
			base.CheckCA_CasualImportDestinationProvince();

			var invoiceHeader = InvoiceHeader;
			if (!invoiceHeader.CA_CasualImportDestinationProvince.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(invoiceHeader.CA_CasualImportDestinationProvinceInfo);
			}
			if (invoiceHeader.CA_IsCasualImport && invoiceHeader.CA_CasualImportDestinationProvince != invoiceHeader.DefaultCasualImportDestinationProvince)
			{
				invoiceHeader.CA_CasualImportDestinationProvinceInfo.AddWarning(DestinationProvinceDoesNotMatch);
			}
		}

		internal static string DestinationProvinceDoesNotMatch => Res.GetString("F1920877-25BF-4F53-96AC-5C1F5B8ED60C", "Destination Province does not match system calculated Destination Province");

		#endregion

		#region CheckCA_CarrierCode (LVS)

		protected override void CheckCA_CarrierCode()
		{
			base.CheckCA_CarrierCode();
			if (InvoiceHeader.IsAttachedToPersistentLVSDeclaration)
			{
				ListValidation.MessageErrorIfInvalidCode(InvoiceHeader.CA_CarrierCodeInfo);
			}

			if (!Parent.CA_CarrierCode.IsEmpty)
			{
				var carrier = new Universal.ZZRefCarrierCombined.Loader(Parent.Factory).LoadFromCode(Enterprise.Core.Constants.CountryCodes.Canada, Parent.CA_CarrierCode);
				var transportMode = InvoiceHeader.JobDeclaration?.JE_TransportMode ?? ZString.Empty;
				if (carrier != null && !carrier.TransportModePairList.Any(x => x.Value && x.Description == transportMode) && !carrier.HasMatchingAttributes(new[] { transportMode }))
				{
					Parent.CA_CarrierCodeInfo.AddWarning(Res.GetString("673D6276-425F-4CC6-859C-50C441146D49", "Carrier not allowed for mode of transport '{0}'", transportMode));
				}
			}
		}

		#endregion
	}
}
