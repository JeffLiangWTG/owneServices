using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.ServiceTask;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS
{
	public class NctsIE29GoodsItemLineWrapper : DocBaseWrapper
	{
		NctsIE29GoodsItemLineWrapper(NctsGoodsItemResponseData lineItem, NctsEdiMessage ediMessage, BusinessObjectFactory factory) : base(lineItem, factory)
		{
			this.lineItem = Argument.NotNull(lineItem, nameof(lineItem));
			this.ediMessage = ediMessage;
		}

		public static NctsIE29GoodsItemLineWrapper New(NctsGoodsItemResponseData lineItem, NctsEdiMessage ediMessage)
		{
			return ediMessage != null && lineItem != null ? new NctsIE29GoodsItemLineWrapper(lineItem, ediMessage, ediMessage.Factory) : null;
		}

		public static NctsIE29GoodsItemLineWrapper New(NctsGoodsItemResponseData lineItem, BusinessObjectFactory factoryToWrap)
		{
			return null;
		}

		public ZString BOX2CONSIGNOR
		{
			get { return Tools.ValueOrThreeDashes(GetWrappedAddressSummary(lineItem.Consignor)); }
		}

		public ZString BOX2CONSIGNORSECURITY
		{
			get { return GetWrappedAddressSummary(lineItem.SecurityConsignor); }
		}

		public ZString BOX7REFERENCE
		{
			get { return lineItem.CommercialReferenceNumber; }
		}

		public ZString BOX7UCR
		{
			get { return (bool)(ediMessage.Header?.IsPhase5) ? lineItem.ReferenceNumberUCR : ZString.Empty; }
		}

		public ZString BOX8CONSIGNEE
		{
			get { return Tools.ValueOrThreeDashes(GetWrappedAddressSummary(lineItem.Consignee)); }
		}

		public ZString BOX8CONSIGNEESECURITY
		{
			get { return GetWrappedAddressSummary(lineItem.SecurityConsignee); }
		}

		public ZString BOX1REGIME
		{
			get { return Tools.ValueOrThreeDashes(lineItem.DeclarationType); }
		}

		public ZString BOX15COUNTRYOFORIGIN
		{
			get { return Tools.ValueOrThreeDashes(lineItem.CountryOfDispatch); }
		}

		public ZString BOX17COUNTRYOFDESTINATION
		{
			get { return Tools.ValueOrThreeDashes(lineItem.CountryOfDestination); }
		}

		public ZString BOX32ITEM
		{
			get { return lineItem.ItemNumber.ToString(); }
		}

		public ZString BOX33COMMODITY
		{
			get { return Tools.ValueOrThreeDashes(lineItem.CommodityCode); }
		}

		public ZString BOX35GROSSMASS
		{
			get
			{
				lineItem.GrossMassInKilograms = Tools.GetFormattedDecimal(lineItem.GrossMassInKilograms);
				return Tools.ValueOrThreeDashes(lineItem.GrossMassInKilograms);
			}
		}

		public ZString BOX38NETTMASS
		{
			get
			{
				lineItem.NetMassInKilograms = Tools.GetFormattedDecimal(lineItem.NetMassInKilograms);
				return lineItem.NetMassInKilograms;
			}
		}

		public ZString BOX40DOCUMENTS
		{
			get { return GetPreviousDocuments(); }
		}

		public ZString BOX44
		{
			get { return GetSupportingDocuments(); }
		}

		public ZString BOX444UNDG
		{
			get { return lineItem.UNDangerousGoodsCode; }
		}

		public ZString BOX441DOCSANDCERTS
		{
			get { return GetSupportingDocuments(); }
		}

		public ZString BOX442SPECIALMENTIONS
		{
			get
			{
				var sb = new ZStringBuilder();
				if (lineItem.SpecialMentions != null)
				{
					foreach (var sm in (from SpecialMentionResponseData sm in lineItem.SpecialMentions where sm.NctsExportFromCountry.IsEmpty && !sm.NctsExportFromEC select sm))
					{
						var description = Factory.GetCachedValue<string>("SM." + sm.TypeCode, delegate
						{
							var query = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
							query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, sm.TypeCode);
							query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation);
							var ai = Factory.LoadTop1<ZZRefCusCodeListCombined>(query);
							return ai == null ? sm.TypeCode : ai.ZZD_Description.Replace(";", "");
						});
						sb.Append(description + ":" + sm.Description);
					}
					foreach (var sm in (from SpecialMentionResponseData sm in lineItem.SpecialMentions where !sm.NctsExportFromCountry.IsEmpty && !sm.NctsExportFromEC select sm))
					{
						sb.Append(Res.GetString("C39B15E3-8438-4996-AB26-AD823A9BB155", "Export from {0} subject to restrictions", sm.NctsExportFromCountry));
					}
					foreach (var sm in (from SpecialMentionResponseData sm in lineItem.SpecialMentions where sm.NctsExportFromEC select sm))
					{
						sb.Append(Res.GetString("2BB91700-416F-4724-92EB-E95D7CD57E34", "Export from EU subject to restrictions"));
					}
				}
				return sb.ToStringWithDelimiterBetweenAppends("; ");
			}
		}

		public ZString BOX311MARKS
		{
			get { return GetWrappedPackageMarks(); }
		}

		ZString GetWrappedPackageMarks()
		{
			var result = new ZStringBuilder();
			if (lineItem.Packages != null)
			{
				foreach (var pack in lineItem.Packages)
				{
					result.Append(pack.MarksAndNumbers);
				}
			}
			return result.ToStringWithDelimiterBetweenAppends("\r\n");
		}

		public ZString BOX312NUMBERS
		{
			get { return GetWrappedPackageNumbersAndKind(); }
		}

		ZString GetWrappedPackageNumbersAndKind()
		{
			var result = new ZStringBuilder();
			if (lineItem.Packages != null)
			{
				var packageTypeList = NctsPackageLookups.GetPackageUnitTypeList(Factory);
				foreach (var pack in lineItem.Packages)
				{
					result.Append(pack.NumberOfPackages + " - " + packageTypeList.GetDescriptionFromCode(pack.PackageType));
				}
			}
			return result.ToStringWithDelimiterBetweenAppends("\r\n");
		}

		public ZString BOX313CONTAINERS
		{
			get { return GetBox31ContainerNumbers(lineItem.ContainerNumbers); }
		}

		ZString GetBox31ContainerNumbers(List<ZString> containerCollection)
		{
			ZStringBuilder containers = new ZStringBuilder();
			if (containerCollection != null)
			{
				foreach (var container in containerCollection)
				{
					containers.Append(container);
				}
				if (!containers.IsEmpty)
				{
					return containers.ToStringWithDelimiterBetweenAppends(", ");
				}
			}
			return ZString.Empty;
		}

		public ZString BOX315SENSITIVE
		{
			get { return BOX314SENSITIVE; }
		}

		public ZString BOX314SENSITIVE
		{
			get { return GetWrappedSgiCodes(); }
		}

		ZString GetWrappedSgiCodes()
		{
			var result = new ZStringBuilder();
			if (lineItem.SgiCodes != null)
			{
				foreach (SgiCodesResponseData sgi in lineItem.SgiCodes)
				{
					result.Append(sgi.TypeCode);
				}
			}
			return result.ToStringWithDelimiterBetweenAppends("\r\n");
		}

		public ZString BOX315SENSITIVEQTY
		{
			get { return GetWrappedSgiQuantities(); }
		}

		ZString GetWrappedSgiQuantities()
		{
			var result = new ZStringBuilder();
			if (lineItem.SgiCodes != null)
			{
				foreach (SgiCodesResponseData sgi in lineItem.SgiCodes)
				{
					result.Append(sgi.Description);
				}
			}
			return result.ToStringWithDelimiterBetweenAppends("\r\n");
		}

		public ZString BOX314DESCRIPTION
		{
			get { return lineItem.DescriptionOfGoods; }
		}

		public ZString BOX312DESCRIPTION
		{
			get { return BOX314DESCRIPTION; }
		}

		public ZString BOXS28SEALS
		{
			get
			{
				var seals = new List<ZString>();
				if (lineItem.NctsCommonGoodsItem != null)
				{
					foreach (var contNo in lineItem.ContainerNumbers)
					{
						foreach (NonPersistentDepartureContainerPivot cont in lineItem.NctsCommonGoodsItem.ContainersPivots)
						{
							if (cont.ContainerNumber == contNo && cont.ContainerSelected)
							{
								if (!cont.Container.BC_Seal1.IsEmpty)
								{
									seals.Add(cont.Container.BC_Seal1);
								}
								if (!cont.Container.BC_Seal2.IsEmpty)
								{
									seals.Add(cont.Container.BC_Seal2);
								}
							}
						}
					}
				}
				return ConsecutiveSequenceCondenser.Condense(seals);
			}
		}

		public ZString BOX311MARKSANDNUMBERS => BOX311MARKS + " " + BOX312NUMBERS;

		public ZString BOXS29TRANSPORTCHARGESMOP => Tools.ValueOrThreeDashes(lineItem.TransportChargesMoP);

		ZString GetWrappedAddressSummary(AddressResponseData address)
		{
			return address != null && !address.CompanyName.IsEmpty ? ZString.Format("{0}, {1}, {2}, {3}, {4}", address.CompanyName, address.Address1, address.City, address.Postcode, address.CountryCode) : ZString.Empty;
		}

		ZString GetSupportingDocuments()
		{
			var sb = new ZStringBuilder();
			if (lineItem.SpecialMentions != null)
			{
				foreach (var sm in lineItem.SpecialMentions)
				{
					FormatSpecialMentions(sb, sm);
				}
			}

			if (lineItem.SupportingDocuments != null)
			{
				foreach (var sd in lineItem.SupportingDocuments)
				{
					FormatSupportingDocuments(sb, sd);
				}
			}
			return sb.ToStringWithDelimiterBetweenAppends("; ");
		}

		void FormatSupportingDocuments(ZStringBuilder result, SupportingDocumentResponseData sd)
		{
			if (sd != null)
			{
				result.Append(GetSupportingDocumentDescriptionFromCode(sd.TypeCode) + "-" + sd.Reference + "-" + sd.Reason);
			}
		}
		ZString GetSupportingDocumentDescriptionFromCode(ZString documentType)
		{
			var codes = RefCusCodeListTypes.GetCachedList(Factory, DataGroupingCode, Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, ZDateTime.Today);
			var desc = codes.GetDescriptionFromCode(documentType);
			return string.IsNullOrEmpty(desc) ? documentType.ToString() : desc;
		}

		void FormatSpecialMentions(ZStringBuilder result, SpecialMentionResponseData sm)
		{
			if (sm != null)
			{
				result.Append(sm.TypeCode + (sm.Description.IsEmpty ? "" : "-" + sm.Description));
			}
		}

		ZString GetPreviousDocuments()
		{
			ZStringBuilder result = new ZStringBuilder();
			if (lineItem.PreviousDocuments != null)
			{
				foreach (var pd in lineItem.PreviousDocuments)
				{
					result.Append(GetPreviousDocumentDescriptionFromCode(pd.TypeCode) + " - " + pd.Reference + " - " + pd.MoreInfo);
				}
			}
			return result.ToStringWithDelimiterBetweenAppends("; ");
		}

		ZString GetPreviousDocumentDescriptionFromCode(ZString documentType)
		{
			var codes = RefCusCodeListTypes.GetCachedList(Factory, DataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS, ZDateTime.Today);
			var desc = codes.GetDescriptionFromCode(documentType);
			return string.IsNullOrEmpty(desc) ? documentType.ToString() : desc;
		}

		ZString DataGroupingCode => (ediMessage.EM_LinkedObject as NctsHeader)?.DefaultDataGroupingCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		readonly NctsGoodsItemResponseData lineItem;
		readonly NctsEdiMessage ediMessage;
	}
}
