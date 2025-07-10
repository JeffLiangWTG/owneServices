using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.DataTransfer
{
	public class CATCPLoadFileDataAdapter : ValueObjectDataAdapter<OrgHeader, CATCPHeaderValueObject>
	{
		public CATCPLoadFileDataAdapter()
			: base()
		{
		}

		protected override void ExportToValueObjectCore(OrgHeader orgHeader, CATCPHeaderValueObject catcpHeader, Enterprise.DataTransfer.Integration.IValueObjectExportContext context)
		{
			var tcpCollection = OrgImpAddInfo.Get(orgHeader).TradeChainPartners;

			if (tcpCollection != null && tcpCollection.Count > 0)
			{
				var brm = GetBRM(orgHeader);
				if (!brm.IsEmpty)
				{
					if (!tcpCollection.Any(x => x.HasMessageErrors))
					{
						catcpHeader.RecordIdentifier = RecordIdentifier.Header;
						catcpHeader.BusinessNumber = brm.Left(9);

						foreach (TradeChainPartner tcp in tcpCollection)
						{
							var tcpLine = catcpHeader.Lines.AddNew();

							tcpLine.RecordIdentifier = tcp.CA_Type == TradeChainPartnersTypeList.Codes.V ? RecordIdentifier.Vendor : RecordIdentifier.Consignee;
							tcpLine.BusinessNumber = brm;
							tcpLine.TCPTypeCode = GetTCPTypeCode(tcp.CA_CSAIDType);
							tcpLine.TCPIdentifier = tcp.CA_CSAID;

							var address = tcp.OrganizationAddress;
							tcpLine.AddressLine1 = address.Address1;
							tcpLine.AddressLine2 = address.Address2;
							tcpLine.City = address.City;
							tcpLine.ProvinceStateCode = address.StateCode;
							tcpLine.CountryCode = address.OA_RN_NKCountryCode;
							tcpLine.PostalZipCode = address.Postcode.Replace(" ", "");

							tcpLine.BusinessName = tcp.Organization.OH_FullName;
						}
					}
					else
					{
						context.AddRange(tcpCollection.GetErrors());
					}
				}
				else
				{
					context.AddError(Res.GetString("87DE9FD8-198B-4B02-A26F-9F99A1952145", "CSA Importer {0} is missing mandatory field BRM in Organization. Unable to generate the required file. Please input a valid BRM and try again.", orgHeader.OH_FullName));
				}
			}
			else
			{
				context.AddError(Res.GetString("734D6E88-0E95-454D-A44A-E495FA15D54A", "Nothing to export."));
			}
		}

		ZString GetTCPTypeCode(ZString csaIDType)
		{
			switch (csaIDType)
			{
				case CSAConsigneeIDTypeList.Codes.BRM:
					return TCPType.BusinessNumber;
				case CSAConsigneeIDTypeList.Codes.CSA:
				case CSAConsigneeIDTypeList.Codes.ORG:
					return TCPType.Internal;
				case CSAVendorIDTypeList.Codes.CCC:
					return TCPType.SCAC;
				case CSAVendorIDTypeList.Codes.DUN:
					return TCPType.DunnBradstreet;
				case CSAVendorIDTypeList.Codes.EIN:
				case CSAVendorIDTypeList.Codes.SSN:
					return TCPType.Other;
				default:
					return "00";
			}
		}

		ZString GetBRM(OrgHeader orgHeader)
		{
			return orgHeader.CustomsCodes?.GetOrgCusCodesForCodeAndCountry(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, Core.Constants.CountryCodes.Canada).FirstOrDefault()?.OK_CustomsRegNo ?? ZString.Empty;
		}

		public static class RecordIdentifier
		{
			public const string Header = "00";
			public const string Consignee = "02";
			public const string Vendor = "03";
			public const string Trailer = "99";
		}

		public static class TCPType
		{
			public const string DunnBradstreet = "01";
			public const string Internal = "02";
			public const string BusinessNumber = "03";
			public const string InternalRevenueService = "04";
			public const string SCAC = "05";
			public const string Other = "06";
		}

		#region Import

		protected override void ImportFromValueObjectCore(OrgHeader bizObj, CATCPHeaderValueObject value, Enterprise.DataTransfer.Integration.IValueObjectImportContext context)
		{
			throw GetNotSupportedException("ImportFromValueObjectCore");
		}

		#endregion

		#region Implementation

		public override System.Xml.Schema.XmlSchema CollectionSchema
		{
			get { throw GetNotSupportedException("CollectionSchema"); }
		}

		public override string RootCollectionElementName
		{
			get { throw GetNotSupportedException("RootCollectionElementName"); }
		}

		public override string RootElementName
		{
			get { throw GetNotSupportedException("RootElementName"); }
		}

		public override System.Xml.Schema.XmlSchema Schema
		{
			get { throw GetNotSupportedException("Schema"); }
		}

		public NotSupportedException GetNotSupportedException(string name)
		{
			throw new NotSupportedException(Res.GetString("637BFC1C-2C65-4F05-841A-36E41306045B", "{0} is not supported by TCP Load File Adapter.", name));
		}

		#endregion
	}
}
