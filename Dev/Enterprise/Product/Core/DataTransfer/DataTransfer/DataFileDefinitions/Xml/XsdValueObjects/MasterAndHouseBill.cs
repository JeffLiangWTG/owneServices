using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	public class MasterAndHouseBill
	{
		public MasterAndHouseBill(ZString masterBill, ZString houseBill)
		{
			this.MasterBill = masterBill;
			this.HouseBill = houseBill;
		}

		public static ZString GetMasterAndHouseBillsAsString(Xsd.MasterAndHouseBill[] masterAndHouseBills)
		{
			ZStringBuilder result = new ZStringBuilder();
			foreach (Xsd.MasterAndHouseBill masterAndHouseBill in masterAndHouseBills)
			{
				if (!result.IsEmpty)
				{
					result.Append(", ");
				}

				result.Append(masterAndHouseBill.MasterBill);
				if (!masterAndHouseBill.HouseBill.IsEmpty && !masterAndHouseBill.HouseBill.IsEmpty)
				{
					result.Append("/");
				}
				result.Append(masterAndHouseBill.HouseBill);
			}
			return result.ToString();
		}

		public readonly ZString MasterBill;
		public readonly ZString HouseBill;
	}
}
