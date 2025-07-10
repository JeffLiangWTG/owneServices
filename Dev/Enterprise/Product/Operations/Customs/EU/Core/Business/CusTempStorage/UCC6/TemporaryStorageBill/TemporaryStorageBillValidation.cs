using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageBillValidation : AsycudaBillValidation
	{
		public TemporaryStorageBillValidation(AutoAsycudaBill parent) : base(parent)
		{
		}

		public new TemporaryStorageBill Parent => (TemporaryStorageBill)base.Parent;

		public bool IsNotEnsReuse => !(header?.IsENSReuse ?? true);

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateTypeOfBillDocument();
			ValidateConsignorOrgPK();
			ValidateConsigneeOrgPK();
			ValidateAnyPackItemsWhenENSIsNotReused();
			ValidateHouseBillOnDeconsolidation();
		}

		public void ValidateAnyPackItemsWhenENSIsNotReused()
		{
			var parent = Parent;
			var header = parent.Header;
			if (IsNotTransfer && IsNotEnsReuse && parent.PackedItems.Count == 0)
			{
				if (parent.ABL_Calc_IsMaster)
				{
					if (!header.HouseBills.Any() && IsNotDeconsolidation)
					{
						parent.AddRowMessageError(Res.GetString("86459FFB-36E9-40F8-82ED-25CA0FF0C02B", "For a PNTS that includes only Master bill, at least one item must be entered."));
					}
				}
				else
				{
					parent.AddRowMessageError(Res.GetString("51CFFEC1-2B39-4B3E-B7B1-F2799635BD73", "For a PNTS that includes House bills, all House bills must include at least one item."));
				}
			}
		}

		public void ValidateConsignorOrgPK()
		{
			ValidateCalculatedProperty(Parent.ConsignorOrgPKInfo);
		}

		protected override void CheckABL_GrossWeight()
		{
			base.CheckABL_GrossWeight();
			var parent = Parent;
			var propertyInfo = parent.ABL_GrossWeightInfo;
			if (IsValidationActive(x => x.IsGrossWeightCheckSupported))
			{
				MandatoryValidation.CheckNotNegative(propertyInfo);
				var header = parent.Header;
				var messageMode = header.AMA_MessageType;
				if (IsNotEnsReuse && !parent.ABL_Calc_IsMaster && (messageMode == PNTSMessageTypeList.Codes.PreLodgedTempStorage || messageMode == PNTSMessageTypeList.Codes.CombinedTemporaryStorage))
				{
					MandatoryValidation.MessageErrorIfIsZero(propertyInfo);
				}
			}
		}

		protected override void CheckABL_GrossWeightUQ()
		{
			base.CheckABL_GrossWeightUQ();
			if (IsValidationActive(x => x.IsGrossWeightCheckSupported))
			{
				var parent = Parent;
				MandatoryValidation.MessageErrorIfUnitNotEntered(parent.ABL_GrossWeightUQInfo, parent.ABL_GrossWeightInfo);
			}
		}

		protected virtual void CheckConsignorOrgPK()
		{
			if (IsNotTransfer && IsNotDeconsolidation)
			{
				var parent = Parent;
				if (IsValidationActive(x => x.IsConsignorOrgPKCheckSupported)
					&& parent.ValidationDecider is ITemporaryStorageBillValidationDecider validationDecider
					&& validationDecider.IsConsignorOrgPKMandatory(parent)
					&& IsNotEnsReuse
					&& parent.ABL_OA_Shipper.IsEmpty
					&& parent.ABL_ShipperName.IsEmpty)
				{
					parent.ConsignorOrgPKInfo.AddMessageError(Res.GetString("53DE3862-3277-4C50-A1D1-2CED187271AA", "Consignor must be filled. Select an organization or fill a consignor address in the ‘Bill Parties’ tab."));
				}
				ListValidation.ErrorIfInvalidPK(parent.ConsignorOrgPKInfo);
				if (!parent.ConsignorOrgPKInfo.HasError(InvalidConsignorCode))
				{
					if (parent.ABL_OA_Shipper.IsEmpty && !parent.ConsignorOrgPK.IsEmpty)
					{
						parent.ConsignorOrgPKInfo.AddWarning(NoAddressSelectedWarning);
					}
				}
			}
		}

		protected override void CheckABL_ShipperName()
		{
			if (IsNotTransfer && IsNotDeconsolidation)
			{
				base.CheckABL_ShipperName();
				CheckABL_ShipperNameWhenNotENSIsReused();
			}
		}

		protected void CheckABL_ShipperNameWhenNotENSIsReused()
		{
			var parent = Parent;
			if (IsValidationActive(x => x.IsShipperNameCheckSupported) 
				&& IsMandatoryFor(x => x.IsShipperNameMandatory)
				&& IsNotEnsReuse 
				&& parent.ABL_ShipperName.IsEmpty)
			{
				parent.ABL_ShipperNameInfo.AddMessageError(Res.GetString("E0F857D0-BE78-4D97-AE7F-356CFB37DC38", "The ‘Name’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected."));
			}
		}

		protected void CheckABL_ShipperRegNoTypeWhenNotENSIsReused()
		{
			var parent = Parent;
			if (IsValidationActive(x => x.IsShipperRegNoTypeCheckSupported) 
				&& IsMandatoryFor(x => x.IsShipperRegNoTypeMandatory)
				&& IsNotEnsReuse 
				&& parent.ABL_ShipperRegNoType.IsEmpty)
			{
				parent.ABL_ShipperRegNoTypeInfo.AddMessageError(Res.GetString("65321F59-0BE3-46E7-BC75-CD1C854B78FB", "The ‘Type of Person’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected."));
			}
		}

		protected override void CheckABL_RN_NKShipperCountry()
		{
			if (IsNotTransfer && IsNotDeconsolidation)
			{
				base.CheckABL_RN_NKShipperCountry();
				ListValidation.MessageErrorIfInvalidCode(Parent.ABL_RN_NKShipperCountryInfo);
				CheckABL_RN_NKShipperCountryWhenNotENSIsReused();
			}
		}

		protected void CheckABL_RN_NKShipperCountryWhenNotENSIsReused()
		{
			var parent = Parent;
			if (IsValidationActive(x => x.IsShipperCountryCheckSupported) 
				&& IsMandatoryFor(x => x.IsShipperCountryMandatory)
				&& IsNotEnsReuse 
				&& parent.ABL_RN_NKShipperCountry.IsEmpty)
			{
				parent.ABL_RN_NKShipperCountryInfo.AddMessageError(Res.GetString("9F389F81-F948-4D2B-BB08-440506D71B98", "The ‘Country’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected."));
			}
		}

		protected override void CheckABL_ShipperPostcode()
		{
			if (IsNotTransfer && IsNotDeconsolidation)
			{
				base.CheckABL_ShipperPostcode();
				CheckABL_ShipperPostcodeWhenNotENSIsReused();
			}
		}

		protected void CheckABL_ShipperPostcodeWhenNotENSIsReused()
		{
			var parent = Parent;
			if (IsValidationActive(x => x.IsShipperPostcodeCheckSupported) 
				&& IsMandatoryFor(x => x.IsShipperPostcodeMandatory)
				&& IsNotEnsReuse 
				&& parent.ABL_ShipperPostcode.IsEmpty)
			{
				parent.ABL_ShipperPostcodeInfo.AddMessageError(Res.GetString("A1DA90D1-8C5B-4FB5-A845-C45723517E9B", "The ‘Postcode’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected."));
			}
		}

		public void ValidateConsigneeOrgPK()
		{
			ValidateCalculatedProperty(Parent.ConsigneeOrgPKInfo);
		}

		protected virtual void CheckConsigneeOrgPK()
		{
			if (IsNotTransfer && IsNotDeconsolidation)
			{
				var parent = Parent;
				if (IsValidationActive(x => x.IsConsigneeOrgPKCheckSupported)
					&& parent.ValidationDecider is ITemporaryStorageBillValidationDecider validationDecider
					&& validationDecider.IsConsigneeOrgPKMandatory(parent)
					&& IsNotEnsReuse
					&& parent.ABL_OA_Consignee.IsEmpty
					&& parent.ABL_ConsigneeName.IsEmpty)
				{
					parent.ConsigneeOrgPKInfo.AddMessageError(Res.GetString("751ACCB5-8C53-481E-9FBF-03B1BEF8E940", "Consignee must be filled. Select an organization or fill a Consignee address in the ‘Bill Parties’ tab."));
				}
				ListValidation.ErrorIfInvalidPK(parent.ConsigneeOrgPKInfo);
				if (!parent.ConsigneeOrgPKInfo.HasError(InvalidConsigneeCode))
				{
					if (parent.ABL_OA_Consignee.IsEmpty && !parent.ConsigneeOrgPK.IsEmpty)
					{
						parent.ConsigneeOrgPKInfo.AddWarning(NoAddressSelectedWarning);
					}
				}
			}
		}

		protected override void CheckABL_ConsigneeName()
		{
			if (IsNotTransfer && IsNotDeconsolidation)
			{
				base.CheckABL_ConsigneeName();
				CheckABL_ConsigneeNameWhenNotENSIsReused();
			}
		}

		protected void CheckABL_ConsigneeNameWhenNotENSIsReused()
		{
			var parent = Parent;
			if (IsValidationActive(x => x.IsConsigneeNameCheckSupported) 
				&& IsMandatoryFor(x => x.IsConsigneeNameMandatory)
				&& IsNotEnsReuse 
				&& parent.ABL_ConsigneeName.IsEmpty)
			{
				parent.ABL_ConsigneeNameInfo.AddMessageError(Res.GetString("A91AA22D-CDBF-42E7-B2A7-5CCD23CEF5D4", "The ‘Name’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected."));
			}
		}

		protected void CheckABL_ConsigneeRegNoTypeWhenNotENSIsReused()
		{
			var parent = Parent;
			if (IsValidationActive(x => x.IsConsigneeRegNoTypeCheckSupported) 
				&& IsMandatoryFor(x => x.IsConsigneeRegNoTypeMandatory)
				&& IsNotEnsReuse 
				&& parent.ABL_ConsigneeRegNoType.IsEmpty)
			{
				parent.ABL_ConsigneeRegNoTypeInfo.AddMessageError(Res.GetString("EF6DADD0-0165-442B-9359-FE0522D0119B", "The ‘Type of Person’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected."));
			}
		}

		protected override void CheckABL_RN_NKConsigneeCountry()
		{
			var parent = Parent;
			if (IsNotTransfer && IsNotDeconsolidation)
			{
				base.CheckABL_RN_NKConsigneeCountry();
				ListValidation.MessageErrorIfInvalidCode(parent.ABL_RN_NKConsigneeCountryInfo);
				CheckABL_RN_NKConsigneeCountryWhenNotENSIsReused();
			}
		}

		protected void CheckABL_RN_NKConsigneeCountryWhenNotENSIsReused()
		{
			var parent = Parent;
			if (IsValidationActive(x => x.IsConsigneeCountryCheckSupported) 
				&& IsMandatoryFor(x => x.IsConsigneeCountryMandatory)
				&& IsNotEnsReuse 
				&& parent.ABL_RN_NKConsigneeCountry.IsEmpty)
			{
				parent.ABL_RN_NKConsigneeCountryInfo.AddMessageError(Res.GetString("E0FC80F8-5B1A-4726-A935-64B295FEE3F3", "The ‘Country’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected."));
			}
		}

		protected override void CheckABL_ConsigneePostcode()
		{
			if (IsNotTransfer && IsNotDeconsolidation)
			{
				base.CheckABL_ConsigneePostcode();
				CheckABL_ConsigneePostcodeWhenNotENSIsReused();
			}
		}

		protected void CheckABL_ConsigneePostcodeWhenNotENSIsReused()
		{
			var parent = Parent;
			if (IsValidationActive(x => x.IsConsigneePostcodeCheckSupported) 
				&& IsMandatoryFor(x => x.IsConsigneePostcodeMandatory)
				&& IsNotEnsReuse 
				&& parent.ABL_ConsigneePostcode.IsEmpty)
			{
				parent.ABL_ConsigneePostcodeInfo.AddMessageError(Res.GetString("2B2FA66D-615B-4BFD-86B6-7EE5E24832AC", "The ‘Postcode’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected."));
			}
		}

		public void ValidateTypeOfBillDocument()
		{
			ValidateCalculatedProperty(Parent.TypeOfBillDocumentInfo);
		}

		protected virtual int MaximumAllowedHWBCount => 99999;

		protected virtual void CheckTypeOfBillDocument()
		{
			var parent = Parent;
			var propertyInfo = parent.TypeOfBillDocumentInfo;

			if (IsValidationActive(x => x.IsTypeOfBillDocumentCheckSupported) && parent.ValidationDecider is ITemporaryStorageBillValidationDecider validationDecider)
			{
				ListValidation.MessageErrorIfInvalidCode(propertyInfo);
				if (validationDecider.IsTypeOfBillDocumentMandatory(parent))
				{
					MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
				}
				CheckTypeOfBillDocumentWhenENSIsReused();
				CheckDuplicateTypeAndNumber(propertyInfo);
			}
		}

		protected void CheckTypeOfBillDocumentWhenENSIsReused()
		{
			var bill = Parent;
			var header = bill.Header;
			if ((header?.IsENSReuse ?? false) && !header.PreviousDocuments.Any() && !bill.PreviousDocuments.Any() && (!bill.PackedItems.Any() || !bill.PackedItems.All(i => i.PreviousDocuments.Any())))
			{
				bill.TypeOfBillDocumentInfo.AddMessageError(Res.GetString("0E59E049-400D-40AB-903F-BDD2A2511AF4", "Please create a previous document."));
			}
		}

		protected override void CheckABL_BillNumber()
		{
			base.CheckABL_BillNumber();

			var parent = Parent;
			var propertyInfo = parent.ABL_BillNumberInfo;
			if (parent.ValidationDecider is ITemporaryStorageBillValidationDecider validationDecider
				&& validationDecider.IsABL_BillNumberMandatory(parent))
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
			CheckDuplicateTypeAndNumber(propertyInfo);
		}

		protected virtual void CheckDuplicateTypeAndNumber(ZPropertyInfo propertyInfo)
		{
			var parent = Parent;
			if (parent.ValidationDecider is ITemporaryStorageBillValidationDecider validationDecider && !validationDecider.AllowDuplicateTypeAndNumber(parent)
				&& (header?.Bills.Any(x => x.TypeOfBillDocument == parent.TypeOfBillDocument && x.ABL_BillNumber == parent.ABL_BillNumber && x.PK != parent.PK) ?? false))
			{
				propertyInfo.AddMessageError(Res.GetString("65849214-1FC5-41C1-A4D3-940A7D4D2B65", "Bill Number and Type must be unique."));
			}
		}

		protected override void CheckABL_BolType()
		{
			base.CheckABL_BolType();
			CheckMaxCountOfHwb();
		}

		void CheckMaxCountOfHwb()
		{
			var parent = Parent;
			if (parent.ABL_BolType == TemporaryStorageBillKindList.Codes.HWB)
			{
				var hwbCount = header?.Bills.Cast<TemporaryStorageBill>().Count(s => s.ABL_BolType == TemporaryStorageBillKindList.Codes.HWB) ?? 0;
				if (hwbCount > MaximumAllowedHWBCount)
				{
					parent.ABL_BolTypeInfo.AddError($"Only {MaximumAllowedHWBCount} house waybills allowed in 1 Temporary Storage declaration. Please delete present line.");
				}
			}
		}

		protected override void CheckABL_ShipperRegNoType()
		{
			if (IsNotTransfer && IsNotDeconsolidation)
			{
				var parent = Parent;
				base.CheckABL_ShipperRegNoType();
				ListValidation.MessageErrorIfInvalidCode(parent.ABL_ShipperRegNoTypeInfo);
				MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(parent.ABL_ShipperRegNoTypeInfo, parent.ABL_OA_ShipperInfo);
				CheckABL_ShipperRegNoTypeWhenNotENSIsReused();
			}
		}

		protected override void CheckABL_ShipperState()
		{
			var parent = Parent;
			base.CheckABL_ShipperState();
			ListValidation.MessageErrorIfInvalidCode(parent.ABL_ShipperStateInfo);
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(parent.ABL_ShipperStateInfo, parent.ABL_OA_ShipperInfo);
		}

		protected override void CheckABL_ConsigneeRegNoType()
		{
			if (IsNotTransfer && IsNotDeconsolidation)
			{
				var parent = Parent;
				base.CheckABL_ConsigneeRegNoType();
				ListValidation.MessageErrorIfInvalidCode(parent.ABL_ConsigneeRegNoTypeInfo);
				MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(parent.ABL_ConsigneeRegNoTypeInfo, parent.ABL_OA_ConsigneeInfo);
				CheckABL_ConsigneeRegNoTypeWhenNotENSIsReused();
			}
		}

		protected override void CheckABL_ConsigneeState()
		{
			var parent = Parent;
			base.CheckABL_ConsigneeState();
			ListValidation.MessageErrorIfInvalidCode(parent.ABL_ConsigneeStateInfo);
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(parent.ABL_ConsigneeStateInfo, parent.ABL_OA_ConsigneeInfo);
		}

		protected override void CheckABL_NotifyPartyRegNoType()
		{
			var parent = Parent;
			base.CheckABL_NotifyPartyRegNoType();
			ListValidation.MessageErrorIfInvalidCode(parent.ABL_NotifyPartyRegNoTypeInfo);
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(parent.ABL_NotifyPartyRegNoTypeInfo, parent.ABL_OA_NotifyPartyInfo);
		}

		protected override void CheckABL_NotifyPartyState()
		{
			var parent = Parent;
			base.CheckABL_NotifyPartyState();
			ListValidation.MessageErrorIfInvalidCode(parent.ABL_NotifyPartyStateInfo);
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(parent.ABL_NotifyPartyStateInfo, parent.ABL_OA_NotifyPartyInfo);
		}

		void ValidateHouseBillOnDeconsolidation()
		{
			var parent = Parent;
			if (header.IsDeconsolidation && !header.Bills.Any(x => !x.ABL_Calc_IsMaster))
			{
				parent.AddRowError(Res.GetString("358486A9-A658-48A2-ADA7-E715FFC8CAE4", "When Deconsolidation, one master bill and at least one house bill must be entered."));
			}
		}

		TemporaryStorageHeader header => Parent.Header;

		bool IsNotTransfer => !(header?.IsTransfer ?? true);

		bool IsNotDeconsolidation => !(header?.IsDeconsolidation ?? true);

		bool IsValidationActive(Func<ITemporaryStorageBillValidationDecider, bool> isCheckSupported) =>
			Parent.ValidationDecider is ITemporaryStorageBillValidationDecider validationDecider && isCheckSupported(validationDecider);

		bool IsMandatoryFor(Func<ITemporaryStorageBillValidationDecider, Func<AsycudaBill, bool>> mandatoryCheck)
		{
			return Parent.ValidationDecider is ITemporaryStorageBillValidationDecider validationDecider
				&& mandatoryCheck(validationDecider)(Parent);
		}

		public static string NoAddressSelectedWarning => Res.GetString("21f2b4e0-be55-4c3f-988d-04a077ffb7fd", "The current organization will not be saved because no address is selected.");

		public static string InvalidConsignorCode => Res.GetString("db87fe19-46bd-4057-959c-cbb8fc5375b1", "Enter a valid Consignor Code.");

		public static string InvalidConsigneeCode => Res.GetString("27fa72e1-69f2-4e65-b3a7-2cb89d22d77c", "Enter a valid Consignee Code.");
	}
}
