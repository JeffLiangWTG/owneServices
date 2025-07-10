using Enterprise.Core.Environment;

namespace Enterprise.Environment
{
	public interface IWebEnvironment : IEnvironment
	{
		IContactBase WebUser { get; }
	}
}
