using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISPermitId : AQISSingleValueBusinessObject
	{
		public AQISPermitId(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override int Code_MaxLength
		{
			get { return 35; }
		}

		public override AQISSingleValueValidation Validation
		{
			get { return new AQISPermitIdValidation(this); }
		}
	}
}
