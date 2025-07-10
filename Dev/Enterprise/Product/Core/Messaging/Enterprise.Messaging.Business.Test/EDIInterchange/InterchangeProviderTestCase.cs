using System;
using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.InterchangeProviders.Testing
{
	public abstract class InterchangeProviderTestCase : TestCaseWithFactory
	{
		public abstract void TestMessagesPopulateNewInterchange();

		#region Implementation

		protected abstract InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection);

		protected virtual bool SupportsMessageOwner
		{
			get { return true; }
		}

		protected virtual BusinessObject LinkedObject
		{
			get { return null; }
		}

		protected string ReadFile(string filename)
		{
			string result = String.Empty;
			using (StreamReader reader = new StreamReader(filename, Encoding.ASCII))
			{
				result = reader.ReadToEnd();
			}
			return result;
		}

		protected NonDependentEDIMessageCollection MakeCopyOfMessages(NonDependentEDIMessageCollection messages)
		{
			var result = new NonDependentEDIMessageCollection(messages.Factory);
			result.AddRange((BusinessObject[])messages.ToArray(typeof(BusinessObject)));
			return result;
		}

		protected NonDependentEDIMessageCollection SetupMessages(string applicationCode, string messageTextFilename)
		{
			var messages = new NonDependentEDIMessageCollection(Factory);
			var message = messages.AddNew();
			message.EM_ApplicationCode = applicationCode;
			message.EM_MessageText = ReadFile(messageTextFilename)
				.Replace("'", System.Text.Encoding.ASCII.GetString(new byte[1] { 28 }))
				.Replace("+", System.Text.Encoding.ASCII.GetString(new byte[1] { 29 }))
				.Replace(":", System.Text.Encoding.ASCII.GetString(new byte[1] { 31 }))
				.Replace("\r\n", "");

			message.EM_MessageType = applicationCode;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message.EM_IsActive = true;
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageOwner = "";

			var message2 = messages.AddNew();
			message2.EM_ApplicationCode = applicationCode;
			message2.EM_MessageText = ReadFile(messageTextFilename)
				.Replace("'", System.Text.Encoding.ASCII.GetString(new byte[1] { 28 }))
				.Replace("+", System.Text.Encoding.ASCII.GetString(new byte[1] { 29 }))
				.Replace(":", System.Text.Encoding.ASCII.GetString(new byte[1] { 31 }))
				.Replace("\r\n", "");

			message2.EM_MessageType = applicationCode;
			message2.EM_Status = EDIMessage.Status.Queued;
			message2.EM_GB = GlbBranch.CurrentBranch.PK;
			message2.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message2.EM_IsActive = true;
			message2.EM_ReceiveTransmit = "TRX";
			message2.EM_MessageOwner = "";
			return messages;
		}

		protected GlbCompany OtherCompany
		{
			get
			{
				var otherCompanyFilter = new ZQuery();
				otherCompanyFilter.AddToFilter(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
				return Factory.LoadTop1<GlbCompany>(otherCompanyFilter);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			OtherBranchPK = Factory.LoadFromNaturalKey(typeof(GlbBranch), GlbBranchSchema.GB_Code, "SYD").PK.ToGuid();
		}

		protected const int MaxNumberOfMessages = 100;

		protected Guid OtherBranchPK = Guid.Empty;

		protected EDIMessage Message
		{
			get
			{
				if (fMessage == null)
				{
					fMessage = EDIMessageTestFactory.New(Factory);
					fMessage.EM_GB = GlbBranch.CurrentBranch.PK;
				}

				return fMessage;
			}
		}
		EDIMessage fMessage;

		#endregion
	}
}
