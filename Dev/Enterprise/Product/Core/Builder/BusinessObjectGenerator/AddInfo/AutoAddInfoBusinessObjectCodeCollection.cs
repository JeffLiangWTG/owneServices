
namespace Enterprise.BusinessObjectGenerator
{
	public class AutoAddInfoBusinessObjectCodeCollection : AutoBusinessObjectCodeCollection
	{
		public AutoAddInfoBusinessObjectCodeCollection(BusinessObjectInfo info) : base(info)
		{
		}

		public AutoAddInfoBusinessObjectCodeCollection(BusinessObjectInfo info, bool isInZArchitecture) : base(info, isInZArchitecture)
		{
		}

		#region BusinessObject

		public override AutoBusinessObject AutoBusinessObject
		{
			get
			{
				if (fAutoBusinessObject == null)
				{
					fAutoBusinessObject = new AutoAddInfoBusinessObject(Info);
				}
				return fAutoBusinessObject;
			}
		}

		#endregion

		#region Lookups

		public override AutoBusinessObjectLookups AutoBusinessObjectLookups
		{
			get
			{
				if (fAutoBusinessObjectLookups == null)
				{
					fAutoBusinessObjectLookups = new AutoAddInfoBusinessObjectLookups(Info, AutoBusinessObject.AutoProperties);
				}
				return fAutoBusinessObjectLookups;
			}
		}

		#endregion

		#region Schema

		public override AutoBusinessObjectSchema AutoBusinessObjectSchema
		{
			get
			{
				if (fAutoBusinessObjectSchema == null)
				{
					fAutoBusinessObjectSchema = new AutoAddInfoBusinessObjectSchema(Info);
				}
				return fAutoBusinessObjectSchema;
			}
		}

		#endregion

		#region Validation

		public override AutoBusinessObjectValidation AutoBusinessObjectValidation
		{
			get
			{
				if (fAutoBusinessObjectValidation == null)
				{
					fAutoBusinessObjectValidation = new AutoAddInfoBusinessObjectValidation(Info, AutoBusinessObject.AutoProperties);
				}
				return fAutoBusinessObjectValidation;
			}
		}

		#endregion

		#region Implementation

		AutoAddInfoBusinessObject fAutoBusinessObject;
		AutoAddInfoBusinessObjectLookups fAutoBusinessObjectLookups;
		AutoAddInfoBusinessObjectSchema fAutoBusinessObjectSchema;
		AutoAddInfoBusinessObjectValidation fAutoBusinessObjectValidation;

		#endregion
	}
}
