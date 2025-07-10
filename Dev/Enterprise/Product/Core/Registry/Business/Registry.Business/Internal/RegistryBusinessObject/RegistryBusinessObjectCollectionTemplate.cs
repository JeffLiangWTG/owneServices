using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public abstract class RegistryBusinessObjectCollectionTemplate<T> : RegistryBusinessObjectCollectionTemplate, IBusinessObjectCollection<T> where T : RegistryBusinessObjectTemplate
	{
		public RegistryBusinessObjectCollectionTemplate(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new T this[int i]
		{
			get { return (T)Elements[i]; }
		}

		public new T AddNew()
		{
			return (T)base.AddNew();
		}

		public IEnumerator<T> GetEnumerator()
		{
			foreach (var item in Elements)
			{
				yield return (T)item;
			}
		}
	}

	public abstract class RegistryBusinessObjectCollectionTemplate : NonPersistentBusinessObjectCollection<RegistryBusinessObjectTemplate>, Enterprise.Integration.Registry.IRegistryBusinessObjectCollectionTemplate, IRegistryBusiness
	{
		public RegistryBusinessObjectCollectionTemplate()
			: this(null, null)
		{
		}

		public RegistryBusinessObjectCollectionTemplate(BusinessObjectFactory factory)
			: this(null, factory)
		{
		}

		public RegistryBusinessObjectCollectionTemplate(FallbackLevel fallbackLevel)
			: this(fallbackLevel, null)
		{
		}

		public RegistryBusinessObjectCollectionTemplate(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(null)
		{
			CurrentFallbackLevel = fallbackLevel;
			CurrentFactory = factory;
		}

		#region Clone

		public IRegistryBusiness Clone(FallbackLevel currentFallbackLevel, BusinessObjectFactory factory)
		{
			RegistryBusinessObjectCollectionTemplate result = GetClone(currentFallbackLevel, factory);

			foreach (RegistryBusinessObjectTemplate element in this)
			{
				result.Add((RegistryBusinessObjectTemplate)element.Clone(currentFallbackLevel, factory));
			}

			PerformPostCloneAction(result);

			return result;
		}

		protected abstract RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory);

		protected virtual void PerformPostCloneAction(IRegistryBusiness registryBusiness)
		{
		}

		#endregion

		#region Current Factory

		protected BusinessObjectFactory CurrentFactory
		{
			get { return currentFactory ?? (currentFactory = RegistryFactory.Instance); }
			set { currentFactory = value; }
		}
		BusinessObjectFactory currentFactory;

		#endregion

		#region IBusinessWithFallback Members

		public FallbackLevel CurrentFallbackLevel
		{
			get { return CurrentFallbackLevelCore; }
			set { CurrentFallbackLevelCore = value; }
		}

		protected virtual FallbackLevel CurrentFallbackLevelCore { get; set; }

		#endregion

		#region Registry Property

		public ZString RegistryName { get; set; }

		#endregion

		#region Validation

		internal void RunPreSaveValidationOnElements()
		{
			foreach (var element in Elements)
			{
				element.RunPreSaveValidation();
			}
		}

		internal bool HasValidationErrors()
		{
			return Elements.Any(x => x.HasErrors);
		}

		#endregion
	}
}
