using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1039:ListsAreStronglyTyped")]
	public class TestRigRegistryCollection : RegistryBusinessObjectCollectionTemplate
	{
		public TestRigRegistryCollection()
		{
		}

		public TestRigRegistryCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public IEnumerable<TestRigRegistryOptions> GetOptionsForWorkItemCombination(string product, string productArea, string module, string changeType)
		{
			return this.Cast<TestRigRegistryOptions>().Where(x => x.Product == product && x.ProductArea == productArea && x.Module == module && x.ChangeType == changeType);
		}

		public new TestRigRegistryOptions this[int index] => (TestRigRegistryOptions)Elements[index];

		public new TestRigRegistryOptions AddNew()
		{
			return (TestRigRegistryOptions)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TestRigRegistryOptions(CurrentFallbackLevel, CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TestRigRegistryCollection(fallbackLevel, factory);
		}

		public void CopyTo(TestRigRegistryOptions[] array, int index)
		{
			Elements.ToArray().CopyTo(array, index);
		}
	}
}

