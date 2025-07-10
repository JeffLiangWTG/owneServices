using System;
using System.Collections.Generic;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	public interface IBusinessObjectCreationSource
	{
		/// <summary>
		/// A unique 3 letter code used to identify the creation source.
		/// However, there is no current check to ensure it is actually unique
		/// So you'll have to search the code for it.
		/// </summary>
		string CreationSourceCode { get; }
	}

	public interface IBusinessObjectCreationSourceStack
	{
		IBusinessObjectCreationSource CurrentSource { get; }

		IDisposable PushCreationSource(IBusinessObjectCreationSource creator);
	}

	/// <summary>
	/// This class is used to represent a stack of creation sources for
	/// BusinessObject. Someone wishing to record who creates a particular business
	/// object may use an instance of this - gotten from the ObjectFactory -
	/// to register their new 'creator' of business objects. There after
	/// their particular business object being created can query the
	/// ObjectFactory of this instance to know who created it.
	/// </summary>
	public class BusinessObjectCreationSourceStack : IBusinessObjectCreationSourceStack
	{
		internal BusinessObjectCreationSourceStack()
		{
			Sources = new Stack<IBusinessObjectCreationSource>();
		}

		public IDisposable PushCreationSource(IBusinessObjectCreationSource creator)
		{
			var popper = new DisposableAction(() =>
			{
				var popped = Sources.Pop();
				if (popped != creator)
				{
					throw new InvalidOperationException("The pushing and popping of IBusinessObjectCreationSources is out of order");
				}
			});
			Sources.Push(creator);

			return popper;
		}

		public IBusinessObjectCreationSource CurrentSource => Sources.Count > 0 ? Sources.Peek() : null;

		public Stack<IBusinessObjectCreationSource> Sources { get; }
	}
}
