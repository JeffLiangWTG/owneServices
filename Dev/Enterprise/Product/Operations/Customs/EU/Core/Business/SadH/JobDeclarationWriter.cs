using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using CusEntryInstruction = Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.EU.Business.SADH
{
	public class JobDeclarationWriter : Customs.Business.SADH.JobDeclarationWriter
	{
		public JobDeclarationWriter(JobDeclaration declaration)
			: base(declaration)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		protected override void WriteDeclarationFields(Customs.Business.SADH.SADHFormData formData)
		{
			base.WriteDeclarationFields(formData);

			//Box 1 Declaration
			declaration.JE_EntryStyle = formData.D1_EntryType;
			var cei = declaration.CustomsEntryInstructions.Any() ? declaration.CustomsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault() : declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_SubStyle = formData.D1_EntrySubType;
			//Box 14 Representation Type (top of box)
			declaration.JE_DeclarantType = formData.D1_RepresentationType;
			//Box 18 Identity and nationality of means of transport on arrival
			declaration.ZG_Box18TransportID = formData.D1_Box18TransportID;
			//Box 21 Identity and nationality of means of transport crossing the border
			if (declaration.JE_TransportMode != TransportTypeList.Codes.Sea &&
				declaration.JE_TransportMode != TransportTypeList.Codes.Air)
			{
				declaration.JE_VesselName = formData.D1_DepartureTransportID;
			}
			//Box 26 Inland mode of transport
			declaration.JE_TransportModeInland = declaration.TransportModeTranslator.TranslateToCargoWiseCode(formData.D1_InlandModeOfTransport, true);
			//Box 30 Location of Goods		
			// Box 1 (third sub div) - Community Transit
			declaration.ZG_CTStatusID = formData.D1_CTStatusID;
		}

		protected override void WriteInvoiceLineFields(Customs.Business.SADH.SADHFormData formData, BaseJobComInvoiceLine line)
		{
			base.WriteInvoiceLineFields(formData, line);

			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)line;
			// Box 36 Customs Preference Code (codes dictated by Customs for Imports only).
			invoiceLine.JI_PrimaryPreference = formData.D1_PreferenceCode;
			// Box 37 Customs Procedure Code (CPC).
			invoiceLine.JI_Procedure = formData.D1_ProcedureCode;
			// "line.refresh" here sorts out gui update problem when using sadh form, but causes test failures..
		}
	}
}
