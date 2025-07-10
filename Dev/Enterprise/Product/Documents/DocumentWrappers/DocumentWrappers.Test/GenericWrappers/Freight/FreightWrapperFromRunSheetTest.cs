using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromRunSheet))]
	sealed class FreightWrapperFromRunSheetTest : FreightWrapperTest
	{
		#region TestProperties

		public void TestProperties()
		{
			var cartage = Helper.CreateCartage(Core.Constants.CartageJobType.NEW_AirExport, 3);
			var leg1 = cartage.CartageLegs[0];
			var leg2 = cartage.CartageLegs[1];
			var leg3 = cartage.CartageLegs[2];

			leg1.JU_RunSheetSequence = 3;
			leg2.JU_RunSheetSequence = 1;
			leg3.JU_RunSheetSequence = 2;

			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.CartageLegs.Add(leg1);
			runSheet.CartageLegs.Add(leg2);
			runSheet.CartageLegs.Add(leg3);
			runSheet.EY_RunSheetNumber = "RS12345";

			var wrapper = new FreightWrapperFromRunSheet(runSheet, Factory);

			AssertEquals("JobNumberHeading", "Run Sheet", wrapper.JobNumberHeading);
			AssertEquals("JobNumber", "RS12345", wrapper.JobNumber);
			AssertEquals("Leg 2", leg2.PK, wrapper.LocalTransportLegs[0].WrappedObjectPK);
			AssertEquals("Leg 3", leg3.PK, wrapper.LocalTransportLegs[1].WrappedObjectPK);
			AssertEquals("Leg 1", leg1.PK, wrapper.LocalTransportLegs[2].WrappedObjectPK);
		}

		#endregion

		#region Overrides

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumberHeading", "Run Sheet" }
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
RunSheet : (No Default Field Value Available on RunSheetFromCommonRunSheet)";
			}
		}

		protected override bool IsCarrierUsed
		{
			get
			{
				return false;
			}
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<CommonWorkSheet>();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromRunSheet(null, Factory);
		}

		#endregion

		#region TestDocTypeCode

		public void TestDocTypeCode()
		{
			var workSheet = Factory.New<CommonWorkSheet>();
			var wrapper = new FreightWrapperFromRunSheet(workSheet, Factory);

			((IDocTypeCode)wrapper).DocTypeCode = "MFD";
			AssertEquals("MFD", ((IDocTypeCode)wrapper).DocTypeCode);

			((IDocTypeCode)wrapper).DocTypeCode = "CAR";
			AssertEquals("CAR", ((IDocTypeCode)wrapper).DocTypeCode);
		}

		#endregion

		#region TestBarcodeTextForFont

		protected override void SetJobNumberForBarcodeTesting(BusinessObject bizO)
		{
			var worksheet = (CommonWorkSheet)bizO;
			worksheet.EY_RunSheetNumber = "R1";
		}

		protected override string ExpectedBarcodeText()
		{
			return "È^CWS=R1;CAD;|@Ê";
		}

		#endregion

		#region Helper

		LocalCartageTestHelper Helper
		{
			get { return new LocalCartageTestHelper(Factory); }
		}

		#endregion
	}
}
