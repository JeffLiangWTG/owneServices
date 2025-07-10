using CargoWise.EntityFramework;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.GUI.Testing
{
	[TestedType(typeof(FeatureSetLicenceDatabaseModuleButtonGrid))]
	public class FeatureSetLicenceDatabaseModuleButtonGridTest : ZModuleButtonGridTestBase
	{
		public void TestGetFeatureSetUpdateConfirmationMessage()
		{
			var factory = new BusinessObjectFactory();
			var db1 = factory.NewWithValidTestData<LicenceDatabase>();
			db1.LD_ServerCode = "DB1";
			var db2 = factory.NewWithValidTestData<LicenceDatabase>();
			db2.LD_ServerCode = "DB2";

			var featureSet1 = factory.NewWithValidTestData<FeatureControlSet>();
			featureSet1.FCS_ProductName = "Product1";
			db1.LD_FCS_FeatureSet = featureSet1.PK;

			var featureSet2 = factory.NewWithValidTestData<FeatureControlSet>();
			featureSet2.FCS_ProductName = "Product2";

			factory.Save();

			using (var form = new FeatureSetForm(featureSet2))
			using (var grid = new FeatureSetLicenceDatabaseModuleButtonGridForTest())
			{
				form.Controls.Add(grid);
				grid.AttachDbs(new LicenceDatabase[] { db1, db2 });
				var expectedMessage =
@"There are one or more Licence Databases are already linked to another Feature Set. Please confirm to continue linking to this Feature Set.
Database 100: Product1 => Product2
";
				AssertEquals(expectedMessage, grid.GetFeatureSetUpdateConfirmationMessage());
			}
		}

		class FeatureSetLicenceDatabaseModuleButtonGridForTest : FeatureSetLicenceDatabaseModuleButtonGrid
		{
			public FeatureSetLicenceDatabaseModuleButtonGridForTest()
			{
				ModuleID = ClientModuleRegistration.LicenceDatabase;
			}

			public void AttachDbs(LicenceDatabase[] dbList)
			{
				Attach(null, new ModuleButtonGridOnAttachEventArgs(dbList));
			}
		}
	}
}
