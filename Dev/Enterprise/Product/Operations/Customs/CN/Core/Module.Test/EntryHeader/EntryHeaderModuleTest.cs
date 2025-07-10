using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Module.Testing
{
	[TestedType(typeof(EntryHeaderModule))]
	class EntryHeaderModuleTest : Customs.Module.Testing.EntryHeaderModuleTest
	{
		protected override BaseJobDeclaration GetDeclarationForTesting(BusinessObjectFactory factory)
		{
			var declaration = base.GetDeclarationForTesting(factory);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			return declaration;
		}

		protected override CusEntryHeader GetEntryThatMatchesGridCollectionFilter(BaseJobDeclaration declaration)
		{
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "TEST_REF";
			return entry;
		}

		protected override Type ExpectedOperationalActionSupporterType => typeof(EntryHeaderOperationalActionSupporter);
	}
}
