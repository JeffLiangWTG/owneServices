using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.AutoDeploy.Business;
using Enterprise.MailManager.Business;

namespace Enterprise.Client.EDI.Mail.Business
{
	public class ClientMailRecipient : AutoClientMailDBRecipients
	{
		public ClientMailRecipient(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("Upgrade")]
		public override ZGuid MRX_L1
		{
			get { return base.MRX_L1; }
			set { base.MRX_L1 = value; }
		}

		public UpgradesToClient Upgrade
		{
			get { return Factory.Load<UpgradesToClient>(MRX_L1); }
		}

		[RelatedBusinessObject("Recipient")]
		public override ZGuid MRX_MR
		{
			get { return base.MRX_MR; }
			set { base.MRX_MR = value; }
		}

		public MailRecipient Recipient
		{
			get { return Factory.Load<MailRecipient>(MRX_MR); }
		}
	}
}

