using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PackProductWrapperCollection : GenericWrapperCollection<PackProductWrapper>
	{
		public PackProductWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public PackProductWrapperCollection(ForwardingPackLine packLine, BusinessObjectFactory factory)
			: this(factory)
		{
			if (packLine != null)
			{
				foreach (PackProduct product in packLine.Products)
				{
					Add(new PackProductWrapper(product, factory));
				}
			}
		}

		[ThreadStatic]
		static PackProductWrapperCollection empty;
		public static PackProductWrapperCollection Empty
		{
			get { return empty ?? (empty = new PackProductWrapperCollection(null)); }
		}
	}
}
