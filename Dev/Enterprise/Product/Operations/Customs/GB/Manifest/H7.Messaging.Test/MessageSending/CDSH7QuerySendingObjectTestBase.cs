using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders.Query;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.GB.H7.Messaging.Testing
{
	public abstract class CDSH7QuerySendingObjectTestBase : TestCaseWithFactory
	{
		public void TestContextCollection()
		{
			var sendingObject = GetNewMessageSendingObject() as IQueryDataProvider;
			var contextCollection = sendingObject.GetContextCollection(bill.PK);
			AssertEquals(expectedEntryNumberType, contextCollection.FirstOrDefault(c => c.Type == CDSDISQueryHelper.Constants.ContextTypes.EntryNumberType).Value);
			AssertEquals(expectedEntryNumber, contextCollection.FirstOrDefault(c => c.Type == CDSDISQueryHelper.Constants.ContextTypes.EntryNumber).Value);
			AssertEquals(expectedNotificationType, contextCollection.FirstOrDefault(c => c.Type == CDSDISQueryHelper.Constants.ContextTypes.NotificationType).Value);
			AssertEquals(CDSDISQueryHelper.Constants.QueryStringParameters.PartyRole, contextCollection.FirstOrDefault(c => c.Type == CDSDISQueryHelper.Constants.ContextTypes.QueryString).Value);
		}

		public void TestContextReference()
		{
			var sendingObject = GetNewMessageSendingObject() as IQueryDataProvider;
			AssertEquals("H7D0001BN001", sendingObject.ContextReference);
		}

		public void TestCredentialKey()
		{
			var sendingObject = GetNewMessageSendingObject() as IQueryDataProvider;
			AssertEquals("EDIDAT.GB987654321ABC.PR1", sendingObject.CredentialKey);
		}

		public void TestContextType()
		{
			var sendingObject = GetNewMessageSendingObject() as IQueryDataProvider;
			AssertEquals(DataContextType.AsycudaBill, sendingObject.ContextType);
		}

		protected abstract ZString expectedEntryNumberType { get;  }
		protected abstract ZString expectedEntryNumber { get;  }
		protected abstract ZString expectedNotificationType { get;  }

		protected override void SetUp()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "H7D0001";
			header.AMA_CustomsProfile = "PR1";
			header.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321ABC");
			bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "BN001";
		}

		protected AsycudaBill bill;
		protected abstract BaseCDSQuerySendingObject GetNewMessageSendingObject();
	}
}
