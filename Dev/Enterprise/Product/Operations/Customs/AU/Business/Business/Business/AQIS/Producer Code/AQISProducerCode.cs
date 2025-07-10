using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISProducerCode : AQISSingleValueBusinessObject
	{
		public AQISProducerCode(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override int Code_MaxLength
		{
			get { return 35; }
		}

		public AQISProducerCodeLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new AQISProducerCodeLookups(this);
				}

				return fLookups;
			}
		}
		AQISProducerCodeLookups fLookups;

		public override AQISSingleValueValidation Validation
		{
			get { return new AQISProducerCodeValidation(this); }
		}
	}
}
