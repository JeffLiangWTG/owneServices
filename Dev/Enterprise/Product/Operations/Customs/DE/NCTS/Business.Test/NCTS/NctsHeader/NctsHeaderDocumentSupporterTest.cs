using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DE.NCTS.Business.DocumentWrappers;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
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
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var documentSupporter = new NctsHeaderDocumentSupporterForTesting(nctsHeader);
			var wrappers = documentSupporter.GetDocumentWrappersInternalExposed(DataContext.EuNcts, menuItemForTesting);
			nctsHeader.BH_FTZMove = ZBool.False;
			AssertType<NctsHeaderDocumentWrapper>(wrappers.Single());
		}

		public void TestSupportedDataContexts()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var documentSupporter = new NctsHeaderDocumentSupporterForTesting(nctsHeader);
			AssertEquals("EuNcts is Supported", true, documentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.EuNcts)));
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			return nctsHeader;
		}

		class NctsHeaderDocumentSupporterForTesting : NctsHeaderDocumentSupporter
		{
			public NctsHeaderDocumentSupporterForTesting(NctsHeader nctsHeader) : base(nctsHeader)
			{
			}

			public DocumentWrapper[] GetDocumentWrappersInternalExposed(DataContext dataContext, IStmMenuItem commandBeingRun) => GetDocumentWrappersInternal(dataContext, commandBeingRun);
		}
	}
}
