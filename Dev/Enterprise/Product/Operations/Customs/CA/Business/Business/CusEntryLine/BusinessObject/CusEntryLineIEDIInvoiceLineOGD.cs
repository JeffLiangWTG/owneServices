using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;

namespace Enterprise.Customs.CA.Business
{
	partial class CusEntryLine : IEDIInvoiceLineOGD
	{
		#region IEDIInvoiceLineOGD Members

		ZString IEDIInvoiceLineOGD.ImportReasonCode
		{
			get { return RandomLine.CA_ImportReasonCode; }
		}

		ZString[] IEDIInvoiceLineOGD.RegistrationNumbers
		{
			get
			{
				var result = new List<ZString>();
				foreach (CFIARegistrationNumber number in RandomLine.CFIARegistrationNumbers)
				{
					result.Add(number.CY_Data);
				}
				foreach (SITTCertificationNumber number in RandomLine.SITTCertificationNumbers)
				{
					result.Add(number.CY_Data);
				}
				return result.ToArray();
			}
		}

		ZString[] IEDIInvoiceLineOGD.RegistrationTypes
		{
			get
			{
				var result = new List<ZString>();
				foreach (CFIARegistrationNumber number in RandomLine.CFIARegistrationNumbers)
				{
					result.Add(number.CY_Code);
				}
				foreach (SITTCertificationNumber number in RandomLine.SITTCertificationNumbers)
				{
					result.Add(number.CY_Code);
				}
				return result.ToArray();
			}
		}

		ZBool IEDIInvoiceLineOGD.CompliantCompletionIndicator
		{
			get { return RandomLine.CA_CompliantCompletion; }
		}

		ZBool IEDIInvoiceLineOGD.CompliantImportDateIndicator
		{
			get { return RandomLine.CA_CompliantImportDate; }
		}

		ZString IEDIInvoiceLineOGD.TIIN
		{
			get { return RandomLine.CA_TIIN; }
		}

		ZString IEDIInvoiceLineOGD.Model
		{
			get { return RandomLine.CA_Model; }
		}

		ZString IEDIInvoiceLineOGD.ModelNumber
		{
			get { return RandomLine.CA_ModelNumber; }
		}

		ZString IEDIInvoiceLineOGD.BrandName
		{
			get { return RandomLine.JI_BrandName; }
		}

		ZString IEDIInvoiceLineOGD.TypeSize
		{
			get { return RandomLine.CA_TypeSize; }
		}

		ZString IEDIInvoiceLineOGD.RequirementID
		{
			get { return RandomLine.CA_RequirementID; }
		}

		ZString IEDIInvoiceLineOGD.RequirementVersion
		{
			get { return RandomLine.CA_RequirementVer; }
		}

		ZString IEDIInvoiceLineOGD.DestinationProvince
		{
			get { return RandomLine.CA_DestinationProvince; }
		}

		ZString IEDIInvoiceLineOGD.MiscID
		{
			get { return RandomLine.CA_MiscID; }
		}

		ZString IEDIInvoiceLineOGD.CFIAOrigin
		{
			get
			{
				return RandomLine.CA_RN_NKCFIAOrigin == Core.Constants.CountryCodes.UnitedStates
						? (ZString)("U" + RandomLine.CA_CFIAUSStateOfOrigin)
						: RandomLine.CA_RN_NKCFIAOrigin;
			}
		}

		ZString IEDIInvoiceLineOGD.AirsCode
		{
			get { return RandomLine.CA_AirsCode; }
		}

		ZString IEDIInvoiceLineOGD.EndUse
		{
			get { return RandomLine.CA_EndUse; }
		}

		ZString IEDIInvoiceLineOGD.Make
		{
			get { return string.Empty; }
		}

		ZString IEDIInvoiceLineOGD.VehicleClass
		{
			get { return string.Empty; }
		}

		ZString[] IEDIInvoiceLineOGD.VIN
		{
			get { return Array.Empty<ZString>(); }
		}

		ZString[] IEDIInvoiceLineOGD.AssemblyMonth
		{
			get { return Array.Empty<ZString>(); }
		}

		#endregion

		#region IEDIInvoiceLineAQ Members

		ZInt IEDIInvoiceLineAQ.PageNumber
		{
			get { return RandomLine.CA_PageNumber; }
		}

		ZInt IEDIInvoiceLineAQ.LineNumber
		{
			get { return LineNumber; }
		}

		ZDecimal IEDIInvoiceLineAQ.UnitPrice
		{
			get { return 0; }
		}

		ZDecimal IEDIInvoiceLineAQ.LinePrice
		{
			get { return TotalLinePrice.Amount; }
		}

		ZString IEDIInvoiceLineAQ.LinePriceCurrency
		{
			get { return TotalLinePrice.Currency != null ? TotalLinePrice.Currency.Code : string.Empty; }
		}

		#endregion

		#region IEDIInvoiceLineMin Members

		ZString IEDIInvoiceLineMin.TariffNumber
		{
			get { return CL_AdValoremTariff; }
		}

		bool IsInvoiceQuantityInCustomsUnits
		{
			get
			{
				return InvoiceQuantity > 0 && Factory.GetCachedValue<CustomsUnitOfMeasureList>().ContainsCode(InvoiceUQ);
			}
		}

		ZDecimal IEDIInvoiceLineMin.Quantity
		{
			get { return IsInvoiceQuantityInCustomsUnits ? InvoiceQuantity : CustomsQuantity; }
		}

		ZString IEDIInvoiceLineMin.QuantityUnits
		{
			get { return IsInvoiceQuantityInCustomsUnits ? InvoiceUQ : CustomsUnitQty; }
		}

		ZString IEDIInvoiceLineMin.CountryOfOrigin
		{
			get
			{
				return
					RandomLine.JI_CountryOfOrigin == Core.Constants.CountryCodes.UnitedStates
						? (ZString)("U" + RandomLine.JI_StateOrRegionOfOrigin)
						: RandomLine.JI_CountryOfOrigin;
			}
		}

		ZString IEDIInvoiceLineMin.ItemDescription
		{
			get { return GoodsDescription; }
		}

		#endregion

		#region Implementation

		public ZInt LineNumber { get; set; }

		#endregion
	}
}
