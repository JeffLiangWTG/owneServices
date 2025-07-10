using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class ChargeCodeToGlobalChargeCodeMapTest : TestCaseWithFactory
	{
		TestObjectCreator fTestObjectCreator;

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}

		public void Setup()
		{
		}

		public void TestLoadingOfOneChargeCodeAndOneUnmappedChargeCode()
		{
			// Arrange
			var chargeCode = TestObjectCreator.CreateChargeCode("CC");
			var unMappedchargeCode = TestObjectCreator.CreateChargeCode("UCC");
			var orgHeader = TestObjectCreator.CreateOrgHeader("OH1", false, true);
			InvoicingBase invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			invoice.AH_OH = orgHeader.PK;

			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = chargeCode.PK;

			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCodeMapWithPivot(
				"GCC", "DescriptionOfChargeCode", orgHeader.PK, chargeCode.PK, LedgerTypes.AccountsReceivable);

			Factory.Save();

			// Act
			var map = ChargeCodeToGlobalChargeCodeMap.GetARMap(Factory, orgHeader.PK, new[] { chargeCode.PK });

			// Assert
			AssertNotNull(map[chargeCode.PK]);
			AssertEquals(1, map[chargeCode.PK].Count());
			AssertEquals("GCC", map[chargeCode.PK].First().YG_Code);
			AssertNotNull(map[unMappedchargeCode.PK]);
			AssertEquals(0, map[unMappedchargeCode.PK].Count());
			AssertEquals(true, map.WasIncludedInQuery(chargeCode.PK));
			AssertEquals(false, map.WasIncludedInQuery(unMappedchargeCode.PK));
		}

		public void TestLoadingOfTwoChargeCodes()
		{
			// Arrange
			var chargeCode = TestObjectCreator.CreateChargeCode("CC");
			var orgHeader = TestObjectCreator.CreateOrgHeader("OH1", false, true);
			InvoicingBase invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			invoice.AH_OH = orgHeader.PK;

			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = chargeCode.PK;

			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCodeMapWithPivot(
				"GCC", "DescriptionOfChargeCode", orgHeader.PK, chargeCode.PK, LedgerTypes.AccountsReceivable);
			globalChargeCode = TestObjectCreator.CreateGlobalChargeCodeMapWithPivot(
				"GCC2", "DescriptionOfChargeCode", orgHeader.PK, chargeCode.PK, LedgerTypes.AccountsReceivable);

			Factory.Save();

			// Act
			var map = ChargeCodeToGlobalChargeCodeMap.GetARMap(Factory, orgHeader.PK, new[] { chargeCode.PK });

			// Assert
			AssertNotNull(map[chargeCode.PK]);
			AssertEquals(2, map[chargeCode.PK].Count());
			var globalChargeCodeNames = String.Join(",", map[chargeCode.PK].Select(x => x.YG_Code).OrderBy(x => x).ToArray());
			AssertEquals("GCC,GCC2", globalChargeCodeNames);
		}

		public void TestNotLoadAP()
		{
			// Arrange
			var chargeCode = TestObjectCreator.CreateChargeCode("CC");
			var orgHeader = TestObjectCreator.CreateOrgHeader("OH1", false, true);
			InvoicingBase invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			invoice.AH_OH = orgHeader.PK;

			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = chargeCode.PK;

			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCodeMapWithPivot(
				"GCC", "DescriptionOfChargeCode", orgHeader.PK, chargeCode.PK, LedgerTypes.AccountsPayable);

			Factory.Save();

			// Act
			var map = ChargeCodeToGlobalChargeCodeMap.GetARMap(Factory, orgHeader.PK, new[] { chargeCode.PK });

			// Assert
			AssertNotNull(map[chargeCode.PK]);
			AssertEquals(0, map[chargeCode.PK].Count());
		}
	}
}
