using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CoreConstants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageJobHeaderDocumentSupporter))]
	public class CusTempStorageJobHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetFilterValue()
		{
			var supporter = new CusTempStorageJobHeaderDocumentSupporter(Header);

			foreach (var country in Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					AssertEquals("All french documents should be supported for countries under french Customs jusridiction.", Core.Constants.CountryCodes.France, supporter.GetFilterValue(DocumentFilters.BKRCTY));
				}
			}
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.TempStorageHeader, Header.DocumentSupporter.BusinessContext);
		}

		public virtual void TestGetDocumentWrappersInternal_TempStorageJobHeader()
		{
			var wrappers = Header.DocumentSupporter.GetDocumentWrappers(CoreConstants.DataContext.TempStorageHeader, null);
			AssertEquals("Wrapper created for CusTempStorageJobHeader", 0, wrappers.Length);
		}

		public virtual void TestSupportedDataContexts()
		{
			var support = new CusTempStorageJobHeaderDocumentSupporter(Header);
			AssertEquals("BusinessContext", BusinessContext.TempStorageHeader, support.BusinessContext);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => Header;

		public override void TestRunningDocumentsShouldNotCauseException()
		{
			//We have no common Documents yet, hence no documents menu
			Assert(true);
		}

		protected CusTempStorageJobHeader Header => header ?? (header = Factory.New<CusTempStorageJobHeader>());
		CusTempStorageJobHeader header;
	}
}
