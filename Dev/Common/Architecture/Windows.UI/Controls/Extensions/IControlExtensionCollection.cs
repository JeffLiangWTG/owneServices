using System;
using System.Collections.Generic;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// A collection of IControlExtension objects.
	/// </summary>
	public interface IControlExtensionCollection : IEnumerable<IControlExtension>, IDisposable
	{
		IExtendedControl Owner { get; }

		void Add(IControlExtension extension);
		void Remove(IControlExtension extension);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		void Remove<TExtension>() where TExtension : class, IControlExtension;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		void Substitute<TExtension>(IControlExtension extension) where TExtension : class, IControlExtension;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		bool Supports<TExtension>() where TExtension : class, IControlExtension;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		TExtension Get<TExtension>() where TExtension : class, IControlExtension;

		void SetDataBinding(object dataSource, string dataMember);
	}
}
