using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.DataMapping;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	public class FtpJobConfig : AutoFtpJobConfig
	{
		public FtpJobConfig(BusinessObject parent)
			: base(parent.Factory)
		{
			this.parent = ObjectFactory.Get<IServiceTaskAccessor>().GetServiceTask(parent);
			FromXml(this.parent.ConfigString);
			parent.RegisterEditableChildObject(this);
		}

		public FtpJobConfig(string configString)
			: base(new BusinessObjectFactory())
		{
			FromXml(configString);
		}

		readonly IServiceTaskSchedule parent;

		void FromXml(string configString)
		{
			var configXml = XmlSerializableSetting.FromXml<FtpJobConfigXml>(configString);
			if (configXml != null)
			{
				NotifyOnFailure = configXml.NotifyOnFailure;
				NotifyOnSuccess = configXml.NotifyOnSuccess;
				NotifyPrintUser = configXml.NotifyPrintUser;
				NotificationGroup_PK = configXml.NotificationGroup_PK;
			}
		}

		FtpJobConfigXml ToXml()
		{
			var configXml = new FtpJobConfigXml();
			configXml.NotifyOnFailure = NotifyOnFailure;
			configXml.NotifyOnSuccess = NotifyOnSuccess;
			configXml.NotifyPrintUser = NotifyPrintUser;
			configXml.NotificationGroup_PK = NotificationGroup_PK;
			return configXml;
		}

		public string ConfigString => ToXml().AsXml();

		public virtual GlbGroupCollection GlbGroups
		{
			get
			{
				return new GlbGroupCollection(Factory);
			}
		}

		protected override void OnFactorySaving()
		{
			var configXml = ToXml();
			parent.ConfigString = configXml.AsXml();

			base.OnFactorySaving();
		}

		public sealed override ZBool NotifyOnSuccess
		{
			get => base.NotifyOnSuccess;
			set
			{
				base.NotifyOnSuccess = value;
				Validation.ValidateNotificationGroup_PK();
			}
		}

		public sealed override ZBool NotifyOnFailure
		{
			get => base.NotifyOnFailure;
			set
			{
				base.NotifyOnFailure = value;
				Validation.ValidateNotificationGroup_PK();
			}
		}

		public sealed override ZBool NotifyPrintUser
		{
			get => base.NotifyPrintUser;
			set
			{
				base.NotifyPrintUser = value;
				Validation.ValidateNotificationGroup_PK();
			}
		}

		public sealed override ZGuid NotificationGroup_PK { get => base.NotificationGroup_PK; set => base.NotificationGroup_PK = value; }

		public GlbGroup NotificationGroup => Factory.Load<GlbGroup>(NotificationGroup_PK);

		class FtpJobConfigXml : XmlSerializableSetting
		{
			public FtpJobConfigXml()
			{
				NotifyPrintUser = false;
				NotifyOnSuccess = false;
				NotifyOnFailure = false;
				NotificationGroup_PK = ZGuid.Empty;
			}

			public bool NotifyPrintUser { get; set; }
			public bool NotifyOnSuccess { get; set; }
			public bool NotifyOnFailure { get; set; }
			public ZGuid NotificationGroup_PK { get; set; }
		}
	}
}
