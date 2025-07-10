using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ExperimentalSettingsProvider))]
	class ExperimentalSettingsProviderTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ExperimentalSettingsProvider(ZGuid.Empty, Factory);
		}

		public void TestReadExperimentalSettingsInvalid()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var stmData = Factory.New<StmData>();
			stmData.SD_Owner = system.PK;
			stmData.SD_Name = ExperimentalSettingsProvider.ExperimentalSettingsName;

			string json = "something went wrong here";
			stmData.SD_BinaryValue = Encoding.UTF8.GetBytes(json);

			Factory.Save();

			var provider = new ExperimentalSettingsProvider(system.PK, Factory);
			AssertEquals(false, provider.ExperimentalSettings.Any());
		}

		public void TestReadExperimentalSettings()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var stmData = Factory.New<StmData>();
			stmData.SD_Owner = system.PK;
			stmData.SD_Name = ExperimentalSettingsProvider.ExperimentalSettingsName;

			string json = @"[
				{""Key"":""Setting1"", ""Value"":""true""},
				{""Key"":""Setting2"", ""Value"":""10""},
				{""Key"":""Setting3"", ""Value"":""2/2/2020 12:00""}]";
			stmData.SD_BinaryValue = Encoding.UTF8.GetBytes(json);

			Factory.Save();

			var provider = new ExperimentalSettingsProvider(system.PK, Factory);
			var expected = new List<ExperimentalSetting> {
				new ExperimentalSetting { Key = "Setting1", Value = "true" },
				new ExperimentalSetting { Key = "Setting2", Value = "10" },
				new ExperimentalSetting { Key = "Setting3", Value = "2/2/2020 12:00" } };

			CombineAssertions(() =>
			{
				for (int i = 0; i < Math.Max(expected.Count, provider.ExperimentalSettings.Count); i++)
				{
					AssertEquals(expected[i].Key, provider.ExperimentalSettings[i].Key);
					AssertEquals(expected[i].Value, provider.ExperimentalSettings[i].Value);
				}
			});
		}

		public void TestWriteExperimentalSettings()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var provider = new ExperimentalSettingsProvider(system.PK, Factory);
			provider.ExperimentalSettings.Add(new ExperimentalSetting { Key = "Setting1", Value = "1000" });
			provider.ExperimentalSettings.Add(new ExperimentalSetting { Key = "Setting2", Value = "false" });
			provider.SaveSettings();

			var stmData = Factory.LoadTop1<StmData>(new ZQuery());
			string expected = @"[{""Key"":""Setting1"",""Value"":""1000""},{""Key"":""Setting2"",""Value"":""false""}]";
			AssertEquals(expected, Encoding.UTF8.GetString(stmData.SD_BinaryValue)
				.Replace(System.Environment.NewLine, string.Empty).Replace(" ", string.Empty));
		}
	}
}
