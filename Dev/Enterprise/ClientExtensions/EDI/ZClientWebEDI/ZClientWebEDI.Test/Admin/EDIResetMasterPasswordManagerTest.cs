using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.Login.Testing;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(ResetMasterPasswordManager))]
	public class EDIResetMasterPasswordManagerTest : ResetMasterPasswordManagerTest
	{
	}

	[TestedType(typeof(EDIResetMasterPasswordManager))]
	public class EDIResetMasterPasswordManagerTest1 : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new EDIResetMasterPasswordManager(string.Empty, Factory);
		}
	}
}
