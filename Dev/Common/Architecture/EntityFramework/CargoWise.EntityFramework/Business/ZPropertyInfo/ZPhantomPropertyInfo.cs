using System;
using System.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	internal sealed class ZPhantomPropertyInfo : ZPropertyInfo
	{
		public ZPhantomPropertyInfo(BusinessObject bizObj, PropertyDescriptor property) : base(bizObj, property.Name, property) { }

		protected override IZType ValueCore
		{
			get { return new ZGuid(); }
			set { throw new ApplicationException("Value of ZPhantomPropertyInfo should never be set"); }
		}

		protected override Type PropertyTypeCore
		{
			get { return typeof(ZGuid); }
		}
	}
}
