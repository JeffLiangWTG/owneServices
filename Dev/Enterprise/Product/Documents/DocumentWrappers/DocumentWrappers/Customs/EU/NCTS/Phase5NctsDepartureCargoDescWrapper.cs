using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS
{
	public class Phase5NctsDepartureCargoDescWrapper : DocBaseWrapper
	{
		protected Phase5NctsDepartureCargoDescWrapper(NctsDepartureCargoDesc line, BusinessObjectFactory factory) : base(line, factory)
		{
			item = line;
			bill = line.Bill;
			movementHeader = line.MoveHeader;
		}

		readonly NctsDepartureCargoDesc item;
		readonly NctsBill bill;
		readonly NctsDepartureMovementHeader movementHeader;

		public static Phase5NctsDepartureCargoDescWrapper New(NctsDepartureCargoDesc line, BusinessObjectFactory factory)
		{
			return new Phase5NctsDepartureCargoDescWrapper(line, factory);
		}

		public ZString BOX12_08UCR => bill.B0_ReferenceID;

		public ZString BOX13_02CONSIGNOREORI => bill.Consignor.GetEuIdentificationNumber();
		public ZString BOX13_02CONSIGNORNAME => bill.Consignor?.CompanyName ?? ZString.Empty;
		public ZString BOX13_02CONSIGNORADDRESS => bill.Consignor?.Address.MultiLine() ?? ZString.Empty;
		public ZString BOX13_02CONSIGNORCONTACTNAME => bill.Consignor?.E2_Contact ?? ZString.Empty;
		public ZString BOX13_02CONSIGNORCONTACTPHONE => bill.Consignor?.E2_Phone ?? ZString.Empty;
		public ZString BOX13_02CONSIGNORCONTACTEMAIL => bill.Consignor?.E2_Email ?? ZString.Empty;

		public ZString BOX13_03CONSIGNEEEORI => bill.Consignee.GetEuIdentificationNumber();
		public ZString BOX13_03CONSIGNEENAME => bill.Consignee?.CompanyName ?? ZString.Empty;
		public ZString BOX13_03CONSIGNEEADDRESS => bill.Consignee?.Address.MultiLine() ?? ZString.Empty;

		IReadOnlyCollection<CusSupplyChainActorReference> cusSupplyChainActorReferences => cusSupplyChainActorReferencesCached ?? bill.CusSupplyChainActorReferences.OrderBy(x => x.CFR_Type).ToArray();
		readonly CusSupplyChainActorReference[] cusSupplyChainActorReferencesCached;

		public ZString BOX13_14SUPPLYCHAINACTORROLE
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in cusSupplyChainActorReferences.Select(x => x.CFR_Code))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}
		public ZString BOX13_14SUPPLYCHAINACTORID
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var reference in cusSupplyChainActorReferences.Select(x => x.CFR_Reference))
				{
					stringBuilder.AppendLine(reference);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX16_06COUNTRYOFDISPATCH => bill.B0_RN_NKCountryOfExport;

		public ZString BOX18_04GROSSMASS => bill.B0_Weight.ToString();

		public ZString BOX19_05DEPTRANSPORTMEANSTYPEOFID => movementHeader.TransportTypeAtDeparture;
		public ZString BOX19_05DEPTRANSPORTMEANSID => movementHeader.TransportAtDeparture;

		IReadOnlyCollection<CommonPreviousDocument> previousDocuments => previousDocumentsCached ?? bill.PreviousDocuments.OrderBy(x => x.CSI_LineNo).ToArray();
		readonly CommonPreviousDocument[] previousDocumentsCached;

		public ZString BOX12_01PREVIOUSDOCUMENTTYPE
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in previousDocuments.Select(x => x.CSI_Code))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_01PREVIOUSDOCUMENTREFERENCENUMBER
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in previousDocuments.Select(x => x.CSI_ReferenceNumber))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_01PREVIOUSDOCUMENTCOMPLEMENTOFINFORMATION
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in previousDocuments.Select(x => x.CSI_ReferenceNumber2))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		IReadOnlyCollection<NctsSupportingDocument> supportingDocuments => supportingDocumentsCached ?? bill.SupportingDocuments.OrderBy(x => x.CSI_LineNo).ToArray();
		readonly NctsSupportingDocument[] supportingDocumentsCached;

		public ZString BOX12_03SUPPORTINGDOCUMENTTYPE
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in supportingDocuments.Select(x => x.CSI_Code))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_03SUPPORTINGDOCUMENTREFERENCENUMBER
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in supportingDocuments.Select(x => x.CSI_ReferenceNumber))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_03SUPPORTINGDOCUMENTLINEITEMNUMBER
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in supportingDocuments.Select(x => x.CSI_ItemNumber))
				{
					stringBuilder.AppendLine(code.ToString());
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_03SUPPORTINGDOCUMENTCOMPLEMENTOFINFORMATION
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in supportingDocuments.Select(x => x.CSI_ReferenceNumber2))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		IReadOnlyCollection<NctsBillAdditionalDocument> additionalDocuments => additionalDocumentsCached ?? bill.AdditionalDocuments.OrderBy(x => x.CSI_LineNo).ToArray();
		readonly NctsBillAdditionalDocument[] additionalDocumentsCached;

		public ZString BOX12_05TRANSPORTDOCUMENTTYPE
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in additionalDocuments.Where(x => x.CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.TransportDocument)).Select(x => x.CSI_Code))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_05TRANSPORTDOCUMENTREFERENCENUMBER
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in additionalDocuments.Where(x => x.CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.TransportDocument)).Select(x => x.CSI_ReferenceNumber))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_04ADDITIONALREFERENCETYPE
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in additionalDocuments.Where(x => x.CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.AdditionalReference)).Select(x => x.CSI_Code))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_04ADDITIONALREFERENCEREFERENCENUMBER
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in additionalDocuments.Where(x => x.CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.AdditionalReference)).Select(x => x.CSI_ReferenceNumber))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_02ADDITIONALINFORMATIONCODE
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in additionalDocuments.Where(x => x.CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.AdditionalInformation)).Select(x => x.CSI_Code))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_02ADDITIONALINFORMATIONTEXT
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in additionalDocuments.Where(x => x.CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.AdditionalInformation)).Select(x => x.CSI_Description))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX14_02TRANSPORTCHARGES => bill.B0_TransportPaymentMethod;

		public ZString BOX11_03CONSIGNMENTITEMGOODSITEMNUMBER => item.BY_LineNo.ToString();

		public ZString BOX11_11CONSIGNMENTITEMDECLARATIONGOODSITEMNUMBER => item.BY_DeclarationGoodsItemNumber.ToString();

		public ZString BOX11_01CONSIGNMENTITEMDECLARATIONTYPE => item.BY_Type;

		public ZString BOX16_06CONSIGNMENTITEMCOUNTRYOFDISPATCH => item.BY_RN_NKCountryOfDispatch;

		public ZString BOX16_03CONSIGNMENTITEMCOUNTRYOFDESTINATION => item.BY_RN_NKCountryOfDestination;

		public ZString BOX12_08CONSIGNMENTITEMREFERENCENUMBERUCR => item.BY_CommercialReferenceNumber;

		public ZString BOX13_03CONSIGNMENTITEMCONSIGNEEEORI => item.Consignee.GetEuIdentificationNumber();

		public ZString BOX13_03CONSIGNMENTITEMCONSIGNEENAME => item.Consignee?.Organisation?.OH_FullName ?? ZString.Empty;

		public ZString BOX13_03CONSIGNMENTITEMCONSIGNEEADDRESS
		{
			get
			{
				var addressLineFields = new Func<OrgAddress, string>[]
				{
					a => a.Address1,
					a => a.Address2,
					a => a.Postcode,
					a => a.City,
					a => a.Country?.Code ?? string.Empty
				};
				return item.Consignee?.Address?.MultiLine(addressLineFields);
			}
		}

		public ZString BOX13_14ADDITIONALSUPPLYCHAINACTORROLE
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in cusSupplyChainActorReferences?.Select(x => x.CFR_Code))
				{
					if (!code.IsEmpty)
					{
						stringBuilder.AppendLine(code);
					}
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX13_14ADDITIONALSUPPLYCHAINACTORIDENTIFICATIONNUMBER
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in cusSupplyChainActorReferences?.Select(x => x.CFR_Reference))
				{
					if (!code.IsEmpty)
					{
						stringBuilder.AppendLine(code);
					}
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX18_05COMMODITYDESCRIPTIONOFGOODS => item.BY_Description;

		public ZString BOX18_08COMMODITYCUSCODE => item.BY_CusC4Number;

		public ZString BOX18_09COMMODITYCODEHARMONIZEDSYSTEMSUBHEADING => item.BY_FormattedHarmonisedTariff.SubstringSafe(0, 7);

		public ZString BOX18_09COMMODITYCODECOMBINEDNOMENCLATURECODE => item.BY_FormattedHarmonisedTariff.SubstringSafe(8, 2);

		public ZString BOX18_09COMMODITYCODE => item.BY_FormattedHarmonisedTariff.SubstringSafe(0, 10);

		public ZString BOX18_07DANGEROUSGOODSUNNUMBER
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.UNDGs?.Select(d => d.DI_DG))
				{
					if (!code.IsEmpty)
					{
						stringBuilder.AppendLine(code.ToString());
					}
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX18_04GOODSMEASUREGROSSMASS => item.BY_GrossWeight.ToString(6);

		public ZString BOX18_01GOODSMEASURENETTMASS => item.BY_NetWeight.ToString(6);

		public ZString BOX18_02GOODSMEASURESUPPLEMENTARYUNITS => item.BY_CustomsSecondQuantity.ToString(6);

		public ZString BOX18_06PACKAGINGTYPE
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.Packages?.Select(p => p.B5_UnitType))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX18_06PACKAGINGNUMBEROFPACKAGES
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.Packages?.Select(p => p.B5_UnitCount))
				{
					stringBuilder.AppendLine(code.ToString());
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX18_06PACKAGINGSHIPPINGMARKS
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.Packages?.Select(p => p.B5_MarksAndNumbers))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_01GOODSITEMPREVIOUSDOCUMENTTYPE
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.PreviousDocuments?.Select(p => p.CSI_Code))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_01GOODSITEMPREVIOUSDOCUMENTREFERENCENUMBER
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.PreviousDocuments?.Select(p => p.CSI_ReferenceNumber))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_01GOODSITEMPREVIOUSDOCUMENTGOODSITEMNUMBER
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.PreviousDocuments?.Select(p => p.CSI_ItemNumber))
				{
					stringBuilder.AppendLine(code.ToString());
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_01GOODSITEMPREVIOUSDOCUMENTTYPEOFPACKAGES
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.PreviousDocuments?.Select(p => p.CSI_UnitOfQuantity2))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_01GOODSITEMPREVIOUSDOCUMENTNUMBEROFPACKAGES
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.PreviousDocuments?.Select(p => p.CSI_Quantity2))
				{
					stringBuilder.AppendLine(code.ToString());
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_01GOODSITEMPREVIOUSDOCUMENTMEASUREMENTUNIT
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.PreviousDocuments?.Select(p => p.CSI_UnitOfQuantity))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_01GOODSITEMPREVIOUSDOCUMENTQUANTITY
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.PreviousDocuments?.Select(p => p.CSI_Quantity))
				{
					stringBuilder.AppendLine(code.ToString(6));
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_01GOODSITEMPREVIOUSDOCUMENTCOMPLEMENTOFINFORMATION
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.PreviousDocuments?.Select(p => p.CSI_ReferenceNumber2))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_03GOODSITEMSUPPORTINGDOCUMENTTYPE
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.SupportingDocuments?.Select(p => p.CSI_Code))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_03GOODSITEMSUPPORTINGDOCUMENTREFERENCENUMBER
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.SupportingDocuments?.Select(p => p.CSI_ReferenceNumber))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_03GOODSITEMSUPPORTINGDOCUMENTLINEITEMNUMBER
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.SupportingDocuments?.Select(p => p.CSI_ItemNumber))
				{
					stringBuilder.AppendLine(code.ToString());
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_03GOODSITEMSUPPORTINGDOCUMENTCOMPLEMENTOFINFORMATION
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.SupportingDocuments?.Select(p => p.CSI_ReferenceNumber2))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_05_GOODSITEMTRANSPORTDOCUMENTTYPE
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.AdditionalInfos?.Where(x => x.CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.TransportDocument)).Select(p => p.CSI_Code))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_05_GOODSITEMTRANSPORTDOCUMENTREFERENCENUMBER
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.AdditionalInfos?.Where(x => x.CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.TransportDocument)).Select(p => p.CSI_ReferenceNumber))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_04GOODSITEMADDITIONALREFERENCETYPE
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.AdditionalInfos?.Where(x => x.CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.AdditionalReference)).Select(x => x.CSI_Code))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_04GOODSITEMADDITIONALREFERENCEREFERENCENUMBER
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.AdditionalInfos?.Where(x => x.CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.AdditionalReference)).Select(x => x.CSI_ReferenceNumber))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_02GOODSITEMADDITIONALINFORMATIONCODE
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.AdditionalInfos.Where(x => x.CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.AdditionalInformation)).Select(x => x.CSI_Code))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}

		public ZString BOX12_02GOODSITEMADDITIONALINFORMATIONTEXT
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var code in item.AdditionalInfos.Where(x => x.CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.AdditionalInformation)).Select(x => x.CSI_Description))
				{
					stringBuilder.AppendLine(code);
				}
				return stringBuilder.ToString().Trim();
			}
		}
	}
}
