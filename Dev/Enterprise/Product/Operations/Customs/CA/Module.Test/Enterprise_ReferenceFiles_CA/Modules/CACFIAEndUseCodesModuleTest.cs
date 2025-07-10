using System;
using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.Core.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CACFIAEndUseCodesModule))]
	sealed class CACFIAEndUseCodesModuleTest : CACFilterGridModuleTest
	{
		public void TestModuleAllows()
		{
			using (var module = new CACFIAEndUseCodesModule())
			{
				AssertEquals("module.AllowDelete", false, module.AllowDelete);
				AssertEquals("module.AllowNew", false, module.AllowNew);
				AssertEquals("module.AllowEdit", false, module.AllowEdit);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CA.CFIAEndUseCodes;

		protected override ZGridColumnInfo[] ExpectedColumns
		{
			get
			{
				return new[]
				{
					new ZTextBoxColumnStyleInfo { Caption = CodeCaption, ColumnName = CACFIAEndUseCodesSchema.FE_Code.Name, Width = 100 },
					new ZTextBoxColumnStyleInfo { Caption = DescCaption, ColumnName = CACFIAEndUseCodesSchema.FE_Desc.Name, Width = 400 },
					new ZTextBoxColumnStyleInfo { Caption = FrenchDescCaption, ColumnName = CACFIAEndUseCodesSchema.FE_DescFrench.Name, Width = 400 },
				};
			}
		}

		protected override Type ExpectedGridCollectionType => typeof(CACFIAEndUseCodesCollection);

		protected override Dictionary<string, SchemaStringColumn> ExpectedFilters
		{
			get
			{
				return new Dictionary<string, SchemaStringColumn>
				{
					{ CodeCaption, CACFIAEndUseCodesSchema.FE_Code },
					{ DescCaption, CACFIAEndUseCodesSchema.FE_Desc },
					{ FrenchDescCaption, CACFIAEndUseCodesSchema.FE_DescFrench },
				};
			}
		}

		protected override ZFilterGridModule GetNewTestModule() => new CACFIAEndUseCodesModule();

		const string CodeCaption = "End Use Code";
		const string DescCaption = "Description";
		const string FrenchDescCaption = "Français";
	}
}
