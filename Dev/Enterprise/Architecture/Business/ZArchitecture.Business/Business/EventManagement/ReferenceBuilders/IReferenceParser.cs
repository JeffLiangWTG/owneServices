using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public interface IReferenceParser<T> where T : IReferenceVersion
	{
		int Version { get; }

		Event Event { get; }

		void Deserialise(T data, ZString reference);

		string Serialise(T data);
	}
}
