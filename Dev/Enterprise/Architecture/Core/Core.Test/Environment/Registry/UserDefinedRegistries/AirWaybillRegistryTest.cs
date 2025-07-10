using System;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class AirWaybillRegistryTest : TransactionedTestCase
	{
		public void TestIssuingCarrierAgentName()
		{
			Registry.RawRegistry.IssuingCarrierAgentName.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "test");
			AssertEquals("IssuingCarrierAgentName", "test", Registry.Freight.AirWaybill.IssuingCarrierAgentName);
		}

		public void TestIssuingCarrierAgentCity()
		{
			Registry.RawRegistry.IssuingCarrierAgentCity.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "test");
			AssertEquals("IssuingCarrierAgentCity", "test", Registry.Freight.AirWaybill.IssuingCarrierAgentCity);
		}

		public void TestIssuingCarrierAgentIATACode()
		{
			Registry.RawRegistry.IssuingCarrierAgentIATACode.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "test");
			AssertEquals("IssuingCarrierAgentIATACode", "test", Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode);
		}

		public void TestIssuingCarrierAgentAccountNumber()
		{
			Registry.RawRegistry.IssuingCarrierAgentAccountNumber.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "test");
			AssertEquals("IssuingCarrierAgentAccountNumber", "test", Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber);
		}

		public void TestMAWBPaperType()
		{
			AssertEquals("PaperType should be string", typeof(string), Registry.Freight.AirWaybill.MAWBPaperType.GetType());
			AssertEquals("Default PaperType", Constants.AWB.PaperTypes.Iata, Registry.Freight.AirWaybill.MAWBPaperType);
		}

		public void TestHAWBPaperType()
		{
			AssertEquals("PaperType should be string", typeof(string), Registry.Freight.AirWaybill.HAWBPaperType.GetType());
			AssertEquals("Default PaperType", Constants.AWB.PaperTypes.Iata, Registry.Freight.AirWaybill.HAWBPaperType);
		}

		public void TestAllowAutoCalculationOfTax()
		{
			Registry.RawRegistry.AllowAutoCalculationOfTax.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("AllowAutoCalculationOfTax", false, Registry.Freight.AirWaybill.AllowAutoCalculationOfTax);
		}

		public void TestAllowAsAgreed()
		{
			Registry.RawRegistry.AllowAsAgreed.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("AllowAsAgreed", false, Registry.Freight.AirWaybill.AllowAsAgreed);
		}

		public void TestSelectFHLByDefault()
		{
			Registry.RawRegistry.SelectFHLByDefault.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("SelectFHLByDefault", false, Registry.Freight.AirWaybill.SelectFHLByDefault);
		}

		public void TestPrintAsAgreedOnFirstSetHAWB()
		{
			Registry.RawRegistry.PrintAsAgreedOnFirstSetHAWB.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.AWB.AsAgreedTypes.Codes.All);
			AssertEquals("PrintAsAgreedOnFirstSet", Constants.AWB.AsAgreedTypes.Codes.All, Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetHAWB);
		}

		public void TestPrintAsAgreedOnSecondSetHAWB()
		{
			Registry.RawRegistry.PrintAsAgreedOnSecondSetHAWB.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.AWB.AsAgreedTypes.Codes.All);
			AssertEquals("PrintAsAgreedOnSecondSet", Constants.AWB.AsAgreedTypes.Codes.All, Registry.Freight.AirWaybill.PrintAsAgreedOnSecondSetHAWB);
		}

		public void TestPrintAsAgreedOnFirstSetMAWB()
		{
			Registry.RawRegistry.PrintAsAgreedOnFirstSetMAWB.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.AWB.AsAgreedTypes.Codes.All);
			AssertEquals("PrintAsAgreedOnFirstSet", Constants.AWB.AsAgreedTypes.Codes.All, Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetMAWB);
		}

		public void TestPrintAsAgreedOnSecondSetMAWB()
		{
			Registry.RawRegistry.PrintAsAgreedOnSecondSetMAWB.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.AWB.AsAgreedTypes.Codes.All);
			AssertEquals("PrintAsAgreedOnSecondSet", Constants.AWB.AsAgreedTypes.Codes.All, Registry.Freight.AirWaybill.PrintAsAgreedOnSecondSetMAWB);
		}

		public void TestShowChargeCodeForOtherChargesInHAWBScreen()
		{
			AssertEquals("Default should be false", false, Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen);
			Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen = true;
			AssertEquals("Should now be set to true", true, Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen);
		}

		public void TestAllowShortGoodsDescriptionOverrideforFHL()
		{
			AssertEquals("Default should be false", false, Registry.Freight.AirWaybill.AllowShortGoodsDescriptionOverrideforFHL);
			Registry.Freight.AirWaybill.AllowShortGoodsDescriptionOverrideforFHL = true;
			AssertEquals("Should now be set to true", true, Registry.Freight.AirWaybill.AllowShortGoodsDescriptionOverrideforFHL);
		}

		public void TestShipperAddressDefaultsTo()
		{
			Registry.RawRegistry.ShipperAddressDefaultsTo.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, OrgConstants.AddressType.Documentary);
			AssertEquals(OrgConstants.AddressType.Documentary, Registry.Freight.AirWaybill.ShipperAddressDefaultsTo);
		}

		public void TestConsigneeAddressDefaultsTo()
		{
			Registry.RawRegistry.ConsigneeAddressDefaultsTo.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, OrgConstants.AddressType.Documentary);
			AssertEquals(OrgConstants.AddressType.Documentary, Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo);
		}

		public void TestAirWaybillHAWBWeightAndVolumeDisplay()
		{
			AssertEquals("AirWaybillHAWBWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.Freight.AirWaybill.AirWaybillHAWBWeightAndVolumeDisplay);
			Registry.RawRegistry.AirWaybillHAWBWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("AirWaybillHAWBWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Client, Registry.Freight.AirWaybill.AirWaybillHAWBWeightAndVolumeDisplay);

			using (Registry.RawRegistry.AirWaybillHAWBWeightAndVolumeDisplay.DataType.SuspendValidation())
			{
				Registry.RawRegistry.AirWaybillHAWBWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			AssertEquals("AirWaybillHAWBWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.Freight.AirWaybill.AirWaybillHAWBWeightAndVolumeDisplay);
		}

		public void TestHAWBDimensionsDefault()
		{
			Registry.RawRegistry.HAWBDimensionsDefault.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.AWB.Dimensions.PKS);
			AssertEquals("Constants.AWB.Dimensions.PKS", Constants.AWB.Dimensions.PKS, Registry.Freight.AirWaybill.HAWBDimensionsDefault);
		}

		public void TestHAWBDefaultShipperText()
		{
			Registry.RawRegistry.HAWBDefaultShipperText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "abc");
			AssertEquals("HAWBDefaultShipperText", "abc", Registry.Freight.AirWaybill.HAWBDefaultShipperText);
		}

		public void TestHAWBDefaultCarrierText()
		{
			Registry.RawRegistry.HAWBDefaultCarrierText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "abc");
			AssertEquals("HAWBDefaultCarrierText", "abc", Registry.Freight.AirWaybill.HAWBDefaultCarrierText);
		}

		public void TestMAWBDimensionsDefault()
		{
			Registry.RawRegistry.MAWBDimensionsDefault.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.AWB.Dimensions.PKS);
			AssertEquals("Constants.AWB.Dimensions.PKS", Constants.AWB.Dimensions.PKS, Registry.Freight.AirWaybill.MAWBDimensionsDefault);
		}

		public void TestMAWBDefaultShipperText()
		{
			Registry.RawRegistry.MAWBDefaultShipperText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "abc");
			AssertEquals("MAWBDefaultShipperText", "abc", Registry.Freight.AirWaybill.MAWBDefaultShipperText);
		}

		public void TestMAWBDefaultCarrierText()
		{
			Registry.RawRegistry.MAWBDefaultCarrierText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "abc");
			AssertEquals("MAWBDefaultCarrierText", "abc", Registry.Freight.AirWaybill.MAWBDefaultCarrierText);
		}
		public void TestAirWaybillMAWBWeightAndVolumeDisplay()
		{
			AssertEquals("AirWaybillMAWBWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.Freight.AirWaybill.AirWaybillMAWBWeightAndVolumeDisplay);
			Registry.RawRegistry.AirWaybillMAWBWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("AirWaybillMAWBWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Client, Registry.Freight.AirWaybill.AirWaybillMAWBWeightAndVolumeDisplay);

			using (Registry.RawRegistry.AirWaybillMAWBWeightAndVolumeDisplay.DataType.SuspendValidation())
			{
				Registry.RawRegistry.AirWaybillMAWBWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			AssertEquals("AirWaybillMAWBWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.Freight.AirWaybill.AirWaybillMAWBWeightAndVolumeDisplay);
		}

		public void TestHAWBLogo()
		{
			AssertNull(Registry.Freight.AirWaybill.HAWBLogo);
		}

		public void TestGetHAWBDocumentTitles()
		{
			var docTitles = Registry.Freight.AirWaybill.GetHAWBDocumentTitles();
			AssertEquals("Original1.Name", "Original 1 - (for Issuing Carrier)", docTitles.Original1.Name);
			AssertEquals("Original1.Title", "Original 1 - (for Issuing Carrier)", docTitles.Original1.Title);
			AssertEquals("Original1.Printed", true, docTitles.Original1.Printed);

			AssertEquals("Original2.Name", "Original 2 - (for Consignee)", docTitles.Original2.Name);
			AssertEquals("Original2.Title", "Original 2 - (for Consignee)", docTitles.Original2.Title);
			AssertEquals("Original2.Printed", true, docTitles.Original2.Printed);

			AssertEquals("Original3.Name", "Original 3 - (for Shipper)", docTitles.Original3.Name);
			AssertEquals("Original3.Title", "Original 3 - (for Shipper)", docTitles.Original3.Title);
			AssertEquals("Original3.Printed", true, docTitles.Original3.Printed);

			AssertEquals("Copy4.Name", "Copy 4 - (Delivery Receipt)", docTitles.Copy4.Name);
			AssertEquals("Copy4.Title", "Copy 4 - (Delivery Receipt)", docTitles.Copy4.Title);
			AssertEquals("Copy4.Printed", true, docTitles.Copy4.Printed);

			AssertEquals("Copy5.Name", "Copy 5 - (Extra Copy)", docTitles.Copy5.Name);
			AssertEquals("Copy5.Title", "Copy 5 - (Extra Copy)", docTitles.Copy5.Title);
			AssertEquals("Copy5.Printed", true, docTitles.Copy5.Printed);

			AssertEquals("Copy6.Name", "Copy 6 - (Extra Copy)", docTitles.Copy6.Name);
			AssertEquals("Copy6.Title", "Copy 6 - (Extra Copy)", docTitles.Copy6.Title);
			AssertEquals("Copy6.Printed", true, docTitles.Copy6.Printed);

			AssertEquals("Copy7.Name", "Copy 7 - (Extra Copy)", docTitles.Copy7.Name);
			AssertEquals("Copy7.Title", "Copy 7 - (Extra Copy)", docTitles.Copy7.Title);
			AssertEquals("Copy7.Printed", true, docTitles.Copy7.Printed);

			AssertEquals("Copy8.Name", "Copy 8 - (for Agent)", docTitles.Copy8.Name);
			AssertEquals("Copy8.Title", "Copy 8 - (for Agent)", docTitles.Copy8.Title);
			AssertEquals("Copy8.Printed", true, docTitles.Copy8.Printed);
		}

		public void TestGetMAWBDocumentTitles()
		{
			var docTitles = Registry.Freight.AirWaybill.GetMAWBDocumentTitles();
			AssertEquals("Original1.Name", "Original 1 - (for Issuing Carrier)", docTitles.Original1.Name);
			AssertEquals("Original1.Title", "Original 1 - (for Issuing Carrier)", docTitles.Original1.Title);
			AssertEquals("Original1.Printed", true, docTitles.Original1.Printed);

			AssertEquals("Original2.Name", "Original 2 - (for Consignee)", docTitles.Original2.Name);
			AssertEquals("Original2.Title", "Original 2 - (for Consignee)", docTitles.Original2.Title);
			AssertEquals("Original2.Printed", true, docTitles.Original2.Printed);

			AssertEquals("Original3.Name", "Original 3 - (for Shipper)", docTitles.Original3.Name);
			AssertEquals("Original3.Title", "Original 3 - (for Shipper)", docTitles.Original3.Title);
			AssertEquals("Original3.Printed", true, docTitles.Original3.Printed);

			AssertEquals("Copy4.Name", "Copy 4 - (Delivery Receipt)", docTitles.Copy4.Name);
			AssertEquals("Copy4.Title", "Copy 4 - (Delivery Receipt)", docTitles.Copy4.Title);
			AssertEquals("Copy4.Printed", true, docTitles.Copy4.Printed);

			AssertEquals("Copy5.Name", "Copy 5 - (Extra Copy)", docTitles.Copy5.Name);
			AssertEquals("Copy5.Title", "Copy 5 - (Extra Copy)", docTitles.Copy5.Title);
			AssertEquals("Copy5.Printed", true, docTitles.Copy5.Printed);

			AssertEquals("Copy6.Name", "Copy 6 - (Extra Copy)", docTitles.Copy6.Name);
			AssertEquals("Copy6.Title", "Copy 6 - (Extra Copy)", docTitles.Copy6.Title);
			AssertEquals("Copy6.Printed", true, docTitles.Copy6.Printed);

			AssertEquals("Copy7.Name", "Copy 7 - (Extra Copy)", docTitles.Copy7.Name);
			AssertEquals("Copy7.Title", "Copy 7 - (Extra Copy)", docTitles.Copy7.Title);
			AssertEquals("Copy7.Printed", true, docTitles.Copy7.Printed);

			AssertEquals("Copy8.Name", "Copy 8 - (for Agent)", docTitles.Copy8.Name);
			AssertEquals("Copy8.Title", "Copy 8 - (for Agent)", docTitles.Copy8.Title);
			AssertEquals("Copy8.Printed", true, docTitles.Copy8.Printed);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Registry = new DataRegistry();
		}

		DataRegistry Registry;

		#endregion
	}
}
