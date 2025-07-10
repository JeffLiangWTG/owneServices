using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class PickGroupCollection : RegistryBusinessObjectCollectionTemplate<PickGroup>, ICodeDescriptionPairList
	{
		public PickGroupCollection()
			: base(null, null)
		{
		}

		#region Add

		protected override bool AllowNewCore
		{
			get { return true; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PickGroup();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var pickGroup = (PickGroup)child;
			if (pickGroup != null)
			{
				pickGroup.PickSequence = Count > 0 ? this.Cast<PickGroup>().Max(o => o.PickSequence) + 1 : new ZShort(1);
			}
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PickGroupCollection();
		}

		#endregion

		#region ICodeDescriptionPairList Members

		bool ICodeDescriptionPairList.ContainsCode(object code)
		{
			var codeAsString = code.ToString();
			return this.Cast<PickGroup>().Any(o => o.Code == codeAsString);
		}

		string ICodeDescriptionPairList.GetDescriptionFromCode(string code)
		{
			var pickGroup = this.Cast<PickGroup>().FirstOrDefault(o => o.Code == code);
			return (pickGroup != null) ? pickGroup.Description : "";
		}

		#endregion
	}
}
