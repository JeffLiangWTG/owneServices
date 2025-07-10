using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

/*

NOTEZ-BIEN:
	The base Customs.Business.ClassificationDataLoad<CusClassification> class is fully tested by US.Biz. 
	No need to reinvent the wheel here. 

*/

namespace Enterprise.Customs.EU.Business.Classification.Testing
{
	[TestedType(typeof(ClassificationDataLoad))]
	public class ClassificationDataLoadTest : DataLoadTestCase<ClassificationDataLoad>
	{
		[ExpectNoExceptions]
		public void TestCountrySpecificColumns()
		{
			string[] cols = new string[] { ClassificationDataLoad.Column_CPC, ClassificationDataLoad.Column_EcSupplement1, ClassificationDataLoad.Column_EcSupplement2 };
			NUnit.Framework.Assert.That(Runner.ExtraColumns, NUnit.Framework.Is.EquivalentTo(cols));
		}

		[ExpectNoExceptions]
		public void TestImportCsvClassificationsWithEuData()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("CODE,TYPE,DESCRIPTION,TARIFF,CPC,ECSUPPLEMENT1,ECSUPPLEMENT2");
					sw.WriteLine("DAN,BTH,Daniel,1234567890,4000000");
					sw.WriteLine("AGI,BTH,Agnes,2345678901,6900000");
					sw.WriteLine("Agnes,IMP,Pusszi,2345678901,6900000");
					sw.WriteLine("NoCpc,BTH,Missing CPC,2345678901");
					sw.WriteLine("WithSup1&2,BTH,Supplements1&2,2345678901,6900000,1111,2222");
				}

				Runner.ImportData(testFileName.Filename, "row");
				ZQuery checkFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, SQLComparisonOperator.GreaterThan, "");
				checkFilter.OrderBy = CusClassificationSchema.CC_Description.Name;
				CusClassification[] addedRows = Factory.Load<CusClassification>(checkFilter);
				NUnit.Framework.Assert.That(addedRows.Length, NUnit.Framework.Is.EqualTo(4), "There should have been 4 Classification records created. The IMP row is ignored as we support only BTH.");

				NUnit.Framework.Assert.That(addedRows[2].CC_Description, NUnit.Framework.Is.EqualTo("Missing CPC").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(addedRows[2].CC_ProcedureCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Classification loader should store blank CPC");
				NUnit.Framework.Assert.That(addedRows[1].CC_Description, NUnit.Framework.Is.EqualTo("Daniel").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(addedRows[1].CC_ProcedureCode, NUnit.Framework.Is.EqualTo("4000000").Using(CustomComparers.TypeComparison), "Classification loader should store the EU-specific CPC code");
				NUnit.Framework.Assert.That(addedRows[0].CC_Description, NUnit.Framework.Is.EqualTo("Agnes").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(addedRows[0].CC_ProcedureCode, NUnit.Framework.Is.EqualTo("6900000").Using(CustomComparers.TypeComparison), "Classification loader should store the EU-specific CPC code");
				NUnit.Framework.Assert.That(addedRows[3].CC_EcSupplement1, NUnit.Framework.Is.EqualTo("1111").Using(CustomComparers.TypeComparison), "Classification loader should store the EU-specific Supplement 1 code");
				NUnit.Framework.Assert.That(addedRows[3].CC_EcSupplement2, NUnit.Framework.Is.EqualTo("2222").Using(CustomComparers.TypeComparison), "Classification loader should store the EU-specific Supplement 2 code");
				// Validation of BTH:
				NUnit.Framework.Assert.That(Runner.Log[1], NUnit.Framework.Is.EqualTo("Row 4: Only classification type 'BTH' is supported. Change the row to reflect this."));
			}
		}

		#region Implementation

		protected override ClassificationDataLoad GetNewDataLoader()
		{
			return new ClassificationDataLoad();
		}

		ClassificationDataLoadForTest runner;
		ClassificationDataLoadForTest Runner
		{
			get
			{
				if (runner == null)
				{
					runner = new ClassificationDataLoadForTest();
				}
				return runner;
			}
		}

		#endregion
	}

	class ClassificationDataLoadForTest : ClassificationDataLoad
	{
		public IEnumerable<string> ExtraColumns
		{
			get { return this.GetCountrySpecificFieldNames(); }
		}
	}
}
