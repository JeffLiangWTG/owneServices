using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderDocumentSupporter))]
public class NctsHeaderDocumentSupporterTest : DocumentSupporterTest
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when nctsHeader parameter is null", () => new NctsHeaderDocumentSupporter(null));
	}

	public void TestGetDocumentWrappersInternal_EuNctsDataContext()
	{
		var menuItemForTesting = Factory.New<IStmMenuItem>();
		var nctsHeader = NctsHeaderTest.GetNewBusinessObject(Factory);

		var documentSupporter = new NctsHeaderDocumentSupporterForTesting(nctsHeader);
		var wrappers = documentSupporter.GetDocumentWrappersInternalExposed(DataContext.EuNcts, menuItemForTesting);
		nctsHeader.BH_FTZMove = ZBool.False;
		AssertNotNull("Returned wrappers", wrappers);
		AssertEquals("Returned wrappers count", 1, wrappers.Length);
		AssertEquals("Returned wrapper type", "Enterprise.Customs.BE.NCTS.DocumentWrappers.NctsHeaderDocumentWrapper", GetWrapperFullName());

		string GetWrapperFullName() => wrappers.Single().GetType().FullName;
	}

	protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
	{
		return NctsHeaderTest.GetNewBusinessObject(Factory);
	}

	class NctsHeaderDocumentSupporterForTesting : NctsHeaderDocumentSupporter
	{
		public NctsHeaderDocumentSupporterForTesting(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public DocumentWrapper[] GetDocumentWrappersInternalExposed(DataContext dataContext, IStmMenuItem commandBeingRun) => GetDocumentWrappersInternal(dataContext, commandBeingRun);
	}
}
