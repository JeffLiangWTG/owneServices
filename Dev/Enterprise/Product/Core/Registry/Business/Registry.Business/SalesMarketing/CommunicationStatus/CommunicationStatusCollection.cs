using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CommunicationStatusCollection : CodeDescriptionBoolCollection
	{
		public CommunicationStatusCollection()
			: this(false, true)
		{
		}

		public CommunicationStatusCollection(bool defaultClosedForNewChild, bool defaultBoolForNewChild)
			: base(null, defaultBoolForNewChild, 3)
		{
			this.DefaultClosedForNewChild = defaultClosedForNewChild;
		}

		internal bool DefaultClosedForNewChild;

		public new CommunicationStatus this[int i]
		{
			get { return (CommunicationStatus)base[i]; }
		}

		public new CommunicationStatus AddNew()
		{
			return (CommunicationStatus)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CommunicationStatus();
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new CommunicationStatusCollection(DefaultClosedForNewChild, DefaultBoolForNewChild);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((CommunicationStatus)child).Closed = DefaultClosedForNewChild;
		}

		public CommunicationStatus Add(ZString code, MultilingualString description, bool closed, bool booleanValue)
		{
			var result = (CommunicationStatus)base.Add(code, description, booleanValue);
			result.Closed = closed;
			return result;
		}

		public CommunicationStatus AddSystemDefined(ZString code, MultilingualString description, bool closed, bool booleanValue)
		{
			var result = (CommunicationStatus)base.AddSystemDefined(code, description, booleanValue);
			result.Closed = closed;
			return result;
		}

		public bool GetClosedFromCode(string code)
		{
			var element = (CommunicationStatus)FindByCode(code);
			return (element == null) ? ZBool.False : element.Closed;
		}
	}
}
