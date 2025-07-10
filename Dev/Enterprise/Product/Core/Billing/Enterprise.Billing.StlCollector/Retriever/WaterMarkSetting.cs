using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Billing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Billing.StlCollector.Retriever
{
	class WaterMarkSetting : IStlItemRegistrySettings
	{
		public WaterMarkSetting(BusinessObjectFactory businessObjectFactory, string settingName)
		{
			this.settingName = settingName;
			this.businessObjectFactory = businessObjectFactory;
		}

		public WaterMarkSetting(StmData settingData)
		{
			this.settingData = settingData;
			try
			{
				currentHighWaterMark = new DateTimeRegistryDataType().Deserialise(settingData.SD_BinaryValue);
			}
			catch (RegistryParsingException)
			{
				currentHighWaterMark = DateTime.MinValue;
			}
		}

		StmData settingData;
		readonly string settingName;
		readonly BusinessObjectFactory businessObjectFactory;
		DateTime currentHighWaterMark = DateTime.MinValue;

		public DateTime HighWaterMark
		{
			get
			{
				return currentHighWaterMark;
			}
			set
			{
				if (currentHighWaterMark == value)
				{
					return;
				}

				if (settingData == null)
				{
					settingData = businessObjectFactory.New<StmData>();
					settingData.SD_Name = settingName;
					settingData.SD_Type = "DT";
					settingData.SD_IsLogged = ZBool.True;
				}
				settingData.SD_BinaryValue = new DateTimeRegistryDataType().Serialise(value);
				currentHighWaterMark = value;
			}
		}

		public bool HasHighWaterMarkBeenSet => settingData != null;

		public void ClearHighWaterMark()
		{
			if (settingData != null)
			{
				settingData.Delete();
				settingData.Factory.Save();
				settingData = null;
				currentHighWaterMark = DateTime.MinValue;
			}
		}
	}
}
