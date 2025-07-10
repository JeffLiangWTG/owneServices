using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using CACustoms = Enterprise.Integration.Customs.CA;

namespace Enterprise.Customs.CA.DIF.Business.Testing
{
	public sealed class TestHelper
	{
		public TestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public BusinessObject GetJobDeclaration()
		{
			var importer = factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "IMPORTER";
			var cusCode = importer.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CACodeTypes.BusinessNumberForImportExport;
			cusCode.OK_CustomsRegNo = "BRM001";

			var result = (BusinessObject)factory.New<CACustoms.IJobDeclaration>();
			result[JobDeclarationSchema.JE_MessageType.Name] = "IMP";
			result[JobDeclarationSchema.JE_DeclarationReference] = "B00001000";
			result[JobDeclarationSchema.JE_OH_Importer] = importer.PK;

			return result;
		}

		public BusinessObject GetCusPermitHeader()
		{
			var result = (BusinessObject)factory.New<CACustoms.ICusPermitHeader>();
			result[CusPermitHeaderSchema.CPH_Number.Name] = "Per123";
			result[CusPermitHeaderSchema.CPH_StartDate] = ZDate.BrettsBirthday;
			result[CusPermitHeaderSchema.CPH_EndDate] = new ZDate(2017, 1, 1);

			return result;
		}
	}
}
