
namespace Enterprise.BusinessObjectGenerator
{
	public class AutoBusinessObjectCodeCollection
	{
		public AutoBusinessObjectCodeCollection(BusinessObjectInfo info)
			: this(info, false)
		{
		}

		public AutoBusinessObjectCodeCollection(BusinessObjectInfo info, bool isInZArchitecture)
		{
			this.Info = info;
			this.IsInZArchitecture = isInZArchitecture;
		}

		#region BusinessObject

		public virtual AutoBusinessObject AutoBusinessObject
		{
			get
			{
				if (fAutoBusinessObject == null)
				{
					if (IsInZArchitecture)
					{
						fAutoBusinessObject = new AutoBusinessObjectLivesInZArchitecture(Info);
					}
					else
					{
						fAutoBusinessObject = new AutoBusinessObject(Info);
					}
				}
				return fAutoBusinessObject;
			}
		}

		#endregion

		#region Schema

		public virtual AutoBusinessObjectSchema AutoBusinessObjectSchema
		{
			get
			{
				if (fAutoBusinessObjectSchema == null)
				{
					fAutoBusinessObjectSchema = new AutoBusinessObjectSchema(Info);
				}
				return fAutoBusinessObjectSchema;
			}
		}

		#endregion

		#region Lookups

		public virtual AutoBusinessObjectLookups AutoBusinessObjectLookups
		{
			get
			{
				if (fAutoBusinessObjectLookups == null)
				{
					fAutoBusinessObjectLookups = new AutoBusinessObjectLookups(Info, AutoBusinessObject.AutoProperties);
				}
				return fAutoBusinessObjectLookups;
			}
		}

		public AutoBusinessObjectLookups_FirstConcreteClass AutoBusinessObjectLookups_FirstConcreteClass
		{
			get
			{
				if (fAutoBusinessObjectLookups_FirstConcreteClass == null)
				{
					fAutoBusinessObjectLookups_FirstConcreteClass = new AutoBusinessObjectLookups_FirstConcreteClass(Info);
				}
				return fAutoBusinessObjectLookups_FirstConcreteClass;
			}
		}

		#endregion

		#region Validation

		public virtual AutoBusinessObjectValidation AutoBusinessObjectValidation
		{
			get
			{
				if (fAutoBusinessObjectValidation == null)
				{
					fAutoBusinessObjectValidation = new AutoBusinessObjectValidation(Info, AutoBusinessObject.AutoProperties);
				}
				return fAutoBusinessObjectValidation;
			}
		}

		public AutoBusinessObjectValidation_FirstConcreteClass AutoBusinessObjectValidation_FirstConcreteClass
		{
			get
			{
				if (fAutoBusinessObjectValidation_FirstConcreteClass == null)
				{
					fAutoBusinessObjectValidation_FirstConcreteClass = new AutoBusinessObjectValidation_FirstConcreteClass(Info);
				}
				return fAutoBusinessObjectValidation_FirstConcreteClass;
			}
		}

		#endregion

		#region Implementation

		protected readonly BusinessObjectInfo Info;
		readonly bool IsInZArchitecture;

		AutoBusinessObject fAutoBusinessObject;
		AutoBusinessObjectSchema fAutoBusinessObjectSchema;
		AutoBusinessObjectValidation fAutoBusinessObjectValidation;
		AutoBusinessObjectValidation_FirstConcreteClass fAutoBusinessObjectValidation_FirstConcreteClass;
		AutoBusinessObjectLookups fAutoBusinessObjectLookups;
		AutoBusinessObjectLookups_FirstConcreteClass fAutoBusinessObjectLookups_FirstConcreteClass;

		#endregion
	}
}
