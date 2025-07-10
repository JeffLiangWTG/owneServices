using System;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IResourceAccessor
	{
		object Get(Uri uri);
	}
}