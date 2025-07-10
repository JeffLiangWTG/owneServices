using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromOrgSalesCallBO))]
	sealed class FreightWrapperFromOrgSalesCallBOTest : FreightWrapperTest
	{
		public void TestGetDocSalesCall()
		{
			communication.OQ_CommunicationID = "TESTID";
			var communicationDoc = orgGenericFreightWrapper.OrgSalesCallDocument;

			AssertType(typeof(DocSalesCall), communicationDoc);
		}

		public void TestJobNumber()
		{
			communication.OQ_CommunicationID = "TESTID";

			AssertEquals("Job Number is Communication ID", "TESTID", orgGenericFreightWrapper.JobNumber);
		}

		public void TestClient()
		{
			var org = Factory.New<OrgHeader>();
			var contact = Factory.New<OrgContact>();

			communication.OQ_OH = org.PK;
			communication.OQ_OC = contact.PK;
			AssertEquals(org, orgGenericFreightWrapper.Client.Organisation);
			AssertEquals(contact, orgGenericFreightWrapper.Client.WrappedMainContact.ContactBO);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<OrgSalesCall>();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromOrgSalesCallBO(communication, Factory);
		}

		#region Implementation

		OrgSalesCall communication;
		FreightWrapperFromOrgSalesCallBO orgGenericFreightWrapper;

		protected override void SetUp()
		{
			communication = Factory.New<OrgSalesCall>();
			orgGenericFreightWrapper = new FreightWrapperFromOrgSalesCallBO(communication, Factory);
			base.SetUp();
		}

		#endregion
	}
}
