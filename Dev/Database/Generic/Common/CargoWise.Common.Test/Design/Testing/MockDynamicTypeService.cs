using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;

namespace CargoWise.Common.Design.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockDynamicTypeService
	{
		public MockDynamicTypeService()
		{
		}

		public MockDynamicTypeService(ITypeResolutionService activeResolver)
		{ this.activeResolver = activeResolver; }

		public Assembly CreateDynamicAssembly(string assemblyPath)
		{
			Argument.NotNull(assemblyPath, nameof(assemblyPath)); // Suggested By ReviewBot 
			return Assembly.LoadFrom(assemblyPath);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used via reflection")]
		ITypeResolutionService ActiveResolver
		{ get { return activeResolver; } }
		readonly ITypeResolutionService activeResolver;
	}
}
