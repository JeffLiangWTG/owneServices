using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISEntityId : AQISSingleValueBusinessObject
	{
		public AQISEntityId(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override int Code_MaxLength
		{
			get { return 16; }
		}

		#region Lookups

		public AQISEntityIdLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new AQISEntityIdLookups(this);
				}

				return fLookups;
			}
		}
		AQISEntityIdLookups fLookups;

		#endregion

		#region Validation

		public override AQISSingleValueValidation Validation
		{
			get { return new AQISEntityIdValidation(this); }
		}

		#endregion
	}
}
