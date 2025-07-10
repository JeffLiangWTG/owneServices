using System.Diagnostics.CodeAnalysis;

namespace ServiceManager.Runner.Abstractions
{
	public interface IResourceManagement
	{
		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		void ReclaimMemory(ref IServiceTaskHandler serviceTaskHandler);
	}
}
