using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.CA.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	partial class DeclarationDataObjectWriterTest
	{
		public void TestCargoControlNumberMappings()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "MB1234567";
			var ccn0 = declaration.ReleaseStatuses.AddNew();
			ccn0.RL_CargoControlNumber = "12345678";
			var ccn1 = declaration.ReleaseStatuses.AddNew();
			ccn1.RL_CargoControlNumber = "245677890";
			ccn1.RL_Bill = declaration.Bills[0].PK;

			Factory.SaveForTesting();

			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);

			CombineAssertions(delegate
			{
				AssertEquals(2, declarationData.AddInfoGroupCollection.Count);

				var addInfoGroupData0 = declarationData.AddInfoGroupCollection[0];
				AssertEquals(CusAddInfoTypeAttribute.Codes.CACCN, addInfoGroupData0.Type.Code.Value);
				var subAddInfoGroupCollection0 = addInfoGroupData0.AddInfoGroupCollection;
				AssertEquals(1, subAddInfoGroupCollection0.Count);
				AssertEquals(3, subAddInfoGroupCollection0[0].AddInfoCollection.Count);
				AssertEquals("12345678", subAddInfoGroupCollection0[0].AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.CargoControlNumber.CCNumber));
				AssertEquals(ZString.Empty, subAddInfoGroupCollection0[0].AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.CargoControlNumber.BillType));
				AssertEquals(ZString.Empty, subAddInfoGroupCollection0[0].AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.CargoControlNumber.BillNumber));

				var addInfoGroupData1 = declarationData.AddInfoGroupCollection[1];
				AssertEquals(CusAddInfoTypeAttribute.Codes.CACCN, addInfoGroupData1.Type.Code.Value);
				var subAddInfoGroupCollection1 = addInfoGroupData1.AddInfoGroupCollection;
				AssertEquals(1, subAddInfoGroupCollection1.Count);
				AssertEquals(3, subAddInfoGroupCollection1[0].AddInfoCollection.Count);
				AssertEquals("245677890", subAddInfoGroupCollection1[0].AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.CargoControlNumber.CCNumber));
				AssertEquals("MB", subAddInfoGroupCollection1[0].AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.CargoControlNumber.BillType));
				AssertEquals("MB1234567", subAddInfoGroupCollection1[0].AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.CargoControlNumber.BillNumber));
			});
		}
	}
}
