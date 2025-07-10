using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AirCTOExportCustomsManifestLinesValidationTest : BusinessObjectValidationTestCase
	{
		public void TestOwnerPartyIDOrOwnerNameRequiredWhenExemptCode()
		{
			string expectedMessage = "Either a Goods Owner Party ID or an Owner Name is required.";
			AssertForAll(delegate
			{
				Line.EL_GoodsOwner = "";
				Line.EL_GoodsOwnerPartyID = "";
				AssertHasMessageError(Line.EL_GoodsOwnerPartyIDInfo, expectedMessage);
				AssertHasMessageError(Line.EL_GoodsOwnerInfo, expectedMessage);

				Line.EL_GoodsOwner = "foo";
				AssertNoNotifications(Line.EL_GoodsOwnerPartyIDInfo);
				AssertNoNotifications(Line.EL_GoodsOwnerInfo);

				Line.EL_GoodsOwner = "";
				AssertHasMessageError(Line.EL_GoodsOwnerPartyIDInfo, expectedMessage);
				AssertHasMessageError(Line.EL_GoodsOwnerInfo, expectedMessage);

				Line.EL_GoodsOwnerPartyID = "bar";
				AssertNoNotifications(Line.EL_GoodsOwnerPartyIDInfo);
				AssertNoNotifications(Line.EL_GoodsOwnerInfo);
			});
		}

		public void TestGoodsDescriptionRequiredWhenExemptCode()
		{
			AssertForAllCANs(delegate
			{
				AssertNoNotifications(Line.EL_GoodsDescriptionInfo);
			});

			AssertForAllExemptions(delegate
			{
				AssertHasMessageError(Line.EL_GoodsDescriptionInfo, "A Goods Description is required when using an Exemption Code.");
			});
		}

		public void TestCountryOfDestinationRequiredWhenExemptCode()
		{
			AssertForAllCANs(delegate
			{
				AssertNoNotifications(Line.EL_RN_NKCountryOfDestinationInfo);
			});

			AssertForAllExemptions(delegate
			{
				AssertHasMessageError(Line.EL_RN_NKCountryOfDestinationInfo, "A Country/Region of Destination is required when using an Exemption Code.");
			});
		}

		public void TestAirWayBillRequired()
		{
			AssertHasMessageError(Line.EL_AirWayBillInfo, "You have not entered a value.");

			Line.EL_AirWayBill = "12121212122";
			AssertNoNotifications(Line.EL_AirWayBillInfo);
		}

		delegate void Assertions();

		readonly CMRExportExemptionCodesList exemptionList = new CMRExportExemptionCodesList();
		readonly CANTypeList canTypeList = new CANTypeList();

		void AssertForAll(Assertions assertions)
		{
			AssertForAllCANs(assertions);
			AssertForAllExemptions(assertions);
		}

		void AssertForAllExemptions(Assertions assertions)
		{
			foreach (CMRExportExemptionCodes exemptCode in exemptionList)
			{
				Line.EL_TypeOfCAN = exemptCode.Code;
				assertions();
			}
		}

		void AssertForAllCANs(Assertions assertions)
		{
			foreach (ICodeDescription canType in canTypeList)
			{
				if (!exemptionList.Contains(canType))
				{
					Line.EL_TypeOfCAN = canType.Code;
					assertions();
				}
			}
		}

		ExportCustomsManifestLines line;
		public ExportCustomsManifestLines Line
		{
			get
			{
				if (line == null)
				{
					var header = Factory.New<AirCTOExportCustomsManifestHeader>();
					header.ED_ManifestType = AirManifestTypeList.Codes.CtoReceivalRemovalStandAlone;
					line = header.Lines.AddNew();
					line.Validation.ValidateAll();
				}
				return line;
			}
		}
	}
}
