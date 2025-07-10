#region SuppressResourceStringsCheckRegion

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public static class MessageTextInterpretator
	{
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static ZString GetInterpretionForCustomsEntry(CusEntryHeader cusEntryHeader)
		{
			var builder = new InterpretionHtmlBuilder();

			if (cusEntryHeader != null)
			{
				var entryHeaderWrapper = new CusDataHeaderDocumentWrapper(cusEntryHeader);

				var isEntering = cusEntryHeader.IsEntering;
				var ciqRequires = cusEntryHeader.EntryInstruction?.CEI_CIQRequires ?? false;

				var entryHeaderTable = builder.AddTable();
				entryHeaderTable.AddRow(
					new HtmlCell(true, "申报地海关"),
					new HtmlCell(3, entryHeaderWrapper.CustomsOffice.CodeInParenthesesAndDesc),
					new HtmlCell(4));

				entryHeaderTable.AddRow(
					new HtmlCell(true, "统一编号"),
					new HtmlCell(3, $"{entryHeaderWrapper.DeclarationUnifiedNumber}&nbsp;&nbsp;{GetDeclarationUnifiedNumberLink(entryHeaderWrapper.DeclarationUnifiedNumber)}") { EnableHTMLEncoding = false },
					new HtmlCell(true, "预录入编号"),
					new HtmlCell(3, entryHeaderWrapper.PreEntryNumber));

				entryHeaderTable.AddRow(
					new HtmlCell(true, "海关编号"),
					new HtmlCell(3, entryHeaderWrapper.EntryNumber),
					new HtmlCell(true, $"{(isEntering ? "进" : "出")}境关别"),
					new HtmlCell(3, entryHeaderWrapper.OfficeOfEntryOrExit.CodeInParenthesesAndDesc));

				entryHeaderTable.AddRow(
					new HtmlCell(true, "备案号"),
					new HtmlCell(3, entryHeaderWrapper.ManualNo),
					new HtmlCell(true, "合同协议号"),
					new HtmlCell(3, entryHeaderWrapper.ContractNo));

				entryHeaderTable.AddRow(
					new HtmlCell(true, $"{(isEntering ? "进口" : "出口")}日期"),
					new HtmlCell(3, entryHeaderWrapper.ImportOrExportDate.ToString(DateFormat, CultureInfo.InvariantCulture)),
					new HtmlCell(true, "申报日期"),
					new HtmlCell(3, entryHeaderWrapper.DeclarantDate.ToString(DateFormat, CultureInfo.InvariantCulture)));

				entryHeaderTable.AddRow(
					new HtmlCell(true, "境内收发货人"),
					new HtmlCell(entryHeaderWrapper.TradePartyUSCI),
					new HtmlCell(entryHeaderWrapper.TradePartyCCD),
					new HtmlCell(entryHeaderWrapper.TradePartyCIQ),
					new HtmlCell(4, entryHeaderWrapper.TradePartyName));

				entryHeaderTable.AddRow(
					new HtmlCell(true, "境外收发货人"),
					new HtmlCell(3, entryHeaderWrapper.OverseasPartyCode),
					new HtmlCell(4, entryHeaderWrapper.OverseasPartyName));

				entryHeaderTable.AddRow(
					new HtmlCell(true, $"{(isEntering ? "消费使用" : "生产销售")}单位"),
					new HtmlCell(entryHeaderWrapper.CargoOwnerUSCI),
					new HtmlCell(entryHeaderWrapper.CargoOwnerCCD),
					new HtmlCell(entryHeaderWrapper.CargoOwnerCIQ),
					new HtmlCell(4, entryHeaderWrapper.CargoOwnerName));

				entryHeaderTable.AddRow(
					new HtmlCell(true, "申报单位"),
					new HtmlCell(entryHeaderWrapper.DeclarantUSCI),
					new HtmlCell(entryHeaderWrapper.DeclarantCCD),
					new HtmlCell(entryHeaderWrapper.DeclarantCIQ),
					new HtmlCell(4, entryHeaderWrapper.DeclarantName));

				entryHeaderTable.AddRow(
					new HtmlCell(true, "运输方式"),
					new HtmlCell(entryHeaderWrapper.TransportMode.CodeInParenthesesAndDesc),
					new HtmlCell(true, "运输工具名称"),
					new HtmlCell(entryHeaderWrapper.VesselName),
					new HtmlCell(true, "航次号"),
					new HtmlCell(3, entryHeaderWrapper.Voyage));

				entryHeaderTable.AddRow(
					new HtmlCell(true, "提运单号"),
					new HtmlCell(3, entryHeaderWrapper.BillOfLading),
					new HtmlCell(true, "监管方式"),
					new HtmlCell(entryHeaderWrapper.CustomsProcedure.CodeInParenthesesAndDesc),
					new HtmlCell(true, "征免性质"),
					new HtmlCell(entryHeaderWrapper.LevyType.CodeInParenthesesAndDesc));

				entryHeaderTable.AddRow(
					new HtmlCell(true, "许可证号"),
					new HtmlCell(entryHeaderWrapper.LicenseNo),
					new HtmlCell(true, $"{(isEntering ? "启运" : "运抵")}国(地区)"),
					new HtmlCell(entryHeaderWrapper.CountryOfLoadOrDischarge.CodeInParenthesesAndDesc),
					new HtmlCell(true, $"{(isEntering ? "经停" : "指运")}港"),
					new HtmlCell((isEntering ? entryHeaderWrapper.PortOfStopover.CodeInParenthesesAndDesc : entryHeaderWrapper.PortOfOriginOrDest.CodeInParenthesesAndDesc)),
					new HtmlCell(true, "成交方式"),
					new HtmlCell(entryHeaderWrapper.IncoTerm.CodeInParenthesesAndDesc));

				entryHeaderTable.AddRow(
					new HtmlCell(true, "运费"),
					new HtmlCell(entryHeaderWrapper.FreightFee.CurrencyAmountAndMarkDesc),
					new HtmlCell(true, "保险费"),
					new HtmlCell(entryHeaderWrapper.InsuranceFee.CurrencyAmountAndMarkDesc),
					new HtmlCell(true, "杂费"),
					new HtmlCell(entryHeaderWrapper.OtherFee.CurrencyAmountAndMarkDesc),
					new HtmlCell(true, "件数"),
					new HtmlCell(entryHeaderWrapper.NoOfPacks.ToString()));

				entryHeaderTable.AddRow(
					new HtmlCell(true, "包装种类"),
					new HtmlCell(3, $"{entryHeaderWrapper.PackType.Description}/{entryHeaderWrapper.OtherPackages.Cast<CodeAndDescriptionWrapper>().Select(x => x.Description).JoinAsString("/")}"),
					new HtmlCell(true, "毛重(KG)"),
					new HtmlCell(entryHeaderWrapper.GrossWeightInKG.ToString()),
					new HtmlCell(true, "净重(KG)"),
					new HtmlCell(entryHeaderWrapper.NetWeightInKG.ToString()));

				var supportingDocumentsWithoutCOO = entryHeaderWrapper.SupportingDocumentsWithoutCOO.ToList();

				entryHeaderTable.AddRow(
					new HtmlCell(true, "贸易国别(地区)"),
					new HtmlCell(entryHeaderWrapper.CountryOfTrade.CodeInParenthesesAndDesc),
					new HtmlCell(true, "集装箱数"),
					new HtmlCell(entryHeaderWrapper.Containers.Count.ToString()),
					new HtmlCell(true, "随附单证"),
					new HtmlCell(3, supportingDocumentsWithoutCOO.Select(x => x.DocumentType).JoinAsString()));

				if (isEntering)
				{
					entryHeaderTable.AddRow(
						new HtmlCell(true, "入境口岸"),
						new HtmlCell(entryHeaderWrapper.CIQOfficeOfEntryOrExit.CodeInParenthesesAndDesc),
						new HtmlCell(true, "货物存放地点"),
						new HtmlCell(3, entryHeaderWrapper.LocationOfGoods),
						new HtmlCell(true, "启运港"),
						new HtmlCell(entryHeaderWrapper.PortOfOriginOrDest.CodeInParenthesesAndDesc));
				}
				else
				{
					entryHeaderTable.AddRow(
						new HtmlCell(true, "货物存放地点"),
						new HtmlCell(3, entryHeaderWrapper.LocationOfGoods),
						new HtmlCell(true, "离境口岸"),
						new HtmlCell(3, entryHeaderWrapper.CIQOfficeOfEntryOrExit.CodeInParenthesesAndDesc));
				}

				entryHeaderTable.AddRow(
					new HtmlCell(true, "报关单类型"),
					new HtmlCell(entryHeaderWrapper.DocumentSubmissionType.CodeInParenthesesAndDesc),
					new HtmlCell(true, "备注"),
					new HtmlCell(5, entryHeaderWrapper.Remarks));

				entryHeaderTable.AddRow(
					new HtmlCell(2),
					new HtmlCell(true, "标记唛码"),
					new HtmlCell(5, entryHeaderWrapper.MarksAndNumbers));

				entryHeaderTable.AddRow(
					new HtmlCell(true, "特殊关系确认"),
					new HtmlCell(entryHeaderWrapper.SpecialRelationshipConfirm.Description),
					new HtmlCell(true, "价格影响确认"),
					new HtmlCell(entryHeaderWrapper.PriceAffectConfirm.Description),
					new HtmlCell(true, 3, "与货物有关的特许权使用费支付确认"),
					new HtmlCell(entryHeaderWrapper.PaymentOfRoyaltyConfirm.Description));

				entryHeaderTable.AddRow(
					new HtmlCell(true, "担保验放"),
					new HtmlCell(entryHeaderWrapper.IsAssuredInspectClearance.Description),
					new HtmlCell(true, "公式定价确认"),
					new HtmlCell(entryHeaderWrapper.FormulaPricingConfirm.Description),
					new HtmlCell(true, "暂定价格确认"),
					new HtmlCell(entryHeaderWrapper.TemporaryPricingConfirm.Description),
					new HtmlCell(2));

				entryHeaderTable.AddRow(
					new HtmlCell(true, "关联报关单"),
					new HtmlCell(entryHeaderWrapper.RelatedEntryNumber),
					new HtmlCell(true, "关联备案"),
					new HtmlCell(entryHeaderWrapper.RelatedManualNumber),
					new HtmlCell(true, "保税/监管场地"),
					new HtmlCell(entryHeaderWrapper.BondedAreaCode),
					new HtmlCell(true, "场地代码"),
					new HtmlCell(entryHeaderWrapper.FreightYardCode));

				if (ciqRequires)
				{
					var row1 = entryHeaderTable.AddRow(
						new HtmlCell(true, "企业资质"),
						new HtmlCell(isEntering ? 3 : 7, entryHeaderWrapper.EnterpriseQualifications.JoinAsString()));

					if (isEntering)
					{
						row1.AddCells(
							new HtmlCell(true, "启运日期"),
							new HtmlCell(entryHeaderWrapper.DepartureDate.ToString(DateFormat, CultureInfo.InvariantCulture)),
							new HtmlCell(true, "B/L号"),
							new HtmlCell(entryHeaderWrapper.BillNumber));
					}

					var row2 = entryHeaderTable.AddRow(
						new HtmlCell(true, "目的地海关"),
						new HtmlCell(isEntering ? 0 : 3, entryHeaderWrapper.OfficeOfDestination.CodeInParenthesesAndDesc),
						new HtmlCell(true, "关联号码及理由"),
						new HtmlCell(isEntering ? 0 : 3, $"{entryHeaderWrapper.CIQRelatedNumber}/{entryHeaderWrapper.CIQRelatedReason.Description}"));

					if (isEntering)
					{
						row2.AddCells(
							new HtmlCell(true, "使用人"),
							new HtmlCell($"{entryHeaderWrapper.ConsumerContactName}/{entryHeaderWrapper.ConsumerContactPhone}"),
							new HtmlCell(true, "卸毕日期"),
							new HtmlCell(entryHeaderWrapper.DateOfUnloadComplete.ToString(DateFormat, CultureInfo.InvariantCulture)));
					}

					var row3 = entryHeaderTable.AddRow();
					if (isEntering)
					{
						row3.AddCells(
							new HtmlCell(true, "原箱运输"),
							new HtmlCell(entryHeaderWrapper.IsOriginalContainerLoading.Description));
					}

					row3.AddCells(
						new HtmlCell(true, "特殊业务标识"),
						new HtmlCell(isEntering ? 0 : 3, $"{entryHeaderWrapper.SpecialBusinessIdentifiers.Cast<CodeAndDescriptionWrapper>().Select(x => x.Description).JoinAsString()}"),
						new HtmlCell(true, "所需单证"),
						new HtmlCell(3, entryHeaderWrapper.RequiredDocuments.JoinAsString()));

					entryHeaderTable.AddRow(new HtmlCell(8));
				}

				if (entryHeaderWrapper.Containers.Any())
				{
					var containerTable = builder.AddTable();

					containerTable.AddRow(
						new HtmlCell("集装箱号"),
						new HtmlCell("集装箱规格"),
						new HtmlCell("自重(KG)"),
						new HtmlCell("拼箱标识"),
						new HtmlCell("商品项号关系")).IsCaption = true;

					foreach (EntryHeaderContainer cuscontainer in entryHeaderWrapper.Containers)
					{
						containerTable.AddRow(
							new HtmlCell(cuscontainer.ContainerNumber),
							new HtmlCell(cuscontainer.ContainerCodeDescription),
							new HtmlCell(cuscontainer.TareWeightInKG.ToString()),
							new HtmlCell(cuscontainer.IsLessContainerDesc),
							new HtmlCell(cuscontainer.LinkedEntryLineNos.JoinAsString()));
					}

					containerTable.AddRow(new HtmlCell(5));
				}

				if (supportingDocumentsWithoutCOO.Any())
				{
					var documentTable = builder.AddTable();

					documentTable.AddRow(
						new HtmlCell("随附单证代码"),
						new HtmlCell("随附单证编号"),
						new HtmlCell("对应随附单证商品项号")).IsCaption = true;

					foreach (EntryHeaderSupportingDocument document in supportingDocumentsWithoutCOO)
					{
						documentTable.AddRow(
							new HtmlCell(document.DocumentType),
							new HtmlCell(document.DocumentNumber),
							new HtmlCell(document.ItemNumbers.JoinAsString()));
					}

					documentTable.AddRow(new HtmlCell(3));
				}

				var entryLineTable = builder.AddTable();

				foreach (var entryLineWrapper in entryHeaderWrapper.Lines.Cast<CusDataLineDocumentWrapper>().OrderBy(line => line.EntryLineNo))
				{
					entryLineTable.AddRow(
							new HtmlCell(true, "项号") { Bold = true },
							new HtmlCell(entryLineWrapper.EntryLineNo.ToString()),
							new HtmlCell(true, "备案序号"),
							new HtmlCell(entryLineWrapper.ProductManualNo.ToString()),
							new HtmlCell(true, "商品编号"),
							new HtmlCell(entryLineWrapper.Tariff),
							new HtmlCell(true, "检验检疫名称"),
							new HtmlCell(3, $"{entryLineWrapper.CIQSupplementCode} - {entryLineWrapper.CIQTariff.Description}"));

					entryLineTable.AddRow(
							new HtmlCell(true, "商品名称"),
							new HtmlCell(entryLineWrapper.NameOfGoods),
							new HtmlCell(true, "规格型号"),
							new HtmlCell(7, entryLineWrapper.GoodsSpecModel));

					entryLineTable.AddRow(
							new HtmlCell(true, "成交数量"),
							new HtmlCell(entryLineWrapper.TradeQuantity.ToString()),
							new HtmlCell(true, "成交计量单位"),
							new HtmlCell(entryLineWrapper.TradeUnitQty.CodeInParenthesesAndDesc),
							new HtmlCell(true, "单价"),
							new HtmlCell(entryLineWrapper.UnitPrice.ToString()),
							new HtmlCell(true, "总价"),
							new HtmlCell(entryLineWrapper.TotalPrice.ToString()),
							new HtmlCell(true, "币制"),
							new HtmlCell(entryLineWrapper.Currency.CodeInParenthesesAndDesc));

					entryLineTable.AddRow(
							new HtmlCell(true, "法定第一数量"),
							new HtmlCell(entryLineWrapper.CustomsQuantity.ToString()),
							new HtmlCell(true, "法定第一计量单位"),
							new HtmlCell(entryLineWrapper.CustomsUnitQty.CodeInParenthesesAndDesc),
							new HtmlCell(true, "加工成品单耗版本号"),
							new HtmlCell(entryLineWrapper.ProductVersion),
							new HtmlCell(true, "货号"),
							new HtmlCell(entryLineWrapper.ProductCode),
							new HtmlCell(true, "最终目的国(地区)"),
							new HtmlCell(entryLineWrapper.GoodsDest.CodeInParenthesesAndDesc));

					var row4 = entryLineTable.AddRow(
							new HtmlCell(true, "法定第二数量"),
							new HtmlCell((entryLineWrapper.CustomsSecondUnit.Code.IsEmpty ? "" : entryLineWrapper.CustomsSecondQuantity.ToString())),
							new HtmlCell(true, "法定第二计量单位"),
							new HtmlCell(entryLineWrapper.CustomsSecondUnit.CodeInParenthesesAndDesc),
							new HtmlCell(true, "原产国(地区)"),
							new HtmlCell(isEntering ? 3 : 5, entryLineWrapper.GoodsOrigin.CodeInParenthesesAndDesc));

					if (isEntering)
					{
						row4.AddCells(
							new HtmlCell(true, "原产地区"),
							new HtmlCell(entryLineWrapper.OriginState.CodeInParenthesesAndDesc));
					}

					entryLineTable.AddRow(
						new HtmlCell(2),
						new HtmlCell(true, $"境内{(isEntering ? "目的" : "货源")}地"),
						new HtmlCell(2, entryLineWrapper.DomesticDistrict.CodeInParenthesesAndDesc),
						new HtmlCell(3, entryLineWrapper.DomesticRegion.CodeInParenthesesAndDesc),
						new HtmlCell(true, "征免方式"),
						new HtmlCell(entryLineWrapper.DutyMode.CodeInParenthesesAndDesc));

					if (ciqRequires)
					{
						entryLineTable.AddRow(
							new HtmlCell(true, "货物属性"),
							new HtmlCell(3, entryLineWrapper.CargoAttribute.Cast<CodeAndDescriptionWrapper>().Select(x => x.Description).JoinAsString()),
							new HtmlCell(true, "用途"),
							new HtmlCell(entryLineWrapper.EndUse.CodeInParenthesesAndDesc),
							new HtmlCell(true, "产品资质"),
							new HtmlCell(3, entryLineWrapper.ProductQualifications.Cast<EntryLineProductQualification>().JoinAsString()));

						entryLineTable.AddRow(
							new HtmlCell(true, "成分/原料/组分"),
							new HtmlCell(entryLineWrapper.Ingredient),
							new HtmlCell(true, "产品有效期"),
							new HtmlCell(entryLineWrapper.ExpiryDate.ToString(DateFormat, CultureInfo.InvariantCulture)),
							new HtmlCell(true, "产品保质期(天)"),
							new HtmlCell(entryLineWrapper.QGPByDays.ToString()),
							new HtmlCell(true, $"{(isEntering ? "境外" : string.Empty)}生产企业"),
							new HtmlCell(3, (isEntering || entryLineWrapper.ManufacturerCIQ.IsEmpty) ? string.Empty : $"({entryLineWrapper.ManufacturerCIQ}){entryLineWrapper.ManufacturerName}"));

						entryLineTable.AddRow(
							new HtmlCell(true, "货物规格"),
							new HtmlCell(entryLineWrapper.Specification),
							new HtmlCell(true, "货物型号"),
							new HtmlCell(entryLineWrapper.Model),
							new HtmlCell(true, "货物品牌"),
							new HtmlCell(entryLineWrapper.Brand),
							new HtmlCell(true, "生产日期"),
							new HtmlCell(entryLineWrapper.ManufactureDate.ToString(DateFormat, CultureInfo.InvariantCulture)),
							new HtmlCell(true, "生产批次"),
							new HtmlCell(entryLineWrapper.BatchNumber));

						entryLineTable.AddRow(
							new HtmlCell(true, "非危险货物"),
							new HtmlCell(entryLineWrapper.NonDangerousChemical.Description),
							new HtmlCell(true, "UN编码"),
							new HtmlCell(entryLineWrapper.UNDGNumber),
							new HtmlCell(true, "危险类别"),
							new HtmlCell(entryLineWrapper.UNDGClass),
							new HtmlCell(true, "包装类别"),
							new HtmlCell(entryLineWrapper.UNDGPackingGroup),
							new HtmlCell(true, "包装UN标记"),
							new HtmlCell(entryLineWrapper.UNDGPackageType.Code));
					}

					if (!entryLineWrapper.CertOfOriginNumber.IsEmpty)
					{
						entryLineTable.AddRow(
							new HtmlCell(true, "原产地证明编号"), new HtmlCell(entryLineWrapper.CertOfOriginNumber),
							new HtmlCell(true, "优惠贸易协定代码"), new HtmlCell(entryLineWrapper.TradeAgreementCode.CodeInParenthesesAndDesc),
							new HtmlCell(true, "优惠贸易协定项下原产地"), new HtmlCell(entryLineWrapper.CertOfOriginCountry.CodeInParenthesesAndDesc),
							new HtmlCell(true, "原产地证明商品项号"), new HtmlCell(entryLineWrapper.ItemNoOnCertOfOrigin.ToString()),
							new HtmlCell(true, "原产地证明类型"), new HtmlCell(entryLineWrapper.CertOfOriginType.CodeInParenthesesAndDesc)
						);
					}

					entryLineTable.AddRow(new HtmlCell(10));
				}
			}

			return builder.ToHtml();
		}

		static string GetDeclarationUnifiedNumberLink(ZString declarationUnifiedNumber)
		{
			var result = string.Empty;

			if (!declarationUnifiedNumber.IsEmpty)
			{
				result = string.Format(CultureInfo.InvariantCulture, "<a href=\"https://swapp.singlewindow.cn/decserver/entries/ftl/1/0/0/{0}.pdf\" target=\"_blank\">Entry Form(Single Window)</a>", declarationUnifiedNumber);
			}

			return result;
		}

		const string DateFormat = "yyyyMMdd";
	}
}

#endregion
