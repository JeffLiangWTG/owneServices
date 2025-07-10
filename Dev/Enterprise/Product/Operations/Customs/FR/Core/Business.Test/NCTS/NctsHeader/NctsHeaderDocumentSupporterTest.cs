using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(NctsHeaderDocumentSupporter))]
	class NctsHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetDocumentWrappersInternal_EuNctsNctsDataContext()
		{
			var menuItemForTesting = Factory.New<IStmMenuItem>();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "COS", nctsHeader.SecurityConsignor);

			var documentSupporter = new NctsHeaderDocumentSupporterForTesting(nctsHeader);

			nctsHeader.BH_FTZMove = ZBool.True;
			AssertEquals("IsSecurityDeclaration must be true", true, nctsHeader.IsSecurityDeclaration);
			var wrappers = documentSupporter.GetDocumentWrappersInternalExposed(DataContext.EuNcts, menuItemForTesting);
			AssertNotNull("Returned wrappers", wrappers);
			AssertEquals("Returned wrappers count", 1, wrappers.Length);
			AssertEquals("Returned wrapper type", "Enterprise.Customs.FR.DocumentWrappers.NCTS.SecurityNctsHeaderDocumentWrapper", GetWrapperFullName());

			nctsHeader.BH_FTZMove = ZBool.False;
			wrappers = documentSupporter.GetDocumentWrappersInternalExposed(DataContext.EuNcts, menuItemForTesting);
			AssertNotNull("Returned wrappers", wrappers);
			AssertEquals("Returned wrappers count", 1, wrappers.Length);
			AssertEquals("Returned wrapper type", "Enterprise.Customs.FR.DocumentWrappers.NCTS.NctsHeaderDocumentWrapper", GetWrapperFullName());

			string GetWrapperFullName() => wrappers.Single().GetType().FullName;
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
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
