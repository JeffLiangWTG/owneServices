using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Customs.CA.DataTransfer
{
	public class CATCPHeaderValueObject : NonPersistentBusinessObject, IValueObject
	{
		public CATCPHeaderValueObject() : base(null)
		{
		}

		[XmlIgnore]
		public ZString RecordIdentifier { get; set; }

		[XmlIgnore]
		public ZString BusinessNumber { get; set; }

		[XmlIgnore]
		public CATCPLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new CATCPLineCollection();
				}
				return lines;
			}
		}
		CATCPLineCollection lines;

		[XmlIgnore]
		public bool IsSpecified
		{
			get { return true; }
		}

		[XmlIgnore]
		public bool ShouldCreateElementForEmptyValue { get; set; }
	}
}
