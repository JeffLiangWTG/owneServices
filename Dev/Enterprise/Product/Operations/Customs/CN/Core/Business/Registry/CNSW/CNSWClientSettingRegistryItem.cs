using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CN.Business
{
	public class CNSWClientSettingRegistryItem : StronglyTypedRegistryItem<CNSWClientSetting>
	{
		public CNSWClientSettingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CNSWClientSetting defaultValue
		) : base(new CNSWClientSettingRegistryItemImpl(name, category, caption, hint, storage, defaultValue))
		{
			OnUpdateAction = UpdateAction;
			OnAllValuesSavedAction = AllValuesSavedAction;
		}

		readonly Dictionary<(Guid CompanyPk, Guid BranchPk), ZString> settingsToUpdate = new();

		readonly HashSet<(Guid CompanyPk, Guid BranchPk)> registeredSettings = new();

		public BusinessObjectFactory Factory => factory ??= new BusinessObjectFactory { NameForDebugging = "CNCustomsRegistry" };
		BusinessObjectFactory factory;

		protected override object GetValueWithoutFallbackCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var value = (CNSWClientSetting)base.GetValueWithoutFallbackCore(companyPK, branchPK, departmentPK);
			value.RegistryItemInternals = this;
			return value;
		}

		void UpdateAction(Guid companyPk, Guid branchPk, Guid departmentPk, object newValue)
		{
			var setting = (CNSWClientSetting)newValue;
			if (setting.ShouldRegisterEHubClient || setting.ShouldUnregisterEHubClient || registeredSettings.Contains((companyPk, branchPk)))
			{
				UpdateOrAddSetting(companyPk, branchPk, setting.EHubClientID);
			}
		}

		void UpdateOrAddSetting(Guid companyPk, Guid branchPk, string userName)
		{
			if (settingsToUpdate.TryGetValue((companyPk, branchPk), out _))
			{
				settingsToUpdate[(companyPk, branchPk)] = userName;
			}
			else
			{
				settingsToUpdate.Add((companyPk, branchPk), userName);
			}
		}

		void AllValuesSavedAction()
		{
			foreach (var setting in settingsToUpdate)
			{
				if (setting.Value.IsEmpty)
				{
					CNSWClientCredencialSender.SendSettingDelete(Factory, setting.Key.CompanyPk, setting.Key.BranchPk);
					if (registeredSettings.Contains(setting.Key))
					{
						registeredSettings.Remove(setting.Key);
					}
				}
				else
				{
					CNSWClientCredencialSender.SendSettingCreateOrUpdate(Factory, setting.Key.CompanyPk, setting.Key.BranchPk, setting.Value);
					if (!registeredSettings.Contains(setting.Key))
					{
						registeredSettings.Add(setting.Key);
					}
				}
			}
			settingsToUpdate.Clear();
			Factory.Save();
			Factory.CleanUp();
			factory = null;
		}

		protected override void DeleteValueCore(Guid companyPk, Guid branchPk, Guid departmentPk)
		{
			var setting = GetFallBackValueAtAllLevels(companyPk, branchPk, departmentPk);

			if (setting.ShouldUnregisterEHubClient)
			{
				UpdateOrAddSetting(companyPk, branchPk, ZString.Empty);
			}
			base.DeleteValueCore(companyPk, branchPk, departmentPk);
		}
	}
}
