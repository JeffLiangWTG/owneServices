using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ImportPreviousDocumentValidation : PreviousDocumentValidation
	{
		public ImportPreviousDocumentValidation(PreviousDocument parent)
			: base(parent)
		{
		}

		protected override bool IsAvailable(string propertyName)
		{
			return base.IsAvailable(propertyName) || (propertyName == PreviousDocument.Schema.CSI_Tariff && Parent.IsProcedureATZL);
		}

		protected override void CheckCSI_LineNoCore()
		{
			base.CheckCSI_LineNoCore();
			var parent = Parent;
			var lineNo = parent.CSI_LineNo;
			var referenceNumber = parent.CSI_ReferenceNumber;

			if (!referenceNumber.IsEmpty
				&& !lineNo.IsEmpty
				&& parent.Parent is IPreviousDocumentParentProvider previousDocumentParentProvider
				&& previousDocumentParentProvider.PreviousDocuments.Cast<PreviousDocument>().Any(x => x != parent && x.CSI_ReferenceNumber == referenceNumber && x.CSI_LineNo == lineNo))
			{
				parent.CSI_LineNoInfo.AddMessageError(Res.GetString("6E8BD564-6A5B-4834-8839-F8A26B096FF1", "The combination of Reference and Line No. has already been entered."));
			}

			if (parent.IsProcedureATAV || parent.IsProcedureATZL)
			{
				MandatoryValidation.MessageErrorIfIsZero(parent.CSI_LineNoInfo);
				MandatoryValidation.MessageErrorIfIsNegative(parent.CSI_LineNoInfo);
			}
			else if (parent.IsProcedureATNEU)
			{
				var subType = parent.CSI_SubType;
				if (subType == OwnerReferenceTypeList.Codes.AWB || subType == OwnerReferenceTypeList.Codes.ULD)
				{
					if (!lineNo.IsEmpty)
					{
						parent.CSI_LineNoInfo.AddMessageError(Res.GetString("0B220502-A52C-4F50-A799-E27FB3E38223", "Line No. should be empty."));
					}
				}
				else if ((lineNo < 1) || (lineNo > 9999))
				{
					parent.CSI_LineNoInfo.AddMessageError(Res.GetString("5D914C25-9E0E-4BFF-BB73-6A83B6641E8A", "Line No. should be between {0} and {1}.", 1, 9999));
				}
			}
		}

		protected override void CheckCSI_SubTypeCore()
		{
			var parent = Parent;
			if (parent.IsProcedureATNEU)
			{
				var currentSubType = parent.CSI_SubType;
				var invoice = (IPreviousDocumentParentProvider)parent.Parent;
				if (invoice?.PreviousDocuments.Cast<PreviousDocument>().Any(x => x.CSI_SubType != currentSubType && !x.CSI_SubType.IsEmpty) ?? false)
				{
					parent.CSI_SubTypeInfo.AddMessageError(Res.GetString("fe2173cc-aae6-4818-b922-308711d9ce03", "You can't enter different types for Previous Procedure ATNEU"));
				}

				ListValidation.MessageErrorIfInvalidCode(parent.CSI_SubTypeInfo);
			}
			else
			{
				base.CheckCSI_SubTypeCore();
			}
		}

		protected override void CheckCSI_ReferenceNumberCore()
		{
			base.CheckCSI_ReferenceNumberCore();
			var parent = Parent;
			var procedure = parent.CSI_Procedure;
			var referenceNumber = parent.CSI_ReferenceNumber;
			if (parent.Requires18Or21CharactersReference())
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
						parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("2AED8B56-A6B2-410C-8843-B1B36C27FE91", "Structure does not correspond to an ATLAS Registration Reference for Bonded Warehouse."));
					}
					else if (parent.IsProcedureATAV && !referenceNumber.IsValidAtlasReferenceForInwardProcessing())
					{
						parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("36E3D642-9FF4-4D6A-BA0C-2FCAA266A6AA", "Structure does not correspond to an ATLAS Registration Reference for Inward Processing."));
					}
				}
			}
			else if ((parent.IsProcedureATZL && referenceNumber.IsValidAtlasReferenceForBondedWarehouse()) || (parent.IsProcedureATAV && referenceNumber.IsValidAtlasReferenceForInwardProcessing()))
			{
				parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("2D0A69D0-5EF3-4940-BC59-E01C65FAC498", "The entered Reference Number has an ATLAS structure. Please tick the 'Entry via ATLAS' Flag or change the Reference Number."));
			}
			else if (parent.IsProcedureATNEU || parent.IsProcedureATZL || parent.IsProcedureATAV)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_ReferenceNumberInfo);
			}
			else if (!referenceNumber.IsEmpty
				&& (procedure == PreviousProcedureList.Codes._T1 || procedure == PreviousProcedureList.Codes._T2 || procedure == PreviousProcedureList.Codes._ESUMA))
			{
				var mrnError = MRNFormatValidator.CheckMRNFormat(referenceNumber, parent.Factory, Res.GetString("4eaedfb5-2971-4e55-9a6c-1e0d686d2b8d", "When Previous Procedure is 'T1' or 'T2' or 'ESUMA',"));
				if (!mrnError.IsEmpty)
				{
					parent.CSI_ReferenceNumberInfo.AddMessageError(mrnError);
				}
			}
		}

		protected override void CheckCSI_QuantityCore()
		{
			base.CheckCSI_QuantityCore();
			var parent = Parent;
			var commercialQuantity = parent.CSI_Quantity;
			var commercialQuantityInfo = parent.CSI_QuantityInfo;
			if (parent.IsProcedureATNEU)
			{
				MandatoryValidation.MessageErrorIfNotEntered(commercialQuantityInfo);

				if (!commercialQuantity.IsEmpty && !commercialQuantity.IsInRange(1, 99999))
				{
					commercialQuantityInfo.AddMessageError(Res.GetString("5E6D77AB-84B6-447B-A3CF-82E147392E97", "Package Qty. should be between {0} and {1}.", 1, 99999));
				}
			}
			else if (parent.IsProcedureATZL && !commercialQuantity.IsEmpty)
			{
				if (parent.Factory.IsIntegerRequiredUnitOfQuantity(parent.CSI_UnitOfQuantity) && !commercialQuantity.IsInteger)
				{
					commercialQuantityInfo.AddMessageError(Res.GetString("29FD5CD2-3CA2-428E-A1DA-2FD8809DD534", "Only integer values are allowed for this Commercial Qty. Unit."));
				}
				else if (!commercialQuantity.IsInRange(0.001, 999999999.999))
				{
					commercialQuantityInfo.AddMessageError(Res.GetString("9A78D8ED-8874-4A3E-B5AC-F4FF6814C3DF", "Commercial Qty. should be between {0} and {1}.", 0.001, 999999999.999));
				}
			}
		}

		protected override void CheckCSI_ReferenceNumber2Core()
		{
			base.CheckCSI_ReferenceNumber2Core();
			var parent = Parent;
			if (parent.IsProcedureATNEU && (parent.CSI_SubType == OwnerReferenceTypeList.Codes.AWB || parent.CSI_SubType == OwnerReferenceTypeList.Codes.ULD))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_ReferenceNumber2Info);
			}
		}

		protected override void CheckCSI_Quantity2Core()
		{
			base.CheckCSI_Quantity2Core();
			var parent = Parent;
			var debitQuantity = parent.CSI_Quantity2;
			var debitQuantityInfo = parent.CSI_Quantity2Info;
			if (parent.IsProcedureATZL)
			{
				MandatoryValidation.MessageErrorIfNotEntered(debitQuantityInfo, Res.GetString("a8fd05d3-c034-45bd-b2aa-332226200526", "Debit Qty"));

				if (!debitQuantity.IsEmpty)
				{
					if (parent.Factory.IsIntegerRequiredUnitOfQuantity(parent.CSI_UnitOfQuantity2) && !debitQuantity.IsInteger)
					{
						debitQuantityInfo.AddMessageError(Res.GetString("7B9B901D-C45D-4D06-A833-A71187713443", "Only integer values are allowed for this Debit Qty. Unit."));
					}
					else if (!debitQuantity.IsInRange(0.001, 999999999.99))
					{
						debitQuantityInfo.AddMessageError(Res.GetString("F0820A7B-378C-4A46-95C8-DBE54FC1EFC3", "Debit Qty. should be between {0} and {1}.", 0.001, 999999999.99));
					}
				}
			}
		}

		protected override void CheckCSI_UnitOfQuantity2Core()
		{
			base.CheckCSI_UnitOfQuantity2Core();
			var parent = Parent;
			if (parent.IsProcedureATZL && !parent.CSI_Quantity2.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_UnitOfQuantity2Info);
			}
			ListValidation.MessageErrorIfInvalidCode(parent.CSI_UnitOfQuantity2Info);
		}

		protected override void CheckCSI_UnitOfQuantityCore()
		{
			base.CheckCSI_UnitOfQuantityCore();
			var parent = Parent;
			if (parent.IsProcedureATZL && !parent.CSI_Quantity.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_UnitOfQuantityInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(parent.CSI_UnitOfQuantityInfo);
		}

		protected override void CheckCSI_ItemNumberStringCore()
		{
			base.CheckCSI_ItemNumberStringCore();

			var parent = Parent;
			var targetInfo = parent.CSI_ItemNumberStringInfo;
			if (parent.CSI_ItemNumberString == "0")
			{
				targetInfo.AddMessageError(Res.GetString("58A7CE0F-0F5B-4952-B5CD-2BDF52EDB8F4", "Invoice Line No. cannot be zero."));
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
			}
		}
	}
}
