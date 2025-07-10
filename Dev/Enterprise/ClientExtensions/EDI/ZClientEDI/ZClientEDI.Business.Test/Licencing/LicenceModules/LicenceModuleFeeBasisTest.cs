using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	[TestedType(typeof(LicenceModuleFeeBasis))]
	public class LicenceModuleFeeBasisTest : NonPersistentBusinessObjectTestCase
	{
		public void TestModuleCode()
		{
			var updater = new LicenceModuleFeeBasis();
			AssertEquals("default", "", updater.ModuleCode);

			updater.ModuleCode = LegacyLicence.Codes.Core;
			AssertNoNotifications(updater.ModuleCodeInfo);

			updater.ModuleCode = "---";
			AssertHasErrors(updater.ModuleCodeInfo);

			updater.ModuleCode = "";
			AssertHasErrors(updater.ModuleCodeInfo);

			updater.ModuleCode = Env.Licence.RemoteDesktopServices.Name;
			AssertNoNotifications(updater.ModuleCodeInfo);
		}

		public void TestFeeBasis()
		{
			var updater = new LicenceModuleFeeBasis();
			AssertEquals("default", "", updater.FeeBasis);

			updater.FeeBasis = LicenceTypes.Codes.NON;
			AssertNoNotifications(updater.ModuleCodeInfo);

			updater.FeeBasis = "---";
			AssertHasErrors(updater.FeeBasisInfo);

			updater.FeeBasis = "";
			AssertHasErrors(updater.FeeBasisInfo);

			updater.FeeBasis = LicenceTypes.Codes.ODM;
			AssertNoNotifications(updater.FeeBasisInfo);
		}

		public void TestUpdate()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB");
			var lic3 = BillingTestHelper.CreateLicence(Factory, "CCC");
			var lic4 = BillingTestHelper.CreateLicence(Factory, "DDD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			lic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			lic3.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.ConcurrentCountry;
			lic4.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.ConcurrentCountry;

			var updater = new LicenceModuleFeeBasis();
			var moduleCode = Env.Licence.RemoteDesktopServices.Name;
			updater.ModuleCode = moduleCode;
			updater.FeeBasis = LicenceTypes.Codes.ODM;

			var selection = new BusinessObject[] { lic1, lic2, lic3, lic4 };
			foreach (LicenceHeader lic in selection)
			{
				lic.Modules.FindByCode(updater.ModuleCode).LM_LicenceType = LicenceTypes.Codes.NON;
			}

			StringBuilder errors = new StringBuilder();
			int successCount = updater.Update(selection, null, errors);
			AssertEquals("success count", 2, successCount);
			AssertEquals("fee basis updated", LicenceTypes.Codes.ODM, lic1.Modules.FindByCode(moduleCode).LM_LicenceType);
			AssertEquals("fee basis updated", LicenceTypes.Codes.ODM, lic2.Modules.FindByCode(moduleCode).LM_LicenceType);
			AssertEquals("fee basis not updated - invalid", LicenceTypes.Codes.NON, lic3.Modules.FindByCode(moduleCode).LM_LicenceType);
			AssertEquals("fee basis not updated - invalid", LicenceTypes.Codes.NON, lic4.Modules.FindByCode(moduleCode).LM_LicenceType);
			AssertEquals("errors",
				"Fee basis is invalid for " + lic3.LicenceCode + "(Org " + lic3.Company.Header.OH_Code + ")\r\n" +
				"Fee basis is invalid for " + lic4.LicenceCode + "(Org " + lic4.Company.Header.OH_Code + ")\r\n"
				, errors.ToString());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new LicenceModuleFeeBasis();
		}
	}
}
