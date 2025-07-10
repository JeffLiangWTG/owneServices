using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRTreatmentSnapshot))]
	sealed class CMRTreatmentSnapshotTest : EnterpriseBusinessObjectTestCase
	{
		/// <summary>
		/// This test is 'poorly' written to confirm the multiple table prefixes continue to work correctly.
		/// </summary>
		public void TestSQLFilterForDuplicateColumn()
		{
			var treatmentCode = CMRTreatmentSnapshot.New(Factory);
			treatmentCode.TE_Code = "XYZ";
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var column = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(CMRTreatmentSnapshotSchema.Constants.TE_Code, CMRTreatmentSnapshotSchema.Constants.TableName);
			AssertEquals(CMRTreatmentSnapshotSchema.TE_Code, column);
			var bizO = factory2.LoadTop1<CMRTreatmentSnapshot>(new ZQuery(column, "XYZ"));
			AssertEquals(bizO.PK, treatmentCode.PK);
		}

		protected override BusinessObject GetNewBusinessObject() => CMRTreatmentSnapshot.New(Factory);
	}
}
