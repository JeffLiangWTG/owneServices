using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.Customs.IT.Business.ResString;

namespace Enterprise.Customs.IT.Registry;

public sealed class ITAccountsManagementRegistry : RegistryItemSet
{
	ITAccountsManagementRegistry()
	{
	}

	public static ITAccountsManagementRegistry Instance
	{
		get { return instance ?? (instance = new ITAccountsManagementRegistry()); }
	}

	[ThreadStatic]
	static ITAccountsManagementRegistry instance;

	BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory { NameForDebugging = "ITAccountsManagementRegistry" });
	BusinessObjectFactory factory;

	public override bool IsForProductivityWise => false;

	public AccountCollectionRegistryItem AccountsManagement
	{
		get
		{
			return GetItem(ITAccountsManagementHelper.ITAccountsManagementRegistryItem, delegate
			{
				return new AccountCollectionRegistryItem(
					ITAccountsManagementHelper.ITAccountsManagementRegistryItem,
					CustomsDataRegistry.Categories.Customs_Italy,
					ResString.GetMultilingualString("4F44ED62-F2E0-4903-8AC8-A3E6A265181D", "Accounts Management"),
					ResString.GetMultilingualString("730F00D4-0DCF-48F7-BD86-213073915F0F", "Capture Italian Customs Accounts and Nodes. These credentials are used by the system in order to communicate with Italian Customs FTP web service."),
					RegistryStorageFlags.Company,
					new AccountCollection())
				{
					OnUpdateAction = OnUpdateAction,
					OnAllValuesSavedAction = OnAllValuesSavedAction
				};
			});
		}
	}

	void OnUpdateAction(Guid companyPK, Guid branchPK, Guid departmentPK, object newValue)
	{
		var fallbackLevel = new FallbackLevel(companyPK, branchPK, departmentPK);
		AccountsManagement.AddToAccountCollectionInEditingCache((AccountCollection)newValue, fallbackLevel);
	}

	void OnAllValuesSavedAction()
	{
		SendCredentials();
		Factory.Save();
		Factory.CleanUp();
		factory = null;
		AccountsManagement.ResetAccountCollectionsGroupedByCompany();
	}

	void SendCredentials()
	{
		var parentCredential = new CredentialSender(ITAccountsManagementHelper.ConfigurationName);
		foreach (Account systemAccount in AccountsManagement.AccountCollectionsGroupedByCompany.SelectMany(x => x.Accounts))
		{
			var group = CredentialSender.CreateGroup(ITAccountsManagementHelper.MailboxID, systemAccount.AccountNumber, systemAccount.AccountStatus);
			var groupItems = new List<IValueObject>()
			{
				CredentialSender.CreateItem(ITAccountsManagementHelper.Node, systemAccount.AccountNode)
			};

			if (systemAccount.EmcsNotificationEnabled)
			{
				foreach (ExciseNumber exciseNumber in systemAccount.ExciseNumbers)
				{
					groupItems.Add(CredentialSender.CreateItem(ITAccountsManagementHelper.ExciseNumber, exciseNumber.Number));
				}
			}

			groupItems.Add(GetCredentialGroup(systemAccount));
			groupItems.Add(GetCertificateGroup(systemAccount));

			group.Items = groupItems.ToArray();
			parentCredential.AddItems(group);
		}
		parentCredential.SendCredential(Factory);
	}

	IValueObject GetCredentialGroup(Account systemAccount) => CredentialSender.CreateCredential(ITAccountsManagementHelper.MailboxID, ZString.Empty, systemAccount.AccountPassword);

	IValueObject GetCertificateGroup(Account systemAccount)
	{
		var accountCertificateStatus = systemAccount.AccountCertificateStatus == AccountCertificateStatusList.Codes.Expired ? AccountStatusList.Codes.Invalid : systemAccount.AccountCertificateStatus.ToString();
		var certificateGroup = CredentialSender.CreateGroup(ITAccountsManagementHelper.XMLWebService, ZString.Empty, accountCertificateStatus);
		var certificate = CredentialSender.CreateCertificate(systemAccount.AccountCertificate, systemAccount.AccountCertificatePassword);

		certificateGroup.Items = new object[] { certificate };
		return certificateGroup;
	}

	static class ITAccountsManagementHelper
	{
		public const string ConfigurationName = "ITCustomsCredentials";
		public const string MailboxID = "MailboxID";
		public const string XMLWebService = "XMLWebService";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant Node")]
		public const string Node = "Node";
		public const string ITAccountsManagementRegistryItem = "ITAccountsManagementRegistryItem";
		public const string ExciseNumber = "ExciseNumber";
	}

	public ZString GetOtherCompanyNameMatchingCondition(Func<Account, bool> condition, ZGuid companyPk, BusinessObjectFactory factory)
	{
		Argument.NotNull(condition, nameof(condition));
		Argument.NotNull(factory, nameof(factory));

		var otherCompanyPk = AccountsManagement.AccountCollectionsGroupedByCompany
				.FirstOrDefault(x =>
				{
					var isAnotherCompany = x.CompanyPK != companyPk;
					return isAnotherCompany && x.Accounts.Cast<Account>().Any(y => condition(y));
				})
				?.CompanyPK ?? ZGuid.Empty;

		return !otherCompanyPk.IsEmpty
			? factory.Load<GlbCompany>(otherCompanyPk)?.GC_Name ?? ZString.Empty
			: ZString.Empty;
	}
}
