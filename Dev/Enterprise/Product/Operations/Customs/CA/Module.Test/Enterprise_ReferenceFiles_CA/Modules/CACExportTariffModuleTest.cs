using System;
using System.Collections.Generic;
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
	[TestedType(typeof(CACExportTariffModule))]
	sealed class CACExportTariffModuleTest : CACFilterGridModuleTest
	{
		public void TestTariffFilter()
		{
			using (var testModule = GetNewTestModule())
			{
				var filterBusinessObject = (CACFilterStripBusinessObject)((ZFilterStripControl)testModule.EmbeddedControl).FilterBusinessObject;
				var tariffFilter = (ModuleTextFilter)filterBusinessObject[CACExportTariffModule.TariffCaption];

				AssertEquals(13, tariffFilter.MaxLength);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CA.ExportTariff;

		protected override ZGridColumnInfo[] ExpectedColumns
		{
			get
			{
				return new[]
				{
					new ZTextBoxColumnStyleInfo { Caption = TariffCaption, ColumnName = CACExportTariffSchema.CE_Code.Name, Width = 100 },
					new ZTextBoxColumnStyleInfo { Caption = DescriptionCaption, ColumnName = CACExportTariffSchema.CE_Description.Name, Width = 300 },
					new ZTextBoxColumnStyleInfo { Caption = UnitCaption, ColumnName = CACExportTariffSchema.CE_Unit.Name, Width = 80 }
				};
			}
		}

		protected override Type ExpectedGridCollectionType => typeof(CACExportTariffCollection);

		protected override Dictionary<string, SchemaStringColumn> ExpectedFilters
		{
			get
			{
				return new Dictionary<string, SchemaStringColumn>
				{
					{ TariffCaption, CACExportTariffSchema.CE_Code },
					{ DescriptionCaption, CACExportTariffSchema.CE_Description },
					{ UnitCaption, CACExportTariffSchema.CE_Unit }
				};
			}
		}

		protected override ZFilterGridModule GetNewTestModule() => new CACExportTariffModule();

		const string TariffCaption = "Tariff";
		const string DescriptionCaption = "Description";
		const string UnitCaption = "Unit";
	}
}
