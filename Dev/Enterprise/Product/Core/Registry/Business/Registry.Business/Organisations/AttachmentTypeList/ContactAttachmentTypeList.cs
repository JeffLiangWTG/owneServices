using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public static class ContactAttachmentTypeList
	{
		public static CodeDescriptionPairList AttachmentType_List
		{
			get
			{
				var list = new CodeDescriptionPairList(OLookUpEditType.CustomType);

				list.AddPair(OrgConstants.AttachmentType.XLS, Res.GetString("f5b21c67-b0d2-414d-89cb-f311916c6d2f", "Microsoft Excel 97-2003 Spreadsheet"));
				list.AddPair(OrgConstants.AttachmentType.XLSX, Res.GetString("176dc61b-9073-40e9-a3ce-38d5f9331078", "Microsoft Excel 2007 Spreadsheet"));
				list.AddPair(OrgConstants.AttachmentType.PDF, Res.GetString("e8368e75-4e66-4d44-9c68-a0b71c4f9e24", "Portable Document Format"));
				list.AddPair(OrgConstants.AttachmentType.PDFA, Res.GetString("1935d290-a699-4271-9cd1-c95ba6036276", "Portable Document Format Archive (PDF/A-2)"));
				list.AddPair(OrgConstants.AttachmentType.PDFC, Res.GetString("658A1C98-320F-41FA-93BD-E8F05350BC12", "Combined Documents in PDF Format"));
				list.AddPair(OrgConstants.AttachmentType.TIF, Res.GetString("4acfe968-7915-4a79-b0da-54e8e1e05dda", "Tagged Image File"));
				list.AddPair(OrgConstants.AttachmentType.HTML, Res.GetString("27ca821d-ab54-4171-a2e4-cda00dfffa66", "HTML Body"));
				list.AddPair(OrgConstants.AttachmentType.HTMF, Res.GetString("79d4b4fc-0592-4652-ab04-16006c3c5e47", "HTMF - First Document HTML Body, rest as PDF"));

				return list;
			}
		}
	}
}
