namespace Enterprise.Customs.EU.ExitControl.Business;

public class CusExitSealUcc6ValidationDecider : ICusExitSealUcc6ValidationDecider
{
	public bool ValidateBK_UnloadingStateLookups => true;
}
