using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using System.Threading;

namespace CargoWise.Common.Design.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockTypeResolutionService : ITypeResolutionService
	{
		readonly Assembly projectAssembly;

		public MockTypeResolutionService(Assembly projectAssembly)
		{
			Argument.NotNull(projectAssembly, nameof(projectAssembly));
			this.projectAssembly = projectAssembly;
		}

		public Thread ThrowIfNotOnThread;
		public readonly List<Type> TypesToNotFind = new List<Type>();

		public Assembly GetAssembly(AssemblyName name)
		{ return GetAssembly(name, false); }

		public Assembly GetAssembly(AssemblyName name, bool throwOnError)
		{
			CheckCalledOnCorrectThread();

			Assembly result = Assembly.Load(name);
			if (result == null && throwOnError)
			{
				throw new InvalidOperationException();
			}
			return result;
		}

		public string GetPathOfAssembly(AssemblyName name)
		{
			CheckCalledOnCorrectThread();
			var assembly = Assembly.Load(name);
			if (assembly != null)
			{
				return new Uri(assembly.Location).LocalPath;
			}
			else
			{
				throw new NullReferenceException();
			}
		}

		public Type GetType(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}
			return GetType(name, false, false);
		}

		public Type GetType(string name, bool throwOnError)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}
			return GetType(name, throwOnError, false);
		}

		public Type GetType(string name, bool throwOnError, bool ignoreCase)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}
			CheckCalledOnCorrectThread();
			return GetType(projectAssembly, name, throwOnError, ignoreCase);
		}

		public void ReferenceAssembly(AssemblyName name)
		{ throw new Exception("The method or operation is not implemented."); }

		#region Implementation

		Type GetType(Assembly assembly, string name, bool throwOnError, bool ignoreCase)
		{
			Argument.NotNull(assembly, nameof(assembly)); // Suggested By ReviewBot
			Argument.NotNull(name, nameof(name)); // Suggested By ReviewBot
			Type result = assembly.GetType(name, false, ignoreCase);
			if (result == null)
			{
				foreach (AssemblyName referencedAssemblyName in assembly.GetReferencedAssemblies())
				{
					Assembly referencedAssembly = Assembly.Load(referencedAssemblyName);
					if (referencedAssembly != null)
					{
						result = referencedAssembly.GetType(name, false, ignoreCase);
						if (result != null)
						{
							break;
						}
					}
				}
			}
			if (result == null)
			{
				// to throw an exception if throwOnError=true
				result = assembly.GetType(name, throwOnError, ignoreCase);
			}

			if (TypesToNotFind.Contains(result))
			{
				result = null;
			}
			return result;
		}

		void CheckCalledOnCorrectThread()
		{
			if (ThrowIfNotOnThread != null && Thread.CurrentThread != ThrowIfNotOnThread)
			{
				throw new InvalidOperationException("Called from the wrong thread.");
			}
		}

		#endregion
	}
}
