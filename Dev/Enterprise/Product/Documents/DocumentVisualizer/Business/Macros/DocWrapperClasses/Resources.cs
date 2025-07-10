using System;
using CargoWise.Common;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class Resources
	{
		public Resources(IResourceAccessor accessor)
		{
			Argument.NotNull(accessor, nameof(accessor));

			this.resources = accessor;
		}

		readonly IResourceAccessor resources;

		[MacroInvokable]
		public object Get(string uri)
		{
			object result = null;

			if (string.IsNullOrWhiteSpace(uri))
			{
				return null;
			}

			try
			{
				result = resources.Get(new Uri(uri));
			}
			catch (UriFormatException exc)
			{
				throw new MacroRuntimeException(exc);
			}

			return result;
		}
	}
}