using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class DropDownCodeDescriptionBool : CodeDescriptionBool
	{
		public DropDownCodeDescriptionBool()
		{
		}

		public DropDownCodeDescriptionBool(DropDownCodeDescriptionBoolCollection parent)
		{
			this.Parent = parent;
		}

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DropDownCodeDescriptionBool(Parent);
		}

		#endregion

		[List("Parent.CodeLookup")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member CodeAndDescriptionReadOnly")]
		[ReadOnlyMember("CodeAndDescriptionReadOnly")]
		public override ZString Code
		{
			get { return base.Code; }
			set { base.Code = value; }
		}

		[List("Parent.DescriptionLookup")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member CodeAndDescriptionReadOnly")]
		[ReadOnlyMember("CodeAndDescriptionReadOnly")]
		public override ZString EnglishDescription
		{
			get { return base.EnglishDescription; }
			set { base.EnglishDescription = value; }
		}

		[XmlIgnore]
		[BusinessObjectTestExclude]
		public DropDownCodeDescriptionBoolCollection Parent
		{
			get;
			set;
		}
	}
}
