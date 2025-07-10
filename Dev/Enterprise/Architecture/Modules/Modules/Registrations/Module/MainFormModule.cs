using System;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Core.Modules;
using Enterprise.Integration.Licensing;
using Enterprise.Integration.Modules;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Modules
{
	public class MainFormModule : IMainFormModule
	{
		#region Constructor

		protected MainFormModule()
		{
		}

		public MainFormModule(ModuleIdentifier moduleID, Guid recordKey, string recordUrl, string recordDescription)
		{
			ModuleID = Argument.NotNull(moduleID, nameof(moduleID));
			RecordUrl = recordUrl;
			RecordKey = recordKey;

			if (!string.IsNullOrEmpty(recordDescription))
			{
				RecordDescription = recordDescription;
			}
		}

		public MainFormModule(ModuleIdentifier moduleID)
			: this(moduleID, Guid.Empty, null, null)
		{
		}

		public MainFormModule(ModuleInfo moduleInfo)
			: this(moduleInfo.ID)
		{
			description = moduleInfo.Description;
		}

		#endregion

		public bool IsPopup
		{
			get { return ModuleProperties != null && ModuleProperties.IsPopup; }
		}

		public ModuleSection ParentSection { get; set; }

		public string RecordUrl { get; set; }

		public string RecordDescription
		{
			get { return extendedDescription; }
			set { extendedDescription = (NoResString)value; }
		}

		public Guid RecordKey { get; set; }

		#region Security / Licence

		public ISecurityCheckpoint SecurityCheckpoint
		{
			get
			{
				var moduleEnvironment = ObjectFactory.Get<IModuleEnvironment>();
				var securityCheckpoint = ModuleProperties != null ? ModuleProperties.SecurityCheckpoint : moduleEnvironment.SecurityNone;
				if (Description != null && securityCheckpoint != null && securityCheckpoint != moduleEnvironment.SecurityNone
					&& securityCheckpoint.DisplayText.GetUnresolvedString() != Description.GetUnresolvedString()
					&& securityCheckpoint?.Parent?.LookupKey != null
					&& securityCheckpoint.Parent.LookupKey == ParentSection?.SecurityCheckpoint?.LookupKey)
				{
					securityCheckpoint.SetDisplayText(Description);
				}
				return securityCheckpoint;
			}
		}
		public ISecurityCheckpoint[] GetSecurityCheckpointForPopups()
		{
			return null;
		}

		public ILicenceCheckpoint LicenceCheckpoint
		{
			get
			{
				if (licenceCheckpoint == null)
				{
					licenceCheckpoint = ModuleProperties != null ? ModuleProperties.LicenceCheckpoint : ObjectFactory.Get<IModuleEnvironment>().LicenceCore;
				}
				return licenceCheckpoint;
			}
			set { licenceCheckpoint = value; }
		}

		ILicenceCheckpoint licenceCheckpoint;

		#endregion

		public IZModule CreateZModule()
		{
			return ObjectFactory.Get<IModuleFactory>().Create(ModuleID);
		}

		InnerModuleProperties ModuleProperties
		{
			get
			{
				if (innerModuleProperties == null)
				{
					using (IZModule innerModule = CreateZModule())
					{
						if (innerModule != null)
						{
							innerModuleProperties = new InnerModuleProperties(innerModule);
						}
					}
				}
				return innerModuleProperties;
			}
		}

		class InnerModuleProperties
		{
			public InnerModuleProperties(INamedModule innerModule)
			{
				IsPopup = innerModule is IZPopupModule;
				SecurityCheckpoint = innerModule.SecurityCheckpoint;
				LicenceCheckpoint = innerModule.LicenceCheckpoint;
			}

			public bool IsPopup { get; private set; }
			public ISecurityCheckpoint SecurityCheckpoint { get; private set; }
			public ILicenceCheckpoint LicenceCheckpoint { get; private set; }
		}

		InnerModuleProperties innerModuleProperties;

		#region INamedModule

		public string ID
		{
			get { return string.IsNullOrEmpty(RecordUrl) ? ModuleID.ToString() : ModuleID.ToString() + RecordKey.ToString(); }
		}

		public string ModuleTreeID
		{
			get
			{
				return ID.Substring(0, Math.Min(ID.Length, MaxModuleTreeIdLength));
			}
		}

		internal int MaxModuleTreeIdLength = 50;

		public MultilingualString Description
		{
			get { return description ?? (description = ModuleID.Description); }
		}
		MultilingualString description;

		public MultilingualString ExtendedDescription
		{
			get { return extendedDescription ?? (extendedDescription = ModuleID.ExtendedDescription); }
		}
		MultilingualString extendedDescription;

		public ModuleIdentifier ModuleID { get; set; }

		#endregion
	}
}
