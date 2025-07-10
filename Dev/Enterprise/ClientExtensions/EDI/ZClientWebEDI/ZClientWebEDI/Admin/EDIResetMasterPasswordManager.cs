using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class EDIResetMasterPasswordManager : ResetMasterPasswordManager
	{
		public EDIResetMasterPasswordManager(string email, BusinessObjectFactory factory) : base(email, factory)
		{
		}

		public EDIResetMasterPasswordManager(string email, BusinessObjectFactory factory, string orgCodeRestriction) : base(email, factory, orgCodeRestriction)
		{
		}

		public LoginOptionsHelper LoginOptionsHelper
		{
			get
			{
				if (loginOptionsHelper == null)
				{
					loginOptionsHelper = new LoginOptionsHelper(Factory);
				}

				return loginOptionsHelper;
			}
		}

		LoginOptionsHelper loginOptionsHelper;
	}
}

