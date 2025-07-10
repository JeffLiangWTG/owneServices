using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocExportCustomsManifestLine))]
	sealed class DocExportCustomsManifestLineTest : DocumentWrapperTestCase
	{
		public void TestWeight()
		{
			Line.EL_Weight = 0m;
			Line.EL_WeightUQ = Constants.Weight.Kilograms;

			AssertEquals(0m, Wrapper.Weight);
			AssertEquals(Constants.Weight.Kilograms, Wrapper.WeightUnit);

			Line.EL_Weight = 0.25m;
			Line.EL_WeightUQ = Constants.Weight.Tonnes;

			AssertEquals(0.25m, Wrapper.Weight);
			AssertEquals(Constants.Weight.Tonnes, Wrapper.WeightUnit);
		}

		public void TestVolume()
		{
			Line.EL_Volume = 0m;
			Line.EL_VolumeUQ = Constants.Volume.Litre;

			AssertEquals(0m, Wrapper.Volume);
			AssertEquals(Constants.Volume.Litre, Wrapper.VolumeUnit);

			Line.EL_Volume = 0.25m;
			Line.EL_VolumeUQ = Constants.Volume.CubicMetres;

			AssertEquals(0.25m, Wrapper.Volume);
			AssertEquals(Constants.Volume.CubicMetres, Wrapper.VolumeUnit);
		}

		public void TestAirWayBill()
		{
			Line.EL_AirWayBill = "08155555555";
			AssertEquals("08155555555", Wrapper.AirWayBill);

			Line.EL_AirWayBill = "08166666666";
			AssertEquals("08166666666", Wrapper.AirWayBill);
		}

		public void TestGoodsDescription()
		{
			Line.EL_GoodsDescription = "";
			AssertEquals("", Wrapper.GoodsDescription);

			Line.EL_GoodsDescription = "Goods Description";
			AssertEquals("Goods Description", Wrapper.GoodsDescription);
		}

		public void TestPiecesManifested()
		{
			Line.EL_NumberOfPackages = 4;
			AssertEquals(4, Wrapper.PiecesManifested);

			Line.EL_NumberOfPackages = 7;
			AssertEquals(7, Wrapper.PiecesManifested);
		}

		public void TestOriginAndDestination()
		{
			Line.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			Line.ConsignorDocumentaryAddress.E2_City = "Brisbane";
			Line.ConsignorDocumentaryAddress.E2_RN_NKCountryCode = "AU";

			Line.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			Line.ConsigneeDocumentaryAddress.E2_City = "Singapore";
			Line.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = "SG";

			AssertEquals("AUBNE", Wrapper.Origin.Code);
			AssertEquals("SGSIN", Wrapper.Destination.Code);
		}

		public void TestConsignorDocumentaryAddress()
		{
			Line.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			Line.ConsignorDocumentaryAddress.E2_CompanyName = "Bob";
			AssertEquals("Bob", Wrapper.ConsignorDocumentaryAddress.CompanyName);
		}

		public void TestConsigneeDocumentaryAddress()
		{
			Line.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			Line.ConsigneeDocumentaryAddress.E2_CompanyName = "Bob";
			AssertEquals("Bob", Wrapper.ConsigneeDocumentaryAddress.CompanyName);
		}

		#region Implementation

		#region Line

		ExportCustomsManifestLines Line
		{
			get { return line ?? (line = Factory.New<ExportCustomsManifestHeader>().Lines.AddNew()); }
		}

		ExportCustomsManifestLines line;

		#endregion

		#region Wrapper

		public DocExportCustomsManifestLine Wrapper
		{
			get { return DocExportCustomsManifestLine.New(Line, Factory); }
		}

		#endregion

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			ExportCustomsManifestLines line = Factory.New<ExportCustomsManifestLines>();

			return new DocumentWrapper[]
			{
				DocExportCustomsManifestLine.New(line, Factory)
			};
		}

		#endregion
	}
}
