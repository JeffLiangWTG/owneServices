using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(IntercompanyChargeCodeMappingForm))]
	public class IntercompanyChargeCodeMappingFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new IntercompanyChargeCodeMappingForm(Factory);
		}

		public void TestGlobalChargeCodeIntercompanyCollectionIsLoadingInConstructor()
		{
			var currentCompanyChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK));

			var globalChargeCodeValid = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			var validPivot = globalChargeCodeValid.PivotWithoutOverrideLocalClientCollection.AddNew();
			validPivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			validPivot.YP_AC = currentCompanyChargeCode.PK;

			var globalChargeCodeValid2 = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			var validPivot2 = globalChargeCodeValid2.PivotWithoutOverrideLocalClientCollection.AddNew();
			validPivot2.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			validPivot2.YP_AC = currentCompanyChargeCode.PK;

			Factory.Save();

			using (var form = new IntercompanyChargeCodeMappingForm(Factory))
			{
				var globalChargeCodeCollection = (GlobalChargeCodeMapPivotIntercompanyCollection)form.BusinessEntity;
				AssertEquals(2, globalChargeCodeCollection.Count);
				Assert(globalChargeCodeCollection.Contains(validPivot.PK));
				Assert(globalChargeCodeCollection.Contains(validPivot2.PK));
			}
		}
	}
}
