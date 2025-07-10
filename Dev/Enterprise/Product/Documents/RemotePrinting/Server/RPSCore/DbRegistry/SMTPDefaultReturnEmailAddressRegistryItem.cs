
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
#if DEBUG
	public
#endif
	class SMTPDefaultReturnEmailAddressRegistryItem : StringDbRegistryItem
	{
		public override string ItemName
		{
			get { return "SMTPDefaultReturnEmailAddress"; }
		}

		protected override string DefaultValue
		{
			get { return ""; }
		}

		public new string LoadValue(DbConnection conn)
		{
			Argument.NotNull(conn, nameof(conn));

			string result = base.LoadValue(conn);

			if (string.IsNullOrEmpty(result))
			{
				result = new MailboxEmailAddressRegistryItem().LoadValue(conn);
			}

			return result;
		}

		class MailboxEmailAddressRegistryItem : StringDbRegistryItem
		{
			public override string ItemName
			{
				get { return "MailboxEmailAddress"; }
			}

			protected override string DefaultValue
			{
				get { return ""; }
			}
		}
	}
}
