using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Core.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CACClassModule))]
	sealed class CACClassModuleTest : CACFilterGridModuleTest
	{
		public void TestClassificationFilter()
		{
			using (var testModule = GetNewTestModule())
			{
				var filterBusinessObject = (CACFilterStripBusinessObject)((ZFilterStripControl)testModule.EmbeddedControl).FilterBusinessObject;
				var classificationFilter = (ModuleTextFilter)filterBusinessObject[CACClassModule.TariffCaption];

				AssertEquals(13, classificationFilter.MaxLength);
			}
		}

		public void TestGetTariffQuery()
		{
			var tariff1 = Factory.New<CACClass>();
			tariff1.CT_Tariff = "0101012000";
			var tariff2 = Factory.New<CACClass>();
			tariff2.CT_Tariff = "0101020000";
			var tariff3 = Factory.New<CACClass>();
			tariff3.CT_Tariff = "0102010000";

			using (var module = new CACClassModule())
			{
				var filterBizObj = module.FilterBusinessObject;
				var tariffNumberFilter = (ModuleTextFilter)filterBizObj[CACClassModule.TariffCaption];
				tariffNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				tariffNumberFilter.IsActive = true;

				tariffNumberFilter.Property = "010";
				Assert("tariff1 should match filter", tariff1.MatchesFilter(filterBizObj.Filter));
				Assert("tariff2 should match filter", tariff2.MatchesFilter(filterBizObj.Filter));
				Assert("tariff3 should match filter", tariff3.MatchesFilter(filterBizObj.Filter));

				tariffNumberFilter.Property = "0101.0";
				Assert("tariff1 should match filter", tariff1.MatchesFilter(filterBizObj.Filter));
				Assert("tariff2 should match filter", tariff2.MatchesFilter(filterBizObj.Filter));
				Assert("tariff3 should not match filter", !tariff3.MatchesFilter(filterBizObj.Filter));

				tariffNumberFilter.Property = "0101.01";
				Assert("tariff1 should match filter", tariff1.MatchesFilter(filterBizObj.Filter));
				Assert("tariff2 should not match filter", !tariff2.MatchesFilter(filterBizObj.Filter));
				Assert("tariff3 should not match filter", !tariff3.MatchesFilter(filterBizObj.Filter));

				tariffNumberFilter.Property = "0101.01.1";
				Assert("tariff1 should not match filter", !tariff1.MatchesFilter(filterBizObj.Filter));
				Assert("tariff2 should not match filter", !tariff2.MatchesFilter(filterBizObj.Filter));
				Assert("tariff3 should not match filter", !tariff3.MatchesFilter(filterBizObj.Filter));

				tariffNumberFilter.Property = "0102010000";
				Assert("tariff1 should not match filter", !tariff1.MatchesFilter(filterBizObj.Filter));
				Assert("tariff2 should not match filter", !tariff2.MatchesFilter(filterBizObj.Filter));
				Assert("tariff3 should match filter", tariff3.MatchesFilter(filterBizObj.Filter));
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CA.ClassTariff;

		protected override ZGridColumnInfo[] ExpectedColumns
		{
			get
			{
				return new[]
				{
					new ZTextBoxColumnStyleInfo { Caption = TariffCaption, ColumnName = CACClassSchema.CT_Tariff.Name, Width = 100 },
					new ZTextBoxColumnStyleInfo { Caption = DescriptionCaption, ColumnName = CACClassSchema.CT_LongDescription.Name, Width = 300 },
					new ZTextBoxColumnStyleInfo { Caption = UnitCaption, ColumnName = CACClassSchema.CT_UQ.Name, Width = 80 }
				};
			}
		}

		protected override Type ExpectedGridCollectionType => typeof(CACClassCollection);

		protected override Dictionary<string, SchemaStringColumn> ExpectedFilters
		{
			get
			{
				return new Dictionary<string, SchemaStringColumn>
				{
					{ TariffCaption, CACClassSchema.CT_Tariff },
					{ DescriptionCaption, CACClassSchema.CT_LongDescription },
					{ UnitCaption, CACClassSchema.CT_UQ }
				};
			}
		}

		protected override ZFilterGridModule GetNewTestModule() => new CACClassModule();

		const string TariffCaption = "Classification";
		const string DescriptionCaption = "Description";
		const string UnitCaption = "Unit";
	}
}
