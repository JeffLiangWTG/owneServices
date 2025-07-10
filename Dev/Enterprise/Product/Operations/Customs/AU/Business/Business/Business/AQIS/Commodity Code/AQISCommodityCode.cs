using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISCommodityCode : AQISSingleValueBusinessObject
	{
		public AQISCommodityCode(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override int Code_MaxLength
		{
			get { return 4; }
		}

		#region Lookups

		public AQISCommodityCodeLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new AQISCommodityCodeLookups(this);
				}

				return fLookups;
			}
		}
		AQISCommodityCodeLookups fLookups;

		#endregion

		#region Validation

		public override AQISSingleValueValidation Validation
		{
			get { return new AQISCommodityCodeValidation(this); }
		}

		#endregion
	}
}
