using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class FreeWaitingTimeCollection : RegistryBusinessObjectCollectionTemplate
	{
		public FreeWaitingTimeCollection()
			: base()
		{ }

		public FreeWaitingTimeCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{ }

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new FreeWaitingTimeCollection(fallbackLevel, factory);
		}

		#region AddNew

		public new FreeWaitingTime AddNew()
		{
			return (FreeWaitingTime)base.AddNew();
		}

		#endregion

		#region CreateNonPersistentBusinessObject

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new FreeWaitingTime(CurrentFallbackLevel, CurrentFactory);
		}

		#endregion

		#region AllowNewCore

		protected override bool AllowNewCore
		{
			get { return true; }
		}

		#endregion

		#region Index

		public new FreeWaitingTime this[int i]
		{
			get
			{
				return (FreeWaitingTime)Elements[i];
			}
		}

		#endregion

		#region FreeWaitingTimeList

		public HashedBizOList FreeWaitingTimeList
		{
			get { return Elements; }
		}

		#endregion

		#region GetDefault

		public static FreeWaitingTimeCollection GetDefault()
		{
			return new FreeWaitingTimeCollection();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			((FreeWaitingTime)child).DropMode = Constants.EquipmentNeeded.Any;
			base.SetDefaultsForNewChild(child);
		}

		#endregion
	}
}
