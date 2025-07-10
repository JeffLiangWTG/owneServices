using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ExportPreviousProcedureValidation : PreviousDocumentValidation
	{
		public ExportPreviousProcedureValidation(PreviousDocument parent)
			: base(parent)
		{
		}

		protected override bool IsAvailable(string propertyName)
		{
			return base.IsAvailable(propertyName) || (propertyName == PreviousDocument.Schema.CSI_Tariff && Parent.IsProcedureATZL);
		}

		protected override void CheckCSI_ReferenceNumberCore()
		{
			base.CheckCSI_ReferenceNumberCore();
			var parent = Parent;
			var referenceNumber = parent.CSI_ReferenceNumber;
			MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_ReferenceNumberInfo, Res.GetString("978b6fcc-a7cc-4d4e-8823-3328cbbf99d4", "Reference"));

			if (parent.Status)
			{
				var formatErrorDescription = RegistrationNumberValidationHelper.ValidateRegistrationNumberLengthAndMrnFormat(referenceNumber, parent.Factory);
				if (!string.IsNullOrWhiteSpace(formatErrorDescription))
				{
					parent.CSI_ReferenceNumberInfo.AddMessageError(formatErrorDescription);
				}
				else if (referenceNumber.Length == RegistrationNumberValidationHelper.RegistrationNumberLength)
				{
					if (parent.IsProcedureATZL && !referenceNumber.IsValidAtlasReferenceForBondedWarehouse())
					{
						parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("7E2F1524-A1BA-4776-8746-0317167027BE", "Structure does not correspond to an ATLAS Registration Reference for Bonded Warehouse."));
					}
					else if (parent.IsProcedureATAV && !referenceNumber.IsValidAtlasReferenceForInwardProcessing())
					{
						parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("A2B248DF-409B-4370-A1CA-B6FE39CAAFDA", "Structure does not correspond to an ATLAS Registration Reference for Inward Processing."));
					}
				}
			}
		}

		protected override void CheckCSI_LineNoCore()
		{
			base.CheckCSI_LineNoCore();
			var parent = Parent;
			var lineNo = parent.CSI_LineNo;
			var referenceNumber = parent.CSI_ReferenceNumber;
			if (lineNo.IsEmpty || !lineNo.IsInRange(1, 99999))
			{
				parent.CSI_LineNoInfo.AddMessageError(Res.GetString("805FEC18-385A-4B5D-A14D-F706AC5E470A", "Line No. should be between 1 and 99999."));
			}
			if (!referenceNumber.IsEmpty
				&& !lineNo.IsEmpty
				&& parent.Parent is JobComInvoiceLine invoiceLine
				&& invoiceLine.PreviousProcedures.Cast<PreviousDocument>().Any(x => x != parent && x.CSI_ReferenceNumber == referenceNumber && x.CSI_LineNo == lineNo))
			{
				parent.CSI_LineNoInfo.AddMessageError(Res.GetString("3C771DB6-FA49-47CF-BF54-4D4127C70069", "The combination of Reference and Line No. has already been entered."));
			}
		}

		protected override void CheckCSI_QuantityCore()
		{
			base.CheckCSI_QuantityCore();
			if (Parent.IsProcedureATZL && Parent.Factory.IsIntegerRequiredUnitOfQuantity(Parent.CSI_UnitOfQuantity) && !Parent.CSI_Quantity.IsInteger)
			{
				Parent.CSI_QuantityInfo.AddMessageError(Res.GetString("FDD9F1D5-1CA6-40B9-96A3-23EAA1FAE4B0", "Only integer values are allowed for this Unit of Measurement."));
			}
		}

		protected override void CheckCSI_UnitOfQuantityCore()
		{
			base.CheckCSI_UnitOfQuantityCore();
			var parent = Parent;
			if (parent.IsProcedureATZL)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.CSI_UnitOfQuantityInfo);
			}
		}

		protected override void CheckCSI_Quantity2Core()
		{
			base.CheckCSI_Quantity2Core();
			if (Parent.IsProcedureATZL)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_Quantity2Info, Res.GetString("c7f4d425-2aa6-4603-9c02-12c2d59e71d1", "Debit Qty"));

				if (Parent.Factory.IsIntegerRequiredUnitOfQuantity(Parent.CSI_UnitOfQuantity2) && !Parent.CSI_Quantity2.IsInteger)
				{
					Parent.CSI_Quantity2Info.AddMessageError(Res.GetString("6CA03D56-872F-4BCC-A11A-3281F1CF691D", "Only integer values are allowed for this Unit of Measurement."));
				}
			}
		}

		protected override void CheckCSI_UnitOfQuantity2Core()
		{
			base.CheckCSI_UnitOfQuantity2Core();
			if (Parent.IsProcedureATZL)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_UnitOfQuantity2Info, Res.GetString("ec409aa7-9363-4a09-ab0e-95da670ad39e", "UQ (Debit Qty.)"));
				ListValidation.MessageErrorIfInvalidCode(Parent.CSI_UnitOfQuantity2Info);
			}
		}
	}
}
