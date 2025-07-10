using System;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface IServiceContainer
	{
		[SuppressMessage("Microsoft.Design", "CA1004: Generic methods should provide type parameter")]
		void Register<T>(object instance) where T : class;
		void Register<T>(Func<T> serviceProvider) where T : class;
		T Resolve<T>() where T : class;
	}
}