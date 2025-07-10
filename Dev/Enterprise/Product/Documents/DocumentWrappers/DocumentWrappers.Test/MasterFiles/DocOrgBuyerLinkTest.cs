using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocOrgBuyerLink))]
	public class DocOrgBuyerLinkTest : DocumentWrapperTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetDeliveryInfoSetBusinessObjectToLogAgainstCorrectly()
		{
			var menuItem = Factory.New<StmMenuItem>();
			var pack = new DocumentPack(menuItem);
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = buyer.PK;
			link.OL_OH_Supplier = supplier.PK;

			var excelTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", TestFilesSubFolder.ReportTestFiles);
			var wrapper = DocOrgBuyerLink.New(link, Factory);

			using (var report = new Report(pack, excelTemplate, wrapper, "testReport", null, DocumentDirection.ARV, false))
			{
				pack.Add(report);
				var info = ((IDeliverable)report).GetDeliveryInfo(false);
				AssertEquals(Core.Constants.DocManagerCodes.Organisation, info.RelatedBusinessContext);
				AssertEquals(OrgHeaderSchema.Constants.TableName, info.ParentTableName);
				AssertEquals(supplier.PK, info.ParentGuid);
			}
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocOrgBuyerLink.New(header.BuyerLinks[0], Factory)
			};
		}

		OrgHeader header;
		protected override void SetUp()
		{
			header = Factory.New<OrgHeader>();
			header.BuyerLinks.AddNew();
			base.SetUp();
		}
	}
}
