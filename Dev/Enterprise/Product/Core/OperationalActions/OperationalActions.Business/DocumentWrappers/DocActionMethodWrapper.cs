using System;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business.DocumentWrappers
{
	public sealed class DocActionMethodWrapper : DocumentWrapper
	{
		public static DocActionMethodWrapper New(string group, OperationalActionMethod method)
		{
			return new DocActionMethodWrapper(group, method);
		}

		DocActionMethodWrapper(string group, OperationalActionMethod method)
		{
			if (group == null) { throw new ArgumentNullException(nameof(group)); }
			if (method == null) { throw new ArgumentNullException(nameof(method)); }

			this.group = group;
			this.method = method;
		}

		public ZString Group
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return group; }
		}

		public ZString Name
		{
			get { return method.Name; }
		}

		public ZString Description
		{
			get { return method.Description; }
		}

		readonly string group;
		readonly OperationalActionMethod method;
	}
}
