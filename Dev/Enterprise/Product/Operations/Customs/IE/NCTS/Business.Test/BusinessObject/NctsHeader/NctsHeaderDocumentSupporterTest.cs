using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
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
			var nctsHeader = NctsHeaderTest.GetNewBusinessObject(Factory);
			var documentSupporter = new NctsHeaderDocumentSupporterForTesting(nctsHeader);

			using (TemporarilySetNCTSPhase5TransitionPeriod(true))
			{
				var wrappers = documentSupporter.GetDocumentWrappersInternalExposed(DataContext.EuNcts, menuItemForTesting);
				AssertNotNull("Returned wrappers", wrappers);
				AssertEquals("Returned wrappers count", 1, wrappers.Length);
				AssertEquals("Returned wrapper type for Phase 5 transition period", "Enterprise.Customs.IE.NCTS.DocumentWrappers.NctsHeaderDocumentWrapper", wrappers.Single().GetType().FullName);
			}

			using (TemporarilySetNCTSPhase5TransitionPeriod(false))
			{
				var wrappers = documentSupporter.GetDocumentWrappersInternalExposed(DataContext.EuNcts, menuItemForTesting);
				AssertNotNull("Returned wrappers", wrappers);
				AssertEquals("Returned wrappers count", 1, wrappers.Length);
				AssertEquals("Returned wrapper type for Phase 5", "Enterprise.DocumentWrappers.Customs.EU.NCTS.Phase5NctsHeaderDocumentWrapper", wrappers.Single().GetType().FullName);
			}
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return NctsHeaderTest.GetNewBusinessObject(Factory);
		}

		IDisposable TemporarilySetNCTSPhase5TransitionPeriod(bool isInTransitionPeriod) =>
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.NCTSTransitionPeriod,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				ZDate.Today,
				isInTransitionPeriod);

		class NctsHeaderDocumentSupporterForTesting : NctsHeaderDocumentSupporter
		{
			public NctsHeaderDocumentSupporterForTesting(NctsHeader nctsHeader) : base(nctsHeader)
			{
			}

			public DocumentWrapper[] GetDocumentWrappersInternalExposed(DataContext dataContext, IStmMenuItem commandBeingRun) => GetDocumentWrappersInternal(dataContext, commandBeingRun);
		}
	}
}
