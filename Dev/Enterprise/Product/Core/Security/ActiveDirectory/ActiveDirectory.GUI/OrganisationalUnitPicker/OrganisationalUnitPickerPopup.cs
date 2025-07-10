using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ActiveDirectory;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Security.ActiveDirectory.GUI
{
	public sealed class OrganisationalUnitPickerPopup : IFindBoxPopup
	{
		public OrganisationalUnitPickerPopup(Func<DomainCredentials> domainCredentialsRetriever)
		{
			DomainCredentialsRetriever = domainCredentialsRetriever;
		}

		readonly Func<DomainCredentials> DomainCredentialsRetriever;

		public void ShowModal(IFindBox findBox, Form parentForm)
		{
			var domainLoginDetailsErrors = GetDomainLoginDetailsErrosIfAny();
			if (!string.IsNullOrEmpty(domainLoginDetailsErrors))
			{
				Globals.Message.ShowWarning(domainLoginDetailsErrors);
				return;
			}

			using (var form = new OrganisationalUnitPickerForm())
			{
				var domainCredentials = DomainCredentialsRetriever();
				var searcher = new DirectorySearcherWrapper(domainCredentials.DomainUserName, domainCredentials.DomainUserPassword, domainCredentials.DomainName);
				form.OUPicker.PopulateTree(searcher, domainCredentials.DomainName);
				form.OUPicker.FindAndSelectOU(findBox.Code);
				if (ZFormModaliser.ShowDialogAndDispose(form) == DialogResult.OK)
				{
					findBox.Code = form.OUPicker.SelectedOU;
				}
			}
		}

		string GetDomainLoginDetailsErrosIfAny()
		{
			var domainCredentials = DomainCredentialsRetriever();
			if (!domainCredentials.Validation.AreDomainLoginDetailsValid())
			{
				var message = new ZStringBuilder(Res.GetString("027e47b4-b2dd-4765-9338-a959e71f5534", "Your domain credentials are not valid:"));
				var errors = new List<string>();
				ProcessErrors(domainCredentials.DomainNameInfo);
				ProcessErrors(domainCredentials.DomainUserNameInfo);
				ProcessErrors(domainCredentials.DomainUserPasswordInfo);
				errors.Distinct().ForEach(error => message.Append(error));
				return message.ToStringWithNewLineBetweenAppends();

				void ProcessErrors(ZPropertyInfo info) => errors.AddRange(info.GetErrors().Select(e => e.Message));
			}
			return string.Empty;
		}

		public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, ZArchitecture.GUI.Internal.EmbeddedModulePopup popup) => SilentSelectResult.None;

		public void SelectRowByPK(ZGuid pk) { }

		public event System.EventHandler Closed { add { } remove { } }

		public void Dispose()
		{
		}
	}
}
