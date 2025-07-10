using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ReleaseBuilds.Module.Testing
{
	[TestedType(typeof(ReleaseBuildFilterBusinessObject))]
	class ReleaseBuildFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestReleaseRings()
		{
			AssertEquals("ReleaseRings.GetType()", typeof(ReleaseRingsList), FilterBO.ReleaseRings.GetType());
		}

		public void TestProductFilter()
		{
			var list = new SystemProductCollection();
			list.AddNew("YGI", (NoResString)"Yogi", true);
			list.AddNew("BBO", (NoResString)"BooBoo", true);
			list.AddNew("DXT", (NoResString)"Dexter", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			ReleaseBuild build1 = Factory.NewWithValidTestData<ReleaseBuild>();
			build1.HL_Product = "YGI";
			ReleaseBuild build2 = Factory.NewWithValidTestData<ReleaseBuild>();
			build2.HL_Product = "BBO";
			ReleaseBuild build3 = Factory.NewWithValidTestData<ReleaseBuild>();
			build3.HL_Product = "DXT";
			ReleaseBuild build4 = Factory.NewWithValidTestData<ReleaseBuild>();
			build4.HL_Product = ProductTypes.Codes.Enterprise;
			Factory.Save();
			var releaseBuildFilter = new ReleaseBuildFilterBusinessObject();
			var productFilter = (ModuleTextFilter)releaseBuildFilter["Product"];
			ReleaseBuildCollection releaseBuildCollection = new ReleaseBuildCollection(Factory);
			productFilter.Property = "YGI";
			productFilter.IsActive = true;
			releaseBuildCollection.Load(releaseBuildFilter.Filter);
			Assert("Should only contain release build with 'YGI' Product", releaseBuildCollection.Contains(build1.PK));
			Assert("Should only contain release build with 'YGI' Product", !releaseBuildCollection.Contains(build2.PK));
			Assert("Should only contain release build with 'YGI' Product", !releaseBuildCollection.Contains(build3.PK));
			Assert("Should only contain release build with 'YGI' Product", !releaseBuildCollection.Contains(build4.PK));
			productFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			productFilter.IsActive = true;
			releaseBuildCollection.Load(releaseBuildFilter.Filter);
			Assert("Should only contain release build without 'YGI' Product", !releaseBuildCollection.Contains(build1.PK));
			Assert("Should only contain release build without 'YGI' Product", releaseBuildCollection.Contains(build2.PK));
			Assert("Should only contain release build without 'YGI' Product", releaseBuildCollection.Contains(build3.PK));
			Assert("Should only contain release build without 'YGI' Product", releaseBuildCollection.Contains(build4.PK));
			productFilter.IsActive = false;
			productFilter.Property = "BBO";
			productFilter.IsActive = true;
			productFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			releaseBuildCollection.Load(releaseBuildFilter.Filter);
			Assert("Should only contain release build with 'BBO' Product", !releaseBuildCollection.Contains(build1.PK));
			Assert("Should only contain release build with 'BBO' Product", releaseBuildCollection.Contains(build2.PK));
			Assert("Should only contain release build with 'BBO' Product", !releaseBuildCollection.Contains(build3.PK));
			Assert("Should only contain release build with 'BBO' Product", !releaseBuildCollection.Contains(build4.PK));
			productFilter.IsActive = false;
			productFilter.Property = ProductTypes.Codes.Enterprise;
			productFilter.IsActive = true;
			releaseBuildCollection.Load(releaseBuildFilter.Filter);
			Assert("Should only contain release build with 'BBO' Product", !releaseBuildCollection.Contains(build1.PK));
			Assert("Should only contain release build with 'BBO' Product", !releaseBuildCollection.Contains(build2.PK));
			Assert("Should only contain release build with 'BBO' Product", !releaseBuildCollection.Contains(build3.PK));
			Assert("Should only contain release build with 'BBO' Product", releaseBuildCollection.Contains(build4.PK));
		}

		public void TestStatusFilter()
		{
			ReleaseBuildCollection collection = new ReleaseBuildCollection(Factory);
			ReleaseBuild build1 = Factory.New<ReleaseBuild>();
			ReleaseBuild build2 = Factory.New<ReleaseBuild>();
			build1.HL_Superceded = false;
			build2.HL_Superceded = true;
			((ModuleTextFilter)FilterBO["Status"]).Property = ReleaseBuildFilterBusinessObject.StatusCodes.NonSuperseded;
			((ModuleTextFilter)FilterBO["Status"]).IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build1)", true, collection.Contains(build1));
			AssertEquals("Contains(build2)", false, collection.Contains(build2));
			((ModuleTextFilter)FilterBO["Status"]).Property = ReleaseBuildFilterBusinessObject.StatusCodes.Superseded;
			((ModuleTextFilter)FilterBO["Status"]).IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build1)", false, collection.Contains(build1));
			AssertEquals("Contains(build2)", true, collection.Contains(build2));
			((ModuleTextFilter)FilterBO["Status"]).Property = "";
			((ModuleTextFilter)FilterBO["Status"]).IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build1)", true, collection.Contains(build1));
			AssertEquals("Contains(build2)", true, collection.Contains(build2));
		}

		public void TestVersionNumberFilter()
		{
			ReleaseBuildCollection collection = new ReleaseBuildCollection(Factory);
			ReleaseBuild build1 = Factory.New<ReleaseBuild>();
			ReleaseBuild build2 = Factory.New<ReleaseBuild>();
			ReleaseBuild build3 = Factory.New<ReleaseBuild>();
			ReleaseBuild build4 = Factory.New<ReleaseBuild>();
			build1.VersionNumber = new VersionNumber(1, 1, 1, 1);
			build2.VersionNumber = new VersionNumber(1, 1, 2, 2);
			build3.VersionNumber = new VersionNumber(2, 2, 1, 1);
			build4.VersionNumber = new VersionNumber(1, 3, 1234, 56);
			Factory.Save();
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "";
			((ModuleTextFilter)FilterBO["Version Number"]).IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build1)", true, collection.Contains(build1));
			AssertEquals("Contains(build2)", true, collection.Contains(build2));
			AssertEquals("Contains(build3)", true, collection.Contains(build3));
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "1";
			((ModuleTextFilter)FilterBO["Version Number"]).IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build1)", true, collection.Contains(build1));
			AssertEquals("Contains(build2)", true, collection.Contains(build2));
			AssertEquals("Contains(build3)", false, collection.Contains(build3));
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "2";
			((ModuleTextFilter)FilterBO["Version Number"]).IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build1)", false, collection.Contains(build1));
			AssertEquals("Contains(build2)", false, collection.Contains(build2));
			AssertEquals("Contains(build3)", true, collection.Contains(build3));
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "1.1";
			((ModuleTextFilter)FilterBO["Version Number"]).IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build1)", true, collection.Contains(build1));
			AssertEquals("Contains(build2)", true, collection.Contains(build2));
			AssertEquals("Contains(build3)", false, collection.Contains(build3));
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "1.2";
			((ModuleTextFilter)FilterBO["Version Number"]).IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build1)", false, collection.Contains(build1));
			AssertEquals("Contains(build2)", false, collection.Contains(build2));
			AssertEquals("Contains(build3)", false, collection.Contains(build3));
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "1.1.2.";
			((ModuleTextFilter)FilterBO["Version Number"]).IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build1)", false, collection.Contains(build1));
			AssertEquals("Contains(build2)", true, collection.Contains(build2));
			AssertEquals("Contains(build3)", false, collection.Contains(build3));
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "1.1.2.2";
			((ModuleTextFilter)FilterBO["Version Number"]).IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build1)", false, collection.Contains(build1));
			AssertEquals("Contains(build2)", true, collection.Contains(build2));
			AssertEquals("Contains(build3)", false, collection.Contains(build3));
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "1.1.2.1";
			((ModuleTextFilter)FilterBO["Version Number"]).IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build1)", false, collection.Contains(build1));
			AssertEquals("Contains(build2)", false, collection.Contains(build2));
			AssertEquals("Contains(build3)", false, collection.Contains(build3));
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "1.3.1234.56";
			((ModuleTextFilter)FilterBO["Version Number"]).SqlComparisonOperator = SQLComparisonOperator.Equal;
			((ModuleTextFilter)FilterBO["Version Number"]).IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build4)", true, collection.Contains(build4));
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "1.3.1234.57";
			((ModuleTextFilter)FilterBO["Version Number"]).SqlComparisonOperator = SQLComparisonOperator.Equal;
			((ModuleTextFilter)FilterBO["Version Number"]).IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build4)", false, collection.Contains(build4));
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "1.3.1234.56";
			((ModuleTextFilter)FilterBO["Version Number"]).SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			((ModuleTextFilter)FilterBO["Version Number"]).IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build1)", true, collection.Contains(build1));
			AssertEquals("Contains(build2)", true, collection.Contains(build2));
			AssertEquals("Contains(build3)", true, collection.Contains(build3));
			AssertEquals("Contains(build4)", false, collection.Contains(build4));
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "1.3.123";
			((ModuleTextFilter)FilterBO["Version Number"]).SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			((ModuleTextFilter)FilterBO["Version Number"]).IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build4)", true, collection.Contains(build4));
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "1.1";
			((ModuleTextFilter)FilterBO["Version Number"]).SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			((ModuleTextFilter)FilterBO["Version Number"]).IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build1)", false, collection.Contains(build1));
			AssertEquals("Contains(build2)", false, collection.Contains(build2));
			AssertEquals("Contains(build3)", true, collection.Contains(build3));
			AssertEquals("Contains(build4)", true, collection.Contains(build4));
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "1234";
			((ModuleTextFilter)FilterBO["Version Number"]).SqlComparisonOperator = SQLComparisonOperator.Contains;
			((ModuleTextFilter)FilterBO["Version Number"]).IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build4)", true, collection.Contains(build4));
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "1234";
			((ModuleTextFilter)FilterBO["Version Number"]).SqlComparisonOperator = SQLComparisonOperator.NotContains;
			((ModuleTextFilter)FilterBO["Version Number"]).IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build1)", true, collection.Contains(build1));
			AssertEquals("Contains(build2)", true, collection.Contains(build2));
			AssertEquals("Contains(build3)", true, collection.Contains(build3));
			AssertEquals("Contains(build4)", false, collection.Contains(build4));
		}

		public void TestValidateVersionNumber()
		{
			AssertNoErrors("Precondition: VersionNumber should not have errors.", ((ModuleTextFilter)FilterBO["Version Number"]).PropertyInfo);
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "x";
			AssertHasError(((ModuleTextFilter)FilterBO["Version Number"]).PropertyInfo, "Please enter a valid Version Number.");
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "1.x";
			AssertHasError(((ModuleTextFilter)FilterBO["Version Number"]).PropertyInfo, "Please enter a valid Version Number.");
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "1.1.1.1.1";
			AssertHasError(((ModuleTextFilter)FilterBO["Version Number"]).PropertyInfo, "Please enter a valid Version Number.");
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "";
			AssertNoErrors(((ModuleTextFilter)FilterBO["Version Number"]).PropertyInfo);
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "1";
			AssertNoErrors(((ModuleTextFilter)FilterBO["Version Number"]).PropertyInfo);
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "1.1";
			AssertNoErrors(((ModuleTextFilter)FilterBO["Version Number"]).PropertyInfo);
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "1.1.1";
			AssertNoErrors(((ModuleTextFilter)FilterBO["Version Number"]).PropertyInfo);
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "1.1.1.1";
			AssertNoErrors(((ModuleTextFilter)FilterBO["Version Number"]).PropertyInfo);
			((ModuleTextFilter)FilterBO["Version Number"]).Property = "1.1.1.1.";
			AssertNoErrors(((ModuleTextFilter)FilterBO["Version Number"]).PropertyInfo);
		}

		#region Date Filters
		[TestDateIncremental(1, 0, 0, 0)]
		public void TestDateImportedFilter()
		{
			ReleaseBuild build1 = Factory.New<ReleaseBuild>();
			ReleaseBuild build2 = Factory.New<ReleaseBuild>();
			Factory.Save();
			ZQuery addedLogFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystem.Code);
			ZDateTime build1ImportDateTime = build1.Logs.Find(addedLogFilter)[0].SL_PostedTimeUtc;
			ZDateTime build2ImportDateTime = build2.Logs.Find(addedLogFilter)[0].SL_PostedTimeUtc;
			TestDateFilter(ReleaseBuildFilterBusinessObject.DateTypeCodes.DateImported, build1, build2, build1ImportDateTime, build2ImportDateTime);
		}

		public void TestExeDateFilter()
		{
			ReleaseBuild build1 = Factory.New<ReleaseBuild>();
			ReleaseBuild build2 = Factory.New<ReleaseBuild>();
			build1.HL_ExeVersionDate = new ZDateTime(2006, 1, 1);
			build2.HL_ExeVersionDate = new ZDateTime(2006, 1, 2);
			TestDateFilter(ReleaseBuildFilterBusinessObject.DateTypeCodes.ExeDate, build1, build2, build1.HL_ExeVersionDate, build2.HL_ExeVersionDate);
		}

		void TestDateFilter(string dateType, ReleaseBuild build1, ReleaseBuild build2, ZDateTime build1FilterDateTime, ZDateTime build2FilterDateTime)
		{
			((ModuleDateFilter)FilterBO[dateType]).IsActive = true;
			((ModuleDateFilter)FilterBO[dateType]).PropertySearch = "Date range";
			ReleaseBuildCollection collection = new ReleaseBuildCollection(Factory);
			((ModuleDateFilter)FilterBO[dateType]).Property1 = build1FilterDateTime;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build1)", true, collection.Contains(build1));
			AssertEquals("Contains(build2)", true, collection.Contains(build2));
			((ModuleDateFilter)FilterBO[dateType]).Property1 = build2FilterDateTime;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build1)", false, collection.Contains(build1));
			AssertEquals("Contains(build2)", true, collection.Contains(build2));
			((ModuleDateFilter)FilterBO[dateType]).Property1 = ZDateTime.Empty;
			((ModuleDateFilter)FilterBO[dateType]).Property2 = build1FilterDateTime;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build1)", true, collection.Contains(build1));
			AssertEquals("Contains(build2)", false, collection.Contains(build2));
			((ModuleDateFilter)FilterBO[dateType]).Property2 = build2FilterDateTime;
			collection.Load(FilterBO.Filter);
			AssertEquals("Contains(build1)", true, collection.Contains(build1));
			AssertEquals("Contains(build2)", true, collection.Contains(build2));
		}

		#endregion
		#region Implementation
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ReleaseBuildFilterBusinessObject();
		}

		ReleaseBuildFilterBusinessObject FilterBO
		{
			get
			{
				if (filterBO == null)
				{
					filterBO = (ReleaseBuildFilterBusinessObject)GetNewBusinessObject();
				}

				return filterBO;
			}
		}

		ReleaseBuildFilterBusinessObject filterBO;
		#endregion
	}
}
