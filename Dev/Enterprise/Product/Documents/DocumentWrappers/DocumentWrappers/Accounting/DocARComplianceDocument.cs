using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentWrappers
{
	[CodeAlive("Used in Document")]
	public class DocARComplianceDocument : DocBaseWrapper
	{
		protected DocARComplianceDocument(ARComplianceDocumentHeader arComplianceDocumentHeader, BusinessObjectFactory factory)
		: base(arComplianceDocumentHeader, factory)
		{
		}

		public static DocARComplianceDocument New(ARComplianceDocumentHeader arComplianceDocumentHeader, BusinessObjectFactory factory)
		{
			if (arComplianceDocumentHeader == null)
			{
				return null;
			}
			return new DocARComplianceDocument(arComplianceDocumentHeader, factory);
		}

		protected ARComplianceDocumentHeader Header
		{
			get { return (ARComplianceDocumentHeader)WrappedObject; }
		}

		#region Properties

		OrgCusCode OrgProxyCusCode => GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(Header.ADH_DocumentType, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		OrgAddress OrgProxyPremisesAddress => OrgProxyCusCode?.PremisesAddress;

		public ZString CurrentCompanyName => OrgProxyPremisesAddress?.CompanyName ?? ZString.Empty;

		public ZString CompanyVATRegNum => OrgProxyCusCode?.OK_CustomsRegNo ?? ZString.Empty;

		public ZString CompanyPhoneNum => OrgProxyPremisesAddress?.PhoneNumber.FormattedForBinding ?? ZString.Empty;

		public ZString CompanyFaxNum => OrgProxyPremisesAddress?.FaxNumber.FormattedForBinding ?? ZString.Empty;

		public ZString CompanyVATRegAddress
		{
			get
			{
				if (OrgProxyPremisesAddress != null)
				{
					return OrgProxyPremisesAddress.State + " " + OrgProxyPremisesAddress.OA_City + " " + OrgProxyPremisesAddress.OA_Address1 + " " + OrgProxyPremisesAddress.OA_Address2;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public virtual ZString InvoiceDateString => Header.ADH_DocumentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

		public ZString DebtorVATRegNum => Header.VATRegistrationNum;

		public ZString DebtorName => Header.AddressOverride?.CompanyName ?? ZString.Empty;

		public ZString DebtorAddressCode => Header.AddressOverride?.OA_Code ?? ZString.Empty;

		public ZString DebtorCategory => Header.Organisation?.OH_Category ?? ZString.Empty;

		#region ChargeLines

		public DocARComplianceDocumentLineCollection ChargeLines
		{
			get
			{
				return chargeLines ?? (chargeLines = GetChargeLines());
			}
		}

		DocARComplianceDocumentLineCollection chargeLines;

		protected virtual DocARComplianceDocumentLineCollection GetChargeLines()
		{
			var result = new DocARComplianceDocumentLineCollection(Factory);
			if (Header != null)
			{
				foreach (AccComplianceDocumentLine line in Header.ComplianceDocumentLines)
				{
					var docLine = DocARComplianceDocumentLine.New(line, Factory);
					result.Add(docLine);
				}
			}

			return result;
		}

		#endregion

		public ZDateTime DocumentDate => Header.ADH_DocumentDate;

		public ZString DocumentDescription => Header.ADH_Description;

		public ZDecimal SumLineAmount => Header.Amount;

		public ZDecimal SumTaxAmount => Header.TaxAmount;

		public ZDecimal SumTotalAmount => Header.TotalAmount;

		public ZString DocumentNumber => Header.ADH_DocumentNumber;

		public ZString TransactionNumber => FirstTransactionHeader?.AH_TransactionNum ?? ZString.Empty;

		public virtual ZString InternalReference => Header.ADH_InternalReference;

		public ZInt PrintCount => Header.ADH_PrintCount;

		public ZString HouseBill
		{
			get
			{
				var job = FirstTransactionHeader?.Job;
				if (job != null)
				{
					if (fForwardingShipment == null)
					{
						if (job.JH_ParentTableCode == JobShipmentSchema.Constants.Prefix)
						{
							fForwardingShipment = Factory.Load<ForwardingShipment>(job.JH_ParentID);
						}
					}
					return fForwardingShipment?.JS_HouseBill ?? ZString.Empty;
				}
				return ZString.Empty;
			}
		}
		ForwardingShipment fForwardingShipment;

		AccTransactionHeader FirstTransactionHeader => Header.TransactionHeaders.FirstOrDefault() as AccTransactionHeader;

		public ZString Remark
		{
			get
			{
				var collection = AccountingMasterFilesRegistry.Instance.DocumentConfiguration.GetValueWithoutFallback(Header.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
				return collection.Cast<ComplianceDocumentImage>().FirstOrDefault(x => x.Country == Header.Organisation.Country.Code && x.ComplianceSubType == Header.ADH_ComplianceSubType)?.Remark ?? ZString.Empty;
			}
		}

		public Image Stamp
		{
			get
			{
				var collection = AccountingMasterFilesRegistry.Instance.DocumentConfiguration.GetValueWithoutFallback(Header.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
				return collection.Cast<ComplianceDocumentImage>().FirstOrDefault(x => x.Country == Header.Organisation.Country.Code && x.ComplianceSubType == Header.ADH_ComplianceSubType)?.Image ?? new Bitmap(1, 1);
			}
		}

		#endregion

	}
}
