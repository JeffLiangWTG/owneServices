using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(LPCOEntryHeaderModule))]
	class LPCOEntryHeaderModuleTest : Customs.Module.Testing.EntryHeaderModuleTest
	{
		public void TestGetNewController()
		{
			using (var module = new LPCOEntryHeaderModule())
			{
				AssertType<LPCOEntryHeaderController>("Controller must be of type LPCOLicenseEntryHeaderController", module.GetNewController());
			}
		}

		public void TestAllowNewAndDelete()
		{
			using (var module = new LPCOEntryHeaderModule())
			{
				Assert("AllowNew must be FALSE", !module.AllowNew);
				Assert("AllowDelete must be FALSE", !module.AllowDelete);
			}
		}

		protected override BaseJobDeclaration GetDeclarationForTesting(BusinessObjectFactory factory)
		{
			var declaration = base.GetDeclarationForTesting(factory);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			return declaration;
		}

		protected override CusEntryHeader GetEntryThatMatchesGridCollectionFilter(BaseJobDeclaration declaration)
		{
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "TEST_REF";
			entry.CH_MessageType = BRJobMessageTypeList.Codes.LPCO;
			return entry;
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.BR.LPCOEntryHeader;

		public override Type ModuleToBashType => typeof(LPCOEntryHeaderModule);
	}
}
