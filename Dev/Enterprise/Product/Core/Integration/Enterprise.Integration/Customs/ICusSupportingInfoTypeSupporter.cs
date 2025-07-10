using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusSupportingInfoTypeSupporter : IAdditionalBusinessObjectFetchStrategyProvider
		{
			ZGuid PK { get; }
			BusinessObjectFactory Factory { get; }
			bool IsInDatabase { get; }
			IDictionary<ZString, Type> GetCusSupportingInfoTypes();
		}
	}
}
