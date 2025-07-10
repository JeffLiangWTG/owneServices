using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccHotChequeController))]
	public class AccHotChequeControllerBasherTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccHotCheque;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			AccHotCheque cheque = Factory.New<AccHotCheque>();

			Factory.Save();
			return cheque;
		}

		protected override string CountryCode
		{
			get { return "AU"; }
		}
	}
}
