using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.OperationalActions;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing.OperationalActions
{
	[TestedType(typeof(DeclarationUpdatePreviousDocumentsApplicator))]
	class DeclarationUpdatePreviousDocumentsApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestClassCodeList()
		{
			var applicator = new DeclarationUpdatePreviousDocumentsApplicator(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var classCodeList = applicator.ClassCodeList.GetAllCodes();
			var deClassCodeList = new PreviousDocSubTypeList().GetAllCodes();
			var euClassCodeList = new EU.Business.PreviousDocumentClassList().GetAllCodes();
			CombineAssertions("Class code list population", () =>
			{
				Assert("Class code list should partially be populated with previous docs class codes from DE.", classCodeList.Any(x => deClassCodeList.Contains(x)));
				Assert("Class code list should partially be populated with previous docs class codes from EU.", classCodeList.Any(x => euClassCodeList.Contains(x)));
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DeclarationUpdatePreviousDocumentsApplicator(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}
	}
}
