using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class NotClearedByAgentWrapper
	{
		public NotClearedByAgentWrapper(CommonShipment shipmentBO)
		{
			this.ShipmentBO = shipmentBO;
		}
		readonly CommonShipment ShipmentBO;

		public ZString Statement
		{
			get
			{
				if (notClearedByAgentEntryNumber != null)
				{
					return FreightDataRegistry.Instance.NotClearedByAgentStatement.Value;
				}
				return ZString.Empty;
			}
		}

		public ZString Number
		{
			get
			{
				if (notClearedByAgentEntryNumber != null)
				{
					return notClearedByAgentEntryNumber.CE_EntryNum;
				}
				return ZString.Empty;
			}
		}

		public ZDateTime IssueDate
		{
			get
			{
				if (notClearedByAgentEntryNumber != null)
				{
					return notClearedByAgentEntryNumber.CE_IssueDate;
				}
				return ZDateTime.Empty;
			}
		}

		public ZDateTime ExpiryDate
		{
			get
			{
				if (notClearedByAgentEntryNumber != null)
				{
					return notClearedByAgentEntryNumber.CE_ExpiryDate;
				}
				return ZDateTime.Empty;
			}
		}

		CusEntryNumber notClearedByAgentEntryNumber
		{
			get { return cusEntryNumber ?? (cusEntryNumber = GetNotClearedByAgentEntryNumber()); }
		}
		CusEntryNumber cusEntryNumber;

		CusEntryNumber GetNotClearedByAgentEntryNumber()
		{
			CusEntryNumber result = null;
			if (ShipmentBO != null)
			{
				ZString notClearedByAgentType = CusEntryNumberTypes.NotClearedByAgentNumberType(CountryCode);
				if (!notClearedByAgentType.IsEmpty &&
					ShipmentBO.CustomsEntryNumberType == notClearedByAgentType &&
					ShipmentBO.CusEntryNumbers.Count == 1)
				{
					result = ShipmentBO.CusEntryNumbers[0];
				}
			}
			return result;
		}

		ZString CountryCode
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode; }
		}
	}
}
