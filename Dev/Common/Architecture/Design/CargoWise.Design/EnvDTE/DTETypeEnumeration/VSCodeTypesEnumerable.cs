using System;
using System.Collections;
using System.Runtime.InteropServices;
using CargoWise.Common.Interop;

namespace CargoWise.Design.DTE
{
	/// <summary>
	/// An enumerable across fake Type objects in a collection of EnvDTE.CodeElements.
	/// </summary>

	public class VSCodeTypesEnumerable : IEnumerable
	{
		readonly IServiceProvider serviceProvider;

		public VSCodeTypesEnumerable(IServiceProvider serviceProvider, EnvDTE.CodeElements elements)
		{
			this.serviceProvider = serviceProvider;
			CodeElements = elements;
		}

		public EnvDTE.CodeElements CodeElements { get; private set; }

		public IEnumerator GetEnumerator()
		{
			int count = 0;
			try
			{
				count = CodeElements.Count;
			}
			catch (COMException e)
			{
				if (e.ErrorCode != HResult.RPC_E_SERVERCALL_RETRYLATER)
				{
					throw;
				}
			}
			for (int i = 1; i <= count; i++)
			{
				EnvDTE.CodeElement element = null;
				try
				{
					element = CodeElements.Item(i);
				}
				catch (COMException e)
				{
					if (e.ErrorCode != HResult.RPC_E_SERVERCALL_RETRYLATER)
					{
						yield break;
					}
				}
				EnvDTE.CodeType code_type = element as EnvDTE.CodeType;
				if (code_type != null)
				{
					VSCodeType type = VSCodeType.FromCodeType(serviceProvider, code_type.ProjectItem.ContainingProject, code_type);
					yield return type;
				}
				if (code_type != null)
				{
					foreach (Type type in new VSCodeTypesEnumerable(serviceProvider, code_type.Members))
					{
						yield return type;
					}
				}
				if (element is EnvDTE.CodeNamespace code_namespace)
				{
					foreach (Type type in new VSCodeTypesEnumerable(serviceProvider, code_namespace.Members))
					{
						yield return type;
					}
				}
			}
		}
	}
}
