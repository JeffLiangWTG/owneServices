using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CACusPermitFilterStripBusinessObject))]
	sealed class CACusPermitFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestDIFURNFilterAndDIFMessageStatusFilter()
		{
			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var permit = Factory.New<CusPermitHeader>();
			var document11 = permit.RequiredDocuments.AddNew();
			var addinfo111 = document11.AddInfos.AddNew();
			addinfo111.EX_ReferenceNumber = "10000000111";
			addinfo111.EX_Status = "AOC";
			addinfo111.EX_ApplicationCode = "CAD";
			permit.CPH_StartDate = new ZDate(2018, 3, 20);
			permit.CPH_Number = "Dec1";
			permit.CPH_QtyValIndicator = "BTH";
			permit.CPH_OH_PermitHolder = orgHeader.PK;
			permit.CPH_Type = "ADJ";
			permit.CPH_SubType = ZString.Empty;
			Factory.Save();

			var filter = new CACusPermitFilterStripBusinessObject();
			var filterURN = filter["DIF URN"] as ModuleTextFilter;
			filterURN.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filterURN.IsActive = true;
			filterURN.Property = "10000000111";

			var filteredPermits = Factory.Load(typeof(CusPermitHeader), filter.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredPermits.Length);

			filterURN.Property = "10000000110";
			filteredPermits = Factory.Load(typeof(CusPermitHeader), filter.Filter);
			AssertEquals("Should have found 0 Records", 0, filteredPermits.Length);
			filterURN.IsActive = false;

			var filterStatus = filter["DIF Message Status"] as ModuleTextFilter;
			filterStatus.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filterStatus.IsActive = true;
			filterStatus.Property = "AOC";

			filteredPermits = Factory.Load(typeof(CusPermitHeader), filter.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredPermits.Length);

			filterStatus.Property = "AOB";
			filteredPermits = Factory.Load(typeof(CusPermitHeader), filter.Filter);
			AssertEquals("Should have found 0 Records", 0, filteredPermits.Length);

			AssertEquals(ModuleNumberFilter.MultiplyMaxLength(JobRequiredDocumentAddInfoSchema.EX_ReferenceNumber.MaxLength), filterURN.MaxLength);
			AssertEquals(JobRequiredDocumentAddInfoSchema.EX_Status.MaxLength, filterStatus.MaxLength);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CACusPermitFilterStripBusinessObject();
	}
}
