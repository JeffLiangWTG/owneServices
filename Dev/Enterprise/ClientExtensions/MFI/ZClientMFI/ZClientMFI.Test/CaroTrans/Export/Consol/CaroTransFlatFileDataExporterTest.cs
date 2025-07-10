using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.MFI.CaroTrans.Export.Testing
{
	[TestedType(typeof(CaroTransFlatFileDataExporter))]
	public class CaroTransFlatFileDataExporterTest : FlatFileDataExporterTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CaroTransFlatFileDataExporter(Factory);
		}

		public void TestFlatFileFormatIsClientSpecific()
		{
			FlatFileDataExporter exporter = GetDataExporter();
			AssertEquals("Client specific file extension type expected, CaroTrans have their own extension", FileExtensionType.ClientSpecific, exporter.FileExtensionType);
		}

		public override FlatFileDataExporter GetDataExporter()
		{
			return new PopulateExportInstructionsTestClass(Factory);
		}

		public override IBusinessObjectCollection GetPopulatedCollectionToSaveAndExport()
		{
			MainFormConsolCollection collection = new MainFormConsolCollection(Factory);
			ForwardingConsol minimalistConsol = Factory.New<ForwardingConsol>();
			minimalistConsol.AutomaticallyUpdatePackLineContainers = false;
			minimalistConsol.JK_AgentType = "AGT";
			minimalistConsol.JK_TransportMode = "SEA";
			minimalistConsol.JK_UniqueConsignRef = "C00001001";
			ForwardingConsol populatedConsol = Factory.New<ForwardingConsol>();
			populatedConsol.AutomaticallyUpdatePackLineContainers = false;
			populatedConsol.JK_TransportMode = "SEA";
			populatedConsol.JK_UniqueConsignRef = "C00001002";
			populatedConsol.JK_MasterBillNum = "12345";
			populatedConsol.SetDefaultSendingForwarderAddress(Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "ABIGAS")));
			populatedConsol.SetDefaultReceivingForwarderAddress(Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "BARGAL")));
			populatedConsol.JK_RL_NKDischargePort = "CNPEK";
			CommonContainer container1 = populatedConsol.Containers.AddNew();
			container1.JC_ContainerNum = "C111";
			CommonContainer container2 = populatedConsol.Containers.AddNew();
			container2.JC_ContainerNum = "C222";
			CommonContainer container3 = populatedConsol.Containers.AddNew();
			container3.JC_ContainerNum = "C333";
			CommonShipment shipment1 = populatedConsol.Shipments.AddNew();
			shipment1.ConsigneePK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_Code, "ROHAUS")).PK;
			shipment1.ConsignorPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_Code, "KABTEX")).PK;
			shipment1.JS_HouseBill = "11111";
			shipment1.JS_RL_NKDestination = "CNPEK";
			PackLine line1S1 = shipment1.OuterPackLines.AddNew();
			line1S1.Containers.Add(container1);
			line1S1.JL_ActualVolume = 3;
			line1S1.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			line1S1.JL_ActualWeight = 10;
			line1S1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			PackLine line2S1 = shipment1.OuterPackLines.AddNew();
			line2S1.Containers.Add(container2);
			line2S1.JL_ActualVolume = 2;
			line2S1.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			line2S1.JL_ActualWeight = 5;
			line2S1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			CommonShipment shipment2 = populatedConsol.Shipments.AddNew();
			shipment2.ConsigneePK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_Code, "GATCON")).PK;
			shipment2.ConsignorPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_Code, "MANPTY")).PK;
			shipment2.JS_HouseBill = "22222";
			PackLine line1S2 = shipment2.OuterPackLines.AddNew();
			line1S2.Containers.Add(container2);
			line1S2.JL_ActualVolume = 1;
			line1S2.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			line1S2.JL_ActualWeight = 1;
			line1S2.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			CommonShipment shipment3 = populatedConsol.Shipments.AddNew();
			shipment3.ConsigneePK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_Code, "LAMPUB")).PK;
			shipment3.ConsignorPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_Code, "LILROY")).PK;
			shipment3.JS_HouseBill = "33333";
			PackLine line1S3 = shipment3.OuterPackLines.AddNew();
			line1S3.Containers.Add(container3);
			line1S3.JL_ActualVolume = 10;
			line1S3.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			line1S3.JL_ActualWeight = 10;
			line1S3.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			PackLine line2S3 = shipment3.OuterPackLines.AddNew();
			line2S3.Containers.Add(container2);
			line2S3.JL_ActualVolume = 5;
			line2S3.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			line2S3.JL_ActualWeight = 5;
			line2S3.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			collection.Add(minimalistConsol);
			collection.Add(populatedConsol);
			Factory.Save();
			return collection;
		}

		[TestDate(2006, 3, 20, 12, 34, 45)]
		public void TestSetInstructions()
		{
			PopulateExportInstructionsTestClass exporter = new PopulateExportInstructionsTestClass(Factory);
			CollectionWrapperBusinessObjectReader reader = new CollectionWrapperBusinessObjectReader(GetPopulatedCollectionToSaveAndExport());
			var fileExtensionList = new CodeDescriptionPairList();
			fileExtensionList.AddPair(Core.Constants.CountryCodes.China, "CTI");
			MFIDataRegistry.Instance.CaroTransExportFileExtensionItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fileExtensionList);
			ZString expectedFile = Path.Combine(Env.TempPath, "COPEK060320123445.CTI");
			try
			{
				exporter.Export(reader, new NotificationBuffer());
				AssertEquals("File should exist", true, File.Exists(expectedFile));
			}
			finally
			{
				DeleteIfExists(expectedFile);
			}
		}

		class PopulateExportInstructionsTestClass : CaroTransFlatFileDataExporter
		{
			public PopulateExportInstructionsTestClass(BusinessObjectFactory factory) : base(factory)
			{
			}

			public new void SetInstructions(ExportInstructions instructions, IFlatFileConverter converter)
			{
				base.SetInstructions(instructions, converter);
				instructions.BasePath = Env.TempPath;
			}
		}
	}
}
