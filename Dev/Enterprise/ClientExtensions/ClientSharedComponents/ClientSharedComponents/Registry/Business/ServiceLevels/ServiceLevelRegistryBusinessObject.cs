using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ClientSharedComponents.Registry
{
	[XmlSerializerAssembly("Enterprise.ClientSharedComponents.XmlSerializers")]
	public class ServiceLevelRegistryBusinessObject : RegistryBusinessObjectTemplate
	{
		public ServiceLevelRegistryBusinessObject() { }

		public ServiceLevelRegistryBusinessObject(BusinessObjectFactory factory) : base(factory) { }

		#region Schema

		public abstract class Schema
		{
			public const string ServiceLevel = "ServiceLevel";
		}

		#endregion

		#region Overrides

		#region GetClone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ServiceLevelRegistryBusinessObject(factory);
		}

		#endregion

		#region WriteElements

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ServiceLevel, ServiceLevel.ToString());
		}

		#endregion

		#region ReadElements

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ServiceLevel = reader.ReadElementString(Schema.ServiceLevel);
		}

		#endregion

		#region RunPreSaveValidationCore

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateServiceLevel();
		}

		#endregion

		#endregion

		#region Bound Properties

		#region ServiceLevel

		[MaxLength(3)]
		public ZString ServiceLevel
		{
			get { return fServiceLevel; }
			set
			{
				CheckMaximumLength(ServiceLevelInfo, value);
				fServiceLevel = value;
				if (!IsValidationSuspended)
				{
					ValidateServiceLevel();
				}

				ServiceLevelInfo.RefreshBinding();
			}
		}
		ZString fServiceLevel;

		public ZPropertyInfo ServiceLevelInfo
		{
			get { return GetZPropertyInfo(Schema.ServiceLevel); }
		}

		void ValidateServiceLevel()
		{
			ServiceLevelInfo.ClearAllNotifications();

			if (!ServiceLevel.IsValid || ServiceLevelCode == "")
			{
				ServiceLevelInfo.AddError("Please enter a valid service level code.");
			}

			if (ParentCollections.Count > 0 && ((ServiceLevelRegistryBusinessObjectCollection)ParentCollections.First()).IsDuplicateItem(this))
			{
				ServiceLevelInfo.AddError("A duplicate service level already exists.");
			}
		}

		#endregion

		#endregion

		#region SendingAgentCollection

		OrgHeaderCollection fSendingAgentCollection;
		public OrgHeaderCollection SendingAgentCollection
		{
			get
			{
				if (fSendingAgentCollection == null)
				{
					fSendingAgentCollection = new OrgHeaderCollection(ReadOnlyFactory, new ZQuery(OrgHeaderSchema.OH_IsForwarder, ZBool.True));
				}

				return fSendingAgentCollection;
			}
		}

		#endregion

		#region ServiceLevelCode

		public ZString ServiceLevelCode
		{
			get
			{
				RefServiceLevel serviceLevelValue = (RefServiceLevel)ReadOnlyFactory.LoadFromNaturalKey(typeof(RefServiceLevel), RefServiceLevelSchema.RS_Code, ServiceLevel);
				return (serviceLevelValue != null) ? serviceLevelValue.RS_Code : ZString.Empty;
			}
		}

		#endregion

		#region ServiceLevelCollection

		RefServiceLevelCollection fServiceLevelCollection;
		public RefServiceLevelCollection ServiceLevelCollection
		{
			get
			{
				if (fServiceLevelCollection == null)
				{
					fServiceLevelCollection = new RefServiceLevelCollection((ReadOnlyFactory));
				}

				return fServiceLevelCollection;
			}
		}

		#endregion

		#region ReadOnlyFactory

		BusinessObjectFactory fReadOnlyFactory;
		protected BusinessObjectFactory ReadOnlyFactory
		{
			get
			{
				if (fReadOnlyFactory == null)
				{
					fReadOnlyFactory = base.Factory ?? new BusinessObjectFactory();
				}
				return fReadOnlyFactory;
			}
		}

		#endregion
	}
}
