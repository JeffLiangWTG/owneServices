using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.SADH;

namespace Enterprise.Customs.EU.Business.Testing
{
	internal class SADHFormDataValidationTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryType()
		{
			SADHData.D1_MessageType = MessageTypeList.Codes.Import;

			SADHData.D1_EntryType = EntryStyleListImport.Codes.ImportNormal;
			AssertNoMessageError(SADHData.D1_EntryTypeInfo, SADHFormDataValidation.D1_EntryTypeIsInvalid);

			SADHData.D1_EntryType = EntryStyleListExport.Codes.ExportNormal;
			AssertHasMessageError(SADHData.D1_EntryTypeInfo, SADHFormDataValidation.D1_EntryTypeIsInvalid);

			SADHData.D1_EntryType = "XX";
			AssertHasMessageError(SADHData.D1_EntryTypeInfo, SADHFormDataValidation.D1_EntryTypeIsInvalid);

			SADHData.D1_MessageType = MessageTypeList.Codes.Export;

			SADHData.D1_EntryType = EntryStyleListExport.Codes.ExportNormal;
			AssertNoMessageError(SADHData.D1_EntryTypeInfo, SADHFormDataValidation.D1_EntryTypeIsInvalid);

			SADHData.D1_EntryType = EntryStyleListImport.Codes.ImportNormal;
			AssertHasMessageError(SADHData.D1_EntryTypeInfo, SADHFormDataValidation.D1_EntryTypeIsInvalid);

			SADHData.D1_EntryType = "XX";
			AssertHasMessageError(SADHData.D1_EntryTypeInfo, SADHFormDataValidation.D1_EntryTypeIsInvalid);

			SADHData.D1_MessageType = "XYZ";

			SADHData.D1_EntryType = EntryStyleListExport.Codes.ExportNormal;
			AssertHasMessageError(SADHData.D1_EntryTypeInfo, SADHFormDataValidation.D1_EntryTypeIsInvalid);

			SADHData.D1_EntryType = EntryStyleListImport.Codes.ImportNormal;
			AssertHasMessageError(SADHData.D1_EntryTypeInfo, SADHFormDataValidation.D1_EntryTypeIsInvalid);
		}

		#region Implementation
		SADHFormData SADHData
		{
			get { return fSADHData ?? (fSADHData = new SADHFormData(Factory, Factory.New<JobDeclaration>())); }
		}
		SADHFormData fSADHData;
		#endregion
	}
}
