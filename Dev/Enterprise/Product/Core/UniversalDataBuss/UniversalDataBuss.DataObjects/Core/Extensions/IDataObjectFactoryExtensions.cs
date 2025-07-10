using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public static class IDataObjectFactoryExtensions
	{
		public static T Create<T>(this IDataObjectFactory factory)
			where T : IDataObject
		{
			return (T)factory.Create(typeof(T));
		}
	}
}
