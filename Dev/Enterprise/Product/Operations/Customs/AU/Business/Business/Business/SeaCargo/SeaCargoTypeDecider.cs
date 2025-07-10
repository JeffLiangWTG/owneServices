using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class SeaCargoTypeDecider<T> : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(T);
		}

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			return typeof(T);
		}

		public override Type GetTypeForNew()
		{
			return typeof(T);
		}
	}
}
