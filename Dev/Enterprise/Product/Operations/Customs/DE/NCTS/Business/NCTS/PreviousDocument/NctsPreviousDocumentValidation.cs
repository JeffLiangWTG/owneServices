using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.Universal;
using DEReferenceConstants = Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsPreviousDocumentValidation : EU.NCTS.Business.NctsPreviousDocumentPhase5Validation
	{
		public NctsPreviousDocumentValidation(NctsPreviousDocument parent) : base(parent)
		{
		}

		protected new NctsPreviousDocument Parent => (NctsPreviousDocument)base.Parent;

		protected override bool IsSubTypeMandatory => true;

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckRowMaxCount();
		}

		public void ValidateAuthorizationNumber()
		{
			ValidateCalculatedProperty(Parent.AuthorizationNumberInfo);
		}

		protected void CheckAuthorizationNumber()
		{
			if (IsAvailable(NctsPreviousDocument.Schema.AuthorizationNumber))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AuthorizationNumberInfo, Res.GetString("4c9e5065-d5ae-4543-b0a1-0715b3b45c3d", "Authorization Number"));
			}
		}

		protected override void CheckCSI_Description()
		{
			if (IsColumnAvailable(NctsPreviousDocument.Schema.CSI_Description))
			{
				base.CheckCSI_Description();

				var parent = Parent;
				if (parent.IsProcedure9DEY)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_DescriptionInfo);
				}
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			var parent = Parent;
			var targetInfo = parent.CSI_ReferenceNumberInfo;
			if (IsColumnAvailable(NctsPreviousDocument.Schema.CSI_ReferenceNumber))
			{
				CheckCSI_ReferenceNumber_ForPreviousProcedure();
			}
			else if (parent.IsForPreviousDocument)
			{
				base.CheckCSI_ReferenceNumber();
			}

			void CheckCSI_ReferenceNumber_ForPreviousProcedure()
			{
				base.CheckCSI_ReferenceNumber();

				var referenceNumber = parent.CSI_ReferenceNumber;
				var entryViaATLAS = parent.Status;

				if (parent.IsProcedureN337 && parent.CSI_SubType == PreviousDocSubTypeList.Codes.REG
					|| (parent.IsProcedure9DEZ && entryViaATLAS && !PreviousDocumentHelper.IsValidAtlasReferenceForBondedWarehouse(referenceNumber))
					|| (parent.IsProcedure9DEY && entryViaATLAS && !PreviousDocumentHelper.IsValidAtlasReferenceForInwardProcessing(referenceNumber)))
				{
					var referenceNumberValidationError = RegistrationNumberValidationHelper.ValidateRegistrationNumberLengthAndMrnFormat(referenceNumber, parent.Factory);
					if (!string.IsNullOrWhiteSpace(referenceNumberValidationError))
					{
						targetInfo.AddMessageError(referenceNumberValidationError);
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
			}
		}

		protected override void CheckCSI_ReferenceNumber2()
		{
			base.CheckCSI_ReferenceNumber2();
			PropertyIsMandatoryWhenHasAttributeWithValueY(Parent.CSI_ReferenceNumber2Info, DEReferenceConstants.RefCusCodeListAttributes.Name.Complement);
		}

		protected override void CheckCSI_ReferenceNumber2MaxLength()
		{
		}

		protected override void CheckCSI_Tariff()
		{
			if (IsColumnAvailable(NctsPreviousDocument.Schema.CSI_Tariff))
			{
				base.CheckCSI_Tariff();

				var parent = Parent;
				var tariff = parent.CSI_Tariff;
				var targetInfo = parent.CSI_TariffInfo;
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				if (!tariff.IsEmpty && (tariff.Length != 11 || !tariff.IsNumbersOnlyOrEmpty || IsUnknownTariffCode()))
				{
					targetInfo.AddMessageError(Res.GetString("D74B272B-A26E-4E85-9A18-F04DEE51B46E", "The entered Commodity Code is not valid."));
				}

				bool IsUnknownTariffCode() => new TariffView.Loader(parent.Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Germany, Constants.TariffTypes.Import, tariff, ZDateTime.Now) == null;
			}
		}

		protected override void CheckCSI_SubType()
		{
			if (IsColumnAvailable(NctsPreviousDocument.Schema.CSI_SubType))
			{
				base.CheckCSI_SubType();
				var parent = Parent;
				if (parent.Parent is NctsDepartureCargoDesc goodsItem)
				{
					var containTargetPreviousProcedure = goodsItem.PreviousProcedures.OfType<NctsPreviousDocument>().Any(x => x.CSI_SubType != PreviousDocSubTypeList.Codes.REG);
					var isPhase5 = goodsItem.Header?.IsPhase5 ?? ZBool.False;
					if (parent.CSI_SubType == PreviousDocSubTypeList.Codes.REG && containTargetPreviousProcedure && isPhase5)
					{
						parent.CSI_SubTypeInfo.AddMessageError(Res.GetString("0641A901-CF57-4B03-88A9-D17A093E8C49", "Type 'REG' must not be used at the same time with other Types within one item."));
					}
				}
			}
		}

		protected override void CheckCSI_UnitOfQuantity()
		{
			if (IsColumnAvailable(NctsPreviousDocument.Schema.CSI_UnitOfQuantity))
			{
				base.CheckCSI_UnitOfQuantity();
				ListValidation.MessageErrorIfInvalidCode(Parent.CSI_UnitOfQuantityInfo);

				if (!Parent.CSI_Quantity.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_UnitOfQuantityInfo, Res.GetString("BF572911-EA14-45AF-BE42-9DC0D04667F2", "Commercial UQ"));
				}
			}
		}

		protected override void CheckCSI_LineNo()
		{
			if (IsColumnAvailable(NctsPreviousDocument.Schema.CSI_LineNo))
			{
				base.CheckCSI_LineNo();

				var parent = Parent;
				if (parent.IsProcedure9DEY || parent.IsProcedure9DEZ || (parent.IsProcedureN337 && parent.CSI_SubType == PreviousDocSubTypeList.Codes.REG))
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_LineNoInfo);
				}
			}
		}

		protected override void CheckCSI_Quantity()
		{
			if (IsColumnAvailable(NctsPreviousDocument.Schema.CSI_Quantity))
			{
				base.CheckCSI_Quantity();

				var parent = Parent;
				if (parent.IsProcedureN337)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_QuantityInfo);
				}
			}
		}

		protected override void CheckCSI_Quantity2()
		{
			if (IsColumnAvailable(NctsPreviousDocument.Schema.CSI_Quantity2))
			{
				base.CheckCSI_Quantity2();
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_Quantity2Info);
			}
		}

		protected override void CheckCSI_UnitOfQuantity2()
		{
			if (IsColumnAvailable(NctsPreviousDocument.Schema.CSI_UnitOfQuantity2))
			{
				base.CheckCSI_UnitOfQuantity2();
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_UnitOfQuantity2Info, Res.GetString("2E2B677B-FC9E-4AC5-91BD-34FF908A84B2", "Debit UQ"));
			}
		}

		protected override void CheckCSI_Code()
		{
			if (Parent.IsForPreviousDocument)
			{
				base.CheckCSI_Code();
			}
		}

		protected override ZString ItemNumberPropertyDescription => Res.GetString("2A95F024-9558-4110-AF2A-41752B1BEE8D", "Item Number (1-99999)");

		void CheckRowMaxCount()
		{
			var parent = Parent;
			parent.ClearRowNotifications();
			if (parent.Parent is NctsDepartureCargoDesc goodsItem)
			{
				if (new NctsPreviousProcedureList().ContainsCode(parent.CSI_Procedure) && goodsItem.PreviousDocuments.Count > 999)
				{
					parent.AddRowMessageError(Res.GetString("7DFBCE6C-0C12-4654-B315-44ADBCD6F532", "You are only allowed a maximum of 999 Previous Document here."));
				}
			}
		}

		bool IsAvailable(string propertyName) //Checks whether the given property is shown in the grid, (i.e. whether it should be validated or not)
			=> NctsPreviousDocumentHelper.IsAvailable(Parent.CSI_Procedure, propertyName);

		bool IsColumnAvailable(string columnName)
		{
			var parent = Parent;
			return NctsPreviousDocumentHelper.GetAvailableNctsColumns(parent.CSI_Procedure).Any(x => x.ColumnName == columnName) || (columnName == NctsPreviousDocument.Schema.CSI_Tariff && parent.IsProcedure9DEZ);
		}

		void PropertyIsMandatoryWhenHasAttributeWithValueY(ZPropertyInfo propertyInfo, string attributeName)
		{
			var parent = Parent;
			if (parent.IsForPreviousDocument && parent.RefCusCode != null
					&& EU.NCTS.Business.CusSupportingInfoHelper.HasAttributeForMandatoryValidation(parent.RefCusCode, attributeName, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListLevelTypes.Item))
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
		}
	}
}
