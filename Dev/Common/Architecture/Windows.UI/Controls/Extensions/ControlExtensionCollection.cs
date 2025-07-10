using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Testing;

namespace CargoWise.Windows.UI
{
	public class ControlExtensionCollection : IControlExtensionCollection
	{
		public ControlExtensionCollection(IExtendedControl owner)
		{
			Argument.NotNull(owner, "owner");

			this.owner = owner;

			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		public IExtendedControl Owner
		{
			get { return owner; }
		}

		public void Add(IControlExtension extension)
		{
			Argument.NotNull(extension, "extension");

			IControlExtension existent = Get(extension.GetType());
			if (existent != null)
			{
				throw new ArgumentException("This collection already contains compatible extension");
			}

			extensions.AddLast(extension);

			if (extension.Owner == null)
			{
				extension.Initialize(owner);
			}

			last = extension;
		}

		public void Remove(IControlExtension extension)
		{
			last = null;

			extensions.Remove(extension);

			if (extension != null)
			{
				extension.Dispose();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public void Remove<TExtension>() where TExtension : class, IControlExtension
		{
			Remove(Get<TExtension>());
		}

		IEnumerator<IControlExtension> IEnumerable<IControlExtension>.GetEnumerator()
		{
			return extensions.GetEnumerator();
		}

		public IEnumerator GetEnumerator()
		{
			return ((IEnumerable<IControlExtension>)this).GetEnumerator();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public bool Supports<TExtension>() where TExtension : class, IControlExtension
		{
			return Get<TExtension>() != null;
		}

		IControlExtension Get(Type extensionType)
		{
			if (last != null && IsCompatible(last, extensionType))
			{
				return last;
			}

			foreach (IControlExtension extension in extensions)
			{
				if (IsCompatible(extension, extensionType))
				{
					last = extension;
					return extension;
				}
			}

			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public TExtension Get<TExtension>() where TExtension : class, IControlExtension
		{
			return Get(typeof(TExtension)) as TExtension;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public void Substitute<TExtension>(IControlExtension extension) where TExtension : class, IControlExtension
		{
			Remove<TExtension>();
			Add(extension);
		}

		public void SetDataBinding(object dataSource, string dataMember)
		{
			foreach (IControlExtension extension in extensions.ToArray())
			{
				IBindableControlExtension bindableExtension = extension as IBindableControlExtension;

				if (bindableExtension != null)
				{
					bindableExtension.SetDataBinding(dataSource, dataMember);
				}
			}
		}

		static bool IsCompatible(IControlExtension extension, Type type)
		{
			Type extensionType = extension.GetType();
			return type.IsAssignableFrom(extensionType) || extensionType.IsAssignableFrom(type);
		}

		readonly IExtendedControl owner;
		readonly LinkedList<IControlExtension> extensions = new LinkedList<IControlExtension>();
		IControlExtension last;

		#region IDisposable Members

		~ControlExtensionCollection()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				SetDataBinding(null, "");

				foreach (IControlExtension extension in extensions)
				{
					extension.Dispose();
				}

				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
		}

		#endregion
	}
}
