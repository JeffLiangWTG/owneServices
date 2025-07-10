using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class InvoiceLineFromOrderLineSynchroniser
	{
		public InvoiceLineFromOrderLineSynchroniser()
		{
			InitialiseUnitTypeList();
		}

		internal void UpdateInvoiceLineFromOrderLine(OrderLine orderLine, BaseJobComInvoiceLine invoiceLine)
		{
			if (SystemDataRegistry.Instance.AutoPopulateInvoicePackagingAndContainerDetailsFromOrderLines.Value)
			{
				UpdatePackagesBox31OnInvoiceLine(orderLine, invoiceLine);
				UpdateVolumeOnInvoiceLine(orderLine, invoiceLine);
				UpdateContainerSelectionOnInvoiceLine(orderLine, invoiceLine);
			}
		}

		void UpdateVolumeOnInvoiceLine(OrderLine orderLine, BaseJobComInvoiceLine invoiceLine)
		{
			invoiceLine.JI_Volume = orderLine.JO_ActualVolume;
			invoiceLine.JI_VolumeUQ = orderLine.JO_UnitOfVolume;
		}

		void UpdatePackagesBox31OnInvoiceLine(OrderLine orderLine, BaseJobComInvoiceLine invoiceLine)
		{
			// On the master bill, add a new package for the order line, and add a pivot for the invoice line, with matchign package counts. 

			var bill = invoiceLine.Declaration.Bills.OfType<Bill>().FirstOrDefault(b => b.IsMasterBill);
			if (bill == null)
			{
				bill = (Bill)invoiceLine.Declaration.Bills.AddNew();
				bill.CU_BillType = BillTypeList.Codes.MasterBill;
				bill.CU_BillNum = (NoResString)"Master Bill";
			}

			var cw = invoiceLine.Declaration.Packages.OfType<BasePackage>().FirstOrDefault(p => p.Bill.PK == bill.PK && p.CW_MarksAndNos == orderLine.OrderAndOrderLineNumber);
			if (cw == null)
			{
				cw = invoiceLine.Declaration.Packages.AddNew();
				cw.CW_HouseBill = bill.CU_BillUniqueCode;
				cw.CW_MarksAndNos = orderLine.OrderAndOrderLineNumber;
			}
			cw.CW_PackType = GetTwoCharacterUnitType(orderLine.JO_OuterPacksUQ).MappedCode;
			cw.CW_PackQty = (ZInt)orderLine.JO_OuterPacks;
			var packPivot = invoiceLine.PackagesPivot.OfType<InvoiceLinePackagePivot>().FirstOrDefault(p => p.CHC_CW == cw.PK);
			if (packPivot == null)
			{
				packPivot = invoiceLine.PackagesPivot.AddNew();
				packPivot.CHC_CW = cw.PK;
				packPivot.CHC_JE = invoiceLine.Declaration.PK;
				packPivot.CHC_JI = invoiceLine.PK;
			}
			packPivot.CHC_NumberOfPacks = (ZInt)orderLine.JO_OuterPacks;
		}

		void UpdateContainerSelectionOnInvoiceLine(OrderLine orderLine, BaseJobComInvoiceLine invoiceLine)
		{
			var containers = ((JobComInvoiceLine)invoiceLine).Declaration.CusContainers; // because if you do Order>Actions>CreateDeclaration, we create containers but caching hides them from us here. 
			containers.Load();

			foreach (CusContainer declarationCusContainer in containers)
			{
				if (orderLine.JO_ContainerNumber == declarationCusContainer.CO_ContainerNumber)
				{
					invoiceLine.ContainersPivot.AddPivotFor(declarationCusContainer);
				}
			}
			invoiceLine.ContainersPivot.RefreshBinding();
		}

		public (ZString MappedCode, bool IsExactMatch) GetTwoCharacterUnitType(ZString unitTypeCode)
		{
			var mappedCode = ZString.Format("PK");
			var isExactMatch = false;

			if (!unitTypeCode.IsEmpty)
			{
				if (unitTypeCode.Length == 3 && unitTypeList.ContainsKey(unitTypeCode))
				{
					mappedCode = unitTypeList[unitTypeCode];
					isExactMatch = true;
				}
				else if (unitTypeCode.Length == 2 && unitTypeList.ContainsValue(unitTypeCode))
				{
					mappedCode = unitTypeCode;
					isExactMatch = true;
				}
			}
			return (mappedCode, isExactMatch);
		}

		void InitialiseUnitTypeList()
		{
			unitTypeList = new Dictionary<ZString, ZString>
			{
				{ "BAG", "BG" },
				{ "BLC", "BN" },
				{ "BND", "BE" },
				{ "BOX", "BX" },
				{ "BSK", "BK" },
				{ "CAS", "CS" },
				{ "CNT", "CN" },
				{ "COI", "CL" },
				{ "CRT", "CR" },
				{ "CTN", "CT" },
				{ "CYL", "CY" },
				{ "DRM", "DR" },
				{ "ENV", "EN" },
				{ "KEG", "KG" },
				{ "PAI", "PL" },
				{ "PLT", "PX" },
				{ "REL", "RL" },
				{ "RLL", "RO" },
				{ "SHT", "ST" },
				{ "SKD", "SI" },
				{ "SPL", "SO" },
				{ "TUB", "TU" },
				{ "PKG", "PK" }
			};
		}

		Dictionary<ZString, ZString> unitTypeList;
	}
}
