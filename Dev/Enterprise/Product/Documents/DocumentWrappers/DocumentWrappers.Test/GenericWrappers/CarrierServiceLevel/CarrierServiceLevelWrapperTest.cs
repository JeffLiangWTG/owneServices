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
	[TestedType(typeof(CarrierServiceLevelWrapper))]
	sealed class CarrierServiceLevelWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = new CarrierServiceLevelWrapper(null, new DummyListForTesting(), Factory);
			AssertEquals("wrapperEmpty.Code", "", wrapperEmpty.Code);
			AssertEquals("wrapperEmpty.Description", "", wrapperEmpty.Description);
			AssertEquals("wrapperEmpty.CodeAndDescription", "", wrapperEmpty.CodeAndDescription);
			AssertEquals("wrapperEmpty.ToString()", "", wrapperEmpty.ToString());
		}

		public void TestDescriptionGetter_OverriddenDescriptionIsSpecified_ReturnOverriddenDescription()
		{
			var wrapperEmpty = new CarrierServiceLevelWrapper(DummyListForTesting.Codes.Dumb, "Test description", Factory);

			AssertEquals("Description", "Test description", wrapperEmpty.Description);
		}

		public void TestWrapperMappingValidCode()
		{
			var wrapperValidCode = new CarrierServiceLevelWrapper(DummyListForTesting.Codes.Dumb, new DummyListForTesting(), Factory);
			AssertEquals("wrapperValidCode.Code", DummyListForTesting.Codes.Dumb, wrapperValidCode.Code);
			AssertEquals("wrapperValidCode.Description", DummyListForTesting.Descriptions.Dumb, wrapperValidCode.Description);
			string expectedCodeAndDescription = DummyListForTesting.Codes.Dumb + " - " + DummyListForTesting.Descriptions.Dumb;
			AssertEquals("wrapperValidCode.CodeAndDescription", expectedCodeAndDescription, wrapperValidCode.CodeAndDescription);
			AssertEquals("wrapperValidCode.ToString()", expectedCodeAndDescription, wrapperValidCode.ToString());
		}

		public void TestWrapperMappingInvalidCode()
		{
			var wrapperInvalidCode = new CarrierServiceLevelWrapper("ZXZ", new DummyListForTesting(), Factory);
			AssertEquals("wrapperInvalidCode.Code", "ZXZ", wrapperInvalidCode.Code);
			AssertEquals("wrapperInvalidCode.Description", "ZXZ", wrapperInvalidCode.Description);
			AssertEquals("wrapperInvalidCode.CodeAndDescription", "ZXZ", wrapperInvalidCode.CodeAndDescription);
			AssertEquals("wrapperInvalidCode.ToString()", "ZXZ", wrapperInvalidCode.ToString());
		}

		public void TestDescriptionUsingBOCollection()
		{
			var aUSYD = new RefUNLOCO.Loader(Factory).Load("AUSYD");
			var wrapperAUSYD = new CarrierServiceLevelWrapper("AUSYD", new RefUNLOCOCollection(Factory), Factory);
			AssertEquals(aUSYD.RL_Code, wrapperAUSYD.Code);
			AssertEquals(aUSYD.RL_Code + " - " + aUSYD.RL_PortName, wrapperAUSYD.CodeAndDescription);
			AssertEquals(aUSYD.RL_PortName, wrapperAUSYD.Description);
		}

		public void TestCarrierServiceLevelBizo()
		{
			var aPProfileID = "id1";
			var carrierServiceCode = "cod1";
			var carrierServiceLevelDescription = "desc1";
			var chargeCode = "cod2";
			var productCode = "cod3";
			var proofOfDelivery = "proof1";
			var servicePrintDescription = "desc2";
			var isSignatureRequired = true;
			var carrierServiceLevel = Factory.NewWithValidTestData<OrgCarrierServiceLevel>();
			carrierServiceLevel.PL_APProfileID = aPProfileID;
			carrierServiceLevel.PL_CarrierServiceCode = carrierServiceCode;
			carrierServiceLevel.PL_CarrierServiceLevelDescription = carrierServiceLevelDescription;
			carrierServiceLevel.PL_ChargeCode = chargeCode;
			carrierServiceLevel.PL_ProductCode = productCode;
			carrierServiceLevel.PL_ProofOfDelivery = proofOfDelivery;
			carrierServiceLevel.PL_ServicePrintDescription = servicePrintDescription;
			carrierServiceLevel.PL_IsSignatureRequired = isSignatureRequired;

			var wrapper = new CarrierServiceLevelWrapper(carrierServiceLevel, Factory);
			AssertEquals(aPProfileID, wrapper.APProfileID);
			AssertEquals(carrierServiceCode, wrapper.CarrierServiceCode);
			AssertEquals(carrierServiceLevelDescription, wrapper.CarrierServiceLevelDescription);
			AssertEquals(chargeCode, wrapper.ChargeCode);
			AssertEquals(productCode, wrapper.ProductCode);
			AssertEquals(proofOfDelivery, wrapper.ProofOfDelivery);
			AssertEquals(servicePrintDescription, wrapper.ServicePrintDescription);
			AssertEquals(isSignatureRequired, wrapper.IsSignatureRequired);
		}

		public void TestConstructorWontAcceptANullList()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new CarrierServiceLevelWrapper(null, (CodeDescriptionPairList)null, Factory); });
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new CarrierServiceLevelWrapper(null, (BusinessObjectCollection)null, Factory); });
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
CarrierServiceLevel                (Default Field: CodeAndDescription)
======================================================================
Name                                    Type
----------------------------------------------------------------------
APProfileID                             String
CarrierServiceCode                      String
CarrierServiceLevelDescription          String
ChargeCode                              String
Code                                    String
CodeAndDescription                      String
Description                             String
IsSignatureRequired                     Bool
ProductCode                             String
ProofOfDelivery                         String
ServicePrintDescription                 String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new CarrierServiceLevelWrapper(DummyListForTesting.Codes.Dumb, new DummyListForTesting(), Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CarrierServiceLevelWrapper(DummyListForTesting.Codes.Dumb, new DummyListForTesting(), Factory);
		}
	}
}
