using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Business.Testing
{
	class ClientRefundDummy : ClientRefund
	{
		public ClientRefundDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			SendNotificationEmailCalled = false;
			AddRefundNoteCalled = false;
		}

		protected override void SendNotificationEmail()
		{
			if (ThrowExceptionOnEmailSending)
			{
				throw new ArgumentException("Test exception");
			}

			base.SendNotificationEmail();
			SendNotificationEmailCalled = true;
		}

		public bool ThrowExceptionOnEmailSending { get; set; }

		protected override void AddRefundNote(IRefundEnquiry refundEnquiry)
		{
			base.AddRefundNote(refundEnquiry);
			AddRefundNoteCalled = true;
		}

		public EmailGroupUtility EmailUtility
		{
			get
			{
				return EmailGroupUtility;
			}
		}

		public ZString GetNewControlNumber()
		{
			return NewControlNumber();
		}

		public ZBool SendNotificationEmailCalled { get; private set; }

		public ZBool AddRefundNoteCalled { get; private set; }
	}
}
