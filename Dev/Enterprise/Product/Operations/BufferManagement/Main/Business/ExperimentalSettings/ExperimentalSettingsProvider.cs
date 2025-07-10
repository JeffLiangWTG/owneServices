using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.BufferManagement.Business
{
	public class ExperimentalSettingsProvider : NonPersistentBusinessObject
	{
		public const string ExperimentalSettingsName = "ExperimentalSettings";
		readonly BusinessObjectFactory factory;
		readonly ZGuid ownerPK;
		StmData stmData;

		public ExperimentalSettingsProvider(ZGuid ownerPK, BusinessObjectFactory factory)
		{
			this.factory = factory;
			this.ownerPK = ownerPK;

			var filter = new ZQuery(StmDataSchema.SD_Owner, ownerPK);
			filter.AddToFilter(new ZQuery(StmDataSchema.SD_Name, ExperimentalSettingsName));
			stmData = factory.LoadTop1<StmData>(filter);

			ExperimentalSettings = new ExperimentalSettingsCollection();
			if (stmData != null)
			{
				try
				{
					ParseSettings(stmData.SD_BinaryValue);
				}
				catch (JsonReaderException)
				{
				}
			}
		}

		public ExperimentalSettingsProvider(ZGuid ownerPK, byte[] settings)
		{
			this.ownerPK = ownerPK;

			ExperimentalSettings = new ExperimentalSettingsCollection();
			try
			{
				ParseSettings(settings);
			}
			catch (JsonReaderException)
			{
			}
		}

		public ExperimentalSettingsCollection ExperimentalSettings { get; }

		public void SaveSettings()
		{
			if (stmData == null)
			{
				stmData = factory.New<StmData>();
				stmData.SD_Owner = ownerPK;
				stmData.SD_Name = ExperimentalSettingsName;
			}

			string json = ExperimentalSettings.JsonSerialize();
			byte[] utf = Encoding.UTF8.GetBytes(json);
			stmData.SD_BinaryValue = new ZBlob(utf);
			factory.Save();
		}

		void ParseSettings(byte[] settings)
		{
			var json = Encoding.UTF8.GetString(settings);
			var list = json.JsonDeserialize<List<ExperimentalSetting>>();
			ExperimentalSettings.Add(list);
		}

		public ExperimentalSetting GetSetting(string settingKey) => ExperimentalSettings.Cast<ExperimentalSetting>().FirstOrDefault(s => s.Key == settingKey);

		#region HelperMethods

		public const string SimpleCapacityExperimentalSettingsKey = "SimpleCapacity";
		public const string SimpleBoardQueryExperimentalSettingsKey = "SimpleBoardQuery";
		public const string DisableZoneMultipliersExperimentalSettingsKey = "DisableZoneMultipliers";

		public static bool SimpleCapacityQueryEnabled(BMComponent component) => ExperimentalSettingsProvider.GetSetting(component.Factory, component.FC_FS_System.ToGuid(), SimpleCapacityExperimentalSettingsKey, BMSRegistry.Instance.EnableSimpleCapacityCalculationForAllSystems.Value, BMSRegistry.Instance.EnableSimpleCapacityCalculationForAllSystems.Value);

		public static bool SimpleBoardQueryEnabled(BMBoard board) => SimpleBoardQueryEnabled(board.Factory, board.PK);

		public static bool SimpleBoardQueryEnabled(BusinessObjectFactory factory, ZGuid boardPK) => ExperimentalSettingsProvider.GetSetting(factory, boardPK.ToGuid(), SimpleBoardQueryExperimentalSettingsKey);

		public static bool ZoneMultipliersDisabled(BMComponent component) => BMSRegistry.Instance.DisableZoneMultipliersForAllSystems.Value || ExperimentalSettingsProvider.GetSetting(component.Factory, component.FC_FS_System.ToGuid(), DisableZoneMultipliersExperimentalSettingsKey);

		static bool GetSetting(BusinessObjectFactory factory, Guid ownerPK, string settingKey, bool returnValueIfSettingIsEmpty = false, bool returnValueIfPaveExpFeatureDisabled = false)
		{
			if (!BMSRegistry.Instance.EnablePaveExperimentalFeatures.Value)
			{
				return returnValueIfPaveExpFeatureDisabled;
			}

			var setting = new ExperimentalSettingsProvider(ownerPK, factory).GetSetting(settingKey);

			if (string.IsNullOrEmpty(setting?.Value))
			{
				return returnValueIfSettingIsEmpty;
			}

			return setting.Value.EqualsIgnoringCase(true.ToString());
		}

		#endregion
	}
}
