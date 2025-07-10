using System.Collections.Specialized;
using System.Net;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NT025ResponsePrettyFormatter : BasePassarResponsePrettyFormatter<INT025ResponseDetail>, IMessagePrettyFormatter
{
	public NT025ResponsePrettyFormatter(BusinessObjectFactory factory, INT025ResponseDetail responseDetail) : base(factory, responseDetail)
	{
	}

	public ZString GetFormattedText()
	{
		var htmlBuilder = new ZStringBuilder();

		htmlBuilder
			.Append("<h2>")
			.Append(WebUtility.HtmlEncode(MessageTitle))
			.Append("</h2>");

		var formatTable = new HtmlTableCreator(new NameValueCollection());
		WriteRow(formatTable, MRNLabel, $"{ResponseDetail.MRN}.{ResponseDetail.MRNVersion}");
		WriteRow(formatTable, ReleaseDateLabel, ResponseDetail.ReleaseDate.ToString("dd.MM.yyyy"));
		WriteRow(formatTable, ReleaseIndicatorLabel, GetReleaseIndicatorText(ResponseDetail));
		WriteRow(formatTable, CustomsOfficeLabel, ResponseDetail.CustomsOfficeReferenceNumber);

		if (ResponseDetail.IsPartialRelease)
		{
			foreach (var houseConsignment in ResponseDetail.HouseConsignments)
			{
				WriteRow(formatTable, string.Empty, string.Empty);
				WriteRow(formatTable, HouseConsignmentLabel, $"{houseConsignment.SequenceNumber} - {GetHouseConsignmentReleaseTypeText(houseConsignment)}");

				foreach (var item in houseConsignment.ConsignmentItems)
				{
					WriteRowWithIndenting(formatTable, ConsignmentItemLabel, $"{item.GoodsItemNumber} - {GetConsignmentItemReleaseTypeText(item)}");
				}
			}
		}

		htmlBuilder.Append(formatTable.ToHtml());
		return htmlBuilder.ToString();
	}

	void WriteRow(HtmlTableCreator table, string label, string value)
	{
		if (!string.IsNullOrEmpty(label))
		{
			label += ":";
		}

		table.WriteRow(WebUtility.HtmlEncode(label), WebUtility.HtmlEncode(value));
	}

	void WriteRowWithIndenting(HtmlTableCreator table, string label, string value)
	{
		var cells = new CellWithFormatting[2];

		var leftCell = new CellWithFormatting() { CellValue = $"{label}:" };
		leftCell.HtmlAttributes.Add((NoResString)"style", (NoResString)"text-indent: 50px;");

		cells[0] = leftCell;
		cells[1] = new CellWithFormatting() { CellValue = value };

		table.WriteRowWithFormatting(cells);
	}

	ZString GetReleaseIndicatorText(INT025ResponseDetail responseDetail)
	{
		if (responseDetail.IsFullRelease)
		{
			return FullRelease;
		}
		else if (responseDetail.IsPartialRelease)
		{
			return PartialRelease;
		}
		else
		{
			return ZString.Empty;
		}
	}

	ZString GetHouseConsignmentReleaseTypeText(IHouseConsignment houseConsignment)
	{
		if (houseConsignment.IsFullRelease)
		{
			return FullRelease;
		}
		else if (houseConsignment.IsPartialRelease)
		{
			return PartialRelease;
		}
		else if (houseConsignment.IsBlocked)
		{
			return Blocked;
		}
		else
		{
			return ZString.Empty;
		}
	}

	ZString GetConsignmentItemReleaseTypeText(IConsignmentItem consignmentItem)
	{
		if (consignmentItem.IsReleased)
		{
			return Released;
		}
		else if (consignmentItem.IsBlocked)
		{
			return Blocked;
		}
		else
		{
			return ZString.Empty;
		}
	}

	static string MessageTitle => Res.GetString("2665C745-9242-4A19-91F5-58FA22069919", "Release for further processing");
	static string MRNLabel => Res.GetString("B4ED564D-27E9-4BA1-B973-5F476B01B0B4", "MRN");
	static string ReleaseDateLabel => Res.GetString("84FABA90-CCC7-497A-B20A-AE026311505E", "Release Date");
	static string ReleaseIndicatorLabel => Res.GetString("0478E6ED-CB93-431A-A46C-06D079B171CB", "Release Indicator");
	static string CustomsOfficeLabel => Res.GetString("6C3375B3-02FE-46D3-B6C5-BDECE0FB13B0", "Customs Office");
	static string HouseConsignmentLabel => Res.GetString("20B97F9A-5DE2-49E3-960D-4D9A02FCF70A", "House Consignment");
	static string ConsignmentItemLabel => Res.GetString("3D405131-0198-4460-830D-3EE26DF5E883", "Item");
	static string FullRelease => Res.GetString("CF7C92A6-F439-45FA-9EB5-A5023C0D7443", "Full Release");
	static string PartialRelease => Res.GetString("6A2B3415-B579-4632-98BA-303104B97B7F", "Partial Release");
	static string Released => Res.GetString("F5DB2B27-985D-47C2-894D-0CC0C52574C7", "Released");
	static string Blocked => Res.GetString("7C013593-23BC-4026-B895-458ACF6870BA", "Blocked");
}
