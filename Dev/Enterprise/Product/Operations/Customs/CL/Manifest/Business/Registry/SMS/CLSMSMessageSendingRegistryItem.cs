using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Customs.XtCredential;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class CLSMSMessageSendingRegistryItem : StronglyTypedRegistryItem<CLSMSMessageSending>
	{
		public CLSMSMessageSendingRegistryItem(
			string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			RegistryOptions options,
			CLSMSMessageSending defaultValue
		) : base(new RegistryItemImpl(
			name, category, caption, hint, new CLSMSMessageSendingRegistryDataType(), storage, options, defaultValue
		))
		{
			OnUpdateAction = (companyPk, branchPk, departmentPK, setting) => UpdateAction((CLSMSMessageSending)setting);
			OnAllValuesSavedAction = AllValuesSavedAction;
		}

		public BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory { NameForDebugging = nameof(CLSMSMessageSendingRegistryItem) });
		BusinessObjectFactory factory;

		readonly HashSet<CLSMSMessageSending> settingsToRegister = new HashSet<CLSMSMessageSending>();

		void UpdateAction(CLSMSMessageSending setting)
		{
			if (setting.RequiredXtCredentialAction == XtCredentialAction.None && settingsToRegister.Contains(setting))
			{
				settingsToRegister.Remove(setting);
			}
			else if (!settingsToRegister.Contains(setting))
			{
				settingsToRegister.Add(setting);
			}
		}

		void AllValuesSavedAction()
		{
			foreach (var setting in settingsToRegister.Where(setting => setting.RequiredXtCredentialAction != XtCredentialAction.None).ToArray())
			{
				XtCredentialSender.SendXtCredential(setting);
				setting.XtCredentialRequestSent();
			}
			settingsToRegister.Clear();
		}
	}
}
