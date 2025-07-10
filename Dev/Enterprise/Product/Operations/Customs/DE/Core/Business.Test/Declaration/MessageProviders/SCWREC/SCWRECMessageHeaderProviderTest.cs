using System;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(SCWRECMessageHeaderProvider))]
	class SCWRECMessageHeaderProviderTest : ImportMessageHeaderProviderTest<SCWRECMessageHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new SCWRECMessageHeaderProvider(null));
		}

		public void TestHeader()
		{
			AssertType<SCWRECHeaderProvider>("Type", Provider.Header);
		}

		public void TestMessageGroup_LVA()
		{
			entryHeader.EntryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.C;
			AssertEquals(Messaging.ImportMessageSubTypeList.Codes.BondedWarehouseSimplifiedDeclaration, Provider.MessageGroup);
		}

		public void TestMessageGroup_LVV()
		{
			entryHeader.EntryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.F;
			AssertEquals(Messaging.ImportMessageSubTypeList.Codes.BondedWarehouseSimplifiedPrematureDeclaration, Provider.MessageGroup);
		}

		public void TestMessageGroup_InvalidCEI_SubStyle()
		{
			entryHeader.EntryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.A;
			AssertEquals(ZString.Empty, Provider.MessageGroup);
		}

		protected override SCWRECMessageHeaderProvider GetProvider() => new SCWRECMessageHeaderProvider(entryHeader);
	}
}
