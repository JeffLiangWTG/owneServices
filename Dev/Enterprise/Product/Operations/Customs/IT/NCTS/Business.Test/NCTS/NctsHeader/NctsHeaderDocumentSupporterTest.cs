using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderDocumentSupporter))]
sealed class NctsHeaderDocumentSupporterTest : DocumentSupporterTest
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when nctsHeader parameter is null", () => new NctsHeaderDocumentSupporter(null));
	}

	public void TestGetSupportedDataContexts()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var documentSupporter = (NctsHeaderDocumentSupporter)nctsHeader.DocumentSupporter;
		var expectedSupportedDataContext = new DataContext[] { DataContext.ITTADAttachment };
		CombineAssertions("Expected Supported DataContexts", () =>
		{
			foreach (var dataContext in expectedSupportedDataContext)
			{
				Assert($"{dataContext} should be supported", documentSupporter.IsDataContextSupported(new DataContextValueForTesting(dataContext)));
			}
		});
	}

	public void TestGetDocumentWrappersInternal_ITTADAttachmentDataContext()
	{
		var menuItemForTesting = Factory.New<IStmMenuItem>();
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var documentSupporter = new NctsHeaderDocumentSupporterForTesting(nctsHeader);
		var wrappers = documentSupporter.GetDocumentWrappersInternalExposed(DataContext.ITTADAttachment, menuItemForTesting);
		AssertEquals("When no goods item are found", 0, wrappers.Length);

		var goodsItem1 = nctsHeader.MovementHeader.GoodsItems.AddNew();
		wrappers = documentSupporter.GetDocumentWrappersInternalExposed(DataContext.ITTADAttachment, menuItemForTesting);
		AssertEquals("When one goods item is found but it has no remarks", 0, wrappers.Length);

		goodsItem1.Remarks = "XXYYZ";
		wrappers = documentSupporter.GetDocumentWrappersInternalExposed(DataContext.ITTADAttachment, menuItemForTesting);
		AssertEquals("When one goods item is found and it has remarks", 1, wrappers.Length);

		nctsHeader.MovementHeader.GoodsItems.AddNew();
		wrappers = documentSupporter.GetDocumentWrappersInternalExposed(DataContext.ITTADAttachment, menuItemForTesting);
		AssertEquals("When two goods item are found but only one has remarks", 1, wrappers.Length);
	}

	public void TestGetDocumentWrappersInternal_EuNctsDataContext()
	{
		var menuItemForTesting = Factory.New<IStmMenuItem>();
		var nctsHeader = Factory.NewDepartureNctsHeader();
		NCTSTestHelper.CreateJobDocAddressForTest(Factory, "COS", nctsHeader.SecurityConsignor);

		var documentSupporter = new NctsHeaderDocumentSupporterForTesting(nctsHeader);

		nctsHeader.BH_FTZMove = ZBool.True;
		AssertEquals("IsSecurityDeclaration must be true", true, nctsHeader.IsSecurityDeclaration);
		var wrappers = documentSupporter.GetDocumentWrappersInternalExposed(DataContext.EuNcts, menuItemForTesting);
		AssertNotNull("Returned wrappers", wrappers);
		AssertEquals("Returned wrappers count", 1, wrappers.Length);
		AssertEquals("Returned wrapper type", "Enterprise.Customs.IT.NCTS.Business.SecurityNctsHeaderDocumentWrapper", GetWrapperFullName());

		nctsHeader.BH_FTZMove = ZBool.False;
		wrappers = documentSupporter.GetDocumentWrappersInternalExposed(DataContext.EuNcts, menuItemForTesting);
		AssertNotNull("Returned wrappers", wrappers);
		AssertEquals("Returned wrappers count", 1, wrappers.Length);
		AssertEquals("Returned wrapper type", "Enterprise.Customs.IT.NCTS.Business.NctsHeaderDocumentWrapper", GetWrapperFullName());

		string GetWrapperFullName() => wrappers.Single().GetType().FullName;
	}

	public void TestGetDocumentWrappersInternal_EuNcts5TADDataContext()
	{
		var menuItemForTesting = Factory.New<IStmMenuItem>();
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var documentSupporter = new NctsHeaderDocumentSupporterForTesting(nctsHeader);

		var wrappers = documentSupporter.GetDocumentWrappersInternalExposed(DataContext.EuNcts5TAD, menuItemForTesting);
		AssertNotNull("Returned wrappers", wrappers);
		AssertEquals("Returned wrappers count", 1, wrappers.Length);
		AssertType<Phase5NctsHeaderTADDocumentWrapper>("Returned wrapper type", wrappers.Single());
	}

	protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
	{
		return Factory.NewDepartureNctsHeader();
	}

	class NctsHeaderDocumentSupporterForTesting : NctsHeaderDocumentSupporter
	{
		public NctsHeaderDocumentSupporterForTesting(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public DocumentWrapper[] GetDocumentWrappersInternalExposed(DataContext dataContext, IStmMenuItem commandBeingRun) => GetDocumentWrappersInternal(dataContext, commandBeingRun);
	}
}
