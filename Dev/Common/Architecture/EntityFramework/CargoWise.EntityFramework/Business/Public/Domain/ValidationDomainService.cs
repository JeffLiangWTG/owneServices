using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	public class ValidationDomainService : IService
	{
		protected ValidationDomainService()
		{
		}

		public static ValidationDomainService Get(BusinessObjectFactory factory)
		{
			ValidationDomainService result = factory.ServiceContainer.GetService<ValidationDomainService>();
			if (result == null)
			{
				result = new ValidationDomainService();
				factory.ServiceContainer.AddService(result);
			}
			return result;
		}

		#region Domain Validation Groups

		public DomainValidationGroup MainGroup
		{
			get { return mainGroup ?? (mainGroup = new DomainValidationGroup()); }
		}
		DomainValidationGroup mainGroup;

		public bool HasDomainValidation
		{
			get
			{
				return
					mainGroup != null ||
					(additionalGroups != null && additionalGroups.Count > 0) ||
					ValidationRequested != null;
			}
		}

		public void AddAllFrom(ValidationDomainService from)
		{
			if (from != null && from.HasDomainValidation && from != this)
			{
				foreach (DomainValidationGroup group in from.AllValidationGroups)
				{
					if (!AdditionalGroups.Contains(group))
					{
						AdditionalGroups.Add(group);
					}
				}
			}
		}

		public IEnumerable<DomainValidationGroup> AllValidationGroups
		{
			get { return new ZIterator<DomainValidationGroup>(MainGroup, AdditionalGroups); }
		}

		List<DomainValidationGroup> AdditionalGroups
		{
			get { return additionalGroups ?? (additionalGroups = new List<DomainValidationGroup>()); }
		}
		List<DomainValidationGroup> additionalGroups;

		#endregion

		#region ValidationRequested event

		public event ValidationRequestedEventHandler ValidationRequested;

		void OnValidationRequested(ValidationRequestedEventArgs e)
		{
			if (ValidationRequested != null)
			{
				ValidationRequested(this, e);
			}
		}

		internal bool NotifyValidationRequested(ZPropertyInfo property)
		{
			ValidationRequestedEventArgs e = new ValidationRequestedEventArgs(property);
			OnValidationRequested(e);
			return !e.Cancel;
		}

		#endregion
	}
}
