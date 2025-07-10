using System;
using System.Collections.Generic;
using CargoWise.eHub.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Customs.XtCredential;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.JP.Common
{
	public class MailboxAndRemoteWebPrintClientCredentialsRegistryItem : StronglyTypedRegistryItem<MailboxAndRemoteWebPrintClientCredentials>
	{
		public MailboxAndRemoteWebPrintClientCredentialsRegistryItem(
					string name,
					MultilingualString category,
					MultilingualString caption,
					MultilingualString hint,
					RegistryStorageFlags storage)
						: base(new RegistryItemImpl(
						name, category, caption, hint, new MailboxAndRemoteWebPrintClientCredentialsRegistryDataType(), storage))
		{
			BeforeUpdateAction = BeforeUpdate;
			OnUpdateAction = OnUpdate;
			OnDeleteAction = OnDelete;
			OnAllValuesSavedAction = AllValuesSaved;
		}

		readonly Dictionary<string, MailboxAndRemoteWebPrintClientCredentials> credentials = new ();
		readonly HashSet<MailboxAndRemoteWebPrintClientCredentials> credentialsToRegister = new ();

		void BeforeUpdate(Guid companyPK, Guid branchPK, Guid departmentPK, object value)
		{
			var key = string.Concat(companyPK, branchPK, departmentPK);
			var credential = (MailboxAndRemoteWebPrintClientCredentials)value;

			credentials[key] = credential;
		}

		void OnUpdate(Guid companyPK, Guid branchPK, Guid departmentPK, object value)
		{
			var key = string.Concat(companyPK, branchPK, departmentPK);
			var credential = (MailboxAndRemoteWebPrintClientCredentials)value;

			if (credentials.TryGetValue(key, out var oldCredential) && (oldCredential.LocalComputerAlias != credential.LocalComputerAlias || oldCredential.DomainName != credential.DomainName))
			{
				credential.RequiredAction = XtCredentialAction.Update;
				credential.Password = GetApplicationNodePassword(credential);
				credential.OldPassword = GetApplicationNodePassword(oldCredential);
				credential.SendCredentialFactory = SendCredentialFactory;
				credential.LinkUniqueID = ((IRegistryItemInternals)this).GetRegistryItemPK(companyPK, branchPK, departmentPK);
				credential.Status = XtCredentialStatusList.Codes.Unregistered;
				JPRegistry.Instance.MailboxAndRemoteWebPrintClientCredentials.SetValue(companyPK, branchPK, departmentPK, credential);

				credentialsToRegister.Add(credential);
			}
		}

		void OnDelete(Guid companyPK, Guid branchPK, Guid departmentPK, object value)
		{
			var key = string.Concat(companyPK, branchPK, departmentPK);

			if (credentials.TryGetValue(key, out var oldCredential))
			{
				oldCredential.RequiredAction = XtCredentialAction.Delete;
				oldCredential.Password = string.Empty;
				oldCredential.OldPassword = GetApplicationNodePassword(oldCredential);
				oldCredential.SendCredentialFactory = SendCredentialFactory;
				oldCredential.LinkUniqueID = ((IRegistryItemInternals)this).GetRegistryItemPK(companyPK, branchPK, departmentPK);

				credentials.Remove(key);
				credentialsToRegister.Add(oldCredential);
			}
		}

		void AllValuesSaved()
		{
			try
			{
				foreach (var credential in credentialsToRegister)
				{
					XtCredentialSender.SendXtCredential(credential);
				}

				SendCredentialFactory.Save();
			}
			catch (ZSaveException exception)
			{
				ZExceptionReporting.HandleSaveException(exception);
			}
			finally
			{
				credentials.Clear();
				credentialsToRegister.Clear();
			}
		}

		public static string GetApplicationNodePassword(MailboxAndRemoteWebPrintClientCredentials credential)
		{
			return credential.LocalComputerAlias.IsEmpty && credential.DomainName.IsEmpty ? string.Empty : SHA512Encryptor.Encrypt(credential.LocalComputerAlias + credential.DomainName);
		}

		BusinessObjectFactory SendCredentialFactory => sendCredentialFactory ??= new BusinessObjectFactory() { NameForDebugging = nameof(SendCredentialFactory) };
		BusinessObjectFactory sendCredentialFactory;
	}

	[RegistryEditor("Enterprise.Customs.JP.GUI.MailboxAndRemoteWebPrintClientCredentialsRegistryItemEditor, Enterprise.Customs.JP.GUI")]
	public class MailboxAndRemoteWebPrintClientCredentialsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<MailboxAndRemoteWebPrintClientCredentials>
	{
		public MailboxAndRemoteWebPrintClientCredentialsRegistryDataType()
		{
		}
	}
}
