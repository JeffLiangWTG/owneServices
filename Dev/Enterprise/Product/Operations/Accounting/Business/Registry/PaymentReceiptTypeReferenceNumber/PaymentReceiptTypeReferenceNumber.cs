using System.ComponentModel;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class PaymentReceiptTypeReferenceNumber : RegistryBusinessObjectTemplate
	{
		public PaymentReceiptTypeReferenceNumber()
		{
		}

		public PaymentReceiptTypeReferenceNumber(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}
		#region Schema

		public abstract class Schema
		{
			public const string Type = "Type";
			public const string ReferenceNumber = "ReferenceNumber";
		}

		#endregion

		#region Overrides

		#region GetClone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PaymentReceiptTypeReferenceNumber(fallbackLevel, factory);
		}

		#endregion

		#region WriteElements

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Type, Type);
			writer.WriteElementString(Schema.ReferenceNumber, ReferenceNumber);
		}

		#endregion

		#region ReadElements

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Type = reader.ReadElementString(Schema.Type);
			ReferenceNumber = (NoResString)reader.ReadElementString(Schema.ReferenceNumber);
		}

		#endregion

		#endregion

		#region Bound Properties

		#region Type

		ZString type;

		[MaxLength(3)]
		[ReadOnly(true)]
		public ZString Type
		{
			get { return type; }
			set
			{
				CheckMaximumLength(TypeInfo, value);
				SetNonPersistentPropertyValue(TypeInfo, ref type, value, false);
			}
		}

		public ZPropertyInfo TypeInfo
		{
			get { return GetZPropertyInfo(Schema.Type); }
		}

		#endregion
		#region ReferenceNumber

		ZString referenceNumber;

		[MaxLength(20)]
		public ZString ReferenceNumber
		{
			get { return referenceNumber;  }
			set
			{
				CheckMaximumLength(ReferenceNumberInfo, value);
				SetNonPersistentPropertyValue(ReferenceNumberInfo, ref referenceNumber, value, false);
			}
		}

		public ZPropertyInfo ReferenceNumberInfo
		{
			get { return GetZPropertyInfo(Schema.ReferenceNumber); }
		}

		#endregion

		#endregion
	}
}
