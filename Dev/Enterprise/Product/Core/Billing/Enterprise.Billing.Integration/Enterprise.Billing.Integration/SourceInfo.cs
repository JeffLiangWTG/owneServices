using System.IO;
using CargoWise.Types;

namespace Enterprise.Billing.Integration
{
	public class SourceInfo : ISourceInfo
	{
		SourceInfo()
		{
			this.interfaceName = BillingInterfaceName.None;
			this.dataSource = BillingDataSource.None;
		}

		public SourceInfo(BillingDataSource dataSource, BillingInterfaceName interfaceName, ZGuid ediMessagePk, ZGuid ediInterchangePK, ZString senderId, ZString fileName)
		{
			this.dataSource = dataSource;
			this.interfaceName = interfaceName;
			this.ediMessagePK = ediMessagePk;
			this.ediInterchangePK = ediInterchangePK;
			this.senderId = senderId;
			this.FileName = fileName;
		}

		public static SourceInfo EmptySourceInfo
		{
			get { return emptySourceInfo ?? (emptySourceInfo = new SourceInfo()); }
		}
		[System.ThreadStatic]
		static SourceInfo emptySourceInfo;

		public ZString DataSource
		{
			get { return dataSource.ToString(); }
		}
		readonly BillingDataSource dataSource;

		public ZString InterfaceName
		{
			get { return interfaceName.ToString(); }
		}

		public bool ShouldSuspendValidation
		{
			get { return interfaceName.ShouldSuspendValidation; }
		}

		readonly BillingInterfaceName interfaceName;

		public ZGuid EDIMessagePK
		{
			get { return ediMessagePK; }
		}
		readonly ZGuid ediMessagePK;

		public ZGuid EDIInterchangePK
		{
			get { return ediInterchangePK; }
		}
		readonly ZGuid ediInterchangePK;

		public ZString SenderId
		{
			get { return senderId; }
		}
		readonly ZString senderId;

		public ZString FileName
		{
			get { return fileName; }
			private set { fileName = ExtractSBHFileName(value); }
		}
		ZString fileName;

		public static string ExtractSBHFileName(string path) => Path.GetFileName(path);
	}
}
