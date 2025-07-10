using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	public class OrgWebURLCollection : Xsd.AutoOrgWebURLCollection, ISequencedValueObjectCollection
	{
		protected override void OnInsertComplete(int index, object value)
		{
			base.OnInsertComplete(index, value);
			new XsdSequenceIncrementHelper(this).AssignNewIncrementedSequence((ISequencedValueObject)value);
		}
	}
}
