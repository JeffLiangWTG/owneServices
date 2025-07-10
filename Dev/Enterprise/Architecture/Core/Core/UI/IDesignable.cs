using CargoWise.Common;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Core
{
	public static class DesignModeFinder
	{
		[ValidExceptionFilterMember]
		public static bool IsDesigning
		{
			get { return VisualStudioDetector.IsVisualStudio; }
		}

#if DEBUG
		[ThreadSafe]
		static IAssemblyLoader assemblyLoader;

		public static void SetIsDesigningForTest(bool value)
		{
			if (value)
			{
				assemblyLoader = AssemblyLoader.Instance;
			}
			else if (assemblyLoader != null)
			{
				AssemblyLoader.Instance = assemblyLoader;
			}
			VisualStudioDetector.IsVisualStudio = value;
		}
#endif
	}
}
