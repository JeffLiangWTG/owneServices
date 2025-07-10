using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class PaymentReceiptTypeReferenceNumberCollection : RegistryBusinessObjectCollectionTemplate
	{
		public PaymentReceiptTypeReferenceNumberCollection()
		{
		}

		public PaymentReceiptTypeReferenceNumberCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public new PaymentReceiptTypeReferenceNumber this[int index]
		{
			get { return (PaymentReceiptTypeReferenceNumber)Elements[index]; }
		}

		public new PaymentReceiptTypeReferenceNumber AddNew()
		{
			return (PaymentReceiptTypeReferenceNumber)base.AddNew();
		}

		public void AddDefaultValues(string[] types)
		{
			foreach (var type in types)
			{
				var element = new PaymentReceiptTypeReferenceNumber();
				element.Type = type;
				Add(element);
			}
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{ return new PaymentReceiptTypeReferenceNumberCollection(fallbackLevel, factory); }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{ return new PaymentReceiptTypeReferenceNumber(CurrentFallbackLevel, CurrentFactory); }

		public ZString GetValueFromType(string type)
		{
			string trimmedCode = type.Trim();
			foreach (PaymentReceiptTypeReferenceNumber element in this)
			{
				if (String.Equals(element.Type.Trim(), trimmedCode, StringComparison.OrdinalIgnoreCase))
				{
					return element.ReferenceNumber;
				}
			}
			return ZString.Empty;
		}
	}
}
