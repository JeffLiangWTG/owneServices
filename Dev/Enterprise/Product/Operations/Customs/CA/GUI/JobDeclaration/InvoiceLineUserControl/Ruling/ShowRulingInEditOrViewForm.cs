using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public class ShowRulingInEditOrViewForm : ShowSelectedInEditOrViewForm, IDisposable
	{
		public ShowRulingInEditOrViewForm(IShowEditOrViewForm parent)
			: base(parent) { }

		public void Dispose()
		{
			var subscribers = OnCreatingRuling?.GetInvocationList();
			if (subscribers != null)
			{
				foreach (var subscriber in subscribers)
				{
					var realSubscriber = (EventHandler)subscriber;
					OnCreatingRuling -= realSubscriber;
				}
			}
		}

		public override void ShowEditForm(ZFilterModule module)
		{
			if (string.IsNullOrEmpty(parent.SearchCode))
			{
				if (parent.ShowNewFormWhenEmpty && parent.AllowNewForm && !parent.ReadOnly)
				{
					ShowNewFormForNonExistentCode(module);
				}
			}
			else
			{
				base.ShowEditForm(module);
			}
		}

		protected override void ShowNewFormForNonExistentCode(ZFilterModule module)
		{
			if (module.AllowNew)
			{
				ShowFormForCreatingRuling(module);
			}
			else
			{
				if (!module.DefaultMessageOverridingSecurityRightMessage.IsEmpty)
				{
					Globals.Message.Show(module.DefaultMessageOverridingSecurityRightMessage, Res.GetString("5DFAAB2B-8276-40FA-BC93-0A927143BF87", "Form can not be opened"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else
				{
					module.SecurityCheckpoint.ShowError();
				}
			}
		}

		void ShowFormForCreatingRuling(ZFilterModule module)
		{
			using (var messageBox = new MessageBoxForRuling())
			{
				var result = messageBox.ShowDialog();

				if (result == DialogResult.Yes)
				{
					var form = ((IShowNewForm)module).ShowNewForm();
					var ruling = form?.BusinessEntityForPersistingForm as CACusRuling;

					if (ruling != null)
					{
						ruling.ZZX_RulingNumber = new ZString(parent.SearchCode).SubstringSafe(0, AutoZZRefCusRulingCombined.Schema.ZZX_RulingNumberMaxLength);

						if (messageBox.IsCreateForAllOrgs)
						{
							ruling.ZZX_OA_AppliesTo = ZGuid.Empty;
							ruling.ZZX_OA_AppliesTo_ZAddress.OrgPK = ZGuid.Empty;
						}

						OnCreatingRuling?.Invoke(ruling, EventArgs.Empty);
					}
				}
			}
		}

		public event EventHandler OnCreatingRuling;
	}
}
