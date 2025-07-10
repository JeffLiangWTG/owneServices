using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.JP.Common
{
	[XmlSerializerAssembly("Enterprise.Customs.JP.Common.XmlSerializers")]
	public class FTPSettings : RegistryBusinessObjectTemplate
	{
		public FTPSettings() : base()
		{
		}

		public FTPSettings(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		public static class Schema
		{
			public const string InFolder = "InFolder";
			public const string OutFolder = "OutFolder";
			public const string Passive = "Passive";
			public const string Password = "Password";
			public const string Port = "Port";
			public const string Server = "Server";
			public const string UserName = "UserName";
			public const string Status = "Status";
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			Port = 21;
		}

		[ResourceStringData("Enterprise.Customs.JP.Business.FTPSettings|InFolder", Caption = "In Folder")]
		public ZString InFolder
		{
			get => inFolder;
			set
			{
				if (inFolder != value)
				{
					SetNonPersistentPropertyValue(InFolderInfo, ref inFolder, value);
					InFolderInfo.RefreshBinding();
					Status = string.Empty;
				}
			}
		}

		ZString inFolder;

		public ZPropertyInfo InFolderInfo => GetZPropertyInfo(Schema.InFolder);

		[ResourceStringData("Enterprise.Customs.JP.Business.FTPSettings|OutFolder", Caption = "Out Folder")]
		public ZString OutFolder
		{
			get => outFolder;
			set
			{
				if (outFolder != value)
				{
					SetNonPersistentPropertyValue(OutFolderInfo, ref outFolder, value);
					OutFolderInfo.RefreshBinding();
					Status = string.Empty;
				}
			}
		}

		ZString outFolder;

		public ZPropertyInfo OutFolderInfo => GetZPropertyInfo(Schema.OutFolder);

		[ResourceStringData("Enterprise.Customs.JP.Business.FTPSettings|Passive", Caption = "Passive")]
		public ZBool Passive
		{
			get => passive;
			set
			{
				if (passive != value)
				{
					SetNonPersistentPropertyValue(PassiveInfo, ref passive, value);
					PassiveInfo.RefreshBinding();
					Status = string.Empty;
				}
			}
		}

		ZBool passive;

		public ZPropertyInfo PassiveInfo => GetZPropertyInfo(Schema.Passive);

		[ResourceStringData("Enterprise.Customs.JP.Business.FTPSettings|YesSelection", Caption = "Yes")]
		public ZBool PassiveYesSelection
		{
			get => Passive;
			set => Passive = value;
		}

		[ResourceStringData("Enterprise.Customs.JP.Business.FTPSettings|NoSelection", Caption = "No")]
		public ZBool PassiveNoSelection
		{
			get => !Passive;
			set => Passive = !value;
		}

		[ResourceStringData("Enterprise.Customs.JP.Business.FTPSettings|Password", Caption = "Password")]
		public ZString Password
		{
			get => password;
			set
			{
				if (password != value)
				{
					SetNonPersistentPropertyValue(PasswordInfo, ref password, value);
					PasswordInfo.RefreshBinding();
					Status = string.Empty;
				}
			}
		}

		ZString password;

		public ZPropertyInfo PasswordInfo => GetZPropertyInfo(Schema.Password);

		[ResourceStringData("Enterprise.Customs.JP.Business.FTPSettings|Port", Caption = "Port")]
		public ZInt Port
		{
			get => port;
			set
			{
				if (port != value)
				{
					SetNonPersistentPropertyValue(PortInfo, ref port, value);
					PortInfo.RefreshBinding();
					Status = string.Empty;
				}
			}
		}

		ZInt port;

		public ZPropertyInfo PortInfo => GetZPropertyInfo(Schema.Port);

		[ResourceStringData("Enterprise.Customs.JP.Business.FTPSettings|Server", Caption = "Server")]
		public ZString Server
		{
			get => server;
			set
			{
				if (server != value)
				{
					SetNonPersistentPropertyValue(ServerInfo, ref server, value);
					ServerInfo.RefreshBinding();
					Status = string.Empty;
				}
			}
		}

		ZString server;

		public ZPropertyInfo ServerInfo => GetZPropertyInfo(Schema.Server);

		[ResourceStringData("Enterprise.Customs.JP.Business.FTPSettings|UserName", Caption = "User Name")]
		public ZString UserName
		{
			get => userName;
			set
			{
				if (userName != value)
				{
					SetNonPersistentPropertyValue(UserNameInfo, ref userName, value);
					UserNameInfo.RefreshBinding();
					Status = string.Empty;
				}
			}
		}

		ZString userName;

		public ZPropertyInfo UserNameInfo => GetZPropertyInfo(Schema.UserName);

		[ResourceStringData("Enterprise.Customs.JP.Business.FTPSettings|Status", Caption = "Status")]
		public ZString Status
		{
			get => status;
			set
			{
				if (status != value)
				{
					SetNonPersistentPropertyValue(StatusInfo, ref status, value);
					StatusInfo.RefreshBinding();
				}
			}
		}

		ZString status;

		public ZPropertyInfo StatusInfo => GetZPropertyInfo(Schema.Status);

		public bool IsEmpty => Server.IsEmpty || InFolder.IsEmpty || OutFolder.IsEmpty || UserName.IsEmpty || Password.IsEmpty;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new FTPSettings(fallbackLevel, factory);

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			InFolder = reader.ReadElementString(Schema.InFolder);
			OutFolder = reader.ReadElementString(Schema.OutFolder);
			Passive = reader.ReadElementStringAsZBool(Schema.Passive);
			Password = reader.ReadElementString(Schema.Password);
			Port = reader.ReadElementStringAsZInt(Schema.Port);
			Server = reader.ReadElementString(Schema.Server);
			UserName = reader.ReadElementString(Schema.UserName);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.InFolder, InFolder);
			writer.WriteElementString(Schema.OutFolder, OutFolder);
			writer.WriteElementString(Schema.Passive, Passive.ToString());
			writer.WriteElementString(Schema.Password, Password);
			writer.WriteElementString(Schema.Port, Port.ToString());
			writer.WriteElementString(Schema.Server, Server);
			writer.WriteElementString(Schema.UserName, UserName);
		}

		FTPSettingsValidation validation;

		public FTPSettingsValidation Validation => validation ?? (validation = new FTPSettingsValidation(this));

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}
	}
}
