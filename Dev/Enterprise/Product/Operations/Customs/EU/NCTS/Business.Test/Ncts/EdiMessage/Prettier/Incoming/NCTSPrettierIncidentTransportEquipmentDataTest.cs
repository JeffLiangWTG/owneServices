using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NCTSPrettierIncidentTransportEquipmentDataTest : TestCase
	{
		public void TestTransportEquipmentsSequenceNumber() => AssertEquals(1, prettierIncidentTransportEquipmentData.SequenceNumber);
		public void TestTransportEquipmentsNumberOfSeals() => AssertEquals(1, prettierIncidentTransportEquipmentData.NumberOfSeals);
		public void TestTransportEquipmentsContainerIdentificationNumber() => AssertEquals("WGPCGR", prettierIncidentTransportEquipmentData.ContainerIdentificationNumber);
		public void TestTransportEquipmentsSealsSequenceNumber() => AssertEquals(1, prettierIncidentTransportEquipmentData.Seals.FirstOrDefault().SequenceNumber);
		public void TestTransportEquipmentsSealsIdentifier() => AssertEquals("1234", prettierIncidentTransportEquipmentData.Seals.FirstOrDefault().Identifier);
		public void TestTransportEquipmentsGoodsReferencesSequenceNumber() => AssertNull(prettierIncidentTransportEquipmentData.GoodsReferences?.FirstOrDefault()?.SequenceNumber);
		public void TestTransportEquipmentsGoodsReferencesDeclarationGoodsItemNumber() => AssertNull(prettierIncidentTransportEquipmentData.GoodsReferences?.FirstOrDefault()?.DeclarationGoodsItemNumber);
		protected override void SetUp()
		{
			base.SetUp();

			prettierIncidentTransportEquipmentData = new NCTSPrettierIncidentTransportEquipmentData(transportEquipmentSequenceNumber: "1",
					transportEquipmentContainerIdentificationNumber: "WGPCGR",
					transportEquipmentNumberOfSeals: "1",
					transportEquipmentSeals: new List<INCTSSeal> { new NCTSPrettierSealData(sequenceNumber: "1", identifier: "1234") },
					goodsReferences: Enumerable.Empty<INCTSGoodsReference>().ToList());
		}
		NCTSPrettierIncidentTransportEquipmentData prettierIncidentTransportEquipmentData;
	}
}
