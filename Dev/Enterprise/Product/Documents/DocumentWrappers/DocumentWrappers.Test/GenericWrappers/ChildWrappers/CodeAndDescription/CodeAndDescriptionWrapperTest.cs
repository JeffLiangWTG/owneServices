using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CodeAndDescriptionWrapper))]
	sealed class CodeAndDescriptionWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			CodeAndDescriptionWrapper wrapperEmpty = new CodeAndDescriptionWrapper(null, new DummyListForTesting(), Factory);
			AssertEquals("wrapperEmpty.Code", "", wrapperEmpty.Code);
			AssertEquals("wrapperEmpty.Description", "", wrapperEmpty.Description);
			AssertEquals("wrapperEmpty.CodeAndDescription", "", wrapperEmpty.CodeAndDescription);
			AssertEquals("wrapperEmpty.ToString()", "", wrapperEmpty.ToString());
		}

		public void TestDescriptionGetter_OverriddenDescriptionIsSpecified_ReturnOverriddenDescription()
		{
			var wrapperEmpty = new CodeAndDescriptionWrapper(DummyListForTesting.Codes.Dumb, "McLaren", Factory);

			AssertEquals("Description", "McLaren", wrapperEmpty.Description);
		}

		public void TestWrapperMappingValidCode()
		{
			CodeAndDescriptionWrapper wrapperValidCode = new CodeAndDescriptionWrapper(DummyListForTesting.Codes.Dumb, new DummyListForTesting(), Factory);
			AssertEquals("wrapperValidCode.Code", DummyListForTesting.Codes.Dumb, wrapperValidCode.Code);
			AssertEquals("wrapperValidCode.Description", DummyListForTesting.Descriptions.Dumb, wrapperValidCode.Description);
			string expectedCodeAndDescription = DummyListForTesting.Codes.Dumb + " - " + DummyListForTesting.Descriptions.Dumb;
			AssertEquals("wrapperValidCode.CodeAndDescription", expectedCodeAndDescription, wrapperValidCode.CodeAndDescription);
			AssertEquals("wrapperValidCode.ToString()", expectedCodeAndDescription, wrapperValidCode.ToString());
		}

		public void TestWrapperMappingInvalidCode()
		{
			CodeAndDescriptionWrapper wrapperInvalidCode = new CodeAndDescriptionWrapper("ZXZ", new DummyListForTesting(), Factory);
			AssertEquals("wrapperInvalidCode.Code", "ZXZ", wrapperInvalidCode.Code);
			AssertEquals("wrapperInvalidCode.Description", "ZXZ", wrapperInvalidCode.Description);
			AssertEquals("wrapperInvalidCode.CodeAndDescription", "ZXZ", wrapperInvalidCode.CodeAndDescription);
			AssertEquals("wrapperInvalidCode.ToString()", "ZXZ", wrapperInvalidCode.ToString());
		}

		public void TestDescriptionUsingBOCollection()
		{
			RefUNLOCO aUSYD = new RefUNLOCO.Loader(Factory).Load("AUSYD");
			CodeAndDescriptionWrapper wrapperAUSYD = new CodeAndDescriptionWrapper("AUSYD", new RefUNLOCOCollection(Factory), Factory);
			AssertEquals(aUSYD.RL_Code, wrapperAUSYD.Code);
			AssertEquals(aUSYD.RL_Code + " - " + aUSYD.RL_PortName, wrapperAUSYD.CodeAndDescription);
			AssertEquals(aUSYD.RL_PortName, wrapperAUSYD.Description);
		}

		public void TestConstructorWontAcceptANullList()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new CodeAndDescriptionWrapper(null, (CodeDescriptionPairList)null, Factory); });
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new CodeAndDescriptionWrapper(null, (BusinessObjectCollection)null, Factory); });
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
CodeAndDescription                 (Default Field: CodeAndDescription)
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
			return new CodeAndDescriptionWrapper(DummyListForTesting.Codes.Dumb, new DummyListForTesting(), Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CodeAndDescriptionWrapper(DummyListForTesting.Codes.Dumb, new DummyListForTesting(), Factory);
		}
	}
}
