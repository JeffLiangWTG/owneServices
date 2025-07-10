using System.Collections.Generic;
using System.Xml.Linq;

namespace Enterprise.DataTransfer.Native.Business.Xsd.Type
{
	public abstract class XsdDataType : NativeXsd
	{
		public abstract string Name { get; }

		public virtual XObject ToXsdElement()
		{
			return new XAttribute(Tag.ColumnType, Name);
		}
	}

	public interface INamedXsdDataType
	{
		/// <summary>
		/// Not all XsdDataTypes support converting to a complex type.
		/// Those that do can produce a complex type instead of its normal
		/// simple type. This complex type will be declared at the top-level
		/// of the schema so that it can be referred by the Element producing it.
		///
		/// The purpose of this method is to support attributes on an otherwise
		/// simple type.
		/// </summary>
		/// <param name="referenceName">
		/// The string used to refer to the named-type. Should be unique
		/// </param>
		/// <param name="mandatoryAttributes">
		/// </param>
		/// <returns>
		/// If implemented, returns the attribute to refer to the complex type
		/// and the complex type that got generated.
		/// </returns>
		(XObject ReferenceObject, XElement NamedObject) ToXsdElementWithAttributes(string referenceName, IEnumerable<XObject> mandatoryAttributes);
	}
}
