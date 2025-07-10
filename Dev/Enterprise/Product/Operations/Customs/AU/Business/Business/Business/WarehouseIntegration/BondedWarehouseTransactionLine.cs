using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class BondedWarehouseTransactionLine : Customs.Business.BondedWarehouseTransactionLine
	{
		public BondedWarehouseTransactionLine(JobComInvoiceLine line)
			: base(line)
		{
		}

		public BondedWarehouseTransactionLine(CusEntryLine line)
			: base(line)
		{
		}

		public new CusEntryLine EntryLine
		{
			get { return (CusEntryLine)base.EntryLine; }
		}

		public new JobComInvoiceLine InvoiceLine
		{
			get { return (JobComInvoiceLine)base.InvoiceLine; }
		}

		protected override Money GetTILVCore()
		{
			return InvoiceLine.TransportAndInsurance;
		}

		protected override ZString AddInfoString
		{
			get
			{
				AUAddInfo addInfo = InvoiceLine.AddInfo.Clone();
				addInfo.FillEmptyPropertiesFrom(InvoiceLine.InvoiceHeader.AddInfo);
				if (addInfo.ZA_HeaderREL_Hidden == "Y" && (addInfo.ZA_REL_Hidden.IsEmpty || addInfo.ZA_REL_Hidden == CMRRelatedTransaction.Default.Code))
				{
					addInfo.ZA_REL_Hidden = CMRRelatedTransaction.Yes.Code;
				}
				return addInfo.ToString();
			}
		}

		protected override ZString GetEntryKeyFromInvoiceLine()
		{
			return InvoiceLine.AddInfo.ZA_WRN;
		}

		protected override ZShort GetEntryLineNumberFromInvoiceLine()
		{
			return (ZShort)InvoiceLine.AddInfo.ZA_WRL;
		}

		protected override ZDateTime GetEntryDateForWEA()
		{
			return InvoiceLine.Declaration.JE_DateOfFirstArrival;
		}

		protected override RefCountry CountryOfOriginCore
			=> base.CountryOfOriginCore
					?? InvoiceLine.InvoiceHeader.AddInfo.CountryOfOrigin;

		protected override OrgAddress GetWarehouseCore()
			=> InvoiceLine.AddInfo.WarehouseAddress
				?? base.GetWarehouseCore();

		protected override ZDecimal GetCustomsQuantityCore()
		{
			if (HasWRQOverride)
			{
				return InvoiceLine.AddInfo.ZA_WRQ;
			}
			else
			{
				return base.GetCustomsQuantityCore();
			}
		}

		protected override ZString GetCustomsQuantityUnitCore()
		{
			if (HasWRQOverride)
			{
				return InvoiceLine.AddInfo.ZA_WRU;
			}
			else
			{
				return base.GetCustomsQuantityUnitCore();
			}
		}

		bool HasWRQOverride
		{
			get { return !InvoiceLine.AddInfo.ZA_WRU.IsEmpty && InvoiceLine.AddInfo.ZA_WRQ > 0; }
		}
	}
}
