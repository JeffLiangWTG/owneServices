using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocOrgSupplierLink))]
	public class DocOrgSupplierLinkTest : DocumentWrapperTestCase
	{
		public void TestRoutingOrderOpeningText()
		{
			DocOrgSupplierLink link = DocOrgSupplierLink.New(Header.SupplierLinks[0], Factory);

			Header.SupplierLinks[1].ReplacementAgent = Factory.LoadTop1<OrgHeader>(new ZQuery());
			DocOrgSupplierLink linkWithReplacementAgent = DocOrgSupplierLink.New(Header.SupplierLinks[1], Factory);

			AssertEquals("Opening text is default from registry", Env.Registry.RoutingOrderOpeningText, link.RoutingOrderOpeningText);
			AssertEquals("Opening text is same as routing order text from registry as empty", Env.Registry.RoutingOrderOpeningText, linkWithReplacementAgent.RoutingOrderOpeningText);

			Env.Registry.AgentReplacementRoutingOrderOpeningText = "Hello";
			AssertEquals("Opening text is default from registry", Env.Registry.RoutingOrderOpeningText, link.RoutingOrderOpeningText);
			AssertEquals("Opening text is modified value from registry", Env.Registry.AgentReplacementRoutingOrderOpeningText, linkWithReplacementAgent.RoutingOrderOpeningText);
		}

		public void TestRoutingOrderClosingText()
		{
			DocOrgSupplierLink link = DocOrgSupplierLink.New(Header.SupplierLinks[0], Factory);

			Header.SupplierLinks[1].ReplacementAgent = Factory.LoadTop1<OrgHeader>(new ZQuery());
			DocOrgSupplierLink linkWithReplacementAgent = DocOrgSupplierLink.New(Header.SupplierLinks[1], Factory);

			AssertEquals("Closing text is default from registry", Env.Registry.RoutingOrderClosingText, link.RoutingOrderClosingText);
			AssertEquals("Closing text is same as routing order text from registry as empty", Env.Registry.RoutingOrderClosingText, linkWithReplacementAgent.RoutingOrderClosingText);

			Env.Registry.AgentReplacementRoutingOrderClosingText = "Hello";
			AssertEquals("Closing text is default from registry", Env.Registry.RoutingOrderClosingText, link.RoutingOrderClosingText);
			AssertEquals("Closing text is modified value from registry", Env.Registry.AgentReplacementRoutingOrderClosingText, linkWithReplacementAgent.RoutingOrderClosingText);
		}

		public void TestReplacementAgent()
		{
			var replacementAgent = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Header.SupplierLinks[1].ReplacementAgent = replacementAgent;
			DocOrgSupplierLink linkWithReplacementAgent = DocOrgSupplierLink.New(Header.SupplierLinks[1], Factory);

			AssertEquals("1 published agent", 1, linkWithReplacementAgent.RecommendedAgents.Count);

			DocBaseWrapper intermediateWrapper = (DocBaseWrapper)linkWithReplacementAgent.RecommendedAgents[0].WrappedObject;
			AssertEquals("Correct agent - ie replacement agent", replacementAgent, intermediateWrapper.WrappedObject);
		}

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
			var wrapper = DocOrgSupplierLink.New(link, Factory);

			using (var report = new Report(pack, excelTemplate, wrapper, "testReport", null, DocumentDirection.ARV, false))
			{
				pack.Add(report);
				var info = ((IDeliverable)report).GetDeliveryInfo(false);
				AssertEquals(Core.Constants.DocManagerCodes.Organisation, info.RelatedBusinessContext);
				AssertEquals(OrgHeaderSchema.Constants.TableName, info.ParentTableName);
				AssertEquals(buyer.PK, info.ParentGuid);
			}
		}

		#region Implementation

		OrgHeader Header;

		protected override void SetUp()
		{
			Header = Factory.New<OrgHeader>();
			Header.SupplierLinks.AddNew();
			Header.SupplierLinks.AddNew();

			base.SetUp();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocOrgSupplierLink.New(Header.SupplierLinks[0], Factory)
			};
		}

		#endregion
	}
}
