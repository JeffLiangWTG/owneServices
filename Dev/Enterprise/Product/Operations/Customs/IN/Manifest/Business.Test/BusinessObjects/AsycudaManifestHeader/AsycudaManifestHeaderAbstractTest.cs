using CargoWise.Types;
using Enterprise.Customs.IN.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestsSubclassesOf(typeof(AsycudaManifestHeader))]
public abstract class AsycudaManifestHeaderAbstractTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
{
	public void TestGetDefaultCountryCode()
	{
		AssertEquals("Default Country", Core.Constants.CountryCodes.India, ((AsycudaManifestHeader)GetNewBusinessObject()).AMA_RN_NKCountry);
	}

	public void TestIMessageAttachee()
	{
		var headerBo = (AsycudaManifestHeader)GetNewBusinessObject();
		var messageAttachee = (IMessageAttachee)headerBo;
		CombineAssertions(() =>
		{
			AssertSame("Messages", headerBo.Messages, messageAttachee.Messages);
			AssertEquals("MessageOwner", headerBo.AMA_CustomsOffice, messageAttachee.MessageOwner);
			AssertEquals("BranchPK", headerBo.AMA_GB, messageAttachee.BranchPK);

			messageAttachee.MessageStatus = "QUE";
			AssertEquals("MessageStatus", "QUE", messageAttachee.MessageStatus);
			AssertEquals("CH_Status", "QUE", headerBo.AMA_MessageStatus);

			messageAttachee.CustomsStatus = "ACC";
			AssertEquals("CustomsStatus", "ACC", messageAttachee.CustomsStatus);
			AssertEquals("CH_EntryStatus", "ACC", headerBo.RegistrationStatus);
		});
	}

	public void TestIJobNumber()
	{
		var headerBo = (AsycudaManifestHeader)GetNewBusinessObject();
		headerBo.AMA_JobReference = "MAN101";
		var jobNumber = (IJobNumber)headerBo;
		AssertEquals("JobNumber", "MAN101", jobNumber.JobNumber);
	}

	public void TestRollbackChangesOnStatus()
	{
		var headerBo = (AsycudaManifestHeader)GetNewBusinessObject();
		var messageAttachee = (IMessageAttachee)headerBo;

		CombineAssertions(() =>
		{
			messageAttachee.MessageStatus = "QUE";
			messageAttachee.CustomsStatus = "ACC";

			messageAttachee.RollbackChangesOnStatus();
			AssertEquals("MessageStatus", ZString.Empty, messageAttachee.MessageStatus);
			AssertEquals("CustomsStatus", ZString.Empty, messageAttachee.CustomsStatus);

			messageAttachee.MessageStatus = "QUE";
			messageAttachee.CustomsStatus = "ACC";
			Factory.Save();

			messageAttachee.MessageStatus = "FAL";
			messageAttachee.CustomsStatus = "ERR";
			messageAttachee.RollbackChangesOnStatus();
			AssertEquals("CustomsStatus", "QUE", messageAttachee.MessageStatus);
			AssertEquals("MessageStatus", "ACC", messageAttachee.CustomsStatus);
		});
	}

	public void TestCalculateStatusAfterSending()
	{
		var header = (IMessageAttachee)GetNewBusinessObject();
		foreach (var messageType in new ManifestMessageTypeList().GetAllCodes())
		{
			AssertEquals($"Message Type {messageType}", GetExpectedCustomsStatus(messageType), header.CalculateStatusAfterSending(messageType));
		}
	}

	protected virtual ZString GetExpectedCustomsStatus(ZString messageType) => ZString.Empty;
}
