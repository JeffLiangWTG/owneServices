using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocCTOCusMAWB))]
	sealed class DocCTOCusMAWBTest : DocumentWrapperTestCase
	{
		#region TestAirlineName

		public void TestAirlineName()
		{
			RefAirline qF = RefAirline.LoadFromAirline2LetterCode(Factory, "QF");
			RefAirline xX = RefAirline.LoadFromAirline2LetterCode(Factory, "XX");

			AssertNotNull("precondition: QF should exist", qF);
			AssertNull("precondition: XX should not exist", xX);

			Mawb.CM_FlightNo = "";
			AssertEquals("", Wrapper.AirlineName);

			Mawb.CM_FlightNo = "QF1234";
			AssertEquals(qF.RM_AirlineName1, Wrapper.AirlineName);

			Mawb.CM_FlightNo = "XX1234";
			AssertEquals("", Wrapper.AirlineName);
		}

		#endregion

		#region TestChildrenBills

		public void TestChildrenBills()
		{
			Mawb.ChildBills.RemoveAndDeleteAll();
			CTOCusHAWB hawb = Mawb.ChildBills.AddNew();

			DocCTOCusHAWBCollection docHawbs = Wrapper.ChildrenBills;
			AssertEquals("should only have 1 child", 1, docHawbs.Count);
			AssertSame("should wrap hawb", hawb, docHawbs[0].WrappedObject);
		}

		#endregion

		#region TestTotalWeight

		public void TestTotalWeight()
		{
			Env.Registry.FreightWeightUnit = Core.Constants.Weight.Kilograms;

			Mawb.ChildBills.RemoveAndDeleteAll();
			CTOCusHAWB hawb1 = Mawb.ChildBills.AddNew();
			hawb1.CS_WeightUQ = Core.Constants.Weight.Tonnes;
			hawb1.CS_Weight = 0.2m;

			CTOCusHAWB hawb2 = Mawb.ChildBills.AddNew();
			hawb2.CS_WeightUQ = Core.Constants.Weight.Kilograms;
			hawb2.CS_Weight = 20m;

			CTOCusHAWB hawb3 = Mawb.ChildBills.AddNew();
			hawb3.CS_WeightUQ = Core.Constants.Weight.Grams;
			hawb3.CS_Weight = 2000m;

			AssertEquals(222m, Wrapper.TotalWeight);
			AssertEquals(Core.Constants.Weight.Kilograms, Wrapper.TotalWeightUnit);

			Env.Registry.FreightWeightUnit = Core.Constants.Weight.Pounds;

			AssertEquals(489.426m, Wrapper.TotalWeight);
			AssertEquals(Core.Constants.Weight.Pounds, Wrapper.TotalWeightUnit);
		}

		#endregion

		#region TestTotalPiecesManifested

		public void TestTotalPiecesManifested()
		{
			Mawb.ChildBills.RemoveAndDeleteAll();
			CTOCusHAWB hawb1 = Mawb.ChildBills.AddNew();
			hawb1.CS_PiecesManifested = 5;

			CTOCusHAWB hawb2 = Mawb.ChildBills.AddNew();
			hawb2.CS_PiecesManifested = 3;

			AssertEquals(8, Wrapper.TotalPiecesManifested);
		}

		#endregion

		#region Implementation

		#region Mawb

		CTOCusMAWB Mawb
		{
			get
			{
				if (mawb == null)
				{
					mawb = Factory.New<CTOCusMAWB>();
				}

				return mawb;
			}
		}

		CTOCusMAWB mawb;

		#endregion

		#region DocCTOCusMAWB

		DocCTOCusMAWB Wrapper
		{
			get { return DocCTOCusMAWB.New(Mawb, Factory); }
		}

		#endregion

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { Wrapper };
		}

		#endregion
	}
}
