using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(SCIDECMessageHeaderProvider))]
	sealed class SCIDECMessageHeaderProviderTest : ImportMessageHeaderProviderTest<SCIDECMessageHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new SCIDECMessageHeaderProvider(null));
		}

		public void TestHeader()
		{
			AssertType<SCIDECHeaderProvider>("Type", Provider.Header);
		}

		public void TestMessageGroup_AAE_A()
		{
			entryHeader.EntryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.A;
			AssertEquals(ImportMessageSubTypeList.Codes.InwardProcessingSingleDeclaration, Provider.MessageGroup);
		}

		public void TestMessageGroup_AAE_B()
		{
			entryHeader.EntryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.B;
			AssertEquals(ImportMessageSubTypeList.Codes.InwardProcessingSingleDeclaration, Provider.MessageGroup);
		}

		public void TestMessageGroup_AVE_D()
		{
			entryHeader.EntryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.D;
			AssertEquals(ImportMessageSubTypeList.Codes.InwardProcessingSinglePrematureDeclaration, Provider.MessageGroup);
		}

		public void TestMessageGroup_AVE_E()
		{
			entryHeader.EntryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.E;
			AssertEquals(ImportMessageSubTypeList.Codes.InwardProcessingSinglePrematureDeclaration, Provider.MessageGroup);
		}

		public void TestMessageGroup_InvalidCEI_SubStyle()
		{
			entryHeader.EntryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.C;
			AssertEquals(ZString.Empty, Provider.MessageGroup);
		}

		protected override IEnumerable<Expression<Func<SCIDECMessageHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Header;
		}

		protected override SCIDECMessageHeaderProvider GetProvider() => new SCIDECMessageHeaderProvider(entryHeader);
	}
}
