using System;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public static class IDataObjectExtensions
	{
		public static T AdditionalSetup<T>(this T data, Action<T> additionalSetup)
			where T : IDataObject
		{
			additionalSetup(data);
			return data;
		}
	}
}
