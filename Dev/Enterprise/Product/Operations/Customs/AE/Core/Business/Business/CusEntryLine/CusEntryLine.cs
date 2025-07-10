using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Business;

public class CusEntryLine : TypeSafeCusEntryLine, Integration.Customs.AE.ICusEntryLine
{
	public CusEntryLine(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public ZString ExporterCode
	{
		get
		{
			OrgHeader exporter = Supplier();
			if (exporter != null)
			{
				return exporter.LocalCustomsClientCode;
			}
			return "";
		}
	}

	public ZString InvoiceCurrencyCode
	{
		get { return InvoiceCurrency != null ? InvoiceCurrency.RX_Code : ZString.Empty; }
	}

	public ZString OverseasFreightCurrencyCode
	{
		get
		{
			Money result = this.OverseasFreight;
			return result.Currency == null ? "" : result.Currency.Code;
		}
	}

	public ZString ExporterName
	{
		get
		{
			OrgHeader exporter = Supplier();
			if (exporter != null)
			{
				return exporter.OH_FullNameTruncated;
			}
			return "";
		}
	}

	public ZString ExporterAddress
	{
		get
		{
			OrgHeader exporter = Supplier();
			if (exporter != null)
			{
				return exporter.Addresses.MainAddress.AddressAsASingleLineWithoutCompanyName.Trim();
			}
			return "";
		}
	}

	public ZString ImporterCode
	{
		get
		{
			OrgHeader importer = this.Importer();
			if (importer != null)
			{
				return importer.LocalCustomsClientCode;
			}
			return "";
		}
	}

	public ZString ImporterName
	{
		get
		{
			OrgHeader importer = this.Importer();
			if (importer != null)
			{
				return importer.OH_FullNameTruncated;
			}
			return "";
		}
	}

	public ZString ImporterAddress
	{
		get
		{
			OrgHeader importer = this.Importer();
			if (importer != null)
			{
				return importer.Addresses.MainAddress.AddressAsASingleLineWithoutCompanyName.Trim();
			}
			return "";
		}
	}

	OrgHeader Importer()
	{
		if (Header.RandomHeader.Buyer != null)
		{
			return Header.RandomHeader.Buyer;
		}
		if (Declaration != null && Declaration.Importer != null)
		{
			return Declaration.Importer;
		}
		return null;
	}

	OrgHeader Supplier()
	{
		if (Header.RandomHeader.Supplier != null)
		{
			return Header.RandomHeader.Supplier;
		}
		if (Declaration != null && Declaration.Consignor != null)
		{
			return this.Declaration.Consignor;
		}
		return null;
	}

	protected override System.Type GetCusEntryHeaderType() => typeof(CusEntryHeader);

	protected override ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> GetCusEntryLineFeeCollection()
	{
		return new CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>(this, Factory);
	}

	protected override ZDecimal GetDutyAmountCore()
	{
		return Fees.GetAmount(FeeTypeList.Codes.A00);
	}

	protected override ZDecimal GetGSTVATAmountCore()
	{
		return Fees.GetAmount(FeeTypeList.Codes.B00);
	}
}
