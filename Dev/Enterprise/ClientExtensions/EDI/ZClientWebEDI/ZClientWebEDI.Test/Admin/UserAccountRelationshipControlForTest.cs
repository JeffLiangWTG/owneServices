using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class UserAccountRelationshipControlForTest : UserAccountRelationshipControl
	{
		public UserAccountRelationshipControlForTest()
		{
			InitialiseControls();
		}

		public void InitialiseControls()
		{
			ContactUserAccountGroupingRepeater = new ZRepeater();
			ConfirmationDiv = new Panel();
			ConfirmationYes = new ZButton();
			ConfirmationNo = new ZButton();
		}

		public void BindRepeater(List<DeactivationWrapperBase> contactUserAccountWrappers)
		{
			ContactUserAccountGroupingRepeater.Bind(contactUserAccountWrappers);
			var iterator = 0;
			foreach (var item in ContactUserAccountGroupingRepeater.Items.Cast<RepeaterItem>())
			{
				var orgLabel = new ZTextLabel(contactUserAccountWrappers[iterator].Organisation);
				orgLabel.ID = "Organisation";
				var licenceTypeLabel = new ZTextLabel(contactUserAccountWrappers[iterator].LicenceType);
				licenceTypeLabel.ID = "LicenceType";
				var checkBox = new ZCheckBox { ID = "IsContactRelationshipActive" };
				if (contactUserAccountWrappers[iterator] is UserAccountDeactivationWrapper uAWrapper)
				{
					checkBox.Checked = uAWrapper.UserAccount.EUA_IsContactRelationshipActive;
				}

				var referenceNumber = new ZNumericLabel { Text = contactUserAccountWrappers[iterator].ReferenceNumber.ToString(), ID = "ReferenceNumber" };
				item.Controls.Add(orgLabel);
				item.Controls.Add(checkBox);
				item.Controls.Add(licenceTypeLabel);
				item.Controls.Add(referenceNumber);
				iterator++;
			}
		}

		public void OnLoad() => base.OnLoad(EventArgs.Empty);
		public void SaveChangesButton_ClickExposed() => SaveChangesButton_Click(null, EventArgs.Empty);
		public void ConfirmationYes_ClickExposed() => DeactivateLoginContactConfirmationYes_Click(null, EventArgs.Empty);
		public void ConfirmationNo_ClickExposed() => DeactivateLoginContactConfirmationNo_Click(null, EventArgs.Empty);
		public ZRepeater ContactUserAccountGroupingRepeaterExposed => ContactUserAccountGroupingRepeater;
		public Panel ConfirmationDivExposed => ConfirmationDiv;
	}
}