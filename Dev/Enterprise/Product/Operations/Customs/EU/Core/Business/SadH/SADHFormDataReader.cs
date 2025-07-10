using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.SADH
{
	public class SADHFormDataReader : Customs.Business.SADH.SADHFormDataReader
	{
		public SADHFormDataReader(SADHFormData formData)
			: base(formData)
		{
			this.formData = formData;
		}
		readonly SADHFormData formData;

		protected override void ReadDeclarationFields(BaseJobDeclaration declaration)
		{
			base.ReadDeclarationFields(declaration);

			JobDeclaration gbJobDec = (JobDeclaration)declaration;

			//Box 1 Declaration
			formData.D1_EntryType = gbJobDec.JE_EntryStyle;

			//Box 14 Representation Type (top of box)
			formData.D1_RepresentationType = gbJobDec.JE_DeclarantType;
			//Box 18 Identity and nationality of means of transport on arrival
			formData.D1_Box18TransportID = gbJobDec.ZG_Box18TransportID;
			//Box 21 Identity and nationality of means of transport crossing the border
			if (gbJobDec.JE_TransportMode != TransportTypeList.Codes.Sea &&
				gbJobDec.JE_TransportMode != TransportTypeList.Codes.Air)
			{
				formData.D1_DepartureTransportID = gbJobDec.JE_VesselName;
			}
			//Box 26 Inland mode of transport
			formData.D1_InlandModeOfTransport = gbJobDec.TransportModeTranslator.TranslateToWCOCode(gbJobDec.JE_TransportModeInland, true);
			//Box 30 Location of Goods
			formData.D1_LocationOfGoods = gbJobDec.JE_LocationOfGoods.Trim();
		}
	}
}
