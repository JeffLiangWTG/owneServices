using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Registry.Business.Testing
{
	sealed class ContactAttachmentTypeListTest : TestCaseWithFactory
	{
		public void TestAttachmentType_List()
		{
			AssertEquals("AttachmentType_List.Count", 8, ContactAttachmentTypeList.AttachmentType_List.Count);
			Assert("Attachment types should contain XLS.", ContactAttachmentTypeList.AttachmentType_List.ContainsCode(OrgConstants.AttachmentType.XLS));
			Assert("Attachment types should contain XLSX.", ContactAttachmentTypeList.AttachmentType_List.ContainsCode(OrgConstants.AttachmentType.XLSX));
			Assert("Attachment types should contain PDF.", ContactAttachmentTypeList.AttachmentType_List.ContainsCode(OrgConstants.AttachmentType.PDF));
			Assert("Attachment types should contain PDF/A.", ContactAttachmentTypeList.AttachmentType_List.ContainsCode(OrgConstants.AttachmentType.PDFA));
			Assert("Attachment types should contain PDFC.", ContactAttachmentTypeList.AttachmentType_List.ContainsCode(OrgConstants.AttachmentType.PDFC));
			Assert("Attachment types should contain TIF.", ContactAttachmentTypeList.AttachmentType_List.ContainsCode(OrgConstants.AttachmentType.TIF));
			Assert("Attachment types should contain HTML.", ContactAttachmentTypeList.AttachmentType_List.ContainsCode(OrgConstants.AttachmentType.HTML));
			Assert("Attachment types should contain HTMF.", ContactAttachmentTypeList.AttachmentType_List.ContainsCode(OrgConstants.AttachmentType.HTMF));
		}
	}
}
