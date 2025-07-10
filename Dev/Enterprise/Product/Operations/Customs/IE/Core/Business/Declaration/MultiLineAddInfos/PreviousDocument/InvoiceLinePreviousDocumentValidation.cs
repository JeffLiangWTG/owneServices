using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.Business.Declaration
{
	static class InvoiceLinePreviousDocumentValidation
	{
		internal static void CheckCSI_Quantity(PreviousDocument parent)
		{
			var targetInfo = parent.CSI_QuantityInfo;
			MandatoryValidation.MessageErrorIfIsNegative(targetInfo);
			if (parent.CSI_Quantity.IsEmpty && !parent.CSI_UnitOfQuantity.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("E0998D39-6D9A-41B6-A4EC-E2D86D3CF7D0", "Quantity cannot be less than or equal to 0 when Unit of Quantity is entered."));
			}
		}

		internal static void CheckCSI_UnitOfQuantity(PreviousDocument parent)
		{
			var targetInfo = parent.CSI_UnitOfQuantityInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
			if (parent.CSI_UnitOfQuantity.IsEmpty && !parent.CSI_Quantity.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("E6553E2B-9EAA-4709-9862-A74D6646E877", "Unit of Quantity cannot be empty when Quantity is greater than 0."));
			}
		}

		internal static void CheckCSI_PackQty(PreviousDocument parent)
		{
			var targetInfo = parent.CSI_PackQtyInfo;
			MandatoryValidation.MessageErrorIfIsNegative(targetInfo);
			if (parent.CSI_PackQty.IsEmpty && !parent.CSI_PackType.IsEmpty && !DoesPackageHaveBulkAttribute(parent.Factory, parent.CSI_PackType))
			{
				targetInfo.AddMessageError(Res.GetString("84254F12-ED0D-4873-952D-03A4A182B568", "Number of Packages cannot be less than or equal to 0 when Type of Packages is entered."));
			}
		}

		static bool DoesPackageHaveBulkAttribute(BusinessObjectFactory factory, ZString packageType)
		{
			var packageUnitsWithBulkAttribute = Universal.RefCusCodeListTypes.GetCachedListMatchAnyAttributes(factory,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				EU.Business.UniversalReferenceConstants.UNPackTypeStartDate,
				attributeNameValuePairs: new[] { new KeyValuePair<ZString, ZString>(Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, ZString.Empty) }.ToArray());
			return packageUnitsWithBulkAttribute.ContainsCode(packageType);
		}

		internal static void CheckCSI_PackType(PreviousDocument parent)
		{
			var targetInfo = parent.CSI_PackTypeInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
			if (!parent.CSI_PackQty.IsEmpty && parent.CSI_PackType.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("99522E46-66A8-4AF6-A59E-C03D0E9E6B29", "Type of Packages cannot be empty when Number of Packages is greater than 0."));
			}
		}

		internal static void CheckCSI_ItemNumber(PreviousDocument parent)
		{
			if (!parent.CSI_ItemNumber.IsEmpty && parent.Parent is JobComInvoiceLine invoiceLine)
			{
				var style = invoiceLine.EntryInstruction?.CEI_Style.ToString();
				if (!IsStyleWithoutWarning(style))
				{
					parent.CSI_ItemNumberInfo.AddWarning(Res.GetString("855CB4AC-12D1-4BB8-B561-52D1F5C6A342", "Goods Item Identifier is only declared when 'H1', 'H2', 'H3', 'H4', 'H5', 'I1', or 'I2'."));
				}
			}

			bool IsStyleWithoutWarning(string style)
			{
				switch (style)
				{
					case ImportDeclarationTypeList.Codes.H1:
					case ImportDeclarationTypeList.Codes.H2:
					case ImportDeclarationTypeList.Codes.H3:
					case ImportDeclarationTypeList.Codes.H4:
					case ImportDeclarationTypeList.Codes.H5:
					case ImportDeclarationTypeList.Codes.I1:
					case ImportExtendedDeclarationTypeList.I2:
						return true;
					default:
						return false;
				}
			}
		}
	}
}
