using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsDepartureCargoDescPhase4Validation : EU.NCTS.Business.NctsDepartureCargoDescPhase4Validation
	{
		public NctsDepartureCargoDescPhase4Validation(NctsDepartureCargoDesc parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateIsVehicles();
				CheckMaxVehicles(Parent);
				CheckHasTransportDocumentIfSecurityFlagged(Parent);
				CheckHasPackages(Parent);
				CheckHasVehicles(Parent);
			}
		}

		public void ValidateIsVehicles()
		{
			ValidateCalculatedProperty(Parent.IsVehiclesInfo);
		}

		protected void CheckIsVehicles()
		{
			if (Parent.IsVehicles && !(Parent.BY_HarmonisedTariff.StartsWith("87", StringComparison.Ordinal) || Parent.BY_HarmonisedTariff.StartsWith("84", StringComparison.Ordinal)))
			{
				Parent.IsVehiclesInfo.AddWarning(Res.GetString("43524919-7E41-4AAF-9A1E-A30525E83CA3", "The VIN may not be required for tariff: {0}", Parent.BY_HarmonisedTariff));
			}
		}

		public void CheckMaxVehicles(NctsDepartureCargoDesc parent)
		{
			if (parent.IsVehicles && parent.Packages.Count > 99)
			{
				parent.AddRowMessageError(Res.GetString("15B1718C-8ED3-4BB5-99B1-11E85AE42E2E", "Customs will not accept a declaration with more than 99 vehicles."));
			}
		}

		protected void CheckHasTransportDocumentIfSecurityFlagged(NctsDepartureCargoDesc parent)
		{
			var nctsHeader = parent.Header;
			if (nctsHeader != null && nctsHeader.BH_FTZMove && parent.SupportingDocuments.Count == 0)
			{
				parent.AddRowWarning(Res.GetString("1BE2CA89-6E57-45CE-B634-B69248D9427E", "You have not entered a transport document for this item."));
			}
		}

		protected void CheckHasPackages(NctsDepartureCargoDesc parent)
		{
			if (!parent.IsVehicles && parent.Packages.Count == 0)
			{
				parent.AddRowMessageError(Res.GetString("35D333D7-1BAF-4DC5-9565-7D2130725760", "This line has no packaging details."));
			}
		}

		protected void CheckHasVehicles(NctsDepartureCargoDesc parent)
		{
			if (parent.IsVehicles && parent.Packages.Count == 0)
			{
				parent.AddRowMessageError(Res.GetString("1058B240-5672-49CD-AA55-2380A530327A", "This line has no vehicle details."));
			}
		}

		protected override void CheckBY_CustomsSecondQuantityIsValidZDecimal()
		{
			TypeValidation.CheckValidDecimal(Parent.BY_CustomsSecondQuantityInfo, 14, 3);
		}

		protected override void CheckBY_CustomsSecondQuantity()
		{
			base.CheckBY_CustomsSecondQuantity();
			if (!Parent.BY_CustomsSecondUnitQty.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.BY_CustomsSecondQuantityInfo);
			}
		}

		protected override void CheckBY_MonetaryValueIsValidMoney()
		{
			TypeValidation.CheckValidDecimal(Parent.BY_MonetaryValueInfo, 13, 2);
		}

		protected override void CheckBY_GrossWeight()
		{
			base.CheckBY_GrossWeight();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.BY_GrossWeightInfo);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_GrossWeightInfo);
		}

		protected override void CheckBY_GrossWeightUnit()
		{
			base.CheckBY_GrossWeightUnit();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_GrossWeightUnitInfo);
		}

		protected override void CheckBY_NetWeight()
		{
			base.CheckBY_NetWeight();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_NetWeightInfo);
		}

		protected override void CheckBY_MonetaryValue()
		{
			base.CheckBY_MonetaryValue();

			if (Parent.BY_MonetaryValue.IsEmpty)
			{
				var amountFormatted = 10000.ToString("N0", ZArchitecture.Core.Culture.CurrentCompanyCountryCulture.NumberFormat);
				var messageError = string.Format(Res.GetString("3C35E7A3-4F7A-4B61-9408-9293812C7F2A", "If Stat. Value is declared with value = 0, Customs Authorities will lock an amount of {0}€ in the declared guarantee."), amountFormatted);

				Parent.BY_MonetaryValueInfo.AddWarning(messageError);
			}
		}

		new NctsDepartureCargoDesc Parent => (NctsDepartureCargoDesc)base.Parent;
	}
}
