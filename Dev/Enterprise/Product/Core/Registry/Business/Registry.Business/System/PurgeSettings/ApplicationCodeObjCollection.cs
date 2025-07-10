using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ApplicationCodeObjCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ApplicationCodeObjCollection() : base() { }

		public new ApplicationCodeObj this[int i]
		{
			get { return (ApplicationCodeObj)base[i]; }
		}

		public new ApplicationCodeObj AddNew()
		{
			return (ApplicationCodeObj)base.AddNew();
		}

		public void AddApplicationCode(string code, InterchangeObjCollection interchanges, MessageTypeObjCollection messageTypes)
		{
			var appCode = this.AddNew();
			appCode.ApplicationCode = code;
			appCode.Interchanges.AddRange(interchanges);
			appCode.SetMessageTypes(messageTypes);
		}

		public ApplicationCodeObj GetApplicationCodeObj(string code)
		{
			return this.OfType<ApplicationCodeObj>().ToList().Find(x => x.ApplicationCode == code);
		}

		#region Clone

		protected sealed override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ApplicationCodeObjCollection();
		}

		#endregion

		#region Override

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ApplicationCodeObj();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}
