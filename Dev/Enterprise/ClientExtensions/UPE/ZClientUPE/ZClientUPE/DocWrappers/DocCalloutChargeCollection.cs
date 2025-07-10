using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Client.UPE.Business
{
	public class DocCalloutChargeCollection : DocumentWrapperCollection
	{
		public DocCalloutChargeCollection(BusinessObjectFactory factoryToWrap)
			: base(factoryToWrap)
		{
		}

		public DocCalloutChargeCollection(CalloutChargeCollection collection, BusinessObjectFactory factoryToWrap)
			: this(factoryToWrap)
		{
			foreach (CalloutCharge charge in collection)
			{
				Add(DocCalloutCharge.New(charge, factoryToWrap));
			}
		}

		public new DocCalloutCharge this[int i]
		{
			get { return (DocCalloutCharge)base[i]; }
		}

		public new DocCalloutCharge AddNew()
		{
			throw new NotSupportedException();
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException();
		}
	}
}
