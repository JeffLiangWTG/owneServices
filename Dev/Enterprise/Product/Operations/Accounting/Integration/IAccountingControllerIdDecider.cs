using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Integration
{
	public interface IAccountingControllerIdDecider
	{
		ControllerID GetControllerID(string transactionType, string ledger, ModuleIdentifier callingModuleID);
	}
}
