using System.Collections;
using Enterprise.Client.EDI.Telematics.Tca;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.DeviceManagement.GUI
{
	class ClientTelRimRegistrationStateCodeDropEdit : ZDropEdit
	{
		public ClientTelRimRegistrationStateCodeDropEdit()
		{
			codes = new CodeDescriptionPairList();
			codes.AddPair(nameof(TcaCommonXml.RegistrationStateEnum.ACT), Res.GetString("7E5D10CC-0546-435C-A59E-85C162E8D782", "Australian Capital Territory"));
			codes.AddPair(nameof(TcaCommonXml.RegistrationStateEnum.NSW), Res.GetString("0F0E20AD-3115-494C-9456-18AAA7361E60", "New South Wales"));
			codes.AddPair(nameof(TcaCommonXml.RegistrationStateEnum.NT), Res.GetString("07E58E77-4578-4E39-8899-B018BD620BBE", "Northern Territory"));
			codes.AddPair(nameof(TcaCommonXml.RegistrationStateEnum.QLD), Res.GetString("CA60F052-447F-4B2D-BABD-14248F8B1AFA", "Queensland"));
			codes.AddPair(nameof(TcaCommonXml.RegistrationStateEnum.SA), Res.GetString("99F4BA6A-4794-4E65-AE01-1C0B26A755E7", "South Australia"));
			codes.AddPair(nameof(TcaCommonXml.RegistrationStateEnum.TAS), Res.GetString("BBE30DF7-057F-4FE4-9496-669A10CD2A60", "Tasmania"));
			codes.AddPair(nameof(TcaCommonXml.RegistrationStateEnum.VIC), Res.GetString("63367EC0-0D37-4351-8E67-79D2EE4A9E0F", "Victoria"));
			codes.AddPair(nameof(TcaCommonXml.RegistrationStateEnum.WA), Res.GetString("031AE680-00FE-4867-8F9E-B15FC214A017", "Western Australia"));
			codes.AddPair(nameof(TcaCommonXml.RegistrationStateEnum.FIRS), Res.GetString("F599B302-A1BD-4DE8-8F63-DBC350ACD445", "Federal Interstate Registration Scheme"));
		}

		readonly CodeDescriptionPairList codes;

		protected override IList GetFilteredListForDropDown()
		{
			return codes;
		}
	}
}
