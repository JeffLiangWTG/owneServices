using Enterprise.Customs.EU.NCTS.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public class UnloadingDifferencesTabUserControl : Phase5UnloadingDifferencesTabUserControl
{
	protected override string ChangeAllToDECConfirmationMessage => Res.GetString("AC100B40-9CEE-4176-9741-F3E08FDE7BA8", "Not all entries in 'Unloading Differences' and 'House consignment' tabs have the value DEC.\r\nPress CANCEL if you want to check the unloaded state of the containers, seals, house consignments, goods items and packages.\r\nIf you press OK, all entries with the state blanks, MIS or DIF will be set to DEC. The entries with state NEW will be removed.");
}
