using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	static public class AmendmentIDSupporter
	{
		static public ZString GetIDDescription(string id)
		{
			return AmendmentIDs.TryGetValue(id, out var description) ? description : string.Empty;
		}

		static Dictionary<string, string> AmendmentIDs
		{
			get
			{
				return new Dictionary<string, string>()
				{
					{ nameof(IExportEntryHeader), Res.GetString("1EDAD2FB-8B49-45CA-A7F2-D58ECA095E34", "Header") },
					{ nameof(IExportContainer), Res.GetString("E3B24472-F793-4CE2-BC2F-42662F3C5152", "Container") },
					{ nameof(IExportEntryLine), Res.GetString("7F016B2B-6F21-4CAC-87CD-7E5A2B06F086", "Entry Line") },
					{ nameof(IExportInvoiceLine), Res.GetString("458FDAA8-4166-484C-BDEF-930256941AF5", "Invoice Line") },
					{ nameof(IExportGAApprovalDocument), Res.GetString("8C826E32-5B51-470B-A9F2-86C64FC06D38", "Req. Doc.") },
					{ nameof(IExportVehicleNo), Res.GetString("870A7221-A4A5-4CD0-8CE3-6E1BCBE0546C", "Vehicle No.")  },
					{ nameof(IImport5BALine), Res.GetString("FFC9510A-4F34-4647-B869-778F8A0D982A", "Entry Line") },
					{ nameof(IOrganization), Res.GetString("6F26D3F2-BDB2-4761-9244-7AC66FF8DC95", "Organization") },
					{ nameof(ILocalExportEntryLine), Res.GetString("FF482AF0-A1A8-4BD7-AF4A-368D51FE09DC", "Entry Line") },
					{ nameof(ILocalExportEntryHeader), Res.GetString("C757820A-E0FA-4BEA-B691-73009940B9BE", "Header") },
					{ nameof(ILocalExportOtherTransportMeans), Res.GetString("91279DFB-FFC0-4647-AFB3-471B31454141", "Transport Means") },
					{ nameof(IExportCargoMeanagement), Res.GetString("93A68063-A5EF-4BC4-B829-5AA8FCD212E2", "Cargo Management No.") },
					{ nameof(IImportFTALine), Res.GetString("4C4A8058-538B-4F16-83BA-C51D3665D5E6", "Entry Line") },
					{ nameof(IImportDHRInvoiceLine), Res.GetString("58BFF8B5-B605-4118-A57D-0F6805AC3A02", "Invoice Line") },
					{ nameof(IImportEntryHeader), Res.GetString("4BA206BE-6BA7-4C8D-9E46-28D3A7954E27", "Header") },
					{ nameof(IImportContainer), Res.GetString("5D8A74BE-D5E8-43AD-B873-43B88BB54826", "Container") },
					{ nameof(IImportEntryLine), Res.GetString("E96213ED-181A-4530-B994-6156AC541A7C", "Entry Line") },
					{ nameof(IImportInvoiceLine), Res.GetString("F8F92348-0D95-45AF-8393-5B4F4FB3F93C", "Invoice Line") },
					{ nameof(IImportGAApprovalDocument), Res.GetString("2ACA797D-9F83-498B-809E-C71FAA721A54", "Req. Doc.") },
					{ nameof(IImportImmediateDelivery), Res.GetString("951F3BB5-8236-4BC7-9294-201C13557590", "Immediate Delivery")  },
					{ nameof(IImportNonGADetail), Res.GetString("8750C203-C57B-4077-B055-CC161118C030", "Non GA")  },
					{ nameof(IImportOnlineOrder), Res.GetString("133D1D99-B975-4861-BED0-6DDFF396831D", "Online Order")  },
					{ nameof(IImportPreviousExpDecLine), Res.GetString("62893EE1-8200-4139-B581-4186CF211B23", "Previous Export Dec. Line")  }
				};
			}
		}
	}
}
