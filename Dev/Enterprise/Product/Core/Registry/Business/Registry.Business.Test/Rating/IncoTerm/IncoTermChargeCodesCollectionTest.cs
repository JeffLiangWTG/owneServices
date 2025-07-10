using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(IncoTermChargeCodesCollection))]
	sealed class IncoTermChargeCodesCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<IncoTermChargeCodesCollection>
	{
		protected override void SetUp()
		{
			base.SetUp();
			IncoTermChargeCodesCollection.ClearDefaultIncoTermChargeCodesForTesting();
		}

		[TestDate(2020, 1, 1)]
		public void TestGetDefaultPost2020()
		{
			var defaultCodes = IncoTermChargeCodesCollection.GetDefault();

			var i = 0;
			AssertEquals(18, defaultCodes.Count);
			AssertIncoTermChargeCodes(defaultCodes[i++], "CFR", "CNR", "CNR", "CNR", "CNR", "CNE", "CNE", "CNE", "CNE", "CNE");
			AssertIncoTermChargeCodes(defaultCodes[i++], "CIF", "CNR", "CNR", "CNR", "CNR", "CNR", "CNE", "CNE", "CNE", "CNE");
			AssertIncoTermChargeCodes(defaultCodes[i++], "CIP", "CNR", "CNR", "CNR", "CNR", "CNR", "CNR", "CNR", "CNE", "CNE");
			AssertIncoTermChargeCodes(defaultCodes[i++], "CPT", "CNR", "CNR", "CNR", "CNR", "CNE", "CNR", "CNR", "CNE", "CNE");
			AssertIncoTermChargeCodes(defaultCodes[i++], "DAF", "CNR", "CNR", "CNR", "CNR", "CNR", "CNR", "CNR", "CNR", "CNE");
			AssertIncoTermChargeCodes(defaultCodes[i++], "DAP", "CNR", "CNR", "CNR", "CNR", "CNR", "CNE", "CNR", "CNE", "CNE");
			AssertIncoTermChargeCodes(defaultCodes[i++], "DAT", "CNR", "CNR", "CNR", "CNR", "CNR", "CNR", "CNR", "CNE", "CNE");
			AssertIncoTermChargeCodes(defaultCodes[i++], "DDP", "CNR", "CNR", "CNR", "CNR", "CNR", "CNE", "CNR", "CNR", "CNR");
			AssertIncoTermChargeCodes(defaultCodes[i++], "DDU", "CNR", "CNR", "CNR", "CNR", "CNR", "CNR", "CNR", "CNE", "CNE");
			AssertIncoTermChargeCodes(defaultCodes[i++], "DEQ", "CNR", "CNR", "CNR", "CNR", "CNR", "CNR", "CNE", "CNE", "CNE");
			AssertIncoTermChargeCodes(defaultCodes[i++], "DES", "CNR", "CNR", "CNR", "CNR", "CNR", "CNE", "CNE", "CNE", "CNE");
			AssertIncoTermChargeCodes(defaultCodes[i++], "DPU", "CNR", "CNR", "CNR", "CNR", "CNR", "CNR", "CNR", "CNE", "CNE");
			AssertIncoTermChargeCodes(defaultCodes[i++], "EXW", "CNE", "CNE", "CNE", "CNE", "CNE", "CNE", "CNE", "CNE", "CNE");
			AssertIncoTermChargeCodes(defaultCodes[i++], "FAS", "CNR", "CNR", "CNE", "CNE", "CNE", "CNE", "CNE", "CNE", "CNE");
			AssertIncoTermChargeCodes(defaultCodes[i++], "FC1", "CNR", "CNR", "CNR", "CNE", "CNE", "CNE", "CNE", "CNE", "CNE");
			AssertIncoTermChargeCodes(defaultCodes[i++], "FC2", "CNR", "CNE", "CNE", "CNE", "CNE", "CNE", "CNE", "CNE", "CNE");
			AssertIncoTermChargeCodes(defaultCodes[i++], "FCA", "CNR", "CNR", "CNE", "CNE", "CNE", "CNE", "CNE", "CNE", "CNE");
			AssertIncoTermChargeCodes(defaultCodes[i++], "FOB", "CNR", "CNR", "CNR", "CNE", "CNE", "CNE", "CNE", "CNE", "CNE");
		}

		void AssertIncoTermChargeCodes(IncoTermChargeCodes incoTermChargeCodes, string incoTerm, string originBrokerage, string origin, string loading, string freight, string insurance, string unloading, string destination, string brokerage, string customsDuty)
		{
			AssertEquals("IncoTerm", incoTerm, incoTermChargeCodes.IncoTerm);
			AssertEquals(incoTermChargeCodes.IncoTerm + " Origin", originBrokerage, incoTermChargeCodes.OriginBrokerage);
			AssertEquals(incoTermChargeCodes.IncoTerm + " Origin", origin, incoTermChargeCodes.Origin);
			AssertEquals(incoTermChargeCodes.IncoTerm + " Loading", loading, incoTermChargeCodes.Loading);
			AssertEquals(incoTermChargeCodes.IncoTerm + " Freight", freight, incoTermChargeCodes.Freight);
			AssertEquals(incoTermChargeCodes.IncoTerm + " Insurance", insurance, incoTermChargeCodes.Insurance);
			AssertEquals(incoTermChargeCodes.IncoTerm + " Unloading", unloading, incoTermChargeCodes.Unloading);
			AssertEquals(incoTermChargeCodes.IncoTerm + " Destination", destination, incoTermChargeCodes.Destination);
			AssertEquals(incoTermChargeCodes.IncoTerm + " Brokerage", brokerage, incoTermChargeCodes.Brokerage);
			AssertEquals(incoTermChargeCodes.IncoTerm + " CustomsDuty", customsDuty, incoTermChargeCodes.CustomsDuty);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override IncoTermChargeCodesCollection GetCollectionToTest()
		{
			return new IncoTermChargeCodesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new IncoTermChargeCodes();
		}

		#endregion
	}
}
