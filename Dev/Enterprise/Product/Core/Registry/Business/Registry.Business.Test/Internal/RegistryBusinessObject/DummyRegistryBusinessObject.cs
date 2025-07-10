using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.Test.XmlSerializers")]
	public sealed class DummyRegistryBusinessObject : RegistryBusinessObject
	{
		public DummyRegistryBusinessObject()
		{
		}

		public DummyRegistryBusinessObject(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new int CodeMaxLengthDefaultValue
		{
			get { return base.CodeMaxLengthDefaultValue; }
		}

		public new int MaxDescriptionLength
		{
			get { return base.MaxDescriptionLength; }
		}

		public new RegistryBusinessObjectCollectionTemplate GetParentCollection(RegistryBusinessObjectTemplate businessObject, Type parentCollectionType)
		{
			return base.GetParentCollection(businessObject, parentCollectionType);
		}

		protected override void ValidateCodeCore()
		{
			base.ValidateCodeCore();
			ValidateCodeCalled = true;
		}

		protected override void ValidateDescriptionCore()
		{
			base.ValidateDescriptionCore();
			ValidateDescriptionCalled = true;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DummyRegistryBusinessObject(factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			((DummyRegistryBusinessObject)clone).SomeString = SomeString;
		}

		protected override bool IsCodeMandatory
		{
			get { return IsCodeMandatory_Exposed; }
		}

		public bool IsCodeMandatory_Exposed = true;

		public bool ValidateCodeCalled;
		public bool ValidateDescriptionCalled;
		public string SomeString;
	}
}
