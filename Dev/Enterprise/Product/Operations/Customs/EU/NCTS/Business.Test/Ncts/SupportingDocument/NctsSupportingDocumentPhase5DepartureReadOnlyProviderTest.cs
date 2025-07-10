using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsSupportingDocumentPhase5DepartureReadOnlyProviderTest : TestCaseWithFactory
	{
		public void TestCSI_LineNo_ReadOnly()
		{
			AssertEquals(false, provider.CSI_LineNo_ReadOnly);
		}

		public void TestCSI_Status_ReadOnly()
		{
			AssertEquals(false, provider.CSI_Status_ReadOnly);
		}

		public void TestCSI_Code_ReadOnly()
		{
			AssertEquals(false, provider.CSI_Code_ReadOnly);
		}

		public void TestCSI_Referencenumber_ReadOnly()
		{
			NCTSTestHelper.AssertIsReadOnlyWhenAttributeIsMissing(Factory, UniversalReferenceConstants.RefCusCodeListLevelTypes.Header, provider, supportingDocument, (x) => x.CSI_ReferenceNumber_ReadOnly, "Reference", readOnlyWhenCodeNotInList: false);
		}

		public void TestCSI_Referencenumber2_ReadOnly()
		{
			NCTSTestHelper.AssertIsReadOnlyWhenAttributeIsMissing(Factory, UniversalReferenceConstants.RefCusCodeListLevelTypes.Header, provider, supportingDocument, (x) => x.CSI_ReferenceNumber2_ReadOnly, "Complement", readOnlyWhenCodeNotInList: false);
		}

		public void TestCSI_Description_ReadOnly()
		{
			AssertEquals(false, provider.CSI_Description_ReadOnly);
		}

		public void TestCSI_ItemNumber_ReadOnly()
		{
			AssertEquals(false, provider.CSI_ItemNumber_ReadOnly);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			supportingDocument = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			provider = new NctsSupportingDocumentPhase5DepartureReadOnlyProvider(supportingDocument);
		}
		ISupportingDocumentReadOnlyConditions provider;
		NctsSupportingDocument supportingDocument;
	}
}
