using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocOuterPack))]
	sealed class DocOuterPackTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
				{
					DocOuterPack.New(BisOuterPack, Factory),
				};
		}

		public void TestHazardousDescription()
		{
			AssertEquals("HazardousDescription with no packline", ZString.Empty, OuterPackWrapper.HazardousDescription);

			var line = Factory.New<PackLine>();
			var lineWrapped = DocPackLines.New(line, Factory);
			BisOuterPack.DocPackLines = lineWrapped;

			AssertEquals("HazardousDescription with packline no HAZ", " - 0 KG", OuterPackWrapper.HazardousDescription);

			line.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;

			AssertEquals("HazardousDescription with com.code", "HAZ - 0 KG", OuterPackWrapper.HazardousDescription);

			var undg = Factory.New<UNDGSubstance>();
			undg.DG_UNNO = "2345";
			undg.DG_PSN = "Shipping Name";
			undg.DG_Class = "6.2";
			undg.DG_SubLabel1 = "6.1";
			undg.DG_PG = "PG";
			undg.DG_Variant = "b";
			undg.DG_FlashPoint = string.Empty;

			line.UNDGs.AddNew().DI_DG = undg.PK;
			line.JL_ActualWeight = 123.00m;
			line.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;

			AssertEquals("HazardousDescription with com.code+UNDG", "HAZ - UN2345, Shipping Name, class 6.2 (6.1), PG PG - 123 KG", OuterPackWrapper.HazardousDescription);
		}

		#region Implementation

		OuterPack BisOuterPack;
		DocOuterPack OuterPackWrapper;

		protected override void SetUp()
		{
			BisOuterPack = new OuterPack();
			OuterPackWrapper = DocOuterPack.New(BisOuterPack, Factory);
			base.SetUp();
		}

		#endregion
	}
}
