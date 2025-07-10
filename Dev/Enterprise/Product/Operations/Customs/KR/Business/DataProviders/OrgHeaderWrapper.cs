using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class OrgHeaderWrapper : NonPersistentBusinessObject
	{
		public static OrgHeaderWrapper New(OrgHeader organisation)
		{
			OrgHeaderWrapper result = null;
			if (organisation != null)
			{
				result = organisation.Factory.GetCachedValue(organisation.PK.ToStringKey(), delegate
				{
					return new OrgHeaderWrapper(organisation);
				});
			}
			return result;
		}

		OrgHeaderWrapper(OrgHeader organisation) : base(organisation.Factory)
		{
			this.organisation = organisation;
		}

		readonly OrgHeader organisation;

		#region Bindable Properties

		[ResourceStringData("FFB8840D-CC85-434A-B2D8-E8219A3674C1", Caption = "Type Of Business")]
		public ZString ZO_TypeOfBusiness
		{
			get => ImportAddInfo?.ZO_TypeOfBusiness ?? ZString.Empty;
			set
			{
				if (ImportAddInfo != null)
				{
					ImportAddInfo.ZO_TypeOfBusiness = value;
				}
				else
				{
					ErrorReporter.ReportOnce("OrgHeaderWrapper.ImportAddInfo is null", "OrgheaderWrapper.ImportAddInfo is null");
				}
			}
		}
		public ZPropertyInfo ZO_TypeOfBusinessInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(KROrgImpAddInfoSchema.Constants.ZO_TypeOfBusiness, x => ImportAddInfo.ZO_TypeOfBusinessInfo); }
		}

		[ResourceStringData("A455219A-B7D3-4C62-B143-9EB8EEF53729", Caption = "Item Of Business")]
		public ZString ZO_ItemOfBusiness
		{
			get => ImportAddInfo?.ZO_ItemOfBusiness ?? ZString.Empty;
			set
			{
				if (ImportAddInfo != null)
				{
					ImportAddInfo.ZO_ItemOfBusiness = value;
				}
				else
				{
					ErrorReporter.ReportOnce("OrgHeaderWrapper.ImportAddInfo is null", "OrgheaderWrapper.ImportAddInfo is null");
				}
			}
		}
		public ZPropertyInfo ZO_ItemOfBusinessInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(KROrgImpAddInfoSchema.Constants.ZO_ItemOfBusiness, x => ImportAddInfo.ZO_ItemOfBusinessInfo); }
		}

		[ResourceStringData("BE0E8C07-5E0D-43D8-A96B-363F1089BFAE", Caption = "Refund Bank Code")]
		[List(nameof(ImportAddInfoLookups) + "." + nameof(KROrgImpAddInfoLookups.BankTypeList))]
		public ZString ZO_BankCode
		{
			get => ImportAddInfo?.ZO_BankCode ?? ZString.Empty;
			set
			{
				if (ImportAddInfo != null)
				{
					ImportAddInfo.ZO_BankCode = value;
					ZO_BankCodeInfo.RefreshBinding();
				}
				else
				{
					ErrorReporter.ReportOnce("OrgHeaderWrapper.ImportAddInfo is null", "OrgheaderWrapper.ImportAddInfo is null");
				}
			}
		}
		public ZPropertyInfo ZO_BankCodeInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(KROrgImpAddInfoSchema.Constants.ZO_BankCode, x => ImportAddInfo.ZO_BankCodeInfo); }
		}

		[ResourceStringData("D05FEAD4-9C5C-4428-8F4D-5AC364E65FA2", Caption = "Refund Account Number")]
		public ZString ZO_BankAccNo
		{
			get => ImportAddInfo?.ZO_BankAccNo ?? ZString.Empty;
			set
			{
				if (ImportAddInfo != null)
				{
					ImportAddInfo.ZO_BankAccNo = value;
				}
				else
				{
					ErrorReporter.ReportOnce("OrgHeaderWrapper.ImportAddInfo is null", "OrgheaderWrapper.ImportAddInfo is null");
				}
			}
		}
		public ZPropertyInfo ZO_BankAccNoInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(KROrgImpAddInfoSchema.Constants.ZO_BankAccNo, x => ImportAddInfo.ZO_BankAccNoInfo); }
		}

		[ResourceStringData("E4B1CF9F-0212-40A9-A2E4-8A1B8EC95738", Caption = "VAT Deferment")]
		[List(nameof(ImportAddInfoLookups) + "." + nameof(KROrgImpAddInfoLookups.VATDefermentType))]
		public ZString ZO_VATDeferment
		{
			get => ImportAddInfo.ZO_VATDeferment;
			set
			{
				if (ImportAddInfo != null)
				{
					ImportAddInfo.ZO_VATDeferment = value;
					ZO_VATDefermentInfo.RefreshBinding();
				}
				else
				{
					ErrorReporter.ReportOnce("OrgHeaderWrapper.ImportAddInfo is null", "OrgheaderWrapper.ImportAddInfo is null");
				}
			}
		}
		public ZPropertyInfo ZO_VATDefermentInfo
		{
			get { return ImportAddInfo == null ? null : GetWrappedZPropertyInfo(KROrgImpAddInfoSchema.Constants.ZO_VATDeferment, x => ImportAddInfo.ZO_VATDefermentInfo); }
		}
		#endregion

		#region OrgCountryData
		public KROrgImpAddInfoLookups ImportAddInfoLookups
		{
			get { return ImportAddInfo.Lookups; }
		}

		OrgImpAddInfo ImportAddInfo
		{
			get
			{
				if (fImportAddInfo == null && CountryData != null)
				{
					fImportAddInfo = (OrgImpAddInfo)CountryData.ImpAddInfo;
					RegisterEditableChildObject(fImportAddInfo);
				}
				return fImportAddInfo;
			}
		}
		OrgImpAddInfo fImportAddInfo;

		OrgCountryData CountryData
		{
			get
			{
				if (fCountryData == null)
				{
					fCountryData = organisation.CountryData;
					if (fCountryData.OV_RN_NKClientCountryRelation != Core.Constants.CountryCodes.KoreaSouth)
					{
						fCountryData = organisation.GetCountryData(Core.Constants.CountryCodes.KoreaSouth);
					}
				}
				return fCountryData;
			}
		}
		OrgCountryData fCountryData;
		#endregion
	}
}
