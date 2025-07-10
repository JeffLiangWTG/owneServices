using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocumentWrapper))]
	sealed class DocAirCTOExportTest : DocumentWrapperTestCase
	{
		#region TestWeight

		public void TestWeight()
		{
			Env.Registry.FreightWeightUnit = Constants.Weight.Kilograms;

			AssertEquals(0m, Wrapper.TotalWeight);
			AssertEquals(Constants.Weight.Kilograms, Wrapper.TotalWeightUnit);

			ExportCustomsManifestLines line1 = Header.Lines.AddNew();
			line1.EL_Weight = 1500m;
			line1.EL_VolumeUQ = Constants.Weight.Kilograms;

			ExportCustomsManifestLines line2 = Header.Lines.AddNew();
			line2.EL_Weight = 0.75m;
			line2.EL_WeightUQ = Constants.Weight.Tonnes;

			AssertEquals(2250m, Wrapper.TotalWeight);
			AssertEquals(Constants.Weight.Kilograms, Wrapper.TotalWeightUnit);

			Env.Registry.FreightWeightUnit = Constants.Weight.Tonnes;
			AssertEquals(2.25m, Wrapper.TotalWeight);
			AssertEquals(Constants.Weight.Tonnes, Wrapper.TotalWeightUnit);
		}

		#endregion

		#region TestVolume

		public void TestVolume()
		{
			Env.Registry.FreightVolumeUnit = Constants.Volume.CubicMetres;

			AssertEquals(0m, Wrapper.TotalVolume);
			AssertEquals(Constants.Volume.CubicMetres, Wrapper.TotalVolumeUnit);

			ExportCustomsManifestLines line1 = Header.Lines.AddNew();
			line1.EL_Volume = 1.5m;
			line1.EL_VolumeUQ = Constants.Volume.CubicMetres;

			ExportCustomsManifestLines line2 = Header.Lines.AddNew();
			line2.EL_Volume = 750m;
			line2.EL_VolumeUQ = Constants.Volume.Litre;

			AssertEquals(2.25m, Wrapper.TotalVolume);
			AssertEquals(Constants.Volume.CubicMetres, Wrapper.TotalVolumeUnit);

			Env.Registry.FreightVolumeUnit = Constants.Volume.Litre;
			AssertEquals(2250m, Wrapper.TotalVolume);
			AssertEquals(Constants.Volume.Litre, Wrapper.TotalVolumeUnit);
		}

		#endregion

		#region TestAirlineName

		public void TestAirlineName()
		{
			RefAirline qF = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, "QF"));
			RefAirline nZ = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, "NZ"));
			RefAirline xX = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, "XX"));

			AssertNotNull("precondition:", qF);
			AssertNotNull("precondition:", nZ);
			AssertNull("precondition:", xX);

			Header.ED_FlightNumber = "QF1234";
			AssertEquals("QF1234", Wrapper.FlightNo);
			AssertEquals(qF.RM_AirlineName1, Wrapper.AirlineName);

			Header.ED_FlightNumber = "NZ1234";
			AssertEquals(nZ.RM_AirlineName1, Wrapper.AirlineName);

			Header.ED_FlightNumber = "XX1234";
			AssertEquals("", Wrapper.AirlineName);
		}

		#endregion

		#region TestDepartureDate

		public void TestDepartureDate()
		{
			ZDateTime now = ZDateTime.Now;

			Header.ED_DepartureDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, Wrapper.DepartureDate);

			Header.ED_DepartureDate = now;
			AssertEquals(now, Wrapper.DepartureDate);
		}

		#endregion

		#region TestTotalPiecesManifested

		public void TestTotalPiecesManifested()
		{
			AssertEquals(0, Wrapper.TotalPiecesManifested);

			Header.Lines.AddNew().EL_NumberOfPackages = 5;
			Header.Lines.AddNew().EL_NumberOfPackages = 7;
			AssertEquals(12, Wrapper.TotalPiecesManifested);
		}

		#endregion

		#region TestLoadPort

		public void TestLoadPort()
		{
			Header.ED_RL_NKPortOfDeparture = "AUBNE";
			AssertEquals("AUBNE", Wrapper.LoadPort.Code);

			Header.ED_RL_NKPortOfDeparture = "NLAMS";
			AssertEquals("NLAMS", Wrapper.LoadPort.Code);
		}

		#endregion

		#region TestLines

		public void TestLines()
		{
			AssertEquals(0, Wrapper.Lines.Count);

			ExportCustomsManifestLines line = Header.Lines.AddNew();
			AssertEquals(1, Wrapper.Lines.Count);
			AssertEquals(line, Wrapper.Lines[0].WrappedObject);
		}

		#endregion

		#region Implementation

		#region Header

		AirCTOExportCustomsManifestHeader Header
		{
			get { return header ?? (header = Factory.New<AirCTOExportCustomsManifestHeader>()); }
		}
		AirCTOExportCustomsManifestHeader header;

		#endregion

		#region Wrapper

		DocAirCTOExport Wrapper
		{
			get { return DocAirCTOExport.New(Header, Factory); }
		}

		#endregion

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocAirCTOExport.New(Factory.New<AirCTOExportCustomsManifestHeader>(), Factory)
			};
		}

		#endregion
	}
}
