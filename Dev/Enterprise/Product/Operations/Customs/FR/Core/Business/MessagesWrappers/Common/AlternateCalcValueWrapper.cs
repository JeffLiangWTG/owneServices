using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class AlternateCalcValueWrapper : IAlternateCalcValue
	{
		public AlternateCalcValueWrapper(CusEntryLine entryLine)
		{
			this.itemEntryLine = Argument.NotNull(entryLine, "Entry line cannot be null");
			this.calcValue = this.itemEntryLine.RandomLine?.JI_TariffBypassCode ?? ZString.Empty;
			this.motivation = !calcValue.IsEmpty ? this.itemEntryLine.RandomLine?.JI_TariffBypassReason ?? ZString.Empty : ZString.Empty;
		}

		public AlternateCalcValueWrapper(CusEntryHeader entryHeader, EU.Business.ErrorCollector errorCollector)
		{
			this.entryHeader = Argument.NotNull(entryHeader, "Entry header cannot be null");
			this.calcValue = GetSafeBypassCode();
			this.motivation = this.calcValue == FRConstants.ValuationBypassCodes.ReasonRequiredCode ? GetSafeBypassReason() : ZString.Empty;
		}

		ZString GetSafeBypassCode()
		{
			return this.entryHeader.EntryInstruction?.ZG_BypassCode ?? ZString.Empty;
		}

		ZString GetSafeBypassReason()
		{
			return this.entryHeader.EntryInstruction?.ZG_BypassReason ?? ZString.Empty;
		}

		public ZString CalcValue => calcValue;

		public ZString Motivation => motivation;

		protected ZString calcValue;
		protected ZString motivation;

		readonly CusEntryLine itemEntryLine;
		readonly CusEntryHeader entryHeader;
	}
}
