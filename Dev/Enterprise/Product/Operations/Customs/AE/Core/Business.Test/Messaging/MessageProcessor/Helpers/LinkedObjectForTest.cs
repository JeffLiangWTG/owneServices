using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class LinkedObjectForTest : DummyBusinessObject, IMessageAttachee, IEDIMessageCollectionProvider
{
	public LinkedObjectForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public string JobNumberForTesting = "JobNumber";

	public string JobNumber => JobNumberForTesting;

	public ZString EntryStatus { get; set; }

	public ZString MessageStatus { get; set; }

	public ZString DocumentIdentifier => "DocumentIdentifier";

	public EDIMessageCollection Messages => messages ??= new EDIMessageCollection(this, Factory);

	EDIMessageCollection messages;

	IBusinessObjectCollection IMessageAttachee.Messages => Messages;
}
