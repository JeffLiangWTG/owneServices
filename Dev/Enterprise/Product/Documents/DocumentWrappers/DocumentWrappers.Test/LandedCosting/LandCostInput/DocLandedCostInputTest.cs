using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.LandedCosting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocLandedCostInput))]
	sealed class DocLandedCostInputTest : DocumentWrapperTestCase
	{
		public void TestNew()
		{
			AssertNull("Created with null", DocLandedCostInput.New(null, Factory));
			AssertNotNull("Created with a valid object", DocLCInput);
		}

		public void TestChargeCode()
		{
			AssertEquals("ChargeCode", "", DocLCInput.ChargeCode);

			var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery());
			LCInput.LI_AC_ChargeCode = chargeCode.PK;
			AssertEquals("ChargeCode", chargeCode.AC_Code, DocLCInput.ChargeCode);
		}

		public void TestChargeDescription()
		{
			LCInput.LI_ChargeDescription = "TEST";
			AssertEquals("ChargeDescription", LCInput.LI_ChargeDescription, DocLCInput.ChargeDescription);
		}

		public void TestCostAmountInLocalCurrency()
		{
			LCInput.LI_CostAmount = 100m;
			LCInput.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("CostAmountIn Local Currency", 100.00m, DocLCInput.CostAmountInLocalCurrency);

			LCInput.LI_ServiceExRate = 0.5m;
			AssertEquals("CostAmountIn Local Currency", 200.00m, DocLCInput.CostAmountInLocalCurrency);
		}

		public void TestLCGroup1Amount()
		{
			AssertLCGroupAmount(1, "LCGroup1Amount");
		}

		public void TestLCGroup2Amount()
		{
			AssertLCGroupAmount(2, "LCGroup2Amount");
		}

		public void TestLCGroup3Amount()
		{
			AssertLCGroupAmount(3, "LCGroup3Amount");
		}

		public void TestLCGroup4Amount()
		{
			AssertLCGroupAmount(4, "LCGroup4Amount");
		}

		public void TestLCGroup5Amount()
		{
			AssertLCGroupAmount(5, "LCGroup5Amount");
		}

		public void TestLCGroup6Amount()
		{
			AssertLCGroupAmount(6, "LCGroup6Amount");
		}

		public void TestLCGroupMiscAmount()
		{
			AssertLCGroupAmount(7, "LCGroupMiscAmount");
		}

		void AssertLCGroupAmount(ZByte groupID, string propertyName)
		{
			LCInput.LI_LandedCostGroup = groupID;
			LCInput.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			LCInput.LI_CostAmount = 100.025m;
			AssertEquals(groupID + " is rounded", 100.03m, DocLCInput[propertyName]);

			LCInput.LI_CostAmount = 100.024m;
			AssertEquals(groupID + " is rounded", 100.02m, DocLCInput[propertyName]);
		}

		#region Implementation

		LandedCostHeader LCHeader
		{
			get
			{
				if (fLCHeader == null)
				{
					fLCHeader = Factory.New<LandedCostHeader>();
				}
				return fLCHeader;
			}
		}
		LandedCostHeader fLCHeader;

		LandCostInput LCInput
		{
			get
			{
				if (fLCInput == null)
				{
					fLCInput = LCHeader.CostInputs.AddNew();
				}
				return fLCInput;
			}
		}
		LandCostInput fLCInput;

		DocLandedCostInput DocLCInput
		{
			get
			{
				if (fDocLCInput == null)
				{
					fDocLCInput = DocLandedCostInput.New(LCInput, Factory);
				}
				return fDocLCInput;
			}
		}
		DocLandedCostInput fDocLCInput;

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocLCInput };
		}

		#endregion
	}
}
