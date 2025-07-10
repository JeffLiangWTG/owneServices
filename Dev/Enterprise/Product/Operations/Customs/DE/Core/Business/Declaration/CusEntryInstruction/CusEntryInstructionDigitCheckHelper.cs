using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public static class CusEntryInstructionDigitCheckHelper
	{
		public static ZBool Style1stDigitIs0(this CusEntryInstruction entryInstruction) => entryInstruction?.CEI_Style.IsDigitCheckSatisfied(0, '0') ?? ZBool.False;

		public static ZBool Style1stDigitIs1(this CusEntryInstruction entryInstruction) => entryInstruction?.CEI_Style.IsDigitCheckSatisfied(0, '1') ?? ZBool.False;

		public static ZBool Style1stDigitIs2(this CusEntryInstruction entryInstruction) => entryInstruction?.CEI_Style.IsDigitCheckSatisfied(0, '2') ?? ZBool.False;

		public static ZBool Style3rdDigitIs1And5thIs0(this CusEntryInstruction entryInstruction) => entryInstruction.Style3rdDigitIs1() && entryInstruction.Style5thDigitIs0();

		public static ZBool Style1stDigitIs1And2ndIs2(this CusEntryInstruction entryInstruction) => Style1stDigitIs1(entryInstruction) && Style2ndDigitIs2(entryInstruction);

		public static ZBool StyleFirstThreeDigitsAre1(this CusEntryInstruction entryInstruction) => entryInstruction?.CEI_Style.StartsWith("111") ?? ZBool.False;

		public static ZBool Style2ndDigitIs0And3rdDigitIs1(this CusEntryInstruction entryInstruction) => Style2ndDigitIs0(entryInstruction) && Style3rdDigitIs1(entryInstruction);

		public static ZBool Style2ndDigitIs0(this CusEntryInstruction entryInstruction) => entryInstruction?.CEI_Style.IsDigitCheckSatisfied(1, '0') ?? ZBool.False;

		public static ZBool Style2ndDigitIs1(this CusEntryInstruction entryInstruction) => entryInstruction?.CEI_Style.IsDigitCheckSatisfied(1, '1') ?? ZBool.False;

		public static ZBool Style2ndDigitIs2(this CusEntryInstruction entryInstruction) => entryInstruction?.CEI_Style.IsDigitCheckSatisfied(1, '2') ?? ZBool.False;

		public static ZBool Style3rdDigitIs1(this CusEntryInstruction entryInstruction) => entryInstruction?.CEI_Style.IsDigitCheckSatisfied(2, '1') ?? ZBool.False;

		public static ZBool Style4thDigitIs0(this CusEntryInstruction entryInstruction) => entryInstruction?.CEI_Style.IsDigitCheckSatisfied(3, '0') ?? ZBool.False;

		public static ZBool Style4thDigitIs1(this CusEntryInstruction entryInstruction) => entryInstruction?.CEI_Style.IsDigitCheckSatisfied(3, '1') ?? ZBool.False;

		public static ZBool Style4thDigitIs2(this CusEntryInstruction entryInstruction) => entryInstruction?.CEI_Style.IsDigitCheckSatisfied(3, '2') ?? ZBool.False;

		public static ZBool Style4thDigitIs3(this CusEntryInstruction entryInstruction) => entryInstruction?.CEI_Style.IsDigitCheckSatisfied(3, '3') ?? ZBool.False;

		public static ZBool Style4thDigitIs4(this CusEntryInstruction entryInstruction) => entryInstruction?.CEI_Style.IsDigitCheckSatisfied(3, '4') ?? ZBool.False;

		public static ZBool Style4thDigitIs9(this CusEntryInstruction entryInstruction) => entryInstruction?.CEI_Style.IsDigitCheckSatisfied(3, '9') ?? ZBool.False;

		public static ZBool Style5thDigitIs0(this CusEntryInstruction entryInstruction) => entryInstruction?.CEI_Style.IsDigitCheckSatisfied(4, '0') ?? ZBool.False;

		public static ZBool Style5thDigitIs1(this CusEntryInstruction entryInstruction) => entryInstruction?.CEI_Style.IsDigitCheckSatisfied(4, '1') ?? ZBool.False;

		public static ZBool IsSDEExport(this CusEntryInstruction entryInstruction) => Style2ndDigitIs0(entryInstruction) && Style3rdDigitIs1(entryInstruction);

		public static ZBool IsSDEOutwardProcessing(this CusEntryInstruction entryInstruction) => Style1stDigitIs1(entryInstruction) && Style2ndDigitIs1(entryInstruction) && Style3rdDigitIs1(entryInstruction);

		public static ZBool IsSDEExportOrSDEOutwardProcessing(this CusEntryInstruction entryInstruction) => IsSDEExport(entryInstruction) || IsSDEOutwardProcessing(entryInstruction);

		public static ZBool IsCCLExport(this CusEntryInstruction entryInstruction) => Style4thDigitIs4(entryInstruction);

		public static ZBool IsOPOOutwardProcessing(this CusEntryInstruction entryInstruction) => Style1stDigitIs1(entryInstruction) && Style2ndDigitIs1(entryInstruction);

		public static ZBool SubStyle1stDigitIs0(this CusEntryInstruction entryInstruction) => entryInstruction?.CEI_SubStyle.IsDigitCheckSatisfied(0, '0') ?? ZBool.False;

		public static ZBool SubStyle1stDigitIs1(this CusEntryInstruction entryInstruction) => entryInstruction?.CEI_SubStyle.IsDigitCheckSatisfied(0, '1') ?? ZBool.False;

		public static ZBool SubStyle1stDigitIs2(this CusEntryInstruction entryInstruction) => entryInstruction?.CEI_SubStyle.IsDigitCheckSatisfied(0, '2') ?? ZBool.False;

		public static ZBool Constellation1stDigitIs0(this CusEntryInstruction entryInstruction) => entryInstruction?.ZG_PartyConstellation.IsDigitCheckSatisfied(0, '0') ?? ZBool.False;

		public static ZBool Constellation1stDigitIs0And2ndIs1And3rdIs0(this CusEntryInstruction entryInstruction) => Constellation1stDigitIs0(entryInstruction) && Constellation2ndDigitIs1And3rdIs0(entryInstruction);

		public static ZBool Constellation1stDigitIs1(this CusEntryInstruction entryInstruction) => entryInstruction?.ZG_PartyConstellation.IsDigitCheckSatisfied(0, '1') ?? ZBool.False;

		public static ZBool Constellation1stDigitIs1And2ndIs1And3rdIs0(this CusEntryInstruction entryInstruction) => Constellation1stDigitIs1(entryInstruction) && Constellation2ndDigitIs1And3rdIs0(entryInstruction);

		public static ZBool Constellation2ndDigitIs0(this CusEntryInstruction entryInstruction) => entryInstruction?.ZG_PartyConstellation.IsDigitCheckSatisfied(1, '0') ?? ZBool.False;

		public static ZBool Constellation2ndDigitIs0And3rdIs0(this CusEntryInstruction entryInstruction) => Constellation2ndDigitIs0(entryInstruction) && Constellation3rdDigitIs0(entryInstruction);

		public static ZBool Constellation2ndDigitIs0And3rdIs1(this CusEntryInstruction entryInstruction) => Constellation2ndDigitIs0(entryInstruction) && Constellation3rdDigitIs1(entryInstruction);

		public static ZBool Constellation2ndDigitIs1(this CusEntryInstruction entryInstruction) => entryInstruction?.ZG_PartyConstellation.IsDigitCheckSatisfied(1, '1') ?? ZBool.False;

		public static ZBool Constellation2ndDigitIs1And3rdIs0(this CusEntryInstruction entryInstruction) => Constellation2ndDigitIs1(entryInstruction) && Constellation3rdDigitIs0(entryInstruction);

		public static ZBool Constellation2ndDigitIs1And3rdIs1(this CusEntryInstruction entryInstruction) => Constellation2ndDigitIs1(entryInstruction) && Constellation3rdDigitIs1(entryInstruction);

		public static ZBool Constellation2ndDigitIs0And4thDigitIs1(this CusEntryInstruction entryInstruction) => Constellation2ndDigitIs0(entryInstruction) && Constellation4thDigitIs1(entryInstruction);

		public static ZBool Constellation2ndDigitIs1And4thDigitIs1(this CusEntryInstruction entryInstruction) => Constellation2ndDigitIs1(entryInstruction) && Constellation4thDigitIs1(entryInstruction);

		public static ZBool Constellation3rdDigitIs0(this CusEntryInstruction entryInstruction) => entryInstruction?.ZG_PartyConstellation.IsDigitCheckSatisfied(2, '0') ?? ZBool.False;

		public static ZBool Constellation3rdDigitIs0And4thIs0(this CusEntryInstruction entryInstruction) => Constellation3rdDigitIs0(entryInstruction) && Constellation4thDigitIs0(entryInstruction);

		public static ZBool Constellation3rdDigitIs1(this CusEntryInstruction entryInstruction) => entryInstruction?.ZG_PartyConstellation.IsDigitCheckSatisfied(2, '1') ?? ZBool.False;

		public static ZBool Constellation3rdDigitIs1And4thIs0(this CusEntryInstruction entryInstruction) => Constellation3rdDigitIs1(entryInstruction) && Constellation4thDigitIs0(entryInstruction);

		public static ZBool Constellation4thDigitIs0(this CusEntryInstruction entryInstruction) => entryInstruction?.ZG_PartyConstellation.IsDigitCheckSatisfied(3, '0') ?? ZBool.False;

		public static ZBool Constellation4thDigitIs1(this CusEntryInstruction entryInstruction) => entryInstruction?.ZG_PartyConstellation.IsDigitCheckSatisfied(3, '1') ?? ZBool.False;

		public static ZBool Constellation4thDigitIs3(this CusEntryInstruction entryInstruction) => entryInstruction?.ZG_PartyConstellation.IsDigitCheckSatisfied(3, '3') ?? ZBool.False;
	}
}
