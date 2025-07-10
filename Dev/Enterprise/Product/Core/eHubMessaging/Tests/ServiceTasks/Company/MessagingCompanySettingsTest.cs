using System;
using System.Data;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.eServices;
using NUnit.Framework;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.Company
{
	class MessagingCompanySettingsTest : TransactionedTestCase
	{
		public void TestPassword_ProductRegistration()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.PasswordForTest = "xyzzy";

			var company = GlbCompany.CurrentCompany;
			var companyPK = company.PK.ToGuid();
			var settings = new eHubMessagingCompanySettings(companyPK);

			AssertEquals("xyzzy", settings.GetPassword());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1043:EAdaptorNamingRule", Justification = "SQL Query")]
		public void TestEAdaptorPasswordCache()
		{
			var company = GlbCompany.CurrentCompany;
			var companyPK = company.PK.ToGuid();
			var settings = new eAdaptorMessagingCompanySettings(companyPK);

			eAdaptorRegistry.Instance.eAdaptorOutboundPassword.SetValue(companyPK, Guid.Empty, Guid.Empty, "ONE");
			AssertEquals("Password is taken from registry", "ONE", settings.Password);

			eAdaptorRegistry.Instance.eAdaptorOutboundPassword.SetValue(companyPK, Guid.Empty, Guid.Empty, "TWO");
			AssertEquals("Password is not cached internally", "TWO", settings.Password);

			var query = @"
UPDATE dbo.StmData 
SET SD_Type = 'STR', SD_BinaryValue = 0x54004800520045004500, SD_GuidValue = '00000000-0000-0000-0000-000000000000', SD_IsLogged = 1 
WHERE SD_Name = 'eAdapterOutboundPassword'  AND SD_Owner = @SD_Owner  AND SD_DepartmentGuid is null"; // 0x54004800520045004500 == "THREE"
			using (var command = Db.Connection.Command(query))
			{
				command.AddParameter("@SD_Owner", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}
			AssertEquals("Password registry item uses cache", "TWO", settings.Password);
		}
	}
}
