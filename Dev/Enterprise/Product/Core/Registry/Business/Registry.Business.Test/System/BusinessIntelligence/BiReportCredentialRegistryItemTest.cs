using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BiReportCredentialRegistryItem))]
	sealed class BiReportCredentialRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<BiReportCredential>
	{
		public void TestRegistryOptions()
		{
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue, new BiReportCredentialRegistryItem("DUMMY", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hello World", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue, new BiReportCredential()).Options);
		}

		protected override StronglyTypedRegistryItem<BiReportCredential, BiReportCredential> GetNewRegistryItem()
		{
			return new BiReportCredentialRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue, new BiReportCredential());
		}

		public void TestUpdateRegistry()
		{
			SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestDataWarehouseServer");
			SystemDataRegistry.Instance.BiReportUserCredential.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BiReportCredentialRegistryItem.BiReportCredentialTestValue());

			using (var cmd = TestConnection.Command("SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_Name = @name"))
			{
				cmd.AddParameterBasedOnDbColumn("@name", "BiReportUserCredential", StmDataSchema.SD_Name);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						var bytes = reader[StmDataSchema.Constants.SD_BinaryValue];

						if (bytes == DBNull.Value)
						{
							Fail("Invalid value for BiReportUserCredential registry");
						}
						else
						{
							var currentValue = System.Text.Encoding.ASCII.GetString((byte[])bytes).Replace("\0", "");
							var expectedValue = "<?xml version=\"1.0\" encoding=\"utf-16\"?><BiReportCredential><Domain>TestDomain</Domain><UserName>TestUserName</UserName><Password>grd7Gcp4FhzQKTcvuFVbtRcfn0Gf+UJRYXAfVBobh94=</Password></BiReportCredential>";
							AssertEquals(expectedValue, currentValue);
						}
					}
					else
					{
						Fail("No entry for BiReportUserCredential registry");
					}
				}
			}
		}

		public void TestCannotRemoveThisItemWhenPostrequisiteHasValue()
		{
			SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestDataWarehouseServer");
			SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, BiReportCredentialRegistryItem.BiReportCredentialTestValue());
			SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestAnalysisServer");

			var registryItem = new BiReportCredentialRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue, new BiReportCredential());
			var newCredential = new BiReportCredential();

			AssertExceptionThrown(typeof(RegistryValidationException), "\"Report User Credentials\" cannot be removed when \"Analysis Server\" has a value specified.", () => registryItem.ValidateCore(Guid.Empty, Guid.Empty, Guid.Empty, newCredential));
		}

		public void TestCannotSetThisItemWhenPrerequisiteHasNotBeenSet()
		{
			var registryItem = new BiReportCredentialRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue, new BiReportCredential());
			SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "");

			AssertExceptionThrown(typeof(RegistryValidationException), "\"Report User Credentials\" cannot be set if \"Data Warehouse Server\" hasn't been set.", () => registryItem.ValidateCore(Guid.Empty, Guid.Empty, Guid.Empty, BiReportCredentialRegistryItem.BiReportCredentialTestValue()));
		}

		public override void TestCasting()
		{
			SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestDataWarehouseServer");
			SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			base.TestCasting();
		}
	}
}
