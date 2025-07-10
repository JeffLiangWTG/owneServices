using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISConcernType : AQISSingleValueBusinessObject
	{
		public AQISConcernType(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public const string AQISRuralConcernTypeCode = "RURL";

		#region Code

		protected override int Code_MaxLength
		{
			get { return 4; }
		}

		#endregion

		#region Lookups

		public AQISConcernTypeLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new AQISConcernTypeLookups(this);
				}

				return fLookups;
			}
		}
		AQISConcernTypeLookups fLookups;

		#endregion

		#region Validation

		public override AQISSingleValueValidation Validation
		{
			get { return new AQISConcernTypeValidation(this); }
		}

		#endregion
	}
}
