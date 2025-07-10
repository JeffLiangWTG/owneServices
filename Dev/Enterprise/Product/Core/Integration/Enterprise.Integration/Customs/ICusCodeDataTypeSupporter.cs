using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusCodeDataTypeSupporter : IAdditionalBusinessObjectFetchStrategyProvider
		{
			ZGuid PK { get; }
			BusinessObjectFactory Factory { get; }
			IDictionary<ZString, Type> GetCusCodeDataTypes();
		}
	}
}
