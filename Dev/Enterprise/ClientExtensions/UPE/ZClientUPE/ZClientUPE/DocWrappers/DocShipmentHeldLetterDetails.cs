using System;

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Client.UPE.Business
{
	public class DocShipmentHeldLetterDetails : DocumentWrapper
	{
		protected DocShipmentHeldLetterDetails(ShipmentHeldLetterBusinessObject bizObj, BusinessObjectFactory factoryToWrap)
			: base(bizObj, factoryToWrap)
		{
		}

		public static DocShipmentHeldLetterDetails New(ShipmentHeldLetterBusinessObject bizObj, BusinessObjectFactory factoryToWrap)
		{
			return new DocShipmentHeldLetterDetails(bizObj, factoryToWrap);
		}

		public new ShipmentHeldLetterBusinessObject WrappedObject
		{
			get { return (ShipmentHeldLetterBusinessObject)base.WrappedObject; }
		}

		public ZString UPSContactName
		{
			get { return WrappedObject.UPSContactName; }
		}

		public ZString UPSContactPhone
		{
			get { return WrappedObject.UPSContactPhone; }
		}

		public ZString UPSContactFax
		{
			get { return UPEDataRegistry.Instance.UPSContactFax.Value; }
		}

		public ZString UPSContactEmail
		{
			get { return UPEDataRegistry.Instance.UPSContactEmail.Value; }
		}

		public ZString ReasonText
		{
			get { return WrappedObject.ReasonText; }
		}

		public override string ToString()
		{
			throw new NotSupportedException();
		}
	}
}
