using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;
using UniversalConstants = Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class NctsTadTsadDocumentTest : TestCaseWithFactory
	{
		public void TestRenderTadFromShipments()
		{
			var shipment = Factory.New<ForwardingShipment>();
			nctsHeader.BH_ParentID = shipment.PK;
			nctsHeader.BH_ParentTableCode = shipment.TablePrefix;

			using (branch.SetAsTemporaryContext())
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(UniversalConstants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				if (shipment.DocumentSupporter != null)
				{
					var docCommand = GetDocumentCommandThatWeWillFireAsIfUserClickedIt(nctsHeader, "35809bcd-6b9c-4f63-b1d9-d87d94fd62a4");
#pragma warning disable IDE0004 // Remove Unnecessary Cast
					var silentPrinter = new SilentDocumentPrinter(shipment.Factory, (CommonShipment)shipment, docCommand);
#pragma warning restore IDE0004 // Remove Unnecessary Cast
					silentPrinter.Print(ZGuid.Empty, 0, true, true, shipment.DocManagerInfo, language: null);

					var queuedPrintJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, shipment.PK));
					Assert("There should be an IE TAD/TSAD in the transit declaration print job queue.", queuedPrintJobs.Any(p => p.SP_DocumentName.Contains("IE TAD")));
				}
			}
		}

		DocumentCommand GetDocumentCommandThatWeWillFireAsIfUserClickedIt(NctsHeader nctsHeader, ZString menuItemPK)
		{
			var filter = new DocumentZQuery();
			filter.AddToFilter(StmMenuItemSchema.PK, new ZGuid(menuItemPK));
			return nctsHeader.Factory.LoadTop1<DocumentCommand>(filter);
		}

		protected override void SetUp()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "C#@";
			company.GC_Name = "COMP TEST";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			branch = company.Branches.AddNew();
			branch.GB_Code = "B#@";
			branch.GB_BranchName = "BRANCH TEST";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			Factory.Save();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_GB = branch.PK;
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		}

		NctsHeader nctsHeader;
		GlbBranch branch;
	}
}
