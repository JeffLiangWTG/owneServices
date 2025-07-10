using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CargoAttribute))]
	class CargoAttributeTest : Customs.Business.Testing.CusCodeDataTest<CargoAttribute>
	{
		public void TestSupportsNotes()
		{
			AssertEquals("SupportsNotes should be false", false, ((CargoAttribute)BusinessObject).SupportsNotes);
		}

		public void TestCanLinkToAttachment()
		{
			var cargoAttribute = Factory.New<CargoAttribute>();
			AssertEquals("CY_Code is empty", false, cargoAttribute.CanLinkToAttachment);

			var canLinkToAttachmentCargoAttributes = new[] {
				CargoAttributeList.Codes._31,
				CargoAttributeList.Codes._32,
			};

			foreach (var code in new CargoAttributeList().GetAllCodes())
			{
				cargoAttribute.CY_Code = code;
				AssertEquals($"CY_Code={code}", canLinkToAttachmentCargoAttributes.Contains(code), cargoAttribute.CanLinkToAttachment);
			}
		}

		public void TestReloadAttachmentLinks()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = entryInstruction.JobDeclaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CEI = entryInstruction.PK;
			var storageDoc = entryInstruction.CusStorageDocPivots.AddNew();
			storageDoc.CSD_DocType = CSDDocTypeList.Codes._80000001;
			AssertEquals("AttachmentLinks is empty", 0, invoiceLine.AttachmentLinks.Count);

			var attachmentLinksReloaded = false;
			invoiceLine.AttachmentLinks.CountChanged += (o, e) => attachmentLinksReloaded = true;

			void AssertAttachmentLinksReloaded(bool reloaded)
			{
				AssertEquals($"AttachmentLinks should {(reloaded ? "" : "NOT ")}be reloaded", reloaded, attachmentLinksReloaded);
				attachmentLinksReloaded = false;
			}

			var attr11 = invoiceLine.CargoAttributes.AddNew(CargoAttributeList.Codes._11);
			AssertAttachmentLinksReloaded(false);

			var attr31 = invoiceLine.CargoAttributes.AddNew(CargoAttributeList.Codes._31);
			AssertAttachmentLinksReloaded(true);
			AssertEquals("Cargo Attribute 31 supports link to attachment", 1, invoiceLine.AttachmentLinks.Count);
			invoiceLine.AttachmentLinks[0].IsLinked = true;

			var attr32 = invoiceLine.CargoAttributes.AddNew(CargoAttributeList.Codes._32);
			AssertAttachmentLinksReloaded(false);
			AssertEquals("Cargo Attribute 31,32 supports link to attachment", 1, invoiceLine.AttachmentLinks.Count);
			AssertEquals("Attachment Linked", true, invoiceLine.AttachmentLinks[0].IsLinked);

			invoiceLine.CargoAttributes.Remove(attr31);
			AssertAttachmentLinksReloaded(true);
			AssertEquals("Cargo Attribute 32 supports link to attachment", 1, invoiceLine.AttachmentLinks.Count);
			AssertEquals("Attachment Linked", true, invoiceLine.AttachmentLinks[0].IsLinked);

			invoiceLine.CargoAttributes.Remove(attr11);
			AssertAttachmentLinksReloaded(true);
			AssertEquals("Cargo Attribute 32 supports link to attachment", 1, invoiceLine.AttachmentLinks.Count);
			AssertEquals("Attachment Linked", true, invoiceLine.AttachmentLinks[0].IsLinked);

			invoiceLine.CargoAttributes.Remove(attr32);
			AssertAttachmentLinksReloaded(true);
			AssertEquals("No Cargo Attribute supports link to attachment", 0, invoiceLine.AttachmentLinks.Count);
			AssertEquals("InvoiceLineLink deleted", 0, storageDoc.InvoiceLineLinks.Count);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			return declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().CargoAttributes.AddNew();
		}
	}
}
