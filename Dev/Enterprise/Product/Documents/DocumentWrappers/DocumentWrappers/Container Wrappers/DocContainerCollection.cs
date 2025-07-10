using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocContainerCollection : DocumentWrapperCollection
	{
		#region Constructors && Type Overriding
		public DocContainerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static DocContainerCollection New(BusinessObjectFactory factory)
		{
			DocContainerCollection result = null;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(factory);
			}
			else
			{
				result = new DocContainerCollection(factory);
			}
			return result;
		}

		protected delegate DocContainerCollection NewDelegate(BusinessObjectFactory factory);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();
		#endregion

		public new IDocContainer this[int index]
		{
			get
			{
				return (IDocContainer)base[index];
			}
		}

		public IDocSimpleContainerCollection ToIDocSimpleContainerCollection()
		{
			IDocSimpleContainerCollection result = new IDocSimpleContainerCollection(Factory);
			foreach (DocContainer container in this)
			{
				result.Add(container);
			}
			return result;
		}
	}
}

