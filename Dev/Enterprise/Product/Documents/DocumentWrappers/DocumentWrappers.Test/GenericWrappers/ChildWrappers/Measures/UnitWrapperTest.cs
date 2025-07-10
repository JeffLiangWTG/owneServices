using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(UnitWrapper))]
	sealed class UnitWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			UnitWrapper wrapperEmpty = new UnitWrapper(ZString.Empty, new DummyListForTesting(), Factory);
			AssertEquals("wrapperEmpty.Code", ZString.Empty, wrapperEmpty.Code);
			AssertEquals("wrapperEmpty.CodeAndDescription", ZString.Empty, wrapperEmpty.CodeAndDescription);
			AssertEquals("wrapperEmpty.Description", ZString.Empty, wrapperEmpty.Description);
		}

		public void TestWrapperMappingDumb()
		{
			UnitWrapper wrapperDumb = new UnitWrapper(DummyListForTesting.Codes.Dumb, new DummyListForTesting(), Factory);
			AssertEquals("wrapperDumb.Code", DummyListForTesting.Codes.Dumb, wrapperDumb.Code);
			AssertEquals("wrapperDumb.CodeAndDescription", DummyListForTesting.Codes.Dumb + " - " + DummyListForTesting.Descriptions.Dumb, wrapperDumb.CodeAndDescription);
			AssertEquals("wrapperDumb.Description", DummyListForTesting.Descriptions.Dumb, wrapperDumb.Description);
		}

		public void TestWrapperMappingDumber()
		{
			UnitWrapper wrapperDumber = new UnitWrapper(DummyListForTesting.Codes.Dumber, new DummyListForTesting(), Factory);
			AssertEquals("wrapperDumber.Code", DummyListForTesting.Codes.Dumber, wrapperDumber.Code);
			AssertEquals("wrapperDumber.CodeAndDescription", DummyListForTesting.Codes.Dumber + " - " + DummyListForTesting.Descriptions.Dumber, wrapperDumber.CodeAndDescription);
			AssertEquals("wrapperDumber.Description", DummyListForTesting.Descriptions.Dumber, wrapperDumber.Description);
		}

		public void TestWrapperMappingUnknown()
		{
			UnitWrapper wrapperUnknown = new UnitWrapper("UNK", new DummyListForTesting(), Factory);
			AssertEquals("wrapperUnknown.Code", "UNK", wrapperUnknown.Code);
			AssertEquals("wrapperUnknown.CodeAndDescription", "UNK", wrapperUnknown.CodeAndDescription);
			AssertEquals("wrapperUnknown.Description", "UNK", wrapperUnknown.Description);
		}

		public void TestDescriptionUsingBOCollection()
		{
			RefUNLOCO aUSYD = new RefUNLOCO.Loader(Factory).Load("AUSYD");
			UnitWrapper wrapperAUSYD = new UnitWrapper("AUSYD", new RefUNLOCOCollection(Factory), Factory);
			AssertEquals(aUSYD.RL_Code, wrapperAUSYD.Code);
			AssertEquals(aUSYD.RL_Code + " - " + aUSYD.RL_PortName, wrapperAUSYD.CodeAndDescription);
			AssertEquals(aUSYD.RL_PortName, wrapperAUSYD.Description);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Unit                               (Default Field: CodeAndDescription)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Code                                    String
CodeAndDescription                      String
Description                             String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new UnitWrapper(DummyListForTesting.Codes.Dumb, new DummyListForTesting(), Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new UnitWrapper(ZString.Empty, new DummyListForTesting(), Factory);
		}
	}
}
