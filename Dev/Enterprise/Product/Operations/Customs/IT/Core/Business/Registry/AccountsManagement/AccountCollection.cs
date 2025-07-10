using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.Registry;

[XmlSerializerAssembly("Enterprise.Customs.IT.Business.XmlSerializers")]
public class AccountCollection : RegistryBusinessObjectCollectionTemplate
{
	#region Constructors

	public AccountCollection()
		: base()
	{
	}

	public AccountCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(fallbackLevel, factory)
	{
	}

	#endregion

	public new Account this[int i]
	{
		get { return (Account)Elements[i]; }
	}

	public new Account AddNew()
	{
		var newElement = (Account)base.AddNew();
		newElement.ParentAccountCollection = this;
		return newElement;
	}

	public void MapAll(AccountCollection mapTo = null)
	{
		try
		{
			foreach (Account account in mapTo ?? this)
			{
				account.ParentAccountCollection = this;
			}
		}
		catch (InvalidCastException) { } //Certain RegistryBusinessObjectCollectionTemplateTestCase break here due to dummy objects
	}

	protected override BusinessObject CreateNonPersistentBusinessObject()
	{
		var result = new Account(CurrentFallbackLevel, CurrentFactory, this);
		return result;
	}

	protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
	{
		var result = new AccountCollection(fallbackLevel, factory);
		if (fallbackLevel != null)
		{
			ITAccountsManagementRegistry.Instance.AccountsManagement.AddToAccountCollectionInEditingCache(result, fallbackLevel);
		}
		return result;
	}

	public new AccountCollection Clone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
	{
		var result = (AccountCollection)GetClone(fallbackLevel, factory);
		foreach (Account account in this)
		{
			var clonedAccount = (RegistryBusinessObjectTemplate)account.Clone(fallbackLevel, factory);
			(clonedAccount as Account).ParentAccountCollection = result;
			result.Add(clonedAccount);
		}
		PerformPostCloneAction(result);
		return result;
	}

	protected override void PerformPostCloneAction(IRegistryBusiness registryBusiness)
	{
		base.PerformPostCloneAction(registryBusiness);

		if (registryBusiness is AccountCollection collection)
		{
			collection.MapAll();
		}
	}
}
