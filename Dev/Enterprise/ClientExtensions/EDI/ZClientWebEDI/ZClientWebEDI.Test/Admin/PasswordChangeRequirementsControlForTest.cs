using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class PasswordChangeRequirementsControlForTest : PasswordChangeRequirementsControl
	{
		public PasswordChangeRequirementsControlForTest()
		{
		}

		public void OnLoad() => base.OnLoad(EventArgs.Empty);

		protected override Dictionary<ContactPasswordValidator.PasswordRequirements, int> GetAllPasswordRequirements => getAllPasswordRequirements ?? base.GetAllPasswordRequirements;
		Dictionary<ContactPasswordValidator.PasswordRequirements, int> getAllPasswordRequirements;

		public void SetPasswordRequirements(Dictionary<ContactPasswordValidator.PasswordRequirements, int> newRequirements)
		{
			getAllPasswordRequirements = newRequirements;
		}
	}
}
