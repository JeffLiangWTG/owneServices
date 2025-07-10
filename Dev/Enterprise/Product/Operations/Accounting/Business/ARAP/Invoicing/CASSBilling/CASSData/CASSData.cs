using System.ComponentModel;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract class CASSData : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected CASSData(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		[ReadOnly(true)]
		[XmlIgnore]
		public ZString RecordType
		{
			get { return recordType; }
			set { SetNonPersistentPropertyValue(RecordTypeInfo, ref recordType, value); }
		}
		ZString recordType;

		[XmlIgnore]
		public ZPropertyInfo RecordTypeInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(RecordType));
			}
		}
	}
}
