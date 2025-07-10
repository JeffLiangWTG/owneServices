using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.AWB;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.HK.Business
{
	/// <summary>
	/// Provides additional (Traxon Messages related) validation of ForwardingConsol 
	/// </summary>
	public class TraxonForwardingConsolValidation : ForwardingConsolValidation,
		Integration.Customs.HK.IExtendedForwardingConsolValidation
	{
		/// <summary>
		/// Initializes a new instance of the TraxonValidation that validates fields of the specified ForwardingConsol
		/// </summary>
		/// <param name="consol">The consolidation to validate</param>
		public TraxonForwardingConsolValidation(ForwardingConsol consol)
			: base(consol)
		{
			this.consol = Argument.NotNull(consol, "consol", "A valid Consol must be passed to the TraxonValidation constructor");
			this.branchLoader = new GlbBranch.Loader(consol.Factory);
		}

		/// <summary>
		/// Performs validation of the parent ForwardingConsol using current branch settings and stores all messages in the array
		/// </summary>
		/// <param name="errors">Output array containing all validation messages</param>
		/// <returns>true if all validations were successful; otherwise false</returns>
		public bool ValidateAll(out string[] errors)
		{
			var errorsList = new List<string>();
			var result = ValidateAll(GlbBranch.CurrentBranch, m => errorsList.Add(m));
			errors = errorsList.ToArray();
			return result;
		}

		/// <summary>
		/// Performs validation of the parent ForwardingConsol and applies the specified action to all messages
		/// </summary>
		/// <param name="branch">The branch which registry settings are used for validation</param>
		/// <param name="processMessage">The action to apply to error messages</param>
		/// <returns>true if all validations were successful; otherwise false</returns>
		public bool ValidateAll(GlbBranch branch, Action<string> processMessage)
		{
			bool result = true;

			Action<string> action = (message) =>
			{
				processMessage(message);
				result = false;
			};

			if (!SendViaeHub)
			{
				AppendValidation(IsTraxonOutputDirectoryValid(branch), GetTraxonDataRegistryError(TraxonDataExportError, HKDataRegistry.Instance.HKTraxonOutputDirectory, HKDataRegistry.Instance.ISACFTPServerOutputAddress), action);
			}

			AppendValidation(IsCOSACCodeValid(branch), RegistryError + COSACCodeError, action);
			AppendValidation(IsTraxonSenderIDValid(branch), RegistryError + SenderIDError, action);
			AppendValidation(IsTraxonRecipientReferencePasswordValid(branch), RegistryError + RecipientReferenceError, action);

			AppendValidation(IsFlightNumberValid, ConsolError + FlightNumberError, action);
			AppendValidation(IsETAValid, ConsolError + FlightETAError, action);
			AppendValidation(IsMAWBValid, ConsolError + MAWBError, action);
			AppendValidation(IsShipmentCountValid, ConsolError + ShipmentCountError, action);

			var isRequireSendOtherCustomsInformation = false;

			foreach (ForwardingShipment shipment in consol.Shipments)
			{
				AppendValidation(IsHAWBValid(shipment), ShipmentError + shipment.JS_UniqueConsignRef + HAWBError, action);
				AppendValidation(IsTotalPackageCountValid(shipment), ShipmentError + shipment.JS_UniqueConsignRef + NumberOfPacakgesError, action);
				AppendValidation(IsActualWeightValid(shipment), ShipmentError + shipment.JS_UniqueConsignRef + ActualWeightError, action);

				if (shipment.SendOtherInfoForShipment(consol) ||  shipment.SendACASForShipment())
				{
					isRequireSendOtherCustomsInformation = true;
					ValidateOtherCustomsDataOnBill(shipment, action);
				}

				ValidateConsigneeInformation(shipment, action);
				ValidateConsignorInformation(shipment, action);
				ValidateNotifyPartyInformation(shipment, action);

				if (IsHKExport(shipment))
				{
					AppendValidation(IsMOACurrencyValid(shipment), ShipmentError + shipment.JS_UniqueConsignRef + AWBCurrencyError, action);
					AppendValidation(IsCustomsValueValid(shipment), ShipmentError + shipment.JS_UniqueConsignRef + AWBCustomsValueError, action);
					AppendValidation(IsCarriageValueValid(shipment), ShipmentError + shipment.JS_UniqueConsignRef + AWBCarriageValueError, action);
					AppendValidation(IsInsuranceValueValid(shipment), ShipmentError + shipment.JS_UniqueConsignRef + AWBInsuranceValueError, action);
					AppendValidation(IsGoodsDescriptionValid(shipment), ShipmentError + shipment.JS_UniqueConsignRef + GoodsDescriptionError, action);
				}
				else
				{
					AppendValidation(IsGoodsValueCurrencyValid(shipment), ShipmentError + shipment.JS_UniqueConsignRef + GoodsValueCurrencyError, action);
					AppendValidation(IsGoodsValueValid(shipment), ShipmentError + shipment.JS_UniqueConsignRef + ShipmentGoodsValueError, action);
				}

				ValidateLicenseNumbers(shipment, action);
			}

			if (isRequireSendOtherCustomsInformation)
			{
				ValidateACASAccountDetails(consol, action);
			}

			return result;
		}

		static bool SendViaeHub => Registry.Business.eHubMessagingRegistry.Instance.SendHKISACEViaEHub.Value;

		string GetTraxonDataRegistryError(string message, IRegistryItemInternals localDirectoryRegistry, IRegistryItemInternals ftpRegistry)
		{
			ZString ftpSettingLocation = ftpRegistry.Location;
			ftpSettingLocation = ftpSettingLocation.Left(ftpSettingLocation.LastIndexOf(" -> "));
			return RegistryError + string.Format(message, localDirectoryRegistry.Location, ftpSettingLocation);
		}

		readonly GlbBranch.Loader branchLoader;
		readonly ForwardingConsol consol;

		/// <summary>
		/// Provides additional validation for the ForwardingConsol.JK_RL_NKDischargePort property
		/// </summary>
		protected override void CheckJK_RL_NKDischargePort()
		{
			base.CheckJK_RL_NKDischargePort();
			if (IsAir &&
				consol.GetImportTransport(Core.Constants.CountryCodes.HongKong) != null)
			{
				var branch = branchLoader.LoadActiveMatchingBranchInThisCountry(consol.ReceivingForwarder, Core.Constants.CountryCodes.HongKong);
				if (IsProcessingISACMessageValid(branch))
				{
					ZPropertyInfo info = this.Parent.JK_RL_NKDischargePortInfo;
					ValidateAll(branch, s => info.AddMessageError(s));
				}
			}
		}

		/// <summary>
		/// Provides additional validation for the ForwardingConsol.JK_RL_NKLoadPort property
		/// </summary>
		protected override void CheckJK_RL_NKLoadPort()
		{
			base.CheckJK_RL_NKLoadPort();
			if (IsAir &&
				consol.GetImportTransport(Core.Constants.CountryCodes.HongKong) == null &&
				consol.GetExportTransport(Core.Constants.CountryCodes.HongKong) != null)
			{
				var branch = branchLoader.LoadActiveMatchingBranchInThisCountry(consol.SendingForwarder, Core.Constants.CountryCodes.HongKong);
				if (IsProcessingISACMessageValid(branch))
				{
					ZPropertyInfo info = this.Parent.JK_RL_NKLoadPortInfo;
					ValidateAll(branch, s => info.AddMessageError(s));
				}
			}
		}

		/// <summary>
		/// Determines whether the parent ForwardingConsol transport mode is Air 
		/// </summary>
		bool IsAir
		{
			get
			{
				return consol.JK_TransportMode == Enterprise.Core.Constants.TransportModes.Air;
			}
		}

		/// <summary>
		/// Determines whether registry settings indicate that ISAC message processing is valid for the branch specified
		/// </summary>
		/// <param name="branch">The branch to check registry settings</param>
		/// <returns>true if the branch has valid registry settings; false otherwise</returns>
		public static bool IsProcessingISACMessageValid(GlbBranch branch)
		{
			return
				(SendViaeHub || IsTraxonOutputDirectoryValid(branch)) &&
				IsCOSACCodeValid(branch) &&
				IsTraxonSenderIDValid(branch) &&
				IsTraxonRecipientReferencePasswordValid(branch);
		}

		/// <summary>
		/// Gets a routing leg that crosses the HK border (import first)
		/// </summary>
		Transport HKTransport
		{
			get
			{
				return consol.GetImportTransport(Core.Constants.CountryCodes.HongKong) ??
					   consol.GetExportTransport(Core.Constants.CountryCodes.HongKong);
			}
		}

		/// <summary>
		/// Applies the specified action to the validation message if there was a validation error
		/// </summary>
		/// <param name="isValid">Specifies whether validation was successful or not</param>
		/// <param name="message">The validation message</param>
		/// <param name="processMessage">The action to apply to the message</param>
		/// <returns>the value of <paramref name="isValid"/></returns>
		static bool AppendValidation(bool isValid, string message, Action<string> processMessage)
		{
			if (!isValid)
			{
				processMessage(message);
			}
			return isValid;
		}

		/// <summary>
		/// Determines whether the specified shipment is an export from Hong Kong
		/// </summary>
		/// <param name="shipment">The ForwardingShipment to check</param>
		/// <returns>true if the shipment's destination is HK and origin is not; false otherwise</returns>
		static bool IsHKExport(ForwardingShipment shipment)
		{
			return
				shipment.JS_RL_NKOrigin.StartsWith(Core.Constants.CountryCodes.HongKong) &&
				!shipment.JS_RL_NKDestination.StartsWith(Core.Constants.CountryCodes.HongKong);
		}

		/// <summary>
		/// Determines whether the specified shipment is an import to Hong Kong
		/// </summary>
		/// <param name="shipment"></param>
		/// <returns>true if the shipment's origin is HK and destination is not; false otherwise</returns>
		public static bool IsHKImport(ForwardingShipment shipment)
		{
			return
				!shipment.JS_RL_NKOrigin.StartsWith(Core.Constants.CountryCodes.HongKong) &&
				shipment.JS_RL_NKDestination.StartsWith(Core.Constants.CountryCodes.HongKong);
		}

		/// <summary>
		/// Determines whether the beginning of the specified ZString matches United States or Canada country code
		/// </summary>
		/// <param name="s">The ZString to test</param>
		/// <returns>true if "US" or "CA" matches the beginning of the specified ZString</returns>
		static bool StartsWithUSorCA(ZString s)
		{
			return s.StartsWith(Core.Constants.CountryCodes.UnitedStates) || s.StartsWith(Core.Constants.CountryCodes.Canada);
		}

		/// <summary>
		/// Validate that the other customs data is available prior to sending the CUSEXP 
		/// </summary>
		public static void ValidateOtherCustomsDataOnBill(ForwardingShipment shipment, Action<string> action)
		{
			var awbHeader = shipment.AWBHeader;

			if (awbHeader != null)
			{
				var contactInfos = new[]
				{
					(awbHeader.EH_ConsigneeContactNameInfo, "Consignee Contact Name"),
					(awbHeader.EH_ConsigneeContactDetailInfo, "Consignee Contact Details"),
					(awbHeader.EH_ShipperContactDetailInfo, "Shipper Contact Details"),
				};

				foreach (var info in contactInfos)
				{
					var errorMessage = string.Format(CultureInfo.InvariantCulture, " The {0} on air way bill may not be empty.", info.Item2);
					AppendValidation(!info.Item1.Value.IsEmpty, ShipmentError + shipment.JS_UniqueConsignRef + errorMessage, action);
				}
			}
		}

		public static void ValidateACASAccountDetails(ForwardingConsol consol, Action<string> action)
		{
			var invalidAccountHolderAndNameShipments = new List<string>();
			var invalidAccountNumberShipments = new List<string>();

			foreach (ForwardingShipment shipment in consol.Shipments)
			{
				if (shipment.SendACASForShipment() && shipment.AWBHeader?.GetACASCountryHandler() is IACASCountryHandler acasCountryHandler)
				{
					if (!acasCountryHandler.GetCustomerAccountHolderAndName(out string accountHolder, out string accountName))
					{
						invalidAccountHolderAndNameShipments.Add(shipment.JS_UniqueConsignRef);
					}

					if (!acasCountryHandler.GetCustomerAccountIssuerAndNumber(out string accountIssuer, out string accountNumber))
					{
						invalidAccountNumberShipments.Add(shipment.JS_UniqueConsignRef);
					}
				}
			}

			if (invalidAccountHolderAndNameShipments.Count > 0)
			{
				string message = SendISACWarning + string.Join(", ", invalidAccountHolderAndNameShipments) + AccountHolderAndNameRetrieveError;
				AppendValidation(false, message, action);
			}

			if (invalidAccountNumberShipments.Count > 0)
			{
				string message = SendISACWarning + string.Join(", ", invalidAccountNumberShipments) + AccountNumberRetrieveError;
				AppendValidation(false, message, action);
			}
		}

		/// <summary>
		/// Determines whether Traxon Output Directory or Traxon FTP Server Output Address is set up in registry
		/// </summary>
		/// <param name="branch">The branch to test the registry</param>
		/// <returns>true if Traxon Output Directory or Traxon FTP Server Output Address is set up in registry; false otherwise</returns>
		public static bool IsTraxonOutputDirectoryValid(GlbBranch branch)
		{
			return
				branch != null
				&& (!string.IsNullOrEmpty(HKDataRegistry.Instance.HKTraxonOutputDirectory.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty))
				|| !string.IsNullOrEmpty(HKDataRegistry.Instance.ISACFTPServerOutputAddress.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty)));
		}

		/// <summary>
		/// Determines whether COSAC Agent Code is set up in registry
		/// </summary>
		/// <param name="branch">The branch to test the registry</param>
		/// <returns>true if COSAC Agent Code is set up in registry; false otherwise</returns>
		public static bool IsCOSACCodeValid(GlbBranch branch)
		{
			return
				branch != null
				&& !string.IsNullOrEmpty(HKDataRegistry.Instance.CosacAgentCode.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty));
		}

		/// <summary>
		/// Determines whether Traxon Sender ID is set up in registry
		/// </summary>
		/// <param name="branch">The branch to test the registry</param>
		/// <returns>true if Traxon Sender ID is set up in registry; false otherwise</returns>
		public static bool IsTraxonSenderIDValid(GlbBranch branch)
		{
			return
				branch != null
				&& !string.IsNullOrEmpty(HKDataRegistry.Instance.HKTraxonSenderID.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty));
		}

		/// <summary>
		/// Determines whether Traxon Recipient Reference Password is set up in registry
		/// </summary>
		/// <param name="branch">The branch to test the registry</param>
		/// <returns>true if Traxon Recipient Reference Password is set up in registry; false otherwise</returns>
		public static bool IsTraxonRecipientReferencePasswordValid(GlbBranch branch)
		{
			return
				branch != null
				&& !string.IsNullOrEmpty(HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty));
		}

		/// <summary>
		/// Determines whether the specified ZString contains a valid traxon license number
		/// </summary>
		/// <param name="licence">The license number to validate</param>
		/// <returns>true if the specified ZString is not longer than 20 symbols; otherwise false</returns>
		static bool IsTraxonLicenseNumberValid(ZString licence)
		{
			return licence.Length <= 20;
		}

		/// <summary>
		/// Determines whether the house air waybill of the specified ForwardingShipment is not empty
		/// </summary>
		/// <param name="shipment">The shipment to validate</param>
		/// <returns>true if the specified shipment has house air waybill; otherwise false</returns>
		public static bool IsHAWBValid(ForwardingShipment shipment)
		{
			return shipment.IsAir && !shipment.JS_HouseBill.IsEmpty;
		}

		/// <summary>
		/// Determines whether the specified ForwardingShipment has a valid number of packages
		/// </summary>
		/// <param name="shipment">The shipment to validate</param>
		/// <returns>true if the specified shipment has a valid number of packages; otherwise false</returns>
		public static bool IsTotalPackageCountValid(ForwardingShipment shipment)
		{
			return shipment.JS_OuterPacks.IsInRange(1, 99999);
		}

		/// <summary>
		/// Determines whether the specified ForwardingShipment has a valid weight
		/// </summary>
		/// <param name="shipment">The shipment to validate</param>
		/// <returns>true if the specified shipment has actual weight in the interval [0.1, 999999.5]; otherwise false</returns>
		public static bool IsActualWeightValid(ForwardingShipment shipment)
		{
			var weightInKG = shipment.GetWeightInKG();
			return weightInKG >= 0.1m && weightInKG <= 999999.5m;
		}

		/// <summary>
		/// Determines whether the specified ForwardingShipment has a valid carriage value
		/// </summary>
		/// <param name="shipment">The shipment to validate</param>
		/// <returns>true if the specified shipment has AWBHeader.EH_DeclaredValue in the interval [0, 999999999999]; otherwise false</returns>
		public static bool IsCarriageValueValid(ForwardingShipment shipment)
		{
			return shipment.AWBHeader != null && !shipment.AWBHeader.IsDeleted && (shipment.AWBHeader.EH_DeclaredValue.IsEmpty || (shipment.AWBHeader.EH_DeclaredValue >= 0 && shipment.AWBHeader.EH_DeclaredValue <= 999999999999m));
		}

		/// <summary>
		/// Determines whether the specified ForwardingShipment has a valid goods description
		/// </summary>
		/// <param name="shipment">The shipment to validate</param>
		/// <returns>true if the specified shipment has goods description; otherwise false</returns>
		public static bool IsGoodsDescriptionValid(ForwardingShipment shipment)
		{
			return !shipment.JS_GoodsDescription.IsEmpty;
		}

		/// <summary>
		/// Determines whether the specified ForwardingShipment has a valid insurance value
		/// </summary>
		/// <param name="shipment">The shipment to validate</param>
		/// <returns>true if the specified shipment has insurance value in the interval [0, 999999999999); otherwise false</returns>
		public static bool IsInsuranceValueValid(ForwardingShipment shipment)
		{
			return shipment.AWBHeader != null && !shipment.AWBHeader.IsDeleted && (shipment.AWBHeader.EH_InsuranceValue.IsEmpty || (shipment.AWBHeader.EH_InsuranceValue >= 0 && shipment.AWBHeader.EH_InsuranceValue <= 999999999999m));
		}

		/// <summary>
		/// Determines whether the specified ForwardingShipment has a valid MOA currency
		/// </summary>
		/// <param name="shipment">The shipment to validate</param>
		/// <returns>true if the specified shipment has non empty AWBHeader.EH_Currency; otherwise false</returns>
		public static bool IsMOACurrencyValid(ForwardingShipment shipment)
		{
			return shipment.AWBHeader != null && !shipment.AWBHeader.IsDeleted && !shipment.AWBHeader.EH_Currency.IsEmpty;
		}

		/// <summary>
		/// Determines whether the specified ForwardingShipment has a valid goods value currency
		/// </summary>
		/// <param name="shipment">The shipment to validate</param>
		/// <returns>true if the specified shipment has non null GoodsValueCurr; otherwise false</returns>
		public static bool IsGoodsValueCurrencyValid(ForwardingShipment shipment)
		{
			return shipment.GoodsValueCurr != null;
		}

		/// <summary>
		/// Determines whether the specified ForwardingShipment has a valid customs value
		/// </summary>
		/// <param name="shipment">The shipment to validate</param>
		/// <returns>true if the specified shipment has AWBHeader.EH_CustomsValue in the interval [0, 999999999999]; otherwise false</returns>
		public static bool IsCustomsValueValid(ForwardingShipment shipment)
		{
			return IsCustomsValueValid(shipment.AWBHeader);
		}

		/// <summary>
		/// Determines whether the specified ExportAWBHeader has a valid customs value
		/// </summary>
		/// <param name="awbheader">The awbheader to validate</param>
		/// <returns>true if the specified awbheader has EH_CustomsValue in the interval [0, 999999999999]; otherwise false</returns>
		public static bool IsCustomsValueValid(ExportAWBHeader awbheader)
		{
			return awbheader != null && !awbheader.IsDeleted && (awbheader.EH_CustomsValue.IsEmpty || (awbheader.EH_CustomsValue >= 0 && awbheader.EH_CustomsValue <= 999999999999m));
		}

		/// <summary>
		/// Determines whether the specified ForwardingShipment has a valid goods value
		/// </summary>
		/// <param name="shipment">The shipment to validate</param>
		/// <returns>true if the specified shipment has JS_GoodsValue in the interval [0, 999999999999]; otherwise false</returns>
		public static bool IsGoodsValueValid(ForwardingShipment shipment)
		{
			return shipment.JS_GoodsValue.IsEmpty || (shipment.JS_GoodsValue >= 0 && shipment.JS_GoodsValue <= 999999999999m);
		}

		/// <summary>
		/// Determines whether the specified organisation has a valid company name
		/// </summary>
		/// <param name="orgDetails">The organisation details to validate</param>
		/// <returns>true if the specified organisation has non empty company name; otherwise false</returns>
		static bool IsCompanyNameValid(IOrgDetails orgDetails)
		{
			return !orgDetails.Name.IsEmpty;
		}

		/// <summary>
		/// Determines whether the specified organisation has a valid Address1 field
		/// </summary>
		/// <param name="orgDetails">The organisation details to validate</param>
		/// <returns>true if the specified organisation has non empty Address1 field; otherwise false</returns>
		static bool IsCompanyAddress1Valid(IOrgDetails orgDetails)
		{
			return !orgDetails.Address1.IsEmpty;
		}

		/// <summary>
		/// Determines whether the specified organisation has a valid city
		/// </summary>
		/// <param name="orgDetails">The organisation details to validate</param>
		/// <returns>true if the specified organisation has non empty city; otherwise false</returns>
		static bool IsCompanyCityValid(IOrgDetails orgDetails)
		{
			return !orgDetails.City.IsEmpty;
		}

		/// <summary>
		/// Determines whether the specified organisation has a valid country
		/// </summary>
		/// <param name="orgDetails">The organisation details to validate</param>
		/// <returns>true if the specified organisation has non empty country; otherwise false</returns>
		static bool IsCompanyCountryValid(IOrgDetails orgDetails)
		{
			return !orgDetails.Country.IsEmpty;
		}

		/// <summary>
		/// Determines whether the flight number of the parent ForwardingConsol is not empty
		/// </summary>
		public bool IsFlightNumberValid
		{
			get
			{
				var transport = HKTransport;
				return transport != null && !transport.JW_VoyageFlight.IsEmpty;
			}
		}

		/// <summary>
		/// Determines whether the estimated time of arrival of the parent ForwardingConsol is not empty
		/// </summary>
		public bool IsETAValid
		{
			get
			{
				var transport = HKTransport;
				return transport != null && !transport.JW_ETA.IsEmpty;
			}
		}

		/// <summary>
		/// Determines whether the master air waybill of the parent ForwardingConsol is not empty
		/// </summary>
		public bool IsMAWBValid
		{
			get
			{
				return !consol.JK_MasterBillNum.IsEmpty;
			}
		}

		/// <summary>
		/// Determines whether the parent ForwardingConsol has shipments
		/// </summary>
		public bool IsShipmentCountValid
		{
			get
			{
				return consol.Shipments.Count > 0;
			}
		}

		/// <summary>
		/// Performs validation of the specified organisation
		/// </summary>
		/// <param name="orgDetails">The organisation details to validate</param>
		/// <param name="reference">The unique company reference</param>
		/// <param name="company">Company name: "Consignee", "Consignor" or "Notify Party"</param>
		/// <param name="action">The action to apply to error messages</param>
		static void ValidateCompanyAddress(IOrgDetails orgDetails, string reference, string company, Action<string> action)
		{
			AppendValidation(IsCompanyNameValid(orgDetails), ShipmentError + reference + " " + company + NameError, action);
			AppendValidation(IsCompanyAddress1Valid(orgDetails), ShipmentError + reference + " " + company + AddressError, action);
			AppendValidation(IsCompanyCityValid(orgDetails), ShipmentError + reference + " " + company + CityError, action);
			AppendValidation(IsCompanyCountryValid(orgDetails), ShipmentError + reference + " " + company + CountryError, action);
		}

		/// <summary>
		/// Performs validation of a consignee information in the specified shipment
		/// </summary>
		/// <param name="shipment">The ForwardingShipment containing information about the consignee</param>
		/// <param name="processMessage">The action to apply to error messages</param>
		public static void ValidateConsigneeInformation(ForwardingShipment shipment, Action<string> processMessage)
		{
			var consigneeDetails = shipment.GetConsigneeOrgDetails();
			if (!consigneeDetails.IsEmpty)
			{
				if (IsHKExport(shipment) &&
					StartsWithUSorCA(shipment.JS_RL_NKDestination) &&
					consigneeDetails.State.IsEmpty)
				{
					processMessage(ShipmentError + shipment.JS_UniqueConsignRef + ConsigneeStateError);
				}
				ValidateCompanyAddress(consigneeDetails, shipment.JS_UniqueConsignRef, "Consignee", processMessage);
			}
			else
			{
				processMessage(ShipmentError + shipment.JS_UniqueConsignRef + ConsigneeError);
			}
		}

		/// <summary>
		/// Performs validation of a consignor information in the specified shipment
		/// </summary>
		/// <param name="shipment">The ForwardingShipment containing information about the consignor</param>
		/// <param name="processMessage">The action to apply to error messages</param>
		public static void ValidateConsignorInformation(ForwardingShipment shipment, Action<string> processMessage)
		{
			var consignorDetails = shipment.GetConsignorOrgDetails();
			if (!consignorDetails.IsEmpty)
			{
				if (IsHKImport(shipment) &&
					StartsWithUSorCA(shipment.JS_RL_NKOrigin) &&
					consignorDetails.State.IsEmpty)
				{
					processMessage(ShipmentError + shipment.JS_UniqueConsignRef + ConsignorStateError);
				}
				ValidateCompanyAddress(consignorDetails, shipment.JS_UniqueConsignRef, "Consignor", processMessage);
			}
			else
			{
				processMessage(ShipmentError + shipment.JS_UniqueConsignRef + ConsignorError);
			}
		}

		/// <summary>
		/// Performs validation of a notify party information in the specified shipment
		/// </summary>
		/// <param name="shipment">The ForwardingShipment containing information about the notify party</param>
		/// <param name="processMessage">The action to apply to error messages</param>
		public static void ValidateNotifyPartyInformation(ForwardingShipment shipment, Action<string> processMessage)
		{
			var notifyPartyDetails = shipment.GetNotifyPartyOrgDetails();
			if (!notifyPartyDetails.IsEmpty)
			{
				ValidateCompanyAddress(notifyPartyDetails, shipment.JS_UniqueConsignRef, "Notify Party", processMessage);
			}
		}

		/// <summary>
		/// Performs validation of all license numbers in the specified shipment
		/// </summary>
		/// <param name="shipment">The shipment to validate</param>
		/// <param name="processMessage">The action to apply to error messages</param>
		public static void ValidateLicenseNumbers(ForwardingShipment shipment, Action<string> processMessage)
		{
			foreach (var licence in shipment.GetTraxonLicenseNumbersFromShipment())
			{
				AppendValidation(IsTraxonLicenseNumberValid(licence), ShipmentError + shipment.JS_UniqueConsignRef + " ISAC licence number (" + licence + ") should not be longer than 20 in length.", processMessage);
			}
		}

		const string AWBCarriageValueError = " The declared carriage AWB value for customs must be within the range: 0 and 999999999999";
		const string AWBCurrencyError = " Air Waybill currency may not be empty.";
		const string AWBCustomsValueError = " The declared AWB value for customs must be within the range: 0 and 999999999999";
		const string AWBInsuranceValueError = " The declared insurance AWB value for customs must be within the range: 0 and 999999999999";
		const string ActualWeightError = " Actual weight must fall in the range: 0.1 and 999999.5 in KG.";
		const string AddressError = " address may not be blank.";
		const string COSACCodeError = "The registry element under 'Company Options', 'HK Customs' called 'Cosac Agent Code' must not be empty.";
		const string CityError = " city may not be blank.";
		public const string ConsigneeError = " Please enter a Consignee.";
		const string ConsigneeStateError = " Consignee state may not be blank.";
		public const string ConsignorError = " Please enter a Consignor.";
		const string ConsignorStateError = " Consignor state may not be blank.";
		const string ConsolError = "Error on Consol (ISAC): ";
		const string CountryError = " country may not be blank.";
		const string FlightETAError = "Flight arrival date (ETA) on the consol may not be blank.";
		const string FlightNumberError = "Flight number on the consol may not be blank.";
		const string GoodsDescriptionError = " Goods description may not be empty.";
		const string GoodsValueCurrencyError = " Shipment goods value currency may not be empty.";
		const string HAWBError = " Cannot have an empty HAWB number and its transport mode should be Air.";
		const string TraxonDataExportError = "Either the ISAC Output directory registry or the ISAC FTP Settings registries must be entered. These can be entered in the Registry (Admin -> System -> Registry -> [{0} | {1}])";
		const string MAWBError = "MAWB Number may not be blank.";
		public const string NameError = " name may not be blank.";
		const string NumberOfPacakgesError = " Outer Packs must fall in the range: 0 and 99999";
		const string RecipientReferenceError = "The registry element under 'Company Options', 'HK Customs' called 'ISAC Recipient Reference Password' must not be empty.";
		const string RegistryError = "Error on Registry (ISAC): ";
		const string SenderIDError = "The registry element under 'Company Options', 'HK Customs' called 'ISAC Sender ID' must not be empty.";
		const string ShipmentCountError = "At least one shipment is required before a message can be sent.";
		public const string ShipmentError = "Error on Shipment (ISAC): ";
		const string ShipmentGoodsValueError = " The declared goods value for customs must be within the range: 0 and 999999999999";
		const string SendISACWarning = "Warning - SendISAC: ";
		const string AccountHolderAndNameRetrieveError = " - The Customer Account Holder and Customer Account Name could not be determined for ACAS reporting requirements and will not be sent to the airline.";
		const string AccountNumberRetrieveError = " - The Customer Account Number and/or Customer Account Issuer could not be determined for ACAS reporting requirements and will not be sent to the airline.";
	}
}
