using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	partial class DeclarationDataObjectReaderTest
	{
		public void TestImportCargoReleaseNumber()
		{
			var declarationDataObject = SetupDeclaration(JobMessageTypeList.Codes.Import, B3EntryTypeList.Codes.Confirming);
			declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>()
			{
				new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
				{
					BillNumber = "MB12345678",
					BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master },
				},
				new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
				{
					BillNumber = "HB73418921356",
					BillType = new WayBillType() { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House },
				}
			});

			declarationDataObject.SetAddInfoGroupCollection(() => new List<AddInfoGroup>()
			{
				new AddInfoGroup()
				{
					Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.CACCN },
					AddInfoCollection = new List<AddInfo>()
					{
						new AddInfo() { Key = "CCNInfoNumber", Value = "123456789" }
					},
					AddInfoGroupCollection = new List<AddInfoGroup>()
					{
						new AddInfoGroup()
						{
							Type = new CodeDescriptionPair() { Code = Constants.AddInfoKeys.CargoControlNumber.Type },
							AddInfoCollection = new List<AddInfo>()
							{
								new AddInfo() { Key = Constants.AddInfoKeys.CargoControlNumber.CCNumber, Value = "123456789" },
								new AddInfo() { Key = Constants.AddInfoKeys.CargoControlNumber.BillType, Value = "MB" },
								new AddInfo() { Key = Constants.AddInfoKeys.CargoControlNumber.BillNumber, Value = ZString.Empty }
							}
						}
					}
				},
				new AddInfoGroup()
				{
					Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.CACCN },
					AddInfoCollection = new List<AddInfo>()
					{
						new AddInfo() { Key = "CCNInfoNumber", Value = "37234843271" }
					},
					AddInfoGroupCollection = new List<AddInfoGroup>()
					{
						new AddInfoGroup()
						{
							Type = new CodeDescriptionPair() { Code = Constants.AddInfoKeys.CargoControlNumber.Type },
							AddInfoCollection = new List<AddInfo>()
							{
								new AddInfo() { Key = Constants.AddInfoKeys.CargoControlNumber.CCNumber, Value = "37234843271" },
								new AddInfo() { Key = Constants.AddInfoKeys.CargoControlNumber.BillType, Value = "HB" },
								new AddInfo() { Key = Constants.AddInfoKeys.CargoControlNumber.BillNumber, Value = "HB73418921356" }
							}
						}
					}
				}
			});

			Factory.SaveForTesting();

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(delegate
			{
				var newDeclarationBO = new BusinessObjectFactory().Load<JobDeclaration>(declarationBO.PK);
				AssertEquals(2, newDeclarationBO.ReleaseStatuses.Count);

				var cargoControlNumber0 = newDeclarationBO.ReleaseStatuses[0];
				AssertEquals("123456789", cargoControlNumber0.RL_CargoControlNumber);
				AssertEquals(ZGuid.Empty, cargoControlNumber0.RL_Bill);

				var cargoControlNumber1 = newDeclarationBO.ReleaseStatuses[1];
				AssertEquals("37234843271", cargoControlNumber1.RL_CargoControlNumber);
				var houseBill = newDeclarationBO.Bills.OfType<Bill>().FirstOrDefault(x => x.CU_BillNum == "HB73418921356");
				AssertNotNull(houseBill);
				AssertEquals(houseBill.PK, cargoControlNumber1.RL_Bill);
			});
		}
	}
}
