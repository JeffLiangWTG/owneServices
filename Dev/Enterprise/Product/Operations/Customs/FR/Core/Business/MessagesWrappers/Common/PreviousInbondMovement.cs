using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class PreviousInbondMovement
	{
		public PreviousInbondMovement(CusEntryNumber entryNumber)
		{
			this.entryNumber = entryNumber;
		}

		public CusEntryHeader EntryHeader => entryNumber.Parent as CusEntryHeader;

		public ZString Type
		{
			get
			{
				var typeCode = "";
				if (EntryHeader.Declaration.DeltaMode == OrgCusAccountDeltaGTypeList.Codes.G1)
				{
					typeCode = DeltaGFallbackStatusList.IsStatusInFallbackAndNotRegularised(entryNumber.CE_EntryStatus) ? PreviousInbondMovementTypeList.Codes.TypeDecECO_2 : PreviousInbondMovementTypeList.Codes.TypeDecECO_1;
				}
				else if (EntryHeader.Declaration.DeltaMode == OrgCusAccountDeltaGTypeList.Codes.G2)
				{
					typeCode = DeltaGFallbackStatusList.IsStatusInFallbackAndNotRegularised(entryNumber.CE_EntryStatus) ? PreviousInbondMovementTypeList.Codes.TypeDecECO_5 : PreviousInbondMovementTypeList.Codes.TypeDecECO_4;
				}
				return typeCode;
			}
		}

		public ZString Number => entryNumber.CE_EntryNum;

		readonly CusEntryNumber entryNumber;
	}
}
