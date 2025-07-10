using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	public static class LVXJobsConsolidateHelperForTest
	{
		public static JobDeclaration CreateLVX(BusinessObjectFactory factory, string reference, int periodYear, int perodMonth, ZGuid importer, ZGuid branch, ZString portOfClearance, ZString broker)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = reference;
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(periodYear, perodMonth, 3);
			declaration.JE_OH_Importer = importer;
			declaration.JE_GB = branch;
			declaration.JE_GS_NKCusAgent = broker;
			var invoice = declaration.LVXInvoiceHeader;
			invoice.JZ_OH_Buyer = importer;
			invoice.CA_PortOfClearance = portOfClearance;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			factory.Save();
			invoice.CA_ReadyForConsolidation = true;

			return declaration;
		}

		public static JobDeclaration CreateLVS(BusinessObjectFactory factory, string reference, string type, int periodYear, int perodMonth, ZGuid importer, ZGuid branch, ZString provinceOfClearance, ZString broker)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = reference;
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_MessageSubType = type;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(periodYear, perodMonth, 1);
			declaration.JE_OH_Importer = importer;
			declaration.JE_GB = branch;
			declaration.CA_ProvinceOfClearance = provinceOfClearance;
			declaration.JE_GS_NKCusAgent = broker;

			return declaration;
		}
	}
}
