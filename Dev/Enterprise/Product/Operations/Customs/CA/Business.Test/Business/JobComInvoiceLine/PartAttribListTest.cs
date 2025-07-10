namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class PartAttribListTest : Customs.Business.Testing.PartAttribListTest<JobComInvoiceLine, OrgSupplierPart, CusClassPartPivot>
	{
		protected override string HTE => ClassificationTypeList.Codes.HTE;

		protected override string HTI => ClassificationTypeList.Codes.HTI;

		protected override string SHB => ClassificationTypeList.Codes.SHB;

		protected override void SetHTI(JobComInvoiceLine invoiceLine)
		{
			entryHeader.CH_MessageType = MessageTypeList.Codes.G7Export;
		}

		protected override void SetSHB(JobComInvoiceLine invoiceLine)
		{
			entryHeader.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
		}

		protected override void SetupData(JobComInvoiceLine invoiceLine)
		{
			entryHeader = invoiceLine.Declaration.CustomsEntryHeaders.AddNew();
		}

		protected override void SetUp()
		{
			entryHeader = null;
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			entryHeader = null;
		}

		CusEntryHeader entryHeader;
	}
}
